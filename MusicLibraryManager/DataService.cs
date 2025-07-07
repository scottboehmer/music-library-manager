class DataService : IDisposable
{
    private Microsoft.Data.Sqlite.SqliteConnection _connection;

    public DataService(IConfigurationService configService)
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection(configService.GetDatabaseConnectionString());
        _connection.Open();

        EnsureTrackTableExists();
    }

    private void EnsureTrackTableExists()
    {
        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS tracks(
                id INTEGER PRIMARY KEY ASC, 
                title TEXT,
                artist TEXT,
                album TEXT,
                track INTEGER,
                genre TEXT,
                year INTEGER,
                duration INTEGER,
                path TEXT
            );
        ";

        command.ExecuteNonQuery();
    }

    private void EnsureArtistTableExists()
    {
        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS artists(
                id INTEGER PRIMARY KEY ASC, 
                name TEXT
            );
        ";

        command.ExecuteNonQuery();
    }

    public void ClearAllData()
    {
        EnsureTrackTableExists();
        EnsureArtistTableExists();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            DELETE FROM artists;
            DELETE FROM tracks;
        ";

        command.ExecuteNonQuery();
    }

    public void AddTrack(Track track)
    {
        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            INSERT INTO tracks (title, artist, album, track, genre, year, duration, path) VALUES ($title, $artist, $album, $track, $genre, $year, $duration, $path);
        ";
        command.Parameters.AddWithValue("$title", track.Title);
        command.Parameters.AddWithValue("$artist", track.Artist);
        command.Parameters.AddWithValue("$album", track.Album);
        command.Parameters.AddWithValue("$track", track.TrackNumber);
        command.Parameters.AddWithValue("$genre", track.Genre);
        command.Parameters.AddWithValue("$year", track.Year);
        command.Parameters.AddWithValue("$duration", track.Duration);
        command.Parameters.AddWithValue("$path", track.Path);

        command.ExecuteNonQuery();
    }

    public Track[] GetTracks()
    {
        var tracks = new List<Track>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT title, artist, album, track, genre, year, duration, path FROM tracks;
        ";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tracks.Add(new Track(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.GetString(7)));
            }
        }

        return tracks.ToArray();
    }

    public Track[] GetTracks(string title, string artist, string album)
    {
        var tracks = new List<Track>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT title, artist, album, track, genre, year, duration, path FROM tracks WHERE title = $title AND artist = $artist AND album = $album;
        ";
        command.Parameters.AddWithValue("$title", title);
        command.Parameters.AddWithValue("$artist", artist);
        command.Parameters.AddWithValue("$album", album);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tracks.Add(new Track(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.GetString(7)));
            }
        }

        return tracks.ToArray();
    }

    public Track[] GetTracksByPath(string path)
    {
        var tracks = new List<Track>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT title, artist, album, track, genre, year, duration, path FROM tracks WHERE path = $path;
        ";
        command.Parameters.AddWithValue("$path", path);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tracks.Add(new Track(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.GetString(7)));
            }
        }

        return tracks.ToArray();
    }

    public Track[] GetTracksByAlbum(string album)
    {
        var tracks = new List<Track>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT title, artist, album, track, genre, year, duration, path FROM tracks WHERE album = $album ORDER BY track;
        ";
        command.Parameters.AddWithValue("$album", album);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tracks.Add(new Track(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.GetString(7)));
            }
        }

        return tracks.ToArray();
    }

    public string[] GetAlbums()
    {
        var albums = new List<string>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT DISTINCT album FROM tracks ORDER BY album;
        ";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                albums.Add(reader.GetString(0));
            }
        }

        return albums.ToArray();
    }

    public string[] GetArtistsFromTracks()
    {
        var artists = new List<string>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT DISTINCT artist FROM tracks ORDER BY artist;
        ";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                artists.Add(reader.GetString(0));
            }
        }

        return artists.ToArray();
    }

    public string[] GetGenresFromTracks()
    {
        var genres = new List<string>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT DISTINCT genre FROM tracks ORDER BY genre;
        ";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                genres.Add(reader.GetString(0));
            }
        }

        return genres.ToArray();
    }

    public Artist[] GetArtists(string name)
    {
        var artists = new List<Artist>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT name FROM artists WHERE name = $name;
        ";
        command.Parameters.AddWithValue("$name", name);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                artists.Add(new Artist(reader.GetString(0)));
            }
        }

        return artists.ToArray();
    }

    public Artist[] GetArtists()
    {
        var artists = new List<Artist>();

        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            SELECT name FROM artists ORDER BY name;
        ";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                artists.Add(new Artist(reader.GetString(0)));
            }
        }

        return artists.ToArray();
    }

    public void AddArtist(string name)
    {
        var command = _connection.CreateCommand();
        command.CommandText =
        @"
            INSERT INTO artists (name) VALUES ($name);
        ";
        command.Parameters.AddWithValue("$name", name);

        command.ExecuteNonQuery();
    }

    public void CreateAndPopulateArtists()
    {
        EnsureArtistTableExists();

        var trackArtists = GetArtistsFromTracks();
        foreach (var entry in trackArtists)
        {
            var components = entry.Split(";", StringSplitOptions.TrimEntries);
            foreach (var artist in components)
            {
                var matches = GetArtists(artist);
                if (matches.Length == 0)
                {
                    AddArtist(artist);
                }
            }
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}