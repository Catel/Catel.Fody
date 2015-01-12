// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.members.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public static partial class CecilExtensions
    {
        public static string GetFullName(this MemberReference member)
        {
            return string.Format("{0}.{1}", member.DeclaringType.FullName, member.Name);
        }

        public static void MarkAsCompilerGenerated(this TypeReference type, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            MarkAsCompilerGeneratedInternal(type, msCoreReferenceFinder);
        }

        public static void MarkAsCompilerGenerated(this MemberReference member, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            MarkAsCompilerGeneratedInternal(member, msCoreReferenceFinder);
        }

        private static void MarkAsCompilerGeneratedInternal(object obj, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            var compilerGeneratedAttribute = (TypeDefinition)msCoreReferenceFinder.GetCoreTypeReference("System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            if (compilerGeneratedAttribute != null)
            {
                var fieldDefinition = obj as FieldDefinition;
                if (fieldDefinition != null)
                {
                    var attribute = CreateAttribute(compilerGeneratedAttribute, fieldDefinition.Module);
                    fieldDefinition.CustomAttributes.Add(attribute);
                }

                var propertyDefinition = obj as PropertyDefinition;
                if (propertyDefinition != null)
                {
                    var attribute = CreateAttribute(compilerGeneratedAttribute, propertyDefinition.Module);
                    propertyDefinition.CustomAttributes.Add(attribute);
                }

                var methodDefinition = obj as MethodDefinition;
                if (methodDefinition != null)
                {
                    var attribute = CreateAttribute(compilerGeneratedAttribute, methodDefinition.Module);
                    methodDefinition.CustomAttributes.Add(attribute);
                }

                var typeDefinition = obj as TypeDefinition;
                if (typeDefinition != null)
                {
                    var attribute = CreateAttribute(compilerGeneratedAttribute, typeDefinition.Module);
                    typeDefinition.CustomAttributes.Add(attribute);
                }
            }
        }

        private static CustomAttribute CreateAttribute(TypeDefinition attributeDefinition, ModuleDefinition importingModule)
        {
            return new CustomAttribute(importingModule.Import(attributeDefinition.Resolve().Constructor(false)));
        }
    }
}