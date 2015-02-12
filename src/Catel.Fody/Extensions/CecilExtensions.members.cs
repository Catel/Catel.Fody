// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.members.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Linq;
    using Mono.Cecil;
    using Mono.Collections.Generic;

    public static partial class CecilExtensions
    {
        private const string CompilerGeneratedAttributeTypeName = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";

        public static string GetFullName(this MemberReference member)
        {
            return string.Format("{0}.{1}", member.DeclaringType.FullName, member.Name);
        }

        public static bool IsMarkedAsCompilerGenerated(this TypeReference type)
        {
            return IsMarkedAsCompilerGeneratedInternal(type);
        }

        public static bool IsMarkedAsCompilerGenerated(this MemberReference member)
        {
            return IsMarkedAsCompilerGeneratedInternal(member);
        }

        private static bool IsMarkedAsCompilerGeneratedInternal(object obj)
        {
            var fieldDefinition = obj as FieldDefinition;
            if (fieldDefinition != null)
            {
                return ContainsAttribute(fieldDefinition.CustomAttributes, CompilerGeneratedAttributeTypeName);
            }

            var propertyDefinition = obj as PropertyDefinition;
            if (propertyDefinition != null)
            {
                return ContainsAttribute(propertyDefinition.CustomAttributes, CompilerGeneratedAttributeTypeName);
            }

            var methodDefinition = obj as MethodDefinition;
            if (methodDefinition != null)
            {
                return ContainsAttribute(methodDefinition.CustomAttributes, CompilerGeneratedAttributeTypeName);
            }

            var typeDefinition = obj as TypeDefinition;
            if (typeDefinition != null)
            {
                return ContainsAttribute(typeDefinition.CustomAttributes, CompilerGeneratedAttributeTypeName);
            }

            return false;
        }

        private static bool ContainsAttribute(Collection<CustomAttribute> customAttributes, string attributeTypeName)
        {
            if (customAttributes == null)
            {
                return false;
            }

            return customAttributes.Any(x => x.AttributeType.FullName.Contains(attributeTypeName));
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
            var compilerGeneratedAttribute = (TypeDefinition)msCoreReferenceFinder.GetCoreTypeReference(CompilerGeneratedAttributeTypeName);
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