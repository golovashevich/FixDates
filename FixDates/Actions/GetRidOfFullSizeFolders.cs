using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FixDates.Helpers;

namespace FixDates.Actions {
    public class GetRidOfFullSizeFolders {

        // TODO: Get rid of duplication
        private const string DIR_WITH_PHOTO_PATTERN = "????-??-??*";
        public const string PHOTO_DIR_MASK =
            //@"^(?<y>[1-2]\d{3})-?(?<month>[0-1]\d)-?(?<d>[0-3]\d)$";
            @"^(?<y>[1-2]\d{3})-?(?<month>[0-1]\d)-?(?<d>[0-3]\d)(-[1-9])?(( -.*)|$)";

        private static Regex reDirName = new Regex(PHOTO_DIR_MASK);

        private const string FULL_SIZE = "FullSize";


        /// <summary>
        /// In date folders determines if there is a FullSize folder. 
        /// If so, moves its content to the root folder (date folder), replacing its contents.
        /// </summary>
        /// <param name="sourceKey">A folder that contains date folders that we need to process (normalize)</param>
        /// <param name="fullSizeInternalFoldersKey"></param>
        /// <exception cref="InvalidOperationException">Comma-separated list of FullSize contents (it's limited to these folders)</exception>
        public static void Process(string sourceKey, string fullSizeInternalFoldersKey) {
			Console.WriteLine("In a date folder, determines the FullSize and moves its contents up one level");

			var sourcePath = Environment.GetEnvironmentVariable(sourceKey);
            if (String.IsNullOrWhiteSpace(sourcePath)) {
                Console.WriteLine($"Environment variable {sourceKey} is not set");
                return;
            }
            if (!Directory.Exists(sourcePath)) {
                Console.WriteLine($"Directory {sourcePath} does not exist");
                return;
            }

            string fullSizeContentsStr = Environment.GetEnvironmentVariable(fullSizeInternalFoldersKey);
            var fullSizeContents = fullSizeContentsStr?.Split(";");
            if (fullSizeContents == null || fullSizeContents.Length == 0) {
                throw new InvalidOperationException("The [only] dir(s) of FullSize folders should be explicitly specified");
            }
            var fullSizeOnlyDirs = new HashSet<string>();
            foreach (var dir in fullSizeContents) {
                fullSizeOnlyDirs.Add(dir.Trim().ToLowerInvariant());
            }

            var dirs = Directory.GetDirectories(sourcePath, DIR_WITH_PHOTO_PATTERN);
            foreach (var dir in dirs) {
                var dirName = Path.GetFileName(dir);
                // Check if dir is valid Photo Dir
                var match = reDirName.Match(dirName);
                if (!match.Success) {
                    throw new InvalidOperationException($"Unrecognized photo dir name format {dir}");
                }

                // Check if it contains the FullSize folder
                var fullSize = Path.Combine(dir, FULL_SIZE);
                if (!Directory.Exists(fullSize)) {
                    continue;
                }

                // Enumerate folders in the FullSize folder
                // Move them up one level
                var subdirs = Directory.GetDirectories(fullSize);
                foreach (var subdir in subdirs) {
                    var subdirName = Path.GetFileName(subdir).ToLowerInvariant();
                    // Limit FullSize children to be only of those that are specified; report folders that don't match this
                    if (!fullSizeOnlyDirs.Contains(subdirName)) {
                        throw new InvalidOperationException(
                            $"The only dirs the FullSize can contain are: {fullSizeContentsStr} ({subdir})");
                    }
                }
                // Process files in the FullSize dir
                var subdirsIncludingFullSize = new List<string>(subdirs);
                subdirsIncludingFullSize.Add(fullSize);
                foreach (var subdir in subdirsIncludingFullSize) {
                    // Create the dir with this name in the photo dir
                    // Enumerate files in this dir
                    // Move files in this dir to the photo dir's instance
                    Directory.Move(subdir, dir);
                }

                // Enumerate folders in the FullSize folder
                // Move them up one level

                var files = Directory.GetFiles(fullSize);
                foreach(var file in files) {
                    var fileName = Path.GetFileName(file);
                    var photoDirFileName = Path.Combine(dir, fileName);
                    File.Move(file, photoDirFileName, true);
                }

            }

      //      var walker = new FlatPhotoDirListWalker(sourcePath);
		    //walker.UnrecognizedFormat +=
			   // file => throw new InvalidOperationException($"Unrecognized {file}");
		    //var fileWithIncorrectLocationCount = 0;
      //      var alreadyExistingInQuarantine = 0;

		    //walker.NonMatchingDateStamp += (file, dirDate) =>
		    //{
			   // var dirName = Path.GetFileName(Path.GetDirectoryName(file));
      //          var fileName = Path.GetFileName(file);
      //          // // var destFileName = Path.Combine(quarantinePath, fileName);
      //          // if (File.Exists(destFileName)) {
      //          //     Console.WriteLine($"{destFileName} already exists in quarantine; skipping");
      //          //     alreadyExistingInQuarantine++;
      //          //     return;    
      //          // }
      //          // Console.Write($"Moving {file} to quarantine...");
      //          // File.Move(file, destFileName);
      //          Console.WriteLine("OK");
			   // fileWithIncorrectLocationCount++;
		    //};
		    //walker.Process();
		    //Console.WriteLine($"{alreadyExistingInQuarantine} files already exist in quarantine; {fileWithIncorrectLocationCount} have been moved to quarantine");
		}
    }
}
