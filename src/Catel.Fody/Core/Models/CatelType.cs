// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeNode.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Mono.Cecil;

    public enum CatelTypeType
    {
        Model,

        ViewModel,

        Unknown
    }

    [DebuggerDisplay("{Name}")]
    public class CatelType
    {
        public CatelType(TypeDefinition typeDefinition)
        {
            Mappings = new List<MemberMapping>();
            Properties = new List<CatelTypeProperty>();

            TypeDefinition = typeDefinition;
            Name = typeDefinition.FullName;

            DetermineCatelType();
            DetermineTypes();
            DetermineMethods();
            Properties = DetermineProperties();
            DetermineMappings();
        }

        public string Name { get; private set; }

        public TypeDefinition TypeDefinition { get; private set; }
        public CatelTypeType Type { get; private set; }

        public List<MemberMapping> Mappings { get; set; }

        public TypeReference PropertyDataType { get; private set; }

        public MethodReference RegisterPropertyWithDefaultValueInvoker { get; private set; }
        public MethodReference RegisterPropertyWithoutDefaultValueInvoker { get; private set; }
        public MethodReference SetValueInvoker { get; private set; }
        public MethodReference GetValueInvoker { get; private set; }

        public List<CatelTypeProperty> Properties { get; private set; }

        private void DetermineCatelType()
        {
            if (TypeDefinition.ImplementsViewModelBase())
            {
                Type = CatelTypeType.ViewModel;
            }
            else if (TypeDefinition.ImplementsCatelModel())
            {
                Type = CatelTypeType.Model;
            }
            else
            {
                Type = CatelTypeType.Unknown;
            }
        }

        private void DetermineTypes()
        {
            var module = TypeDefinition.Module;

            PropertyDataType = module.Import(TypeDefinition.Module.FindType("Catel.Core", "PropertyData"));
        }

        private void DetermineMethods()
        {
            var module = TypeDefinition.Module;

            RegisterPropertyWithDefaultValueInvoker = module.Import(FindRegisterPropertyMethod(TypeDefinition, true).GetGeneric());
            RegisterPropertyWithoutDefaultValueInvoker = module.Import(FindRegisterPropertyMethod(TypeDefinition, false));
            GetValueInvoker = module.Import(RecursiveFindMethod(TypeDefinition, "GetValue", new[] { "property" }, true).GetGeneric());
            SetValueInvoker = module.Import(RecursiveFindMethod(TypeDefinition, "SetValue", new[] { "property", "value" }));
        }

        private List<CatelTypeProperty> DetermineProperties()
        {
            var properties = new List<CatelTypeProperty>();
            var typeProperties = TypeDefinition.Properties;

            foreach (var typeProperty in typeProperties)
            {
                if (typeProperty.IsDecoratedWithAttribute("NoWeavingAttribute"))
                {
                    typeProperty.RemoveAttribute("NoWeavingAttribute");
                    continue;
                }

                if (typeProperty.SetMethod == null)
                {
                    continue;
                }

                if (typeProperty.SetMethod.IsStatic)
                {
                    continue;
                }

                properties.Add(new CatelTypeProperty(TypeDefinition, typeProperty));
            }

            return properties;
        }

        private void DetermineMappings()
        {
            foreach (var property in Properties)
            {
                Mappings.Add(new MemberMapping(property.BackingFieldDefinition, property.PropertyDefinition));
            }
        }

        private MethodReference FindRegisterPropertyMethod(TypeDefinition typeDefinition, bool includeDefaultValue)
        {
            var typeDefinitions = new Stack<TypeDefinition>();
            MethodDefinition methodDefinition;
            var currentTypeDefinition = typeDefinition;

            do
            {
                typeDefinitions.Push(currentTypeDefinition);

                List<MethodDefinition> methods;

                if (includeDefaultValue)
                {
                    // Search for this method:         
                    // public static PropertyData RegisterProperty<TValue>(string name, Type type, TValue defaultValue, EventHandler<AdvancedPropertyChangedEventArgs> propertyChangedEventHandler = null, bool includeInSerialization = true, bool includeInBackup = true, bool setParent = true)
                    methods = (from method in currentTypeDefinition.Methods
                               where method.Name == "RegisterProperty" && method.IsPublic &&
                                     method.HasGenericParameters && method.Parameters.Count == 7 &&
                                     method.Parameters[0].ParameterType.FullName.Contains("System.String") &&
                                     !method.Parameters[2].ParameterType.FullName.Contains("System.Func")
                               select method).ToList();
                }
                else
                {
                    // Search for this method:         
                    // public static PropertyData RegisterProperty(string name, Type type, object defaultValue, EventHandler<AdvancedPropertyChangedEventArgs> propertyChangedEventHandler = null, bool includeInSerialization = true, bool includeInBackup = true, bool setParent = true)
                    methods = (from method in currentTypeDefinition.Methods
                               where method.Name == "RegisterProperty" && method.IsPublic &&
                                     !method.HasGenericParameters &&
                                     method.Parameters[0].ParameterType.FullName.Contains("System.String")
                               select method).ToList();
                }

                if (methods.Count > 0)
                {
                    methodDefinition = methods.First();
                    break;
                }

                var baseType = currentTypeDefinition.BaseType;
                if (baseType == null || baseType.FullName == "System.Object")
                {
                    return null;
                }

                currentTypeDefinition = baseType.ResolveType();
            } while (true);

            return methodDefinition;
        }

        private MethodReference RecursiveFindMethod(TypeDefinition typeDefinition, string methodName, string[] parameterNames = null, bool findGenericDefinition = false)
        {
            var typeDefinitions = new Stack<TypeDefinition>();
            MethodDefinition methodDefinition;
            var currentTypeDefinition = typeDefinition;

            do
            {
                typeDefinitions.Push(currentTypeDefinition);

                if (FindMethodDefinition(currentTypeDefinition, methodName, parameterNames, findGenericDefinition, out methodDefinition))
                {
                    break;
                }
                var baseType = currentTypeDefinition.BaseType;

                if (baseType == null || baseType.FullName == "System.Object")
                {
                    return null;
                }

                currentTypeDefinition = baseType.ResolveType();
            } while (true);

            return methodDefinition.GetMethodReference(typeDefinitions);
        }

        private bool FindMethodDefinition(TypeDefinition type, string methodName, string[] parameterNames, bool findGenericDefinition, out MethodDefinition methodDefinition)
        {
            List<MethodDefinition> methodDefinitions;

            if (!findGenericDefinition)
            {
                methodDefinitions = type.Methods
                    .Where(x => x.Name == methodName)
                    .OrderBy(definition => definition.Parameters.Count).ToList();
            }
            else
            {
                methodDefinitions = (from method in type.Methods
                                     where method.Name == methodName &&
                                           method.HasGenericParameters
                                     select method).ToList();
            }

            if (parameterNames != null)
            {
                methodDefinitions = methodDefinitions.Where(x => x.Parameters.Select(y => y.Name).ToArray().SequenceEqual(parameterNames)).ToList();
            }

            methodDefinition = methodDefinitions.FirstOrDefault();

            return methodDefinition != null;
        }

    }
}