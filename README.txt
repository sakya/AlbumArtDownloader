AlbumArtDownloader v.1.1.0.0
Copyright © 2013 Paolo Iommarini <sakya_tg@yahoo.it>

Usage: AlbumArtDownloader [OPTIONS]
Tags audio files with the album art and (optionally) saves a file in the folder
with the album art.
By default AlbumArtDownloader writes tag only in files missing the album art,
use the -o option to overwrite the existing album art.
AlbumArtDownloader can save a file with the album art in the folder if all
the files in it has the same artist and album, use the -f option to set
the file name to use.

Options:
  -h, --help            show help and exit
  -l                    show license
  -p PATH, --path PATH  set the path to scan for audio files
  -f NAME, --file NAME  set the name for the album art file
                        Default: empty (don't save file)
  -r                    scan directory recursively
  -o                    overwrite current album art in audio files