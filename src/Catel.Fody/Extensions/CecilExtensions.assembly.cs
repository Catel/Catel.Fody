﻿namespace Catel.Fody
{
    using System;
    using Mono.Cecil;

    public static partial class CecilExtensions
    {
        public static Version GetVersion(this AssemblyDefinition assemblyDefinition)
        {
            return assemblyDefinition.Name.Version;

            //var stringVersion = "0.0.0.0";

            //var assemblyVersionAttributeName = typeof(AssemblyVersionAttribute).FullName;
            //var assemblyFileVersionAttributeName = typeof(AssemblyFileVersionAttribute).FullName;

            //var attribute = assemblyDefinition.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == assemblyVersionAttributeName);
            //if (attribute is null)
            //{
            //    attribute = assemblyDefinition.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == assemblyFileVersionAttributeName);
            //}

            //if (attribute != null)
            //{
            //    stringVersion = (string)attribute.ConstructorArguments.First().Value;
            //}

            //var version = new Version(stringVersion);
            //return version;
        }

        public static bool IsNetCoreLibrary(this AssemblyDefinition assemblyDefinition)
        {
            if (IsNetStandardLibrary(assemblyDefinition))
            {
                return false;
            }

            var mainModule = assemblyDefinition.MainModule;
            if (mainModule.Types.Count == 1)
            {
                return true;
            }

            return false;
        }

        public static bool IsNetStandardLibrary(this AssemblyDefinition assemblyDefinition)
        {
            return assemblyDefinition.MainModule.FileName.IndexOf("netstandard", 0, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static AssemblyDefinition Resolve(this IAssemblyResolver assemblyResolver, string name)
        {
            return assemblyResolver.Resolve(new AssemblyNameReference(name, null));
        }
    }
}
