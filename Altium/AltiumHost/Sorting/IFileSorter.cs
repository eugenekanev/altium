using System.Threading.Tasks;

namespace AltiumHost.Sorting
{
    public interface IFileSorter
    {
        Task Sort(string filePath);
    }
}