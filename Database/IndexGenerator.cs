using Generation;
using System;
using Holism.Infra;
using System.Data;
using System.Linq;

namespace Generation
{
    public class IndexGenerator : Generator
    {
        public void CreateIndexes(Table table)
        {
            DropIndexes(table);
            if (table.HasGuid)
            {
                CreateIndex(table, new Column { Name = "Guid", IsUnique = true });
            }
            foreach(var column in table.Columns)
            {
                if (column.IsUnique || column.HasIndex)
                {
                    CreateIndex(table, column);
                }
            }
            if (table.Indexes != null)
            {
                foreach (var index in table.Indexes)
                {
                    CreateIndex(table, index);
                }
            }
        }

        public void DropIndexes(Table table)
        {
            var query = @$"
                show index
                from `{table.Name}`
                where key_name != 'PRIMARY'
            ";
            var result = DataAccess.Database.Open(ConnectionString).Get(query);
            foreach (DataRow row in result.Rows)
            {
                query = @$"
                    drop index if exists {row["key_name"].ToString()} on `{table.Name}`
                ";
                DataAccess.Database.Open(ConnectionString).Run(query);
            }
        }

        public void CreateIndex(Table table, Column column)
        {
            try
            {
                var query = @$"
                    create {(column.IsUnique ? "unique" : "")} index IX_{table.Name}_{(column.IsUnique ? "Unique_" : "")}{column.Name}
                    on `{table.Name}` (`{column.Name}`)
                ";
                DataAccess.Database.Open(ConnectionString).Run(query);
            }
            catch (Exception ex)
            {
                Logger.LogError(@$"Error creating unique index on {table.Name} for column {column.Name}");
                Logger.LogException(ex);
            }
        }

        public void CreateIndex(Table table, Index index)
        {
            if (index.Columns == null || index.Columns.Count == 0)
            {
                throw new ClientException("Index definition is wrong");
            }
            try
            {
                var columnNames = index.Columns.OrderBy(i => i).Aggregate((a, b) => @$"{a}_And_{b}");
                var columns = index.Columns
                    .OrderBy(i => i)
                    .Select(i => $"`{i}`")
                    .Aggregate((a, b) => @$"{a}, {b}");
                var query = @$"
                    create {(index.IsUnique ? "unique" : "")} index IX_{table.Name}_{(index.IsUnique ? "Unique_" : "")}{columnNames}
                    on `{table.Name}` ({columns})
                ";
                DataAccess.Database.Open(ConnectionString).Run(query);
            }
            catch (Exception ex)
            {
                Logger.LogError(@$"Error creating unique index on {table.Name}");
                Logger.LogException(ex);
            }
        }
    }
}