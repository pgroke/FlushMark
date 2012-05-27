// Copyright (c) 2012, Paul Groke
// For conditions of distribution and use, see copyright notice in LICENSE.txt

#pragma once

#include <windows.h>
#include <wtypes.h>

#ifdef FLUSHMARKCORE_EXPORTS
#define FLUSHMARKCORE_API __declspec(dllexport)
#else
#define FLUSHMARKCORE_API __declspec(dllimport)
#endif

typedef BYTE uint8_t;
typedef USHORT uint16_t;
typedef ULONG uint32_t;
typedef ULONGLONG uint64_t;

static uint32_t const FLUSHMARKCORE_TESTTYPE_RANDOM = 0;
static uint32_t const FLUSHMARKCORE_TESTTYPE_LINEAR = 1;

struct FlushMarkSettings
{
	wchar_t const* filePath;

	uint32_t testMode;
	uint32_t flags;

	uint64_t testSize;
	uint32_t pageSize;

	uint32_t minPageCount;
	uint32_t minMilliseconds;
	uint32_t flushFrequency;
};

struct FlushMarkResult
{
	uint32_t writtenPageCount;
	uint32_t elapsedMilliseconds;
};

extern "C" FLUSHMARKCORE_API BOOL FlushMark_PrepareFile(FlushMarkSettings const* settings, BSTR* out_errorMessage);
extern "C" FLUSHMARKCORE_API BOOL FlushMark_RunBenchmark(FlushMarkSettings* settings, FlushMarkResult* out_result, BSTR* out_errorMessage);
