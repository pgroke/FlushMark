FlushMark
=========

FlushMark is a simple benchmark application for testing flushed IO performance on Windows.

Homepage: https://github.com/pgroke/FlushMark

License
=======

FlushMark is distributed under the Simplified BSD License (see LICENSE.txt).

Installation
============

No installation is required, just unzip and start "FlushMark.exe".

FlushMark requires .NET Framework 2.0.
Besides that it should be able to run an ony version of Windows since Windows XP (probably even Windows 2000 - haven't tested).

Usage
=====

* Start FlushMark.exe.
* Select the drive that shall be tested (only drives that are mapped to a drive letter can be selected)
* Select the test parameters
* Press "Go!"

FlushMark can use Contig ("contig.exe") to defragment the test file before running the benchmark.
For this to work, you have to copy "contig.exe" to the same directory where "FlushMark.exe" is located.
You can get Contig here: http://technet.microsoft.com/en-us/sysinternals/bb897428

NOTE:	FlushMark will create a directory called X:\FlushMarkTestData (where X is the drive you selected)
		and create a single file called "test.dat" there.
		Upon exit, FlushMark will *not* delete that directory/file. This way FlushMark doesn't have to re-create the test-file
		for every test, which is a good thing when testing SSDs (saves write cycles).

Test parameters
===============

Test size: the size of the test file.
	FlushMark will create a file with the specified size, filled with random data.
	(The random algorithm isn't optimized for performance, so this will probably not run at the max. speed your drive can handle.)

Page size: the "page size" that FlushMark will use for IOs.
	You cannot specify a page size smaller then the native block size of your drive.
	Normally 512 byte should be fine, except for "extended format" drives, where the native block size is 4K.

Test mode:
	Random = FlushMark will write pages at random positions in the test file.
	Linear = FlushMark will write pages in a "linear" fashion.

Flush frequency:
	0 = No flushes, except for one single flush before the end-time is taken.
		This is what most benchmarks do (or hopefully do, maybe some just don't flush at all)
	1 = Flush after every single page.
	2 = Flush after two pages have beed written.
	...
