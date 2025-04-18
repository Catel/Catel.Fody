using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Catel.Fody;
using Catel.Fody.Tests;
using Fody;
using Mono.Cecil;

public class AssemblyWeaver
{
    public Assembly Assembly;
    public string BeforeAssemblyPath;
    public string AfterAssemblyPath;

    public List<string> Errors = new List<string>();

    static AssemblyWeaver()
    {
#if CATEL_5
        Instance_NetStandard = new AssemblyWeaver(GetPathToAssemblyToWeave(true), "default");
#endif
        Instance = new AssemblyWeaver(GetPathToAssemblyToWeave(false), "default");
    }

    public static string GetPathToAssemblyToWeave(bool isNetStandard = false)
    {
        var catelVersion = "unknown";

#if CATEL_5
        catelVersion = "5";
#elif CATEL_6
        catelVersion = "6";
#elif CATEL_7
        catelVersion = "7";
#else
        throw new System.Exception("Unknown Catel version");
#endif

        if (isNetStandard)
        {
            return $"Catel.Fody.TestAssembly.NetStandard.Catel{catelVersion}.dll";
        }

        return $"Catel.Fody.TestAssembly.Catel{catelVersion}.dll";
    }

    public static string GenerateConfigurationXml(params KeyValuePair<string, string>[] settings)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine("<Weavers>");
        stringBuilder.Append("  <Catel ");

        foreach (var setting in settings)
        {
            stringBuilder.Append($"{setting.Key}=\"{setting.Value}\" ");
        }

        stringBuilder.AppendLine("/>");
        stringBuilder.AppendLine("</Weavers>");

        return stringBuilder.ToString();
    }

    public AssemblyWeaver(string assemblyLocation, string tag, 
        string configXml = null, List<string> referenceAssemblyPaths = null)
    {
        if (referenceAssemblyPaths is null)
        {
            referenceAssemblyPaths = new List<string>();
        }

        if (string.IsNullOrWhiteSpace(configXml))
        {
            // Disable by default, otherwise the weaving will fail
            configXml = GenerateConfigurationXml(new KeyValuePair<string, string>("DisableWarningsForAutoPropertyInitializers", "true"));
        }

        ////Force ref since MSTest is a POS
        //var type = typeof(ViewModelBaseTest);

        //BeforeAssemblyPath = type.GetAssemblyEx().Location;
        BeforeAssemblyPath = Path.GetFullPath(assemblyLocation);
        AfterAssemblyPath = BeforeAssemblyPath.Replace(".dll", $"_{tag}.dll");

        var oldPdb = Path.ChangeExtension(BeforeAssemblyPath, "pdb");
        var newPdb = Path.ChangeExtension(AfterAssemblyPath, "pdb");
        if (File.Exists(oldPdb))
        {
            File.Copy(oldPdb, newPdb, true);
        }

        Debug.WriteLine("Weaving assembly on-demand from '{0}' to '{1}'", BeforeAssemblyPath, AfterAssemblyPath);

        var assemblyResolver = new TestAssemblyResolver();
        //assemblyResolver.AddSearchDirectory(AssemblyDirectoryHelper.GetCurrentDirectory());
        //foreach (var referenceAssemblyPath in referenceAssemblyPaths)
        //{
        //    var directoryName = Path.GetDirectoryName(referenceAssemblyPath);
        //    assemblyResolver.AddSearchDirectory(directoryName);
        //}

        var metadataResolver = new MetadataResolver(assemblyResolver);

        var readerParameters = new ReaderParameters
        {
            AssemblyResolver = assemblyResolver,
            MetadataResolver = metadataResolver,
            ReadSymbols = File.Exists(oldPdb),
        };

        using (var moduleDefinition = ModuleDefinition.ReadModule(BeforeAssemblyPath, readerParameters))
        {
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyFilePath = AfterAssemblyPath,
                AssemblyResolver = assemblyResolver,
                Config = XElement.Parse(configXml),
                AddinDirectoryPath = Path.Combine(AssemblyDirectoryHelper.GetCurrentDirectory(), "..", "..", "Catel.Fody")
            };

            weavingTask.Execute();
            moduleDefinition.Write(AfterAssemblyPath);
        }

        //        if (Debugger.IsAttached)
        //        {
        //#if DEBUG
        //            var output = "debug";
        //#else
        //            var output = "release";
        //#endif

        //            var targetFile = $@"C:\Source\Catel.Fody\output\{output}\Catel.Fody.Tests\Catel.Fody.TestAssembly2.dll";
        //            var targetDirectory = Path.GetDirectoryName(targetFile);
        //            Directory.CreateDirectory(targetDirectory);
        //            File.Copy(AfterAssemblyPath, targetFile, true);
        //        }

        Assembly = Assembly.LoadFile(AfterAssemblyPath);
    }

    public static AssemblyWeaver Instance { get; private set; }

#if CATEL_5
    public static AssemblyWeaver Instance_NetStandard { get; private set; }
#endif

    private void LogError(string error)
    {
        Errors.Add(error);
    }
}
