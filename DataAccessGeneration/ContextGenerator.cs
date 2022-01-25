using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Holism.Generation;

public class ContextGenerator: Generator 
{
    public void GenerateContext()
    {
        LoadViews();
        var filename = $"{Repository}Context.cs";
        var modelsFolder = PrepareOutputFile(filename, "DataAccess");
        var targetPath = Path.Combine(modelsFolder, filename);
        var targetDirectory = Path.GetDirectoryName(targetPath);
        var classContent = GenerateClass();
        TrySave(classContent, targetPath, targetDirectory);      
    }
    
    private string GenerateClass()
    {

        string @class = @$"namespace {Repository};

public class {Repository}Context : DatabaseContext
{{
    public override string ConnectionStringName => ""{Repository}"";{GenerateProperties()}

    protected override void OnModelCreating(ModelBuilder builder)
    {{{ConfigureComputedColumns()}
        base.OnModelCreating(builder);
    }}
}}
";
        return @class;
    
    }

    private string ConfigureComputedColumns()
    {
        var result = "";
        foreach (var table in Database.Tables)
        {
            foreach (var column in table.ComputedColumns)
            {
                result += @$"
        builder.Entity<{table.SingularName}>()
            .Property(p => p.{column.Name})
            .HasComputedColumnSql(""{column.Formula}"");";
            }
        }
        return result;
    }

    private string GenerateProperties()
    {
        var properties = "";
        var tables = Database.Tables;
        tables = tables.OrderBy(i => i.Name).ToList();
        foreach (var item in tables)
        {
            if (item.IsEnum) 
            {
                continue;
            }
            properties += $"\n\n    public DbSet<{Repository}.{item.SingularName}> {item.PluralName} {{ get; set; }}";
        }
        return properties;
    }
}
