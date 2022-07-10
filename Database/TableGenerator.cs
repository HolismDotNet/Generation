public class TableGenerator : Generator
{
    public void CreateTables()
    {
        new ForeignKeyGenerator().DropForeignKeys();
        foreach(var table in Database.Tables)
        {
            if (table.Name.IsNothing())
            {
                throw new ClientException("Table name is not provided");
            }
            try
            {
                CreateTable(table);
                new IndexGenerator().DropIndexes(table);
                new ColumnGenerator().CreateColumns(table);
                new DefaultGenerator().CreateDefaults(table);
                new IndexGenerator().CreateIndexes(table);
                if (table.IsEnum)
                {
                    InsertEnumValues(table);
                }
                new CheckConstraintGenerator().CreateChecks(table);
                if (table.DataFile.IsSomething())
                {
                    InsertData(table);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        foreach(var table in Database.Tables)
        {
            new ForeignKeyGenerator().CreateForeignKeys(table);
        }
    }

    public void CreateTable(Table table)
    {
        var query = @$"
            create table if not exists {table.Name}
            (
                Id bigint not null primary key {((table.OneToOneWith.IsSomething() || table.IsEnum) ? "" : "auto_increment")}
            )
        ";
        DataAccess.Database.Open(ConnectionString).Run(query);
    }

    public void InsertEnumValues(Table table)
    {
        if (table.EnumItems.Any(i => i.Value == 0))
        {
            throw new ClientException("Enum items can not have value 0");
        }
        if (table.EnumItems.Any(i => i.Value < 0))
        {
            throw new ClientException("Enum items should have positive integer values");
        }
        if  (table.EnumItems.GroupBy(i => i.Value).Any(i => i.Count() > 1))
        {
            throw new ClientException("Enum items can't have the same value");
        }
        foreach (var enumItem in table.EnumItems)
        {
            var query = @$"
                insert into `{table.Name}` (Id, `Key`, `Order`)
                values ({enumItem.Value}, N'{enumItem.Key}', {(enumItem.Order.HasValue ? enumItem.Order.Value.ToString() : "null")})
                on duplicate key update `Key`=N'{enumItem.Key}'
            ";
            DataAccess.Database.Open(ConnectionString).Run(query);
        }
    }

    public void InsertData(Table table)
    {
        Logger.LogWarning($"Insertion code is not completed. Not inserting data for {table.Name}.");
        
        // var jsonFile = Path.Combine(Organization, Repository, table.DataFile);
        // if (!File.Exists(jsonFile))
        // {
        //     throw new ClientException(@$"JSON file {jsonFile} does not exist");
        // }
        // var json = File.ReadAllText(jsonFile);
        // if (!json.IsJson())
        // {
        //     throw new ClientException(@$"Content of {jsonFile} is not valid JSON");
        // }
        // openrowset + openjson
    }
}
