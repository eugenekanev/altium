using System.Numerics;
using System.Threading.Tasks;

namespace AltiumHost.Generator
{
    internal interface IFileGenerator
    {
        Task GenerateAsync(string fileName, BigInteger fileSize);
    }
}