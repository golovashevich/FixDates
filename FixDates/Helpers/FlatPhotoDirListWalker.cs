using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FixDates.Helpers
{
	public class FlatPhotoDirListWalker {
		private const string DIR_WITH_PHOTO_PATTERN = "????-??-??";

		public event Action<string> UnrecognizedFormat; 
		public event Action<string, DateTime> NonMatchingDateStamp;

		private string _sourcePath;
        private readonly TimeStampParser _parser = new TimeStampParser();

		public FlatPhotoDirListWalker(string sourcePath) {
			_sourcePath = sourcePath;
		}

		public const string PHOTO_DIR_MASK =
        	@"^(?<y>[1-2]\d{3})-?(?<month>[0-1]\d)-?(?<d>[0-3]\d)$";

		private static Regex reDirName = new Regex(PHOTO_DIR_MASK);
		public void Process() {
            _parser.UnrecognizedStamp += file => UnrecognizedFormat?.Invoke(file);
            _parser.RecognizedStamp += (filePath, fileStamp) => {
                var dirName = Path.GetFileName(Path.GetDirectoryName(filePath));
                var match = reDirName.Match(dirName);
                if (!match.Success) {
                    throw new InvalidOperationException($"Unrecognized photo dir name format {dirName}");
                }
                var dirDate = ComposeDate(match);

				// We compare DATE parts only 
				if (dirDate.Date != fileStamp.Date)
				{
					NonMatchingDateStamp?.Invoke(filePath, dirDate);
				}
			};

            var dirs = Directory.GetDirectories(_sourcePath, DIR_WITH_PHOTO_PATTERN);
            foreach (var dir in dirs) {
                ProcessDir(dir);
            }
		}

		private static DateTime ComposeDate(Match match)
		{
			var year = Convert.ToInt32(match.Groups["y"].Value);
			var month = Convert.ToInt32(match.Groups["month"].Value);
			var day = Convert.ToInt32(match.Groups["d"].Value);

			var date = new DateTime(year, month, day);
			return date;
		}

		private void ProcessDir(string dir)
        {
            var files = Directory.GetFiles(dir);

            foreach(var file in files) {
				_parser.RecognizeStampFormat(file);
            }
        }
	}
}
