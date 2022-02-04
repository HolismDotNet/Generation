public class ColumnGenerator : Generator
{
    public void CreateColumns(Table table)
    {
        DropGeneratedColumns(table);
        if (table.Columns == null)
        {
            if (!table.IsEnum)
            {
                throw new ClientException($"Columns are not defined for table {table.Name}");
            }
        }
        foreach (var column in table.Columns)
        {
            try
            {
                CreateColumn(table.Name, column);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }

    public void DropGeneratedColumns(Table table)
    {
        var query = @$"
            select 
                table_schema as database_name,
                table_name,
                column_name,
                is_generated
            from information_schema.columns
            where table_schema = '{Database.Name}'
            and table_name = '{table.Name}'
            and is_generated != 'NEVER'
        ";
        var result = DataAccess.Database.Open(ConnectionString).Get(query);
        foreach (DataRow row in result.Rows)
        {
            query = @$"
                alter table `{table.Name}`
                drop `{row["column_name"].ToString()}`
            ";
            DataAccess.Database.Open(ConnectionString).Run(query);
        }
    }

    public void CreateColumn(string tableName, Column column)
    {
        if (column.IsGenerated)
        {
            var query = @$"
                alter table `{tableName}`
                add `{column.Name}` {column.SqlType}
                as ({column.Formula}) virtual
            ";
            DataAccess.Database.Open(ConnectionString).Run(query);
        }
        else 
        {
            var query = @$"
                select count(*)
                from information_schema.columns
                where table_schema = '{Database.Name}'
                and table_name = '{tableName}'
                and column_name = '{column.Name}'
            ";
            var count = DataAccess.Database.Open(ConnectionString).Get(query).Rows[0][0].ToString().ToInt();
            if (count == 0)
            {
                query = @$"
                    alter table `{tableName}`
                    add `{column.Name}` {column.SqlType} {(column.IsNullable ? "null" : "not null")}
                ";
                DataAccess.Database.Open(ConnectionString).Run(query);
            }
            else 
            {
                query = @$"
                    alter table `{tableName}`
                    modify `{column.Name}` {column.SqlType} {(column.IsNullable ? "null" : "not null")}
                ";
                DataAccess.Database.Open(ConnectionString).Run(query);
            }
        }
    }
}
