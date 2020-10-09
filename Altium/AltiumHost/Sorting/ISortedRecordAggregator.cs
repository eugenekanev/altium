using System.Collections.Generic;
using System.Threading.Tasks;

namespace AltiumHost.Sorting
{
    public interface ISortedRecordAggregator
    {
        void Put(List<string> record);

        public Task<List<string>> CompleteAsync();
    }
}