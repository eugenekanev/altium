using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace AltiumHost.Sorting
{
    internal class FileCutter : IFileCutter
    {
        private readonly ISortedRecordAggregator _sortedRecordAggregator;

        private readonly ILogger Log = Serilog.Log.ForContext<FileCutter>();

        public FileCutter(ISortedRecordAggregator sortedRecordAggregator)
        {
            _sortedRecordAggregator = sortedRecordAggregator;
        }

        public async Task<List<string>> CutFileAsync(string inputfilepath)
        {
            using var inputStream = new StreamReader(inputfilepath, Encoding.Unicode, false, 655360);
            Log.Debug($"Starting processing the file with name : {inputfilepath}");
            var cutSmallFiles = await ReadSourceFileAsync(inputStream);
            Log.Debug($"Finished processing the file with name : {inputfilepath}");

            return cutSmallFiles;
        }

        private async Task<List<string>> ReadSourceFileAsync(StreamReader streamreader)
        {
            var batchSize = 1000;
            var batch = new List<string>(batchSize);

            while (streamreader.Peek() >= 0)
                if (batch.Count == batchSize)
                {
                    _sortedRecordAggregator.Put(batch);
                    batch = new List<string>(batchSize);
                }
                else
                {
                    var line = await streamreader.ReadLineAsync().ConfigureAwait(false);
                    batch.Add(line);
                }

            return await _sortedRecordAggregator.CompleteAsync();
        }
    }
}