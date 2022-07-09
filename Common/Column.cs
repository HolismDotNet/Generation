public class Column
{
    private string name;

    public string Name 
    { 
        get
        {
            return name;
        }
        set
        {
            if (value.Contains("Gmt"))
            {
                throw new ClientException("GMT is not accepted in the name of date columns. Use UTC instead.");
            }
            if (value.Contains("Utc"))
            {
                name = value;
            }
            else 
            {
                name = value.Replace("Date", "UtcDate");
            }
        }
    }

    public string Type { get; set; }

    public string DotNetType
    {
        get
        {
            if (Name.EndsWith("Guid"))
            {
                if (nullable)
                {
                    return "Guid?";
                }
                return "Guid";
            }
            if (Name.EndsWith("Id"))
            {
                if (nullable)
                {
                    return "long?";
                }
                return "long";
            }
            if (Name.Contains("Date"))
            {
                if (nullable)
                {
                    return "DateTime?";
                }
                return "DateTime";
            }
            if (Type == "int")
            {
                if (nullable)
                {
                    return "int?";
                }
                return "int";
            }
            if (Type == "long")
            {
                if (nullable)
                {
                    return "long?";
                }
                return "long";
            }
            if (Type == "boolean")
            {
                if (nullable)
                {
                    return "bool?";
                }
                return "bool";
            }
            if (Name == "Json")
            {
                return "string";
            }
            if (Type == null)
            {
                return "string";
            }
            return "string";
        }
    }

    public string SqlType
    {
        get
        {
            if (Name.EndsWith("Guid"))
            {
                return "char(36)";
            }
            if (Name.EndsWith("Id"))
            {
                return "bigint";
            }
            if (Name.Contains("Date"))
            {
                return "datetime";
            }
            if (name.Contains("Content"))
            {
                return "longtext";
            }
            if (Type == "int")
            {
                return "int";
            }
            if (Type == "long")
            {
                return "bigint";
            }
            if (Type == "boolean")
            {
                return "bit";
            }
            if (Type == "decimal")
            {
                return "decimal";
            }
            if (Type == "longText")
            {
                return "longtext character set utf8";
            }
            if (Name == "Json")
            {
                return "json";
            }
            return "varchar(400) character set utf8";
        }
    }

    public bool nullable { get; set; }
    
    public bool HasDefault { get; set; }

    public string DefaultSqlText { get; set; }

    public bool Unique { get; set; }

    public bool HasIndex { get; set; }

    public string IndexFilter { get; set; }

    public bool CascadeDelete { get; set; }

    public bool IsGenerated { get; set; }

    public string Formula { get; set; }

    public int? Precision { get; set; }

    public int? Scale { get; set; }
}
