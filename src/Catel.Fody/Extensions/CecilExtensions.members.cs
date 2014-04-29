// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.members.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using Mono.Cecil;

    public static partial class CecilExtensions
    {
        public static void MarkAsCompilerGenerated(this MemberReference member, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            var module = member.Module;
            if (module == null)
            {
                return;
            }

            var compilerGeneratedAttribute = (TypeDefinition)msCoreReferenceFinder.GetCoreTypeReference("System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            if (compilerGeneratedAttribute != null)
            {
                var attribute = CreateAttribute(compilerGeneratedAttribute, module);

                var fieldDefinition = member as FieldDefinition;
                if (fieldDefinition != null)
                {
                    fieldDefinition.CustomAttributes.Add(attribute);
                }

                var propertyDefinition = member as PropertyDefinition;
                if (propertyDefinition != null)
                {
                    propertyDefinition.CustomAttributes.Add(attribute);
                }

                var methodDefinition = member as MethodDefinition;
                if (methodDefinition != null)
                {
                    methodDefinition.CustomAttributes.Add(attribute);
                }
            }
        }

        private static CustomAttribute CreateAttribute(TypeDefinition attributeDefinition, ModuleDefinition importingModule)
        {
            return new CustomAttribute(importingModule.Import(attributeDefinition.Resolve().Constructor(false)));
        }
    }
}