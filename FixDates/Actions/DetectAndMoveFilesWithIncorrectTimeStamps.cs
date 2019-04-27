using System;
using System.IO;
using System.Linq;
using FixDates.Helpers;

namespace FixDates.Actions
{
    public class DetectAndMoveFilesWithIncorrectTimeStamps {

        public static void Process(string sourceKey, string quarantineKey)
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

			var quarantinePath = Environment.GetEnvironmentVariable(quarantineKey);
			if (String.IsNullOrWhiteSpace(quarantinePath)) {
                Console.WriteLine($"Environment variable {quarantineKey} is not set");
                return;
			}

			if (!Directory.Exists(quarantinePath)) {
                Console.WriteLine($"Directory {quarantinePath} does not exist; creating");
				Directory.CreateDirectory(quarantinePath);
			}

			var walker = new FlatPhotoDirListWalker(sourcePath);
			walker.UnrecognizedFormat +=
				file => throw new InvalidOperationException($"Unrecognized {file}");
			var fileWithIncorrectLocationCount = 0;
            var alreadyExistingInQuarantine = 0;

			walker.NonMatchingDateStamp += (file, dirDate) =>
			{
				var dirName = Path.GetFileName(Path.GetDirectoryName(file));
                var fileName = Path.GetFileName(file);
                var destFileName = Path.Combine(quarantinePath, fileName);
                if (File.Exists(destFileName)) {
                    Console.WriteLine($"{destFileName} already exists in quarantine; skipping");
                    alreadyExistingInQuarantine++;
                    return;    
                }
                Console.Write($"Moving {file} to quarantine...");
                File.Move(file, destFileName);
                Console.WriteLine("OK");
				fileWithIncorrectLocationCount++;
			};
			walker.Process();
			Console.WriteLine($"{alreadyExistingInQuarantine} files already exist in quarantine; {fileWithIncorrectLocationCount} have been moved to quarantine");
		}
	}
}
