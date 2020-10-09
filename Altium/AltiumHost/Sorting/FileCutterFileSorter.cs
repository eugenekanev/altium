using System.Threading.Tasks;
using Serilog;

namespace AltiumHost.Sorting
{
    internal class FileCutterFileSorter : IFileSorter
    {
        private readonly IFileCutter _fileCutter;
        private readonly IFileMerger _fileMerger;

        public FileCutterFileSorter(IFileCutter fileCutter, IFileMerger fileMerger)
        {
            _fileCutter = fileCutter;
            _fileMerger = fileMerger;
        }

        public async Task Sort(string filePath)
        {
            Log.Debug($"Start sorting the file with name {filePath}");
            var files = await _fileCutter.CutFileAsync(filePath);

            foreach (var file in files) Log.Debug($"File {file}");
            Log.Debug($"Stop cutting the file with name {filePath}");

            Log.Debug("Start Merging");
            await _fileMerger.MergeAsync(files);
            Log.Debug("Stop Merging");
        }
    }
}