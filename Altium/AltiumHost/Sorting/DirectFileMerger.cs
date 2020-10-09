using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltiumHost.Sorting
{
    internal class DirectFileMerger : IFileMerger
    {
        private readonly string outputFileName = "out.txt";

        public async Task MergeAsync(List<string> files)
        {
            var fileEnumerators =
                files.Select(file => new MergeFile(file)).Cast<IEnumerator<SortedRecord>>().ToList();

            await using var outputStreamWriter =
                new StreamWriter(outputFileName, false, Encoding.Unicode, 65536);

            IEnumerator<SortedRecord> next = null;

            var comparer = SortedRecord.SortWordNumberAscending();
            while (true)
            {
                var done = true;
                foreach (var fileEnumerator in fileEnumerators)
                {
                    done = false;
                    if (next == null || comparer.Compare(fileEnumerator.Current, next.Current) < 1)
                        next = fileEnumerator;
                }

                if (done) break;

                await outputStreamWriter.WriteLineAsync(next.Current.OriginalString).ConfigureAwait(false);
                if (!next.MoveNext())
                {
                    next.Dispose();
                    fileEnumerators.Remove(next);
                    next = null;
                }
            }
        }
    }
}