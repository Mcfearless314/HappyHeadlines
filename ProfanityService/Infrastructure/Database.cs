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

    public bool ContainsProfanity(string text)
    {
        var connection = _dbProvider.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM ProfanityWords WHERE @text LIKE '%' + Word + '%'";
        var param = command.CreateParameter();
        param.ParameterName = "@text";
        param.Value = text;
        command.Parameters.Add(param);

        return (int)command.ExecuteScalar() > 0;
    }
}