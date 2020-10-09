using System.Collections.Generic;
using System.Threading.Tasks;

namespace AltiumHost.Sorting
{
    public interface IFileCutter
    {
        Task<List<string>> CutFileAsync(string inputfilepath);
    }
}