using System.Collections.Generic;
using System.Threading.Tasks;

namespace AltiumHost.Sorting
{
    public interface IFileMerger
    {
        Task MergeAsync(List<string> files);
    }
}