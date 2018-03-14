var projectName = "Catel.Fody";
var projectsToPackage = new [] { "Catel.Fody" };
var company = "CatenaLogic";
var startYear = 2010;
var defaultRepositoryUrl = string.Format("https://github.com/{0}/{1}", company, projectName);

#l "./deployment/cake/variables.cake"
#l "./deployment/cake/tasks.cake"
