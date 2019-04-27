using System;
using System.IO;
using System.Linq;
using FixDates.Helpers;

namespace FixDates.Actions
{
    public class DetectFilesWithIncorrectTimeStamps
    {
        private readonly TimeStampParser _parser = new TimeStampParser();
        private int _fileWithIncorrectTimeCount = 0;

        public DetectFilesWithIncorrectTimeStamps() {

        }

        public static void Process(string sourceKey)
        {
            var sourcePath = Environment.GetEnvironmentVariable(sourceKey);
            if (String.IsNullOrWhiteSpace(sourcePath))
            {
                Console.WriteLine($"Environment variable {sourceKey} is not set");
                return;
            }
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"Directory {sourcePath} does not exist");
                return;
            }

            var walker = new FlatPhotoDirListWalker(sourcePath);
            walker.UnrecognizedFormat += 
                file => throw new InvalidOperationException($"Unrecognized {file}");
			var fileWithIncorrectLocationCount = 0;
            walker.NonMatchingDateStamp += (file, dirDate) => {
                var dirName = Path.GetFileName(Path.GetDirectoryName(file));
				Console.WriteLine($"{file} has wrong location {dirName}");
				fileWithIncorrectLocationCount++;
			};
            walker.Process();
			Console.WriteLine($"{fileWithIncorrectLocationCount} files have incorrect location");
		}
    }
}
