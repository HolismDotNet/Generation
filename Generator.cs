using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Holism.Infra;
using Holism.Validation;
using Microsoft.Data.SqlClient;

namespace Holism.Generation
{
    public abstract class Generator
    {
        public static string OrganizationPrefix { get; set; }

        public static string Organization { get; set; }

        public static string Repository { get; set; }

        public static string RepositoryPath { get; set; }

        public Database Database { get; private set; }

        public Generator()
        {
            var databaseJsonFile =
                Path.Combine(RepositoryPath, "Database.json");
            if (!File.Exists(databaseJsonFile))
            {
                throw new ServerException(@$"Database.json file does not exist {
                        RepositoryPath}");
            }
            var databaseJson = File.ReadAllText(databaseJsonFile);
            if (!databaseJson.IsJson())
            {
                throw new ServerException(@$"Database.json does not containe valid JSON in {
                        RepositoryPath}");
            }
            Database = databaseJson.Deserialize<Database>();
        }

        protected void LoadViews()
        {
            var query = @$"
                select 
                    table_schema as database_name,
                    table_name as view_name
                from information_schema.views
                where table_schema not in 
                (
                    'sys',
                    'information_schema', 
                    'mysql', 
                    'performance_schema'
                )
                and table_schema = '{Database.Name}'
                order by 
                    table_schema,
                    table_name;         
            ";
            var views = Holism.DataAccess.Database.Open(ConnectionString).Get(query);
            foreach (DataRow row in views.Rows)
            {
                var table = new Table();
                table.Name = row["view_name"].ToString();
                table.IsView = true;
                table.Columns = GetColumns(table);
                Database.Tables.Add(table);
            }
        }

        private List<Column> GetColumns(Table table)
        {
            var query = $@"
                show columns 
                from {table.Name}
                where Field != 'Id'
            ";
            var columns = new List<Column>();
            var definitionsTable = Holism.DataAccess.Database
                .Open(ConnectionString)
                .Get(query);
            for (int i = 0; i < definitionsTable.Rows.Count; i++)
            {
                var column = new Column();
                column.Name = definitionsTable.Rows[i]["Field"].ToString();
                column.Type = MapSqlTypeToType(definitionsTable.Rows[i]["Type"].ToString());
                column.IsNullable = definitionsTable.Rows[i]["Null"].ToString() == "YES";
                // column.Precision = (byte?)definitionsTable.Rows[i]["precision"];
                // column.Scale = (byte?)definitionsTable.Rows[i]["scale"];
                columns.Add(column);
            }
            return columns;
        }

        private string MapSqlTypeToType(string sqlType)
        {
            if 
            (
                sqlType.Contains("varchar")
                ||
                sqlType.Contains("mediumtext")
                ||
                sqlType.Contains("longtext")
            )
            {
                return "string";
            }
            else if (sqlType == "char(36)")
            {
                return "Guid";
            }
            else if (sqlType.Contains("bigint"))
            {
                return "long";
            }
            else if (sqlType.Contains("datetime")) 
            {
                return "Date";
            }
            else if (sqlType.Contains("bit")) 
            {
                return "boolean";
            }
            else if (sqlType.Contains("int")) 
            {
                return "int";
            }
            else 
            {
                throw new ClientException($"SQL data type '{sqlType}' is not mapped to declarative type");
            }
        }

        protected string PrepareOutputFolder(string directory)
        {
            var outputFolder = $"/{Organization}/{Repository}/{directory}";
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
                Directory.CreateDirectory(outputFolder);
            }
            return outputFolder;
        }

        protected string PrepareOutputFile(string filename, string directory)
        {
            var outputFolder = $"/{Organization}/{Repository}/{directory}";
            var targetDirectory =
                $"/{Organization}/{Repository}/{directory}/{filename}";
            if (File.Exists(targetDirectory))
            {
                File.Delete (targetDirectory);
            }
            else if (!File.Exists(outputFolder))
            {
                Directory.CreateDirectory (outputFolder);
            }
            return outputFolder;
        }

        public string NtsUsingStatement(Table table)
        {
            if (table.UsesNts)
            {
                return "using NetTopologySuite.Geometries;\r\n";
            }
            return "";
        }

        protected void TrySave(
            string generatedCode,
            string targetPath,
            string targetDirectory
        )
        {
            var retryCount = 0;
            bool isSaved = false;
            while (!isSaved)
            {
                try
                {
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory (targetDirectory);
                    }
                    File.WriteAllText (targetPath, generatedCode);
                    isSaved = true;
                }
                catch (Exception)
                {
                    Logger
                        .LogWarning(@$"Trying to save {
                            targetPath}. Waiting for a second...");
                    Thread.Sleep(1000);
                    retryCount++;
                }
            }
        }

        protected string ConnectionString
        {
            get
            {
                var connectionString = Config.GetConnectionString(Database.Name);
                return connectionString;
            }
        }

        protected string MasterDatabaseConnectionString
        {
            get
            {
                var masterConnectionString =
                    Regex
                        .Replace(ConnectionString,
                        @"database=.*",
                        "");
                return masterConnectionString;
            }
        }

        public string GetSingularName(Table table)
        {
            var conflictingNames = new List<string> { "TimeZone" };
            if (conflictingNames.Contains(table.SingularName))
            {
                return $"Models.{table.SingularName}";
            }
            return table.SingularName;
        }
    }
}
