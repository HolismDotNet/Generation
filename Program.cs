Generator.RepositoryPath = args[0];
Generator.Organization = args[1];
Generator.OrganizationPrefix = args[2];
Generator.Repository = args[3];

new DatabaseGenerator().GenerateDatabase();
new ModelGenerator().GenerateModels();
new ContextGenerator().GenerateContext();
new RepositoryGenerator().GenerateRepository();
