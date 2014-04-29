// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.members.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;
    using Mono.Cecil;

    public static partial class CecilExtensions
    {
        public static void MarkAsCompilerGenerated(this MemberReference member, ModuleDefinition module = null)
        {
            if (module == null)
            {
                module = member.Module;
            }

            if (module == null)
            {
                return;
            }

            var compilerGeneratedAttribute = module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            if (compilerGeneratedAttribute != null)
            {
                var fieldDefinition = member as FieldDefinition;
                if (fieldDefinition != null)
                {
                    fieldDefinition.CustomAttributes.Add(CreateAttribute(member, compilerGeneratedAttribute));
                }

                var propertyDefinition = member as PropertyDefinition;
                if (propertyDefinition != null)
                {
                    propertyDefinition.CustomAttributes.Add(CreateAttribute(member, compilerGeneratedAttribute));
                }

                var methodDefinition = member as MethodDefinition;
                if (methodDefinition != null)
                {
                    methodDefinition.CustomAttributes.Add(CreateAttribute(member, compilerGeneratedAttribute));
                }
            }
        }

        private static CustomAttribute CreateAttribute(MemberReference member, TypeDefinition attributeDefinition)
        {
            return new CustomAttribute(member.DeclaringType.Module.Import(attributeDefinition.Resolve().Constructor(false)));
        }
    }
}