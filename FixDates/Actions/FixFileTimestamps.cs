using System;
using System.Collections.Generic;
using System.IO;
using FixDates.Helpers;

namespace FixDates.Actions
{
    public class FixFileTimestamps
    {
         public static void Perform(string sourceKey)
        {
            Console.WriteLine("Making file timestamps correspond to stamps in their names");
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

			var stamps = new List<Tuple<string, DateTime>>();
			var parser = new TimeStampParser();
            parser.UnrecognizedStamp += file => 
                throw new InvalidOperationException($"Unrecognized name: {file}");
            parser.RecognizedStamp += (filePath, fileStamp) =>  {
                var fileName = Path.GetFileName(filePath);
				Console.WriteLine("{0} - {1}", fileName, TimeStampParser.FileStampFromDate(fileStamp));
				stamps.Add(Tuple.Create(filePath, fileStamp));
			};
            parser.IgnoredName += file => Console.WriteLine($"Ignoring {file}");

            var files = Directory.GetFiles(sourcePath);
            foreach (var filePath in files)
            {
				parser.RecognizeStampFormat(filePath);
            }

            Console.WriteLine("All stamps are recognized");

            var correctedFiles = 0;
            foreach (var fileNameStamp in stamps)
            {
                var fileTime = File.GetLastWriteTime(fileNameStamp.Item1);
                if (fileTime != fileNameStamp.Item2)
                {
                    Console.WriteLine($"{fileNameStamp.Item1} has wrong stamp {fileTime}");
                    File.SetLastWriteTime(fileNameStamp.Item1, fileNameStamp.Item2);
                    correctedFiles++;
                }
            }
            Console.WriteLine($"{correctedFiles}/{files.Length} had wrong timestamp and were corrected");
        }
   }
}
