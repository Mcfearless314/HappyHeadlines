namespace ProfanityService;

public class Database
{
    private readonly DbProvider _dbProvider;

    public Database(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }
    
    public List<string> GetProfanityWords()
    {
        var connection = _dbProvider.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Word FROM ProfanityWords";

        var words = new List<string>();
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            words.Add(reader.GetString(0));
        }

        return words;
    }

    public async Task<bool> ContainsProfanityAsync(string text)
    {
        var connection = _dbProvider.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM ProfanityWords WHERE CHARINDEX(Word, @text) > 0";

        var param = command.CreateParameter();
        param.ParameterName = "@text";
        param.Value = text;
        command.Parameters.Add(param);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }
}