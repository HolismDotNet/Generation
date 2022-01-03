using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Holism.Generation;

public class RepositoryGenerator : Generator
{
    public void GenerateRepository()
    {
        LoadViews();
        var filename = $"Repository.cs";
        var modelsFolder = PrepareOutputFile(filename, "DataAccess");
        var targetPath = Path.Combine(modelsFolder, filename);
        var targetDirectory = Path.GetDirectoryName(targetPath);
        var classContent = GenerateClass();
        TrySave (classContent, targetPath, targetDirectory);
    }

    private string GenerateClass()
    {
        string @class = $@"namespace {OrganizationPrefix}.{Repository}.DataAccess;

public class Repository
{{
{GenerateProperties()}
}}
";
        return @class;
    }

    private string GenerateProperties()
    {
        var properties = "";
        var last = Database.Tables.Last();
        var tables = Database.Tables;
        tables = tables.OrderBy(i => i.Name).ToList();

        foreach (var item in tables)
        {
            if (item.IsEnum)
            {
                continue;
            }
            properties +=
                $@"    public static Repository<{(GetSingularName(item))}> {item.SingularName}
    {{
        get
        {{
            return new Repository<{GetSingularName(item)}>(new {Repository}Context());
        }}
    }}";
            if (last != item)
            {
                properties += "\n" + "\n";
            }
        }

        return properties;
    }
}
