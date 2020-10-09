using System.Collections.Generic;
using System.Numerics;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AltiumHost.Generator
{
    internal class MagicFileGenerator : IFileGenerator
    {
        private readonly Channel<List<string>> _channel;
        private readonly int _maxDifferentWords;

        public MagicFileGenerator(int maxDifferentWords, int txtprocessorwriterthreshold)
        {
            _maxDifferentWords = maxDifferentWords;
            _channel = Channel.CreateBounded<List<string>>(new BoundedChannelOptions(txtprocessorwriterthreshold)
                {SingleReader = true, SingleWriter = true});
        }

        public async Task GenerateAsync(string fileName, BigInteger fileSize)
        {
            var fileWriter = new FileWriter(fileName, _channel.Reader);

            var tasks = new List<Task>();
            var writerTask = Task.Run(async () => await fileWriter.StartAsync().ConfigureAwait(false));
            tasks.Add(writerTask);

            var magicWordsGenerator =
                new MagicWordsGenerator(fileSize, _maxDifferentWords, _channel.Writer);

            var generatorTask =
                Task.Run(async () => await magicWordsGenerator.GenerateWordsAsync().ConfigureAwait(false));

            tasks.Add(generatorTask);

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}