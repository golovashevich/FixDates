using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FixDates
{
    class Program
    {
        public const string PHOTO_SOURCE = "PHOTO_SOURCE_LOCATION";
        static void Main(string[] args)
        {
            var sourcePath = Environment.GetEnvironmentVariable(PHOTO_SOURCE);
            if (String.IsNullOrWhiteSpace(sourcePath)) {
                Console.WriteLine($"Environment variable {PHOTO_SOURCE} is not set");
                return;
            }
            if (!Directory.Exists(sourcePath)) {
                Console.WriteLine($"Directory {sourcePath} does not exist");
                return;
            }
            var recognizedCount = 0;
            var files = Directory.GetFiles(sourcePath);
            foreach (var filePath in files) {
                if (ProcessFile(filePath)) {
                    recognizedCount++;
                }
            }

            Console.WriteLine($"Sequences recognized: {recognizedCount} ({files.Length})");
            Console.WriteLine("Press Enter to exit...");
        }

        const string TIMESTAMP_MASK = 
            @"(?<y>[1-2]\d{3})-?(?<month>[0-1]\d)-?(?<d>[0-3]\d)[_|-](?<h>[0-2]\d)-?(?<m>[0-5]\d)-?(?<s>[0-5]\d)";

        const string GUID_MASK = 
            @"[0-9a-fA-F]{{8}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{12}}";

        private static string[] MASKS = {
            "VID_{0}.mp4", 
            "IMG_{0}.jpg",
            @"IMG_\d\d\d\d_{0}\d{{3}}.MOV",
            "PANO_{0}.jpg",
            @"PH{0}-\d{{3}}.JPG",
            "VID_{0}.3gp", 
            "IMG_{0}_HDR.jpg",
            @"IMG_{0}_\d.jpg",
            @"IMG_{0}_\d{{3}}.jpg",
            @"IMG_{0}_BURST\d.jpg",
            @"\d{{10}}_{0}\d{{3}}.mp4",
            String.Format(@"{0}_{{0}}\d{{{{3}}}}.mp4", GUID_MASK),
            @"Screenshot_{0}-\d{{3}}_.*.png",
        };

        public static bool RecognizeFormat(string fileName, out DateTime fileDate) {
            // TODO: Extract this into separate method
            var sequences = new List<Tuple<string, Regex>>();
            foreach(var mask in MASKS) {
                var tempResult = String.Format(mask, TIMESTAMP_MASK);
                sequences.Add(Tuple.Create(mask, new Regex(String.Format(mask, TIMESTAMP_MASK))));
            }

            fileDate = DateTime.MinValue;
            foreach (var sequence in sequences) {
                var match = sequence.Item2.Match(fileName);
                if (match.Success)
                {
                    fileDate = ComposeDate(match);
                    // CheckDateAgainstName(fileName, sequence.Item1, fileTime);
                    return true;
                }
            }
            return false;
        }

        private static DateTime ComposeDate(Match match)
        {
            var year = Convert.ToInt32(match.Groups["y"].Value);
            var month = Convert.ToInt32(match.Groups["month"].Value);
            var day = Convert.ToInt32(match.Groups["d"].Value);
            var hour = Convert.ToInt32(match.Groups["h"].Value);
            var minute = Convert.ToInt32(match.Groups["m"].Value);
            var second = Convert.ToInt32(match.Groups["s"].Value);

            var fileTime = new DateTime(year, month, day, hour, minute, second);
            return fileTime;
        }

        private static void CheckDateAgainstName(string fileName, string mask, DateTime fileTime)
        {
            var stampFormat = 
                $"{fileTime.Year}{fileTime.Month:d2}{fileTime.Day:d2}_{fileTime.Hour:d2}{fileTime.Minute:d2}{fileTime.Second:d2}";
            var checkName = String.Format(mask, stampFormat);

            if (checkName != fileName)
            {
                throw new InvalidDataException($"Parsed name does not match original name: {fileName} - {checkName}");
            }
        }

        const string STAMP_FORMAT = "{0}{1:d2}{2:d2}_{3:d2}{4:d2}{5:d2}"; 
        
        private static string FileStampFromDate(DateTime fileTime) {
            return String.Format(STAMP_FORMAT, fileTime.Year, fileTime.Month, fileTime.Day, 
                fileTime.Hour, fileTime.Minute, fileTime.Second);
        }

        public static bool ProcessFile(string filePath) {
            var fileName = Path.GetFileName(filePath);
            if (!RecognizeFormat(fileName, out DateTime fileStamp)) {
                    throw new InvalidOperationException($"Unrecognized {fileName}");
                // return false;
            } else {
                Console.WriteLine("{0} - {1}", fileName, FileStampFromDate(fileStamp));
            }
            return true;
        }
    }
}
