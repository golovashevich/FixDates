﻿using System;
using System.Collections.Generic;
using System.IO;
using FixDates.Actions;

namespace FixDates
{
    class Program
    {
        public const string PHOTO_SOURCE = "PHOTO_SOURCE_LOCATION";
        public const string PHOTO_FOLDERS = "PHOTO_FOLDERS";
        public const string QUARANTINE_LOCATION = "QUARANTINE_LOCATION";
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var arg = args[0].ToLowerInvariant();
                if (arg == "-f" || arg == "--f" || arg == "/f")
                { 
                    FixFileTimestamps.Perform(PHOTO_SOURCE);
                }
                else if (arg == "-d" || arg == "--d" || arg == "/d") { 
                    DetectFilesWithIncorrectTimeStamps.Process(PHOTO_FOLDERS);
                } else if (arg == "-dm" || arg == "--dm" || arg == "/dm") {
					DetectAndMoveFilesWithIncorrectTimeStamps.Process(PHOTO_FOLDERS, QUARANTINE_LOCATION);
                } else {
                    Console.WriteLine($"Unrecognized command line option {args[0]}");
                }
            } else { // Default action
                FixFileTimestamps.Perform(PHOTO_SOURCE);
            }
        }
    }
}
