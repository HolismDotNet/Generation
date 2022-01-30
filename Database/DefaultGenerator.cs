using DataAccess;
using Generation;
using System.Collections.Generic;
using Holism.Infra;
using System.Data;

namespace Generation
{
    public class DefaultGenerator : Generator
    {
        public void CreateDefaults(Table table)
        {
            DropDefaults(table);
            foreach (var column in table.Columns)
            {
                if (column.DefaultSqlText.IsSomething())
                {
                    CreateDefault(table.Name, column);
                }
            }
        }

        public void DropDefaults(Table table)
        {
            var query = @$"
                select 
                    table_schema as database_name,
                    table_name,
                    column_name,
                    column_default
                from information_schema.columns
                where  column_default is not null 
                and column_default != 'NULL'
                and table_schema = '{Database.Name}'
                and table_name = '{table.Name}'
                order by 
                    table_schema,
                    table_name,
                    ordinal_position;
            ";
            var result = DataAccess.Database.Open(ConnectionString).Get(query);
            foreach (DataRow row in result.Rows)
            {
                query = @$"
                    alter table `{table.Name}`
                    alter `{row["column_name"].ToString()}` drop default
                ";
                DataAccess.Database.Open(ConnectionString).Run(query);
            }
        }

        public void CreateDefault(string tableName, Column column)
        {
            var query = @$"
                alter table `{tableName}`
                alter `{column.Name}` set default {column.DefaultSqlText}
            ";
            DataAccess.Database.Open(ConnectionString).Run(query);
        }
    }
}