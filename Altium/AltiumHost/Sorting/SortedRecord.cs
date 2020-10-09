using System.Collections.Generic;

namespace AltiumHost.Sorting
{
    public readonly struct SortedRecord
    {
        public SortedRecord(int number, string word, string originalString)
        {
            Number = number;
            OriginalString = originalString;
            Word = word;
        }

        public int Number { get; }
        public string OriginalString { get; }
        public string Word { get; }

        public static IComparer<SortedRecord> SortWordNumberAscending()
        {
            return new SortWordNumberAscendingHelper();
        }

        public static SortedRecord None = new SortedRecord(0, string.Empty, string.Empty);

        private class SortWordNumberAscendingHelper : IComparer<SortedRecord>
        {
            int IComparer<SortedRecord>.Compare(SortedRecord sortedRecord1, SortedRecord sortedRecord2)
            {
                var wordCompare = string.CompareOrdinal(sortedRecord1.Word, sortedRecord2.Word);

                if (wordCompare != 0) return wordCompare;

                if (sortedRecord1.Number > sortedRecord2.Number)
                    return 1;

                if (sortedRecord1.Number < sortedRecord2.Number)
                    return -1;

                return 0;
            }
        }

        public bool Equals(SortedRecord other)
        {
            return OriginalString == other.OriginalString;
        }

        public override bool Equals(object obj)
        {
            return obj is SortedRecord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return OriginalString != null ? OriginalString.GetHashCode() : 0;
        }
    }
}