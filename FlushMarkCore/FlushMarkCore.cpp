// Copyright (c) 2012, Paul Groke
// For conditions of distribution and use, see copyright notice in LICENSE.txt

#include "stdafx.h"
#include "FlushMarkCore.h"
#include <Windows.h>
#include <MMSystem.h>
#include <oleauto.h>
#include <time.h>
#include <stdexcept>
#include <string>
#include <vector>
#include <stdlib.h>
#include <assert.h>

#pragma comment(lib, "winmm")

/////////////////////////////////////////////////////////////////////////////

namespace
{
	/////////////////////////////////////////////////////////////////////////////

	class RNG
	{
	public:
		static const uint32_t PHI = 0x9e3779b9;
		static const uint64_t A = 18782LL;
		static const uint32_t R = 0xfffffffe;
		static const size_t QSIZE = 4096;

		RNG()
			: m_c(362436),
			m_i(QSIZE - 1)
		{
			Seed();
		}

		void Seed()
		{
			Seed(::timeGetTime() ^ static_cast<uint32_t>(time(0)));
		}

		void Seed(uint32_t x)
		{
			m_q[0] = x;
			m_q[1] = x + PHI;
			m_q[2] = x + PHI + PHI;

			for (int i = 3; i < QSIZE; i++)
				m_q[i] = m_q[i - 3] ^ m_q[i - 2] ^ PHI ^ i;
		}

		uint32_t GetUInt32()
		{
			m_i = (m_i + 1) % QSIZE;
			uint64_t t = A * m_q[m_i] + m_c;
			m_c = (t >> 32);
			uint32_t x = static_cast<uint32_t>(t + m_c);

			if (x < m_c)
			{
				x++;
				m_c++;
			}

			return (m_q[m_i] = R - x);
		}

		uint64_t GetUInt64()
		{
			return GetUInt32() + (static_cast<uint64_t>(GetUInt32()) << 32);
		}

		void GetRandomBytes(uint8_t* buffer, size_t byteCount)
		{
			size_t i = 0;

			while (byteCount > 8)
			{
				uint32_t x = GetUInt32();
				buffer[i] = x & 0xFF;
				buffer[i + 1] = (x >> 8) & 0xFF;
				buffer[i + 2] = (x >> 16) & 0xFF;
				buffer[i + 3] = (x >> 24) & 0xFF;
				uint32_t y = GetUInt32();
				buffer[i + 4] = y & 0xFF;
				buffer[i + 5] = (y >> 8) & 0xFF;
				buffer[i + 6] = (y >> 16) & 0xFF;
				buffer[i + 7] = (y >> 24) & 0xFF;
				i += 8;
				byteCount -= 8;
			}

			while (byteCount > 0)
			{
				buffer[i++] = GetUInt32() & 0xFF;
				byteCount--;
			}
		}

	private:
		uint32_t m_q[QSIZE];
		uint32_t m_c;

		uint32_t m_i;
	};

	/////////////////////////////////////////////////////////////////////////////

	struct FileHandleGuard
	{
		explicit FileHandleGuard(HANDLE fileHandle = INVALID_HANDLE_VALUE)
			: m_handle(fileHandle)
		{
		}

		~FileHandleGuard()
		{
			if (m_handle != INVALID_HANDLE_VALUE)
				CloseHandle(m_handle);
		}

		HANDLE m_handle;
	};

	/////////////////////////////////////////////////////////////////////////////

	struct ThreadPriorityGuard
	{
		explicit ThreadPriorityGuard(int newPriority)
			: m_oldPriority(::GetThreadPriority(::GetCurrentThread()))
		{
			::SetThreadPriority(::GetCurrentThread(), newPriority);
		}

		~ThreadPriorityGuard()
		{
			::SetThreadPriority(::GetCurrentThread(), m_oldPriority);
		}

		int m_oldPriority;
	};

	/////////////////////////////////////////////////////////////////////////////

	struct TimeBeginPeriodGuard
	{
		explicit TimeBeginPeriodGuard()
		{
			m_period = 0;

			for (UINT p = 1; p < 20; p++)
			{
				if (::timeBeginPeriod(p) == TIMERR_NOERROR)
				{
					m_period = p;
					break;
				}
			}
		}

		~TimeBeginPeriodGuard()
		{
			if (m_period != 0)
				::timeEndPeriod(m_period);
		}

		UINT m_period;
	};

	/////////////////////////////////////////////////////////////////////////////

	class FlushMarkTestFile
	{
	public:

		static uint32_t const KIB = 1024;
		static uint32_t const MIB = KIB * KIB;
		static uint32_t const GIB = KIB * KIB * KIB;

