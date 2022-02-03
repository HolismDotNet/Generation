global using System;
global using Holism.Infra;
global using System.Text.Json;
global using System.Dynamic;
global using System.Net.Http;
global using System.Net;
global using System.Collections.Generic;
global using System.Net.Http.Headers;
global using System.Linq;
global using System.Linq.Expressions;

Generator.RepositoryPath = args[0];
Generator.Organization = args[1];
Generator.OrganizationPrefix = args[2];
Generator.Repository = args[3];

new DatabaseGenerator().GenerateDatabase();
new ModelGenerator().GenerateModels();
new ContextGenerator().GenerateContext();
new RepositoryGenerator().GenerateRepository();
