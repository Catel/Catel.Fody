// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.assembly.cs" company="Catel development team">
//   Copyright (c) 2008 - 2017 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using Mono.Cecil;

    public static partial class CecilExtensions
    {
        public static AssemblyDefinition Resolve(this IAssemblyResolver assemblyResolver, string name)
        {
            return assemblyResolver.Resolve(new AssemblyNameReference(name, null));
        }
    }
}