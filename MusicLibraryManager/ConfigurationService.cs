using Tommy;

public interface IConfigurationService
{
    public string GetDatabaseConnectionString();
    public string GetMusicDirectory();
}

public class TomlConfigurationService : IConfigurationService
{
    private string _dbConnection;
    private string _musicDirectory;

    public TomlConfigurationService(string filePath)
    {
        using (StreamReader reader = File.OpenText(filePath))
        {
            TomlTable table = TOML.Parse(reader);

            _dbConnection = table["database"].AsString;
            _musicDirectory = table["library"].AsString;
        }
    }

    public string GetDatabaseConnectionString()
    {
        return _dbConnection;
    }

    public string GetMusicDirectory()
    {
        return _musicDirectory;
    }
}