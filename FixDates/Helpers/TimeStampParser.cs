using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FixDates.Helpers
{
    public class TimeStampParser
    {
        public const string TIMESTAMP_MASK = 
            @"(?<y>[1-2]\d{3})-?(?<month>[0-1]\d)-?(?<d>[0-3]\d)[ _-]?(?<h>[0-2]\d)[\.|-]?(?<m>[0-5]\d)[\.|-]?(?<s>[0-5]\d)";

        public const string GUID_MASK = 
            @"[0-9a-fA-F]{{8}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{4}}-[0-9a-fA-F]{{12}}";

        public const string TIMESTAMP_FORMAT = "{0}{1:d2}{2:d2}_{3:d2}{4:d2}{5:d2}"; 

        public event Action<string, DateTime> RecognizedStamp;

        public event Action<string> UnrecognizedStamp;

        public event Action<string> IgnoredName;
		private static string[] MASKS = {
            "VID_{0}.mp4",
			@"VID_{0}\(\d\).mp4",
			@"video\-{0}\.mp4",
			"{0}.mp4", 
            @"{0}(_\d+)?\.jpg$",
            "^IMG_{0}( - Copy)?.jpg$",
			@"^{0}(_1)?\.jpg$",
			@"IMG_\d\d\d\d_{0}\d{{3}}.MOV",
            "PANO_{0}.jpg",
            @"PH{0}-\d{{3}}.JPG",
            "VID_{0}.3gp", 
            "IMG_{0}_HDR.jpg",
            @"IMG_{0}_\d.jpg",
            @"IMG_{0}_\d{{3}}.jpg",
            @"^IMG_{0}_BURST\d\d?\.jpg$",
			@"^IMG_{0}_HHT\.jpg$",
			@"\d{{10}}_{0}\d{{3}}.mp4",
            String.Format(@"{0}_{{0}}\d{{{{3}}}}.mp4", GUID_MASK),
            @"Screenshot_{0}-\d{{3}}_.*.png",
        };
    
        private static string[] IGNORED_MASKS = {
			@"^Video\d{4}\.mp4$",
            @"^Photo\d{4}\.jpg$",
			@"^Skype_Picture\.jpeg",
			@"^Skype_Picture_\d\.jpeg",
            @"^IMG_\d{4}\.[JPG|CR2]",
			@"^Фото\d{4}\.JPG$",
			@"^STA_\d\d\d\d\.JPG",
			@"^MVI_\d\d\d\d\.AVI",
			@"^MVI_\d\d\d\d\.THM",
            @"^Thumbs\.db",
			@"^\.DS_Store",
			@"^\d\.JPG", 
            @"^\d\d\.JPG$",
            @"^M\d{4}\.ctg$",
            @"^[a-zа-я]+\.jpg$",
			@"^[\p{IsCyrillic} a-z ̈]+\.ogg$"
		};

        private readonly List<Tuple<string, Regex>> _sequences;
        private readonly List<Tuple<string, Regex>> _ignoredSequences; 

        public TimeStampParser() {
            _sequences = new List<Tuple<string, Regex>>();
            foreach(var mask in MASKS) {
                var tempResult = String.Format(mask, TIMESTAMP_MASK);
                _sequences.Add(Tuple.Create(
                        mask, 
                        new Regex(String.Format(mask, TIMESTAMP_MASK), 
                            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)));
            }

			_ignoredSequences = new List<Tuple<string, Regex>>();
			foreach (var mask in IGNORED_MASKS)
			{
				_ignoredSequences.Add(Tuple.Create(
						mask,
						new Regex(mask, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)));
			}
		}

        public void RecognizeStampFormat(string filePath) {
			var fileName = Path.GetFileName(filePath);

			var fileDate = DateTime.MinValue;
            foreach (var sequence in _ignoredSequences) {
                var match = sequence.Item2.Match(fileName);
                if (match.Success) {
                    IgnoredName?.Invoke(fileName);
                    return;
                }
            }
            foreach (var sequence in _sequences) {
                var match = sequence.Item2.Match(fileName);
                if (match.Success)
                {
                    fileDate = ComposeDateTime(match);
                    RecognizedStamp?.Invoke(filePath, fileDate);
                    // CheckDateAgainstName(fileName, sequence.Item1, fileTime);
                    return;
                }
            }
            UnrecognizedStamp.Invoke(fileName);
        }

        private static DateTime ComposeDateTime(Match match)
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

        public static string FileStampFromDate(DateTime fileTime) {
            return String.Format(TIMESTAMP_FORMAT, fileTime.Year, fileTime.Month, fileTime.Day, 
                fileTime.Hour, fileTime.Minute, fileTime.Second);
        }
    }
}
