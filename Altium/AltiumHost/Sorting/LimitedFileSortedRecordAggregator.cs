using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace AltiumHost.Sorting
{
    internal class LimitedFileSortedRecordAggregator : ISortedRecordAggregator
    {
        private readonly SemaphoreSlim _concurrencySemaphore;
        private readonly int _thresholdCountOfRecordsInTempFile;
        private readonly string folderName = "temp";
        private List<string> _currentChunk;
        private int _fileCounter;

        private List<Task<string>> _sortAndFlushTasks = new List<Task<string>>();

        public LimitedFileSortedRecordAggregator(int thresholdCountOfRecordsInTempFile, int maxCuttingParallelism)
        {
            _thresholdCountOfRecordsInTempFile = thresholdCountOfRecordsInTempFile;
            _currentChunk = new List<string>();
            _concurrencySemaphore = new SemaphoreSlim(maxCuttingParallelism);

            CreateFolder();
        }

        public void Put(List<string> records)
        {
            _currentChunk.AddRange(records);

            if (_currentChunk.Count > _thresholdCountOfRecordsInTempFile)
            {
                _concurrencySemaphore.Wait();
                _sortAndFlushTasks.Add(SortAndFlush());
            }
        }

        public async Task<List<string>> CompleteAsync()
        {
            _sortAndFlushTasks.Add(SortAndFlush());

            var files = await Task.WhenAll(_sortAndFlushTasks);

            _currentChunk = null;
            _sortAndFlushTasks = null;
            _concurrencySemaphore.Dispose();

            return files.ToList();
        }

        private void CreateFolder()
        {
            if (Directory.Exists(folderName)) Directory.Delete(folderName, true);

            Directory.CreateDirectory(folderName);
        }

        private StreamWriter CreateNewFile(string filePath)
        {
            return new StreamWriter(filePath, false, Encoding.Unicode, 65536);
        }

        private Task<string> SortAndFlush()
        {
            _fileCounter = _fileCounter + 1;

            var fileName = _fileCounter + ".txt";

            var closedChunk = _currentChunk;

            _currentChunk = new List<string>(_currentChunk.Count);

            return Task.Factory.StartNew(() =>
            {
                Log.Debug($"Start sorting the file with name {fileName}");

                var sortedRecords = new List<SortedRecord>(closedChunk.Count);

                foreach (var line in closedChunk)
                {
                    var wordsinline = line.Split(". ", StringSplitOptions.RemoveEmptyEntries);

                    var word = wordsinline[1];
                    var number = int.Parse(wordsinline[0]);

                    var record = new SortedRecord(number, word, line);
                    sortedRecords.Add(record);
                }

                sortedRecords.Sort(SortedRecord.SortWordNumberAscending());
                Log.Debug($"Stop sorting the file with name {fileName}");

                var filePath = Path.Combine(folderName, fileName);
                using var currentFileStream = CreateNewFile(filePath);
                Log.Debug($"Start writing the file with name {fileName}");
                foreach (var record in sortedRecords) currentFileStream.WriteLine(record.OriginalString);
                Log.Debug($"Stop writing the file with name {fileName}");

                _concurrencySemaphore.Release();

                return filePath;
            }, TaskCreationOptions.LongRunning);
        }
    }
}