public class InitialDataInserter : Generator
{
    public void InsertData()
    {
        var initialDataPath = $"{RepositoryPath}/InitialData.sql";
        if (!File.Exists(initialDataPath))
        {
            Logger.LogInfo($"Repository {Repository} does not have InitialData.sql");
            return;
        }
        try
        {
            var query = File.ReadAllText(initialDataPath);
            DataAccess.Database.Open(ConnectionString).Run(query);
            Logger.LogSuccess($"Initial data of {Repository} is inserted");
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
        }
    }
}