using Generation;
using System.Text.RegularExpressions;
using System;
using Holism.Infra;

namespace Generation
{
    public class DatabaseGenerator : Generator
    {
        public void GenerateDatabase()
        {
            if (Database.Name.IsNothing())
            {
                throw new ClientException($"Please provide database name");
            }
            try
            {
                CreateDatabase();
                new TableGenerator().CreateTables();
                new ViewGenerator().CreateViews();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void CreateDatabase()
        {
            var query = @$"
                create database if not exists {Database.Name}
            ";
            DataAccess.Database.Open(MasterDatabaseConnectionString).Run(query);
            // todo: set to simple model to reduce space
        }
    }
}