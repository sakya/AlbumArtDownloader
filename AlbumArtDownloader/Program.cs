using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace AlbumArtDownloader
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length == 0 || args.Contains("--help") || args.Contains("--h")){
        PrintHelp();
        return;
      }

      if (args.Contains("-l")){
        PrintLicense();
        return;
      }

      bool result;
      ArtDownloaderOptions opt = ParseArguments(args, out result);
      if (result) {
        ArtDownloader downloader = new ArtDownloader(opt);
        downloader.Run();
      }
    }

    static void PrintInfo()
    {
      Console.WriteLine(string.Format("AlbumArtDownloader v.{0}", Assembly.GetExecutingAssembly().GetName().Version));
      Console.WriteLine("Copyright © 2013 Paolo Iommarini <sakya_tg@yahoo.it>");
      Console.WriteLine(string.Empty);
    }

    static void PrintLicense()
    {
      PrintInfo();

      Console.WriteLine("This program is free software: you can redistribute it and/or modify");
      Console.WriteLine("it under the terms of the GNU General Public License as published by");
      Console.WriteLine("the Free Software Foundation, either version 3 of the License, or");
      Console.WriteLine("(at your option) any later version.");
      Console.WriteLine(string.Empty);
      Console.WriteLine("This program is distributed in the hope that it will be useful,");
      Console.WriteLine("but WITHOUT ANY WARRANTY; without even the implied warranty of");
      Console.WriteLine("MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the");
      Console.WriteLine("GNU General Public License for more details.");

      Console.WriteLine("You should have received a copy of the GNU General Public License");
      Console.WriteLine("along with this program.  If not, see <http://www.gnu.org/licenses/>.");
    }

    static void PrintHelp()
    {
      PrintInfo();

      Console.WriteLine("Usage: AlbumArtDownloader [OPTIONS]");
      Console.WriteLine("Tags audio files with the album art and (optionally) saves a file in the folder");
      Console.WriteLine("with the album art.");
      Console.WriteLine("By default AlbumArtDownloader writes tag only in files missing the album art,");
      Console.WriteLine("use the -o option to overwrite the existing album art.");
      Console.WriteLine("AlbumArtDownloader can save a file with the album art in the folder if all");
      Console.WriteLine("the files in it has the same artist and album, use the -f option to set");
      Console.WriteLine("the file name to use.");

      Console.WriteLine(string.Empty);

      Console.WriteLine("Options:");
      Console.WriteLine("  -h, --help            show help and exit");
      Console.WriteLine("  -l                    show license");
      Console.WriteLine("  -p PATH, --path PATH  set the path to scan for audio files");
      Console.WriteLine("  -f NAME, --file NAME  set the name for the album art file");
      Console.WriteLine("                        Default: empty (don't save file)");
      Console.WriteLine("  -r                    scan directory recursively");
      Console.WriteLine("  -o                    overwrite current album art in audio files");
    }

    static ArtDownloaderOptions ParseArguments(string[] args, out bool result)
    {
      result = true;
      ArtDownloaderOptions res = new ArtDownloaderOptions();

      int count = args.Length;
      for (int index = 0; index < count; index++){
        string s = args[index];
        if (s == "--path" || s == "-p") {
          if (index < count) {
            res.Path = args[++index];
          } else {
            Console.WriteLine(string.Format("Missing value for {0}", s));
            result = false;
          }
        }else if (s == "--file" || s == "-f") {
          if (index < count) {
            res.FileName = args[++index];
          } else {
            Console.WriteLine(string.Format("Missing value for {0}", s));
            result = false;
          }
        }else if (s == "-r") {
          res.Recursive = true;
        } else if (s == "-o") {
          res.Overwrite = true;
        }
      }

      if (string.IsNullOrEmpty(res.Path)) {
        Console.WriteLine("Missing argument -p,--path");
        result = false;
      }

      return res;
    }
  }
}
