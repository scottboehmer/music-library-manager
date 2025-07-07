using System.Collections;

public static class TrackDataConverter
{
    public static Track Convert(ATL.Track atl)
    {
        string title = atl.Title;
        string artist = atl.Artist;
        string album = atl.Album;
        int trackNumber = atl.TrackNumber.HasValue ? atl.TrackNumber.Value : 0;
        string genre = atl.Genre;
        int year = atl.Year.HasValue ? atl.Year.Value : 0;
        int duration = atl.Duration;
        string path = atl.Path;

        // Fix tag entries with null characters
        foreach (var field in atl.AdditionalFields)
        {
            switch (field.Key)
            {
                case "TAL\0": album = field.Value; break;
                case "TP1\0": artist = field.Value; break;
                case "TRK\0": trackNumber = Int32.TryParse(field.Value, out int t) ? t : trackNumber; break;
                case "TT2\0": title = field.Value; break;
                case "TYE\0": year = Int32.TryParse(field.Value, out int y) ? y : year; break;
                default: break;
            }
        }

        // If no album name, set it to track title
        if (String.IsNullOrEmpty(album))
        {
            album = title;
        }

        return new Track(title, artist, album, trackNumber, genre, year, duration, path);
    }
}