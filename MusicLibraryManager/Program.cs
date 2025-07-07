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
            var files = Directory.GetFiles(library, "*.mp3", new EnumerationOptions() { RecurseSubdirectories = true });
            foreach (var file in files)
            {
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
            genreOption
        };
        rootCommand.Subcommands.Add(listTracksCommand);
        listTracksCommand.SetAction(parseResult =>
        {
            Console.WriteLine("Tracks");
            var album = parseResult.GetValue<string>("--album");
            var genre = parseResult.GetValue<string>("--genre");
            var tracks = String.IsNullOrEmpty(album) ? dataService.GetTracks() : dataService.GetTracksByAlbum(album);
            foreach (var t in tracks)
            {
                if (!String.IsNullOrEmpty(genre))
                {
                    if (!String.Equals(t.Genre, genre, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                }
                Console.WriteLine($"    {t.TrackNumber} • {t.Title} • {t.Album} • {t.Artist}");
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
}


