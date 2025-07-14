using System.CommandLine;

class Program
{
    static int Main(string[] args)
    {
        IConfigurationService configService = new TomlConfigurationService("config.toml");
        DataService dataService = new DataService(configService);

        RootCommand rootCommand = new("Music Library Manager");

        Option<string> albumOption = new("--album")
        {
            Description = "The album name.",
            Required = false
        };

        Option<string> artistOption = new("--artist")
        {
            Description = "The artist name.",
            Required = false
        };

        Option<string> titleOption = new("--title")
        {
            Description = "The title.",
            Required = false
        };

        Option<string> genreOption = new("--genre")
        {
            Description = "The genre name.",
            Required = false
        };

        Option<bool> cleanOption = new("--clean")
        {
            Description = "Whether to remove all data before import.",
            Required = false,
            DefaultValueFactory = (argResult) => false
        };

        Option<DirectoryInfo> directoryOption = new("--directory")
        {
            Description = "The directory to use.",
            Required = true
        };

        Command importCommand = new("import", "Import a music library.") { cleanOption };
        rootCommand.Subcommands.Add(importCommand);
        importCommand.SetAction(parseResult =>
        {

            if (parseResult.GetValue<bool>(cleanOption))
            {
                Console.WriteLine("Clearing Database");
                dataService.ClearAllData();
            }

            Console.WriteLine("Importing Library");

            var library = configService.GetMusicDirectory();
            var files = Directory.EnumerateFiles(library, "*", new EnumerationOptions() { RecurseSubdirectories = true });
            foreach (var file in files)
            {
                if (!IsMusicFile(file))
                {
                    continue;
                }
                if (dataService.GetTracksByPath(file).Length == 0)
                {
                    var atlTrack = new ATL.Track(file);
                    dataService.AddTrack(TrackDataConverter.Convert(atlTrack));
                }
            }

            dataService.CreateAndPopulateArtists();
        });

        Command listAlbumsCommand = new("list-albums", "List albums in the collection.") { };
        rootCommand.Subcommands.Add(listAlbumsCommand);
        listAlbumsCommand.SetAction(parseResult =>
        {
            Console.WriteLine("Albums");
            var albums = dataService.GetAlbums();
            foreach (var a in albums)
            {
                Console.WriteLine($"    {a}");
            }
        });

        Command listArtistsCommand = new("list-artists", "List artists in the collection.") { };
        rootCommand.Subcommands.Add(listArtistsCommand);
        listArtistsCommand.SetAction(parseResult =>
        {
            Console.WriteLine("Artists");
            var artists = dataService.GetArtists();
            foreach (var a in artists)
            {
                Console.WriteLine($"    {a.Name}");
            }
        });

        Command listGenresCommand = new("list-genres", "List genres in the collection.") { };
        rootCommand.Subcommands.Add(listGenresCommand);
        listGenresCommand.SetAction(parseResult =>
        {
            Console.WriteLine("Genres");
            var genres = dataService.GetGenresFromTracks();
            foreach (var g in genres)
            {
                Console.WriteLine($"    {g}");
            }
        });

        Command listTracksCommand = new("list-tracks", "List tracks in the collection.")
        {
            albumOption,
            genreOption,
            artistOption,
            titleOption
        };
        rootCommand.Subcommands.Add(listTracksCommand);
        listTracksCommand.SetAction(parseResult =>
        {
            Console.WriteLine("Tracks");
            var album = parseResult.GetValue<string>("--album");
            var genre = parseResult.GetValue<string>("--genre");
            var artist = parseResult.GetValue<string>("--artist");
            var title = parseResult.GetValue<string>("--title");
            var tracks = dataService.QueryTracks(title, artist, album);
            foreach (var t in tracks)
            {
                if (!String.IsNullOrEmpty(genre))
                {
                    if (!String.Equals(t.Genre, genre, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                }
                if (String.IsNullOrEmpty(album))
                {
                    Console.WriteLine($"    {t.Title} • {t.Album} • {t.TrackNumber} • {t.Artist}");
                }
                else
                {
                    Console.WriteLine($"    {t.TrackNumber} • {t.Title} • {t.Album} • {t.Artist}");
                }
                
            }
        });

        Command updateFilesCommand = new("update-files", "Update the metadata on files.")
        {
            directoryOption,
            albumOption,
            artistOption
        };
        rootCommand.Subcommands.Add(updateFilesCommand);
        updateFilesCommand.SetAction(parseResult =>
        {
            Console.WriteLine("Updating Files");

            var dir = parseResult.GetValue<DirectoryInfo>(directoryOption);
            if (dir == null || !dir.Exists)
            {
                Console.WriteLine("Directory does not exist");
                return;
            }

            var newAlbumName = parseResult.GetValue<string>(albumOption);
            var newArtistName = parseResult.GetValue<string>(artistOption);

            foreach (var file in dir.EnumerateFiles("*", new EnumerationOptions() { RecurseSubdirectories = true }))
            {
                if (!IsMusicFile(file.FullName))
                {
                    continue;
                }

                var atl = new ATL.Track(file.FullName);

                if (!String.IsNullOrEmpty(newAlbumName))
                {
                    atl.Album = newAlbumName;
                    dataService.UpdateTrackAlbum(atl.Path, newAlbumName);
                }

                if (!String.IsNullOrEmpty(newArtistName))
                {
                    atl.Artist = newArtistName;
                    dataService.UpdateTrackArtist(atl.Path, newArtistName);
                }

                atl.Save();
            }
        });

        Command reorganizeCommand = new("reorganize", "Copy and organize library.")
        {
            directoryOption
        };
        rootCommand.Subcommands.Add(reorganizeCommand);
        reorganizeCommand.SetAction(parseResult =>
        {
            Console.WriteLine("Reorganizing");

            var output = parseResult.GetValue<DirectoryInfo>(directoryOption);

            if (output == null || !output.Exists)
            {
                Console.WriteLine("Output directory does not exist");
                return;
            }

            var tracks = dataService.GetTracks();
            foreach (var t in tracks)
            {
                var album = t.Album.Trim();
                var track = $"{t.TrackNumber:d2} - {t.Title}";
                var extension = Path.GetExtension(t.Path);
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    album = album.Replace(c, '-');
                    track = track.Replace(c, '-');
                }
                var albumPath = Path.Combine(output.FullName, album);
                if (!Directory.Exists(albumPath))
                {
                    Directory.CreateDirectory(albumPath);
                }

                var newTrackPath = Path.Join(albumPath, $"{track}{extension}");

                if (File.Exists(newTrackPath))
                {
                    Console.WriteLine($"  !DUPLICATE: {newTrackPath}");
                    foreach (var possibleDupe in dataService.GetTracksByAlbum(t.Album))
                    {
                        if (String.Equals(possibleDupe.Title, t.Title))
                        {
                            Console.WriteLine($"    * {possibleDupe.Path}");
                        }
                    }
                }
                else
                {
                    File.Copy(t.Path, newTrackPath);

                    // This approach seems much slower...
                    /*var atlTrack = new ATL.Track(t.Path);
                    TrackDataConverter.Cleanup(atlTrack);
                    atlTrack.SaveTo(newTrackPath);*/
                }
            }
        });

        ParseResult parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach (var parseError in parseResult.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }
            return 1;
        }
        parseResult.Invoke();
        return 0;
    }

    static bool IsMusicFile(string? path)
    {
        if (String.IsNullOrEmpty(path))
        {
            return false;
        }

        var extension = Path.GetExtension(path);

        if (new string[] { ".mp3", ".m4a" }.Contains(extension.ToLowerInvariant()))
        {
            return true;
        }

        return false;
    }
}


