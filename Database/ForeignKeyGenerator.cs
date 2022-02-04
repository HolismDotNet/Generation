public class ForeignKeyGenerator : Generator
{
    public void CreateForeignKeys(Table table)
    {
        if (table.OneToOneWith.IsSomething())
        {
            CreateOneToOneForeignKey(table);
        }
        var foreignKeyColumns = table.Columns.Where(i => i.Name.EndsWith("Id") && i.Name != "Id").ToList();
        foreach (var foreignKeyColumn in foreignKeyColumns)
        {
            CreateForeignKey(table, foreignKeyColumn);
        }
    }

    public void DropForeignKeys()
    {
        var query = @$"
            select *
            from information_schema.key_column_usage
            where constraint_schema = '{Database.Name}'
            and referenced_table_schema is not null
        ";
        var result = DataAccess.Database.Open(ConnectionString).Get(query);
        foreach (DataRow row in result.Rows)
        {
            query = @$"
                alter table `{row["table_name"].ToString()}`
                drop constraint {row["constraint_name"].ToString()}
            ";
            DataAccess.Database.Open(ConnectionString).Run(query);
        }
    }

    public void CreateOneToOneForeignKey(Table table)
    {
        var query = @$"
            alter table `{table.Name}`
            add constraint FK_{table.Name}_Id_{table.OneToOneWith}_Id 
            foreign key (`Id`)
            references `{table.OneToOneWith}` (Id)
            on update cascade
            on delete cascade
            ";
        DataAccess.Database.Open(ConnectionString).Run(query);
    }

    public void CreateForeignKey(Table table, Column column, bool cascadeDrop = true)
    {
        var referencedTableName = column.Name.Replace("Id", "").Pluralize();
        if (column.Name == "ParentId")
        {
            referencedTableName = table.Name;
        }
        try
        {
            var query = @$"
                alter table `{table.Name}`
                add constraint FK_{table.Name}_{column.Name}_{referencedTableName}_Id 
                foreign key (`{column.Name}`)
                references `{referencedTableName}` (Id)";
            if (cascadeDrop) 
            {
                query = @$"
                {query}
                on update cascade
                {(column.CascadeDelete ? "on delete cascade" : "")}
                ";
            }
            DataAccess.Database.Open(ConnectionString).Run(query);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("multiple cascade paths"))
            {
                Logger.LogWarning($"SQL Server complained about multiple cascade paths for table {table.Name} and column {column.Name}. Dropping update and delete cascades ...");
                CreateForeignKey(table, column, false);
                return;
            }
            Logger.LogError(@$"Error creating foreign key on {table.Name} for column {column.Name}");
            Logger.LogException(ex);
        }
    }
}
