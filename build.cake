//=======================================================
// DEFINE PARAMETERS
//=======================================================

// Define the required parameters
var Parameters = new Dictionary<string, object>();
Parameters["SolutionName"] = "Catel.Fody";
Parameters["Company"] = "CatenaLogic";
Parameters["RepositoryUrl"] = string.Format("https://github.com/{0}/{1}", GetBuildServerVariable("SolutionName"), GetBuildServerVariable("SolutionName"));
Parameters["StartYear"] = "2010";
Parameters["UseVisualStudioPrerelease"] = "true";

// Note: Catel.Fody is a special project where the Attributes projects generates
// the correct package with both the attributes *and* the weaver. We should build 
// and package Catel.Fody.Attributes, but Catel.Fody is the package that needs to be
// deployed
Parameters["BuildCatelFody"] = "true";
Parameters["PackageCatelFody"] = "true";
Parameters["DeployCatelFody"] = "true";
Parameters["BuildCatelFodyAttributes"] = "true";
Parameters["PackageCatelFodyAttributes"] = "true";
Parameters["DeployCatelFodyAttributes"] = "false";

// Note: the rest of the variables should be coming from the build server,
// see `/deployment/cake/*-variables.cake` for customization options
// 
// If required, more variables can be overridden by specifying them via the 
// Parameters dictionary, but the build server variables will always override
// them if defined by the build server. For example, to override the code
// sign wild card, add this to build.cake
//
// Parameters["CodeSignWildcard"] = "Orc.EntityFramework";

//=======================================================
// DEFINE COMPONENTS TO BUILD / PACKAGE
//=======================================================

Components.Add("Catel.Fody");
Components.Add("Catel.Fody.Attributes");

// Components as dependencies since they are required by the test projects
Dependencies.Add("Catel.Fody");
Dependencies.Add("Catel.Fody.Attributes");

Dependencies.Add("Catel.Fody.TestExternalTypesAssembly.Catel5", new []
{
    "Catel.Fody.Tests.Catel5"
});
Dependencies.Add("Catel.Fody.TestAssembly.NetStandard.Catel5", new []
{
    "Catel.Fody.Tests.Catel5"
});
Dependencies.Add("Catel.Fody.TestAssembly.Catel5", new []
{
    "Catel.Fody.Tests.Catel5"
});

Dependencies.Add("Catel.Fody.TestExternalTypesAssembly.Catel6", new []
{
    "Catel.Fody.Tests.Catel6"
});
Dependencies.Add("Catel.Fody.TestAssembly.Catel6", new []
{
    "Catel.Fody.Tests.Catel6"
});

Dependencies.Add("Catel.Fody.TestExternalTypesAssembly.Catel7", new []
{
    "Catel.Fody.Tests.Catel7"
});
Dependencies.Add("Catel.Fody.TestAssembly.Catel7", new []
{
    "Catel.Fody.Tests.Catel7"
});

// Test projects as dependencies since they don't following naming convention
Dependencies.Add("Catel.Fody.Tests.Catel5");
Dependencies.Add("Catel.Fody.Tests.Catel6");
Dependencies.Add("Catel.Fody.Tests.Catel7");

TestProjects.Add("Catel.Fody.Tests.Catel5");
TestProjects.Add("Catel.Fody.Tests.Catel6");
TestProjects.Add("Catel.Fody.Tests.Catel7");

//=======================================================
// REQUIRED INITIALIZATION, DO NOT CHANGE
//=======================================================

// Now all variables are defined, include the tasks, that
// script will take care of the rest of the magic

#l "./deployment/cake/tasks.cake"
