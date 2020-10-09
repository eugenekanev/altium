using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;

namespace AltiumHost.Generator
{
    internal class FileWriter
    {
        private readonly string _fileName;
        private readonly ChannelReader<List<string>> _reader;

        public FileWriter(string fileName, ChannelReader<List<string>> reader)
        {
            _fileName = fileName;
            _reader = reader;
        }

        public async Task StartAsync()
        {
            Log.Debug($"Start writing magic words sequence to file {_fileName}");
            await using var file =
                new StreamWriter(_fileName, false, Encoding.Unicode, 65536);

            await foreach (var batch in _reader.ReadAllAsync().ConfigureAwait(false))
            foreach (var item in batch)
                await file.WriteLineAsync(item).ConfigureAwait(false);

            Log.Debug($"Stop writing magic words sequence to file {_fileName}");
        }
    }
}