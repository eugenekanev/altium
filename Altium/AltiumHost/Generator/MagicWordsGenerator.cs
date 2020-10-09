using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;

namespace AltiumHost.Generator
{
    internal class MagicWordsGenerator
    {
        private readonly BigInteger _fileSize;
        private readonly int _maxDifferentWords;
        private readonly Dictionary<int, (string, int)> _numbers;
        private readonly Dictionary<int, (string, int)> _words;
        private readonly ChannelWriter<List<string>> _writer;

        public MagicWordsGenerator(BigInteger fileSize, int maxDifferentWords, ChannelWriter<List<string>> writer)
        {
            _maxDifferentWords = maxDifferentWords;
            _words = PopulateWordsDictionary(maxDifferentWords);
            _numbers = PopulateNumbersStringPresentation(maxDifferentWords);

            _fileSize = fileSize;
            _writer = writer;
        }

        public async Task GenerateWordsAsync()
        {
            try
            {
                Log.Debug($"Start magic words sequence. maxDifferentWords is {_maxDifferentWords}");

                BigInteger effectiveSize = 0;
                var randWord = new Random();
                var wordsNumberUpperBound = _maxDifferentWords - 1;

                var batchSize = 1000;
                var currentBatchSize = 0;
                var batch = new List<string>(batchSize);

                var newlinebytecount = Encoding.Unicode.GetByteCount(Environment.NewLine);

                while (effectiveSize < _fileSize)
                {
                    var nextWordIndex = randWord.Next(0, wordsNumberUpperBound);

                    var (nextWord, size) = _words[nextWordIndex];

                    var nextNumber = randWord.Next(0, wordsNumberUpperBound);
                    var (numberRepresentation, numberRepresentationSize) = _numbers[nextNumber];

                    if (currentBatchSize == batchSize)
                    {
                        await _writer.WriteAsync(batch).ConfigureAwait(false);
                        batch = new List<string>(batchSize);
                        currentBatchSize = 0;
                    }
                    else
                    {
                        batch.Add(numberRepresentation + nextWord);
                        currentBatchSize += 1;
                    }

                    effectiveSize = effectiveSize + size + numberRepresentationSize + newlinebytecount;
                }


                await _writer.WriteAsync(batch).ConfigureAwait(false);

                _writer.TryComplete();

                Log.Debug($"End magic words sequence. maxDifferentWords is {_maxDifferentWords}");
            }
            catch (Exception e)
            {
                Log.Error(e, "Unpredictable error has occurred");
            }
        }

        private Dictionary<int, (string, int)> PopulateWordsDictionary(int maxDifferentWords)
        {
            Log.Debug($"Start Words generation. maxDifferentWords is {maxDifferentWords}");

            var words = new Dictionary<int, (string, int)>(maxDifferentWords);

            // Make an array of the letters we will use.
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            // Make a random number generator.
            var rand = new Random();
            var countOfLettersInWords = new Random();

            var lettersUpperBound = letters.Length - 1;

            for (var i = 0; i < maxDifferentWords; i++)
            {
                // Make a word.
                var word = "";
                for (var j = 1; j <= countOfLettersInWords.Next(1, 20); j++)
                {
                    // Pick a random number between 0 and 25
                    // to select a letter from the letters array.
                    var letterNum = rand.Next(0, lettersUpperBound);

                    // Append the letter.
                    word += letters[letterNum];
                }

                words.Add(i, (word, Encoding.Unicode.GetByteCount(word)));
            }

            Log.Debug($"End Words generation. maxDifferentWords is {maxDifferentWords}");

            return words;
        }

        private Dictionary<int, (string, int)> PopulateNumbersStringPresentation(int maxDifferentWords)
        {
            Log.Debug($"Start Numbers' string presentation generation. maxDifferentWords is {maxDifferentWords}");

            var numbers = new Dictionary<int, (string, int)>(maxDifferentWords);

            for (var i = 0; i < maxDifferentWords; i++)
            {
                var stringRepresentation = i + ". ";
                numbers.Add(i, (stringRepresentation, Encoding.Unicode.GetByteCount(stringRepresentation)));
            }

            Log.Debug($"End Numbers' string presentation generation. maxDifferentWords is {maxDifferentWords}");

            return numbers;
        }
    }
}