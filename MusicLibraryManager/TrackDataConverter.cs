using System.Collections;

public static class TrackDataConverter
{
    public static Track Convert(ATL.Track atl)
    {
        Cleanup(atl);

        string title = atl.Title;
        string artist = atl.Artist;
        string album = atl.Album;
        int trackNumber = atl.TrackNumber.HasValue ? atl.TrackNumber.Value : 0;
        string genre = atl.Genre;
        int year = atl.Year.HasValue ? atl.Year.Value : 0;
        int duration = atl.Duration;
        string path = atl.Path;


        return new Track(title, artist, album, trackNumber, genre, year, duration, path);
    }

    public static void Cleanup(ATL.Track atl)
    {
        if (atl.AdditionalFields.ContainsKey("TAL\0"))
        {
            atl.Album = atl.AdditionalFields["TAL\0"];
            atl.AdditionalFields.Remove("TAL\0");
        }

        if (atl.AdditionalFields.ContainsKey("TP1\0"))
        {
            atl.Artist = atl.AdditionalFields["TP1\0"];
            atl.AdditionalFields.Remove("TP1\0");
        }

        if (atl.AdditionalFields.ContainsKey("TRK\0"))
        {
            atl.TrackNumber = Int32.TryParse(atl.AdditionalFields["TRK\0"], out int t) ? t : atl.TrackNumber;
            atl.AdditionalFields.Remove("TRK\0");
        }

        if (atl.AdditionalFields.ContainsKey("TT2\0"))
        {
            atl.Title = atl.AdditionalFields["TT2\0"];
            atl.AdditionalFields.Remove("TT2\0");
        }

        if (atl.AdditionalFields.ContainsKey("TYE\0"))
        {
            atl.Year = Int32.TryParse(atl.AdditionalFields["TYE\0"], out int y) ? y : atl.Year;
            atl.AdditionalFields.Remove("TYE\0");
        }

        // TODO: Genre...

        if (String.IsNullOrEmpty(atl.Album))
        {
            atl.Album = atl.Title;
        }
    }
}