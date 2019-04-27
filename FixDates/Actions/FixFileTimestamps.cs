using System;
using System.Collections.Generic;
using System.IO;

namespace FixDates.Actions
{
    public class FixFileTimestamps
    {
         public static void Perform(string sourceKey)
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

            var parser = new TimeStampParser();
            var files = Directory.GetFiles(sourcePath);

            var stamps = new List<Tuple<string, DateTime>>();
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                if (!parser.RecognizeStampFormat(fileName, out DateTime fileStamp))
                {
                    throw new InvalidOperationException($"Unrecognized {fileName}");
                }
                Console.WriteLine("{0} - {1}", fileName, TimeStampParser.FileStampFromDate(fileStamp));
                stamps.Add(Tuple.Create(filePath, fileStamp));
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