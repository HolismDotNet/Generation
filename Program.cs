using System;
using Holism.Infra;
using System.Text.Json;
using System.Dynamic;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Holism.Generation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // args = new string[] {
            //     "/SadraTrade/Opportunities",
            //     "SadraTrade",
            //     "Sadra",
            //     "Opportunities"
            // };

            if (args.Length < 4)
            {
                Logger.LogWarning($"Wrong arguments are specified for the generator");
                return;
            }
                        
            Generator.RepositoryPath = args[0];
            Generator.Organization = args[1];
            Generator.OrganizationPrefix = args[2];
            Generator.Repository = args[3];

            new DatabaseGenerator().GenerateDatabase();
            new ModelGenerator().GenerateModels();
            new ContextGenerator().GenerateContext();
            new RepositoryGenerator().GenerateRepository();
        }
    }
}