		explicit FlushMarkTestFile(FlushMarkSettings const& settings)
			: m_settings(settings)
		{
			::SetLastError(NO_ERROR);

			if (!m_settings.filePath)
			{
				::SetLastError(ERROR_INVALID_PARAMETER);
				throw std::runtime_error("Invalid benchmark file path!");
			}

			// Substitute defaults

			if (!m_settings.testSize)
				m_settings.testSize = 1024 * 1024 * 1024;

			if (!m_settings.pageSize)
				m_settings.pageSize = 4096;

			if (!m_settings.minPageCount)
				m_settings.minPageCount = 30;

			if (!m_settings.minMilliseconds)
				m_settings.minMilliseconds = 3000;

			if (m_settings.testSize < m_settings.pageSize)
				m_settings.testSize = m_settings.pageSize;
			else
				m_settings.testSize = (m_settings.testSize / m_settings.pageSize) * m_settings.pageSize;

			InitPageBuffer();
			InitRandomPool();
			InitLastPageOffset();

			// Open the benchmark file

			DWORD const desiredAccess = GENERIC_READ | GENERIC_WRITE;
			DWORD const shareMode = 0;
			DWORD const creationDisposition = OPEN_ALWAYS;
			DWORD const flagsAndAttributes = FILE_ATTRIBUTE_NORMAL | FILE_FLAG_NO_BUFFERING | FILE_FLAG_WRITE_THROUGH;

			m_testFile.m_handle = ::CreateFileW(m_settings.filePath, desiredAccess, shareMode, 0, creationDisposition, flagsAndAttributes, 0);
			if (m_testFile.m_handle == INVALID_HANDLE_VALUE)
				throw std::runtime_error("Could not open file.");

			memset(&m_overlapped, 0, sizeof(m_overlapped));

			m_currentSize = GetFileLength();
		}

		void PrepareFile()
		{
			uint64_t const pageCount = m_settings.testSize / m_settings.pageSize;

			size_t const maxChunk = (MIB / m_settings.pageSize + 1) * m_settings.pageSize;
			std::vector<uint8_t> buffer(maxChunk);

			while (m_currentSize < m_settings.testSize)
			{
				uint64_t const offset = (m_currentSize / m_settings.pageSize)  * m_settings.pageSize;
				uint64_t const missing = m_settings.testSize - offset;

				size_t const chunk = missing <= maxChunk ? static_cast<size_t>(missing) : maxChunk;
				m_rng.GetRandomBytes(&buffer[0], chunk);
				Write(offset, &buffer[0], chunk);
			}
		}

		FlushMarkResult RunBenchmark()
		{
			PrepareFile();

			TimeBeginPeriodGuard timeBeginPeriodGuard;
			ThreadPriorityGuard threadPriorityGuard(THREAD_PRIORITY_TIME_CRITICAL);

			WarmUp();

			size_t writtenPageCount = 0;
			DWORD elapsedMilliseconds = 0;

			DWORD const t0 = ::timeGetTime();

			bool flushed = false;
			for (;;)
			{
				WriteSinglePage(true);
				writtenPageCount++;

				if (m_settings.flushFrequency && (writtenPageCount % m_settings.flushFrequency) == 0)
				{
					Flush();
					flushed = true;
				}
				else
					flushed = false;

				elapsedMilliseconds = ::timeGetTime() - t0;

				if (flushed || m_settings.flushFrequency == 0)
				{
					// allow exit
					if (writtenPageCount >= m_settings.minPageCount && elapsedMilliseconds >= m_settings.minMilliseconds)
						break;
				}
			}

			if (!flushed)
				Flush();

			DWORD t1 = ::timeGetTime();

			FlushMarkResult result = {};
			result.writtenPageCount = writtenPageCount;
			result.elapsedMilliseconds = t1 - t0;
			return result;
		}

	private:

		void Flush()
		{
			BOOL rc = ::FlushFileBuffers(m_testFile.m_handle);
			if (!rc)
				throw std::runtime_error("FlushFileBuffers failed.");
		}

		void WarmUp()
		{
			Flush();
			WriteSinglePage();
			WriteSinglePage();
			Flush();

			::Sleep(1000);

			HANDLE currentThread = ::GetCurrentThread();
			::SetThreadPriority(currentThread, ::GetThreadPriority(currentThread));
		}

		void WriteSinglePage(bool writeActualData = true)
		{
			RandomizePageBuffer();
			Write(GetNextPageOffset(), m_pageBufferAligned, writeActualData ? m_settings.pageSize : 0);
		}

		void Write(uint64_t position, uint8_t const* data, size_t byteCount)
		{
			m_overlapped.Offset = position & 0xFFFFFFFF;
			m_overlapped.OffsetHigh = (position >> 32) & 0xFFFFFFFF;

			DWORD written = 0;
			BOOL rc = ::WriteFile(m_testFile.m_handle, data, byteCount, &written, &m_overlapped);
			if (!rc && ::GetLastError() == ERROR_IO_PENDING)
				rc = ::GetOverlappedResult(m_testFile.m_handle, &m_overlapped, &written, true);
			if (!rc || written != byteCount)
			{
				DWORD error = ::GetLastError();
				throw std::runtime_error("WriteFile failed.");
			}

			uint64_t newpos = position + byteCount;
			if (newpos > m_currentSize)
				m_currentSize = newpos;
		}

		uint64_t GetNextPageOffset()
		{
			if (m_settings.testMode == FLUSHMARKCORE_TESTTYPE_LINEAR)
			{
				uint64_t offset = ((m_lastPageOffset + m_settings.pageSize) % m_settings.testSize);
				offset = (offset / m_settings.pageSize) * m_settings.pageSize;

				m_lastPageOffset = offset;
				return offset;
			}
			else
			{
				uint64_t offset = (m_rng.GetUInt64() % m_settings.testSize);
				offset = (offset / m_settings.pageSize) * m_settings.pageSize;

				m_lastPageOffset = offset;
				return offset;
			}
		}

		void RandomizePageBuffer()
		{
			size_t const poolSize = m_randomPool.size();
			if (m_settings.pageSize > (poolSize / 2))
				throw std::logic_error("internal error");

			size_t const maxOffset = poolSize - m_settings.pageSize;
			size_t const offset = static_cast<size_t>(m_rng.GetUInt64() % maxOffset);

			memcpy(m_pageBufferAligned, &m_randomPool[offset], m_settings.pageSize);
		}

		void InitPageBuffer()
		{
			m_pageBuffer.resize(m_settings.pageSize * 2);
			uint8_t* ptr = &m_pageBuffer[0];
			size_t bufferAddress = reinterpret_cast<size_t>(ptr);
			size_t offset = (m_settings.pageSize - (bufferAddress % m_settings.pageSize)) % m_settings.pageSize;

			m_pageBufferAligned = ptr + offset;

			assert(reinterpret_cast<size_t>(m_pageBufferAligned) % m_settings.pageSize == 0);
		}

		void InitRandomPool()
		{
#ifdef _DEBUG
			size_t randomPoolSize = 16 * MIB;
#else
			size_t randomPoolSize = 128 * MIB;
#endif

			if (randomPoolSize < (m_settings.pageSize * 3))
				randomPoolSize = m_settings.pageSize * 3;

			m_randomPool.resize(randomPoolSize);
			m_rng.GetRandomBytes(&m_randomPool[0], randomPoolSize);
		}

		void InitLastPageOffset()
		{
			uint64_t offset = (m_rng.GetUInt64() % (m_settings.testSize / 2));
			offset = (offset / m_settings.pageSize) * m_settings.pageSize;

			m_lastPageOffset = offset;
		}

		uint64_t GetFileLength() const
		{
			::SetLastError(NO_ERROR);
			DWORD high = 0;
			DWORD low = ::GetFileSize(m_testFile.m_handle, &high);
			if (low == INVALID_FILE_SIZE && ::GetLastError() != NO_ERROR)
				throw std::runtime_error("GetFileSize failed.");

			return low + (static_cast<uint64_t>(high) << 32);
		}

		FlushMarkSettings m_settings;
		FileHandleGuard m_testFile;
		FileHandleGuard m_overlappedEvent;
		OVERLAPPED m_overlapped;
		uint64_t m_currentSize;

		RNG m_rng;
		std::vector<uint8_t> m_randomPool;
		std::vector<uint8_t> m_pageBuffer;
		uint8_t* m_pageBufferAligned;

		uint64_t m_lastPageOffset;
	};

	void StoreErrorMessage(std::exception const& e, BSTR* out_bstr)
	{
		if (!out_bstr)
			return;

		try
		{
			std::string errorMessage = e.what();

			size_t requiredSize = mbstowcs(0, errorMessage.c_str(), 0);
			std::vector<wchar_t> buffer(requiredSize + 1);
			mbstowcs(&buffer[0], errorMessage.c_str(), requiredSize + 1);

			BSTR newBstr = ::SysAllocString(&buffer[0]);
			if (*out_bstr)
				::SysFreeString(*out_bstr);
			*out_bstr = newBstr;
		}
		catch (...)
		{
			*out_bstr = 0;
		}
	}

} // namespace

extern "C" FLUSHMARKCORE_API BOOL FlushMark_PrepareFile(FlushMarkSettings const* settings, BSTR* out_errorMessage)
{
	try
	{
		::SetLastError(NO_ERROR);
		if (out_errorMessage)
			*out_errorMessage = 0;

		if (!settings)
		{
			::SetLastError(ERROR_INVALID_PARAMETER);
			return false;
		}

		FlushMarkTestFile testFile(*settings);
		testFile.PrepareFile();

		return true;
	}
	catch (std::exception const& e)
	{
		StoreErrorMessage(e, out_errorMessage);
		return false;
	}
}

extern "C" FLUSHMARKCORE_API BOOL FlushMark_RunBenchmark(FlushMarkSettings* settings, FlushMarkResult* out_result, BSTR* out_errorMessage)
{
	try
	{
		::SetLastError(NO_ERROR);
		if (out_errorMessage)
			*out_errorMessage = 0;
		if (out_result)
		{
			out_result->writtenPageCount = 0;
			out_result->elapsedMilliseconds = 0;
		}

		if (!settings || !out_result)
		{
			::SetLastError(ERROR_INVALID_PARAMETER);
			return false;
		}

		FlushMarkTestFile testFile(*settings);
		*out_result = testFile.RunBenchmark();

		return true;
	}
	catch (std::exception const& e)
	{
		StoreErrorMessage(e, out_errorMessage);
		return false;
	}
}
