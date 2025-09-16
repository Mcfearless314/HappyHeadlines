namespace ProfanityService;

public class DbInitializer
{
    private readonly DbProvider _dbProvider;

    public DbInitializer(DbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public void Initialize()
    {
        var connection = _dbProvider.GetConnection();
        
        var command = connection.CreateCommand();
        command.CommandText = @"DROP TABLE IF EXISTS ProfanityWords;
        CREATE TABLE ProfanityWords(Id INT IDENTITY(1,1) PRIMARY KEY, Word NVARCHAR(100))";
        command.ExecuteNonQuery();

        var badWords = new[] { "ugly", "stupid", "fat" };

        foreach (var word in badWords)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM ProfanityWords WHERE Word = @word)
                INSERT INTO ProfanityWords (Word) VALUES (@word)";
            var param = insertCommand.CreateParameter();
            param.ParameterName = "@word";
            param.Value = word;
            insertCommand.Parameters.Add(param);

            insertCommand.ExecuteNonQuery();
        }
    }
}
