public class ModelGenerator : Generator
{
    public void GenerateModels()
    {
        LoadViews();
        foreach (var table in Database.Tables)
        {
            if (table.IsEnum)
            {
                table.GeneratedCode = GenerateEnumForTable(table);
            }
            else
            {
                table.GeneratedCode = GenerateClassForTable(table);
            }
        }
        SaveModels();
    }

    private string GenerateEnumForTable(Table table)
    {
        var @enum = @$"namespace {Repository};

public enum {table.SingularName}
{{
    Unknwon = 0,{GetEnumItems(table)}
}}
";
        return @enum;
    }

    private string GetEnumItems(Table table)
    {
        var enumItems = @"";
        foreach (var enumItem in table.EnumItems)
        {
            enumItems += $"\n    {enumItem.Key} = {enumItem.Value},";
        }
        return enumItems.Trim(',');
    }

    private string GenerateClassForTable(Table table)
    {
        var properties = "";
        foreach (var column in table.Columns)
        {
            properties += GeneratePropertyForColumn(column) + "\n" + "\n";
        }
        properties += "    public dynamic RelatedItems { get; set; }" + "\n" + "\n";
        var @namespace = Namespace;
        string interfaces = "";
        if (Interfaces.Count > 0)
        {
            interfaces = ", " + Interfaces.ToCsv();
        }
        var entityInterfaceInheritance = "";
        var isGuidEntity = table.HasGuid || table.Columns.Any(i => i.Name == "Guid");
        if (isGuidEntity)
        {
            entityInterfaceInheritance += "IGuidEntity";
        }
        else
        {
            entityInterfaceInheritance += "IEntity";
        }
        if (table.Columns.Any(i => i.Name == "Slug"))
        {
            entityInterfaceInheritance += ", ISlug";
        }
        if (table.Columns.Any(i => i.Name == "Key"))
        {
            entityInterfaceInheritance += ", IKey";
        }
        if (table.Columns.Any(i => i.Name == "Order"))
        {
            entityInterfaceInheritance += ", IOrder";
        }
        // if (table.Columns.Any(i => i.Name == "IsSystemic"))
        // {
        //     entityInterfaceInheritance += ", ISystemic"
        // }
        string @class = $@"namespace {Repository};

public class {table.SingularName} : {entityInterfaceInheritance}{interfaces}
{{
    public {table.SingularName}()
    {{
        RelatedItems = new ExpandoObject();
    }}

    public long Id {{ get; set; }}

{properties}
}}
";
        @class = Regex.Replace(@class, @"(\n){2}\n", "$1");
        return @class;
    }

    private string GeneratePropertyForColumn(Column column)
    {
        string property;
        property = $@"    public {column.DotNetType} {column.Name} {{ get; set; }}";
        return property;
    }

    public string Namespace 
    { 
        get 
        {
            return $"{OrganizationPrefix}.{Repository}";
        }
    }

    public List<string> Interfaces
    {
        get
        {
            return new List<string> { };
        }
    }

    public void SaveModels()
    {
        var modelsFolder = PrepareOutputFolder("Models");
        foreach (var model in Database.Tables)
        {
            var targetPath = Path.Combine(modelsFolder, (model.IsView ? @"Views" : ""), model.SingularName + ".cs");
            var targetDirectory = Path.GetDirectoryName(targetPath);
            TrySave(model.GeneratedCode, targetPath, targetDirectory);
        }
    }

    public string OutputFolder { get; }
}