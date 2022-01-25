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
        string @class = $@"namespace {Repository};

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
        var tables = Database.Tables;
        tables = tables.OrderBy(i => i.Name).ToList();
        var last = tables.Last();

        foreach (var item in tables)
        {
            if (item.IsEnum)
            {
                continue;
            }
            properties +=
                $@"    public static Repository<{Repository}.{item.SingularName}> {item.SingularName}
    {{
        get
        {{
            return new Repository<{Repository}.{item.SingularName}>(new {Repository}Context());
        }}
    }}";
            if (last != item)
            {
                properties += "\n" + "\n";
            }
        }

        return properties.TrimEnd();
    }
}
