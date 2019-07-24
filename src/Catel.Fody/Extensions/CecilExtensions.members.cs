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
        public static string GetFullName(this MemberReference member)
        {
            return $"{member.DeclaringType.FullName}.{member.Name}";
        }

        public static bool IsMarkedAsGeneratedCode(this TypeReference type)
        {
            return IsMarkedAsGeneratedCodeInternal(type);
        }

        public static bool IsMarkedAsGeneratedCode(this MemberReference member)
        {
            return IsMarkedAsGeneratedCodeInternal(member);
        }

        private static bool IsMarkedAsGeneratedCodeInternal(object obj)
        {
            if (obj is FieldDefinition fieldDefinition)
            {
                return ContainsAttribute(fieldDefinition.CustomAttributes, MsCoreReferenceFinder.GeneratedCodeAttributeTypeName);
            }

            if (obj is PropertyDefinition propertyDefinition)
            {
                return ContainsAttribute(propertyDefinition.CustomAttributes, MsCoreReferenceFinder.GeneratedCodeAttributeTypeName);
            }

            if (obj is MethodDefinition methodDefinition)
            {
                return ContainsAttribute(methodDefinition.CustomAttributes, MsCoreReferenceFinder.GeneratedCodeAttributeTypeName);
            }

            if (obj is TypeDefinition typeDefinition)
            {
                return ContainsAttribute(typeDefinition.CustomAttributes, MsCoreReferenceFinder.GeneratedCodeAttributeTypeName);
            }

            return false;
        }

        private static bool ContainsAttribute(Collection<CustomAttribute> customAttributes, string attributeTypeName)
        {
            if (customAttributes is null)
            {
                return false;
            }

            return customAttributes.Any(x => x.AttributeType.FullName.Contains(attributeTypeName));
        }

        public static void MarkAsCompilerGenerated(this TypeReference type, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            MarkAsGeneratedCodeInternal(type, msCoreReferenceFinder);
        }

        public static void MarkAsCompilerGenerated(this MemberReference member, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            MarkAsGeneratedCodeInternal(member, msCoreReferenceFinder);
        }

        private static void MarkAsGeneratedCodeInternal(object obj, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            if (obj is FieldDefinition fieldDefinition)
            {
                fieldDefinition.CustomAttributes.MarkAsGeneratedCodeInternal(msCoreReferenceFinder, fieldDefinition.Module);
            }

            if (obj is PropertyDefinition propertyDefinition)
            {
                propertyDefinition.CustomAttributes.MarkAsGeneratedCodeInternal(msCoreReferenceFinder, propertyDefinition.Module);
            }

            if (obj is MethodDefinition methodDefinition)
            {
                methodDefinition.CustomAttributes.MarkAsGeneratedCodeInternal(msCoreReferenceFinder, methodDefinition.Module);
            }

            if (obj is TypeDefinition typeDefinition)
            {
                typeDefinition.CustomAttributes.MarkAsGeneratedCodeInternal(msCoreReferenceFinder, typeDefinition.Module);
            }
        }

        private static void MarkAsGeneratedCodeInternal(this Collection<CustomAttribute> customAttributes, MsCoreReferenceFinder msCoreReferenceFinder, ModuleDefinition importingModule)
        {
            var generatedCodeAttribute = CreateGeneratedCodeAttribute(msCoreReferenceFinder, importingModule);
            if (generatedCodeAttribute != null)
            {
                customAttributes.Add(generatedCodeAttribute);
            }

            var debuggerAttribute = CreateDebuggerNonUserCodeAttribute(msCoreReferenceFinder, importingModule);
            if (debuggerAttribute != null)
            {
                customAttributes.Add(debuggerAttribute);
            }
        }

        private static CustomAttribute CreateGeneratedCodeAttribute(MsCoreReferenceFinder msCoreReferenceFinder, ModuleDefinition importingModule)
        {
            var attributeType = msCoreReferenceFinder.GeneratedCodeAttribute;
            if (attributeType is null)
            {
                return null;
            }

            var stringType = (TypeDefinition)msCoreReferenceFinder.GetCoreTypeReference("System.String");

            var constructor = attributeType.Resolve().FindConstructor(new[] {stringType, stringType}.ToList());
            if (constructor is null)
            {
                return null;
            }

            var version = typeof(ModuleWeaver).Assembly.GetName().Version.ToString();
            var name = typeof(ModuleWeaver).Assembly.GetName().Name;

            var generatedAttribute = new CustomAttribute(importingModule.ImportReference(constructor));
            generatedAttribute.ConstructorArguments.Add(new CustomAttributeArgument(stringType, name));
            generatedAttribute.ConstructorArguments.Add(new CustomAttributeArgument(stringType, version));

            return generatedAttribute;
        }

        private static CustomAttribute CreateDebuggerNonUserCodeAttribute(MsCoreReferenceFinder msCoreReferenceFinder, ModuleDefinition importingModule)
        {
            var attributeType = msCoreReferenceFinder.DebuggerNonUserCodeAttribute;
            if (attributeType is null)
            {
                return null;
            }

            var attribute = new CustomAttribute(importingModule.ImportReference(attributeType.Resolve().Constructor(false)));
            return attribute;
        }
    }
}
