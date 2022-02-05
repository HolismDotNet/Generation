public class Table
{
    private List<Column> columns = new List<Column>();

    public string Name { get; set; }

    public string SingularName
    {
        get
        {
            EnsureNameIsSomething();
            var singularName = Name.Singularize();
            if (singularName == Name)
            {
                throw new ClientException(@$"Table names should be plural => {
                        Name}");
            }
            return singularName;
        }
    }

    public string PluralName
    {
        get
        {
            EnsureNameIsSomething();
            var pluralName = Name.Pluralize();
            if (pluralName != Name)
            {
                throw new ClientException(@$"Table names should be plural => {
                        Name}");
            }
            return pluralName;
        }
    }

    public string SqlQualifiedName
    {
        get
        {
            return $"[{PluralName}]";
        }
    }

    public bool UsesNts
    {
        get
        {
            var sqlServerSpatialDataTypes =
                new List<string> { "geometry", "geography" };
            if (
                Columns
                    .Any(i => sqlServerSpatialDataTypes.Contains(i.SqlType))
            )
            {
                return true;
            }
            return false;
        }
    }

    public bool HasGuid { get; set; }

    public bool HasKey { get; set; }

    public bool HasOrder { get; set; }

    public bool IsEnum { get; set; }

    public List<Column> Columns
    {
        get
        {
            if (IsEnum)
            {
                columns = new List<Column>();
                HasGuid = true;
                columns
                    .Add(new Column { 
                        Name = "Key", 
                        Type = "string", 
                        IsUnique = true 
                    });
                columns
                    .Add(new Column {
                        Name = "Order",
                        Type = "int",
                        IsNullable = true
                    });
            }
            if (HasGuid)
            {
                var guidColumn = columns.FirstOrDefault(i => i.Name == "Guid");
                if (guidColumn != null)
                {
                    columns.Remove(guidColumn);
                }
                columns.Insert(0, new Column { 
                    Name = "Guid", 
                    Type = "Guid", 
                    HasDefault = true, 
                    DefaultSqlText = "uuid()"
                });
            }
            if (HasKey)
            {
                var keyColumn = columns.FirstOrDefault(i => i.Name == "Key");
                if (keyColumn != null)
                {
                    columns.Remove(keyColumn);
                }
                columns.Add(new Column { 
                    Name = "Key",
                    Type = "string",
                    IsUnique = true
                });
            }
            if (HasOrder)
            {
                var orderColumn = columns.FirstOrDefault(i => i.Name == "Order");
                if (orderColumn != null)
                {
                    columns.Remove(orderColumn);
                }
                columns.Add(new Column {
                    Name = "Order",
                    Type = "string",
                    HasDefault = true,
                    DefaultSqlText = "0"
                });
            }
            return columns;
        }
        set
        {
            if (value.Any(i => i.Name == "Id"))
            {
                throw new ClientException("Please remove Id column. Infra handles it.");
            }
            columns = value;
        }
    }

    public List<Column> ComputedColumns
    {
        get
        {
            var result = Columns.Where(i => i.IsGenerated).ToList();
            return result;
        }
    }

    public string OneToOneWith { get; set; }

    public List<ForeignKey> ForeignKeys { get; set; }

    public List<Index> Indexes { get; set; }

    public List<CheckConstraint> Checks { get; set; }

    public string GeneratedCode { get; set; }

    public bool IsView { get; set; }

    public List<EnumItem> EnumItems { get; set; }

    public string DataFile { get; set; }

    private void EnsureNameIsSomething()
    {
        if (Name.IsNothing())
        {
            throw new ClientException("Table name is not provided");
        }
    }
}
