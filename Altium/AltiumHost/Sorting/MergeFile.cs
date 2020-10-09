using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AltiumHost.Sorting
{
    internal class MergeFile : IEnumerator<SortedRecord>
    {
        private readonly StreamReader _reader;

        private SortedRecord _current;

        public MergeFile(string file)
        {
            _reader = new StreamReader(file, Encoding.Unicode, false, 65536);
            _current = ReadRecord();
        }

        public void Dispose()
        {
            _reader.Close();
        }

        public bool MoveNext()
        {
            _current = ReadRecord();
            return !_current.Equals(SortedRecord.None);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        SortedRecord IEnumerator<SortedRecord>.Current => _current;

        public object Current => _current;

        private SortedRecord ReadRecord()
        {
            if (_reader.Peek() >= 0)
            {
                var line = _reader.ReadLine();
                var wordsinline = line.Split(". ", StringSplitOptions.RemoveEmptyEntries);

                var word = wordsinline[1];
                var number = int.Parse(wordsinline[0]);

                return new SortedRecord(number, word, line);
            }

            return SortedRecord.None;
        }
    }
}