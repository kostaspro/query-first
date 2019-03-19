namespace QueryFirst
{
    public interface IConfigResolver
    {
        IQFConfigModel GetConfig(string filePath, string queryText);
    }
}