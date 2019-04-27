using System;
using System.IO;
using System.Linq;

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

            var instance = new DetectFilesWithIncorrectTimeStamps();
            instance.ProcessInternal(sourcePath);

        }
        private void ProcessInternal(string sourcePath)  {
            var dirs = Directory.GetDirectories(sourcePath, "????-??-??");
            foreach (var dir in dirs) {
                ProcessDir(dir);
            }
            Console.WriteLine($"{_fileWithIncorrectTimeCount} files have incorrect timestamps");
        }

        private void ProcessDir(string dir)
        {
            // Only check big ones (movies) at the moment
            var files = Directory.EnumerateFiles(dir, "*.*")
                .Where(s => {
                    var name = s.Trim().ToLowerInvariant();
                    return name.EndsWith(".mp4") 
                        || name.EndsWith(".mov")
                        || name.EndsWith(".3gp");
                })
                .ToList();

            if (files.Any() || files.Count() > 0) {

            }

            foreach(var file in files) {
                var fileName = Path.GetFileName(file);
                if (!_parser.RecognizeStampFormat(fileName, out DateTime fileStamp))
                {
                    throw new InvalidOperationException($"Unrecognized {fileName}");
                }
                var fileTime = File.GetLastWriteTime(file);
                if (fileTime != fileStamp)
                {
                    Console.WriteLine($"{file} has wrong stamp {fileTime}");
                    _fileWithIncorrectTimeCount++;
                }
            }
        }
    }
}
