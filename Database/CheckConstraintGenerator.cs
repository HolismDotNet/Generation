using DataAccess;
using Generation;
using System.Collections.Generic;
using Holism.Infra;
using System.Data;
using System;

namespace Generation
{
    public class CheckConstraintGenerator : Generator
    {
        public void CreateChecks(Table table)
        {
            DropChecks(table);
            foreach (var column in table.Columns)
            {
                if (column.Name.Contains("Guid"))
                {
                    CreateGuidCheck(table.Name, column);
                }
            }
            if (table.Checks != null)
            {
                foreach (var check in table.Checks)
                {
                    if (check.Name.IsNothing())
                    {
                        throw new ServerException("Check has no name");
                    }
                    CreateCheck(table.Name, check);
                }
            }
        }

        public void DropChecks(Table table)
        {
            var query = @$"
                select table_schema,
                    table_name,
                    constraint_name
                from information_schema.table_constraints
                where constraint_type = 'CHECK'
                and table_schema = '{Database.Name}'
                and table_name = '{table.Name}'
                order by table_schema,
                        table_name;
            ";
            var result = DataAccess.Database.Open(ConnectionString).Get(query);
            foreach (DataRow row in result.Rows)
            {
                query = @$"
                    alter table `{table.Name}`
                    drop constraint `{row["constraint_name"].ToString()}`
                ";
                DataAccess.Database.Open(ConnectionString).Run(query);
            }
        }

        public void CreateGuidCheck(string tableName, Column column)
        {
            var query = @$"
                alter table `{tableName}`
                add constraint Ck_{tableName}_NonEmpty{column.Name}
                check (`{column.Name}` != '{Guid.Empty.ToString()}')
            ";
            DataAccess.Database.Open(ConnectionString).Run(query);
        }

        public void CreateCheck(string tableName, CheckConstraint check)
        {
            var query = @$"
                alter table `{tableName}`
                add constraint Ck_{tableName}_{check.Name}
                check ({check.Query})
            ";
            DataAccess.Database.Open(ConnectionString).Run(query);
        }
    }
}