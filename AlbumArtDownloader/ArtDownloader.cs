using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WpfMpdClient;
using System.Net;
using System.Drawing;

namespace AlbumArtDownloader
{
  public class ArtDownloaderOptions
  {
    public ArtDownloaderOptions()
    {
      Recursive = false;
      Overwrite = false;
      FileName = string.Empty;
    }

    public string Path
    {
      get;
      set;
    }

    public bool Recursive
    {
      get;
      set;
    }

    public bool Overwrite
    {
      get;
      set;
    }

    public string FileName
    {
      get;
      set;
    }
  }

  public class ArtDownloader
  {
    ArtDownloaderOptions m_Options = null;
    int m_TaggedFiles = 0;
    Dictionary<string, string> m_ImagesCache = new Dictionary<string, string>();
    static string m_TempPath = System.IO.Path.GetTempPath();

    public ArtDownloader(ArtDownloaderOptions opt)
    {
      m_Options = opt;
    }

    public void Run()
    {
      ProcessFolder(m_Options.Path);
      Console.WriteLine(string.Empty);
      Console.WriteLine(string.Format("Total files tagged: {0}", m_TaggedFiles));
      foreach (string value in m_ImagesCache.Values) {
        try {
          File.Delete(value);
        }
        catch (Exception) {
        }
      }
    }

    private void ProcessFolder(string path)
    {
      Console.WriteLine(string.Format("Folder: {0}", path));

      string[] files;
      try {
        files = Directory.GetFiles(path);
      }catch (Exception ex) {
        Console.WriteLine("Error accessing folder");
        Console.WriteLine(ex.Message);
        return;
      }

      List<TagLib.File> tagFiles = new List<TagLib.File>();
      foreach (string file in files) {
        try {
          TagLib.File f = TagLib.File.Create(file);
          if (f.Properties.MediaTypes == TagLib.MediaTypes.Audio)
            tagFiles.Add(f);
        }
        catch (Exception) {
        }
      }

      if (tagFiles.Count > 0) {
        string fArtist = null;
        string fAlbum = null;
        bool unique = true;

        foreach (TagLib.File f in tagFiles) {
          string artist = !string.IsNullOrEmpty(f.Tag.FirstAlbumArtist) ? f.Tag.FirstAlbumArtist : f.Tag.FirstPerformer;
          string album = f.Tag.Album;

          if (fArtist == null && fAlbum == null) {
            fArtist = artist;
            fAlbum = album;
          } else {
            if (fArtist != artist || fAlbum != album) {
              unique = false;
            }
          }

          if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(album)) {
            if (f.Tag.Pictures.Length == 0 || m_Options.Overwrite) {
              artist = artist.Trim();
              album = album.Trim();
              Console.WriteLine(string.Format("  File  : {0}", f.Name));
              Console.WriteLine(string.Format("  Artist: {0}", artist));
              Console.WriteLine(string.Format("  Album : {0}", album));
              Console.Write("  Getting album art...");

              string url = LastfmScrobbler.GetAlbumArt(artist, album, Scrobbler.ImageSize.mega);
              if (!string.IsNullOrEmpty(url)) {
                Console.WriteLine("found");
                string imagePath = string.Empty;
                if (!m_ImagesCache.TryGetValue(url, out imagePath)) {
                  Guid imageId = System.Guid.NewGuid();
                  imagePath = string.Format("{0}\\{1}", m_TempPath, imageId.ToString() + ".jpg");
                  if (SaveFile(url, imagePath))
                    m_ImagesCache[url] = imagePath;
                  else
                    imagePath = string.Empty;
                }

                if (!string.IsNullOrEmpty(imagePath)) {
                  TagLib.Picture[] pictures = new TagLib.Picture[1];
                  TagLib.Picture picture = new TagLib.Picture(imagePath);
                  pictures[0] = picture;
                  f.Tag.Pictures = pictures;
                  Console.WriteLine("  Tagging file...");
                  try {
                    f.Save();
                    m_TaggedFiles++;
                  }
                  catch (Exception ex) {
                    Console.WriteLine("   Error tagging file");
                    Console.WriteLine(ex.Message);
                  }
                }
              } else {
                Console.WriteLine("not found");
              }
            }
          }
        }

        if (unique && !string.IsNullOrEmpty(m_Options.FileName)) {
          string filename = string.Format("{0}\\{1}", path, m_Options.FileName);
          if (m_Options.Overwrite || !File.Exists(filename)){
            Console.WriteLine("  Saving folder file...");
            string url = LastfmScrobbler.GetAlbumArt(fArtist, fAlbum, Scrobbler.ImageSize.mega);
            if (!string.IsNullOrEmpty(url)) {
              string imagePath = string.Empty;
              if (!m_ImagesCache.TryGetValue(url, out imagePath)) {
                Guid imageId = System.Guid.NewGuid();
                imagePath = string.Format("{0}\\{1}", m_TempPath, imageId.ToString() + ".jpg");
                if (SaveFile(url, imagePath))
                  m_ImagesCache[url] = imagePath;
                else
                  imagePath = string.Empty;
              }

              if (!string.IsNullOrEmpty(imagePath)) {
                try {
                  File.Copy(imagePath, filename);
                }catch (Exception ex) {
                  Console.WriteLine("   Error copying file");
                  Console.WriteLine(ex.Message);
                }
              }
            }
          }
        }
      }

      if (m_Options.Recursive) {
        string[] dirs = Directory.GetDirectories(path);
        foreach (string dir in dirs) {
          ProcessFolder(dir);
        }
      }
    }

    private bool SaveFile(string url, string path)
    {
      Console.Write("  Saving album art...");
      using (WebClient client = new WebClient()) {
        try {
          using (Stream data = client.OpenRead(url)) {
            byte[] readBuffer = new byte[4096];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = data.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0) {
              totalBytesRead += bytesRead;

              if (totalBytesRead == readBuffer.Length) {
                int nextByte = data.ReadByte();
                if (nextByte != -1) {
                  byte[] temp = new byte[readBuffer.Length * 2];
                  Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                  Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                  readBuffer = temp;
                  totalBytesRead++;
                }
              }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead) {
              buffer = new byte[totalBytesRead];
              Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }

            using (MemoryStream byteStream = new MemoryStream(buffer)) {
              Bitmap image = new Bitmap(byteStream);
              image.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            }            
          }
        }
        catch (Exception) {
          Console.WriteLine("failed");
          return false;
        }
      }

      Console.WriteLine("done");
      return true;
    }
  }
}
