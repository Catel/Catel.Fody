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
    using System.Globalization;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public enum CatelTypeType
    {
        ObservableObject,

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

            Ignore = true;
            TypeDefinition = typeDefinition;
            Name = typeDefinition.FullName;

            DetermineCatelType();
            if (Type == CatelTypeType.Unknown)
            {
                FodyEnvironment.WriteWarning($"Cannot determine the Catel type used for '{Name}', type will be ignored for weaving");
                return;
            }

            Version = TypeDefinition.GetCatelVersion();
            if (Version == CatelVersion.Unknown)
            {
                FodyEnvironment.WriteWarning($"Cannot determine the Catel version used for '{Name}', type will be ignored for weaving");
                return;
            }

            try
            {
                DetermineTypes();

                if (!DetermineMethods())
                {
                    FodyEnvironment.WriteWarning($"Cannot determine the Catel methods used for '{Name}', type will be ignored for weaving");
                    return;
                }

                DetermineProperties();
                DetermineMappings();

                Ignore = false;
            }
            catch (Exception)
            {
                FodyEnvironment.WriteWarning($"Failed to get additional information about type '{Name}', type will be ignored for weaving");
            }
        }

        public string Name { get; private set; }

        public bool Ignore { get; private set; }

        public TypeDefinition TypeDefinition { get; private set; }

        public CatelVersion Version { get; private set; }

        public CatelTypeType Type { get; private set; }

        public List<MemberMapping> Mappings { get; set; }

        public TypeReference PropertyDataType { get; private set; }

        public MethodReference RegisterPropertyWithDefaultValueInvoker { get; private set; }

        public MethodReference RegisterPropertyWithoutDefaultValueInvoker { get; private set; }

        public MethodReference SetValueInvoker { get; private set; }

        public MethodReference SetValueGenericInvoker { get; private set; }

        public MethodReference PreferredSetValueInvoker
        {
            get { return SetValueGenericInvoker ?? SetValueInvoker; }
        }

        public MethodReference GetValueInvoker { get; private set; }

        public MethodReference RaisePropertyChangedInvoker { get; private set; }

        public List<CatelTypeProperty> Properties { get; private set; }

        public List<PropertyDefinition> NoWeavingProperties { get; private set; }

        private void DetermineCatelType()
        {
            if (TypeDefinition.ImplementsViewModelBase())
            {
                Type = CatelTypeType.ViewModel;
            }
            else if (TypeDefinition.ImplementsModelBase())
            {
                Type = CatelTypeType.Model;
            }
            else if (TypeDefinition.ImplementsObservableObject())
            {
                Type = CatelTypeType.ObservableObject;
            }
            else
            {
                Type = CatelTypeType.Unknown;
            }
        }

        private void DetermineTypes()
        {
            var module = TypeDefinition.Module;

            PropertyDataType = module.ImportReference(TypeDefinition.Module.FindType("Catel.Core", "PropertyData"));
            AdvancedPropertyChangedEventArgsType = module.ImportReference(TypeDefinition.Module.FindType("Catel.Core", "AdvancedPropertyChangedEventArgs"));
        }

        public TypeReference AdvancedPropertyChangedEventArgsType { get; private set; }

        public MethodReference BaseOnPropertyChangedInvoker
        {
            get
            {
                var typeDefinition = TypeDefinition;
                var baseTypeDefinition = TypeDefinition.BaseType.Resolve();

                return typeDefinition.Module.ImportReference(RecursiveFindMethod(baseTypeDefinition, "OnPropertyChanged"));
            }
        }

        public IEnumerable<PropertyDefinition> AllProperties
        {
            get
            {
                var typeDefinition = TypeDefinition;
                var propertyDefinitions = new List<PropertyDefinition>();
                while (typeDefinition.BaseType.FullName != "System.Object")
                {
                    propertyDefinitions.AddRange(typeDefinition.Properties.ToList());
                    typeDefinition = typeDefinition.BaseType.Resolve();
                }

                return propertyDefinitions;
            }
        }

        private bool DetermineMethods()
        {
            var module = TypeDefinition.Module;

            // ObservableObject or derived methods
            RaisePropertyChangedInvoker = module.ImportReference(RecursiveFindMethod(TypeDefinition, "RaisePropertyChanged", new[] { "propertyName" }));
            if (RaisePropertyChangedInvoker is null)
            {
                return false;
            }

            // ModelBase or derived methods
            if (Type == CatelTypeType.ObservableObject)
            {
                return true;
            }

            var registerPropertyWithDefaultValueInvokerMethod = FindRegisterPropertyMethod(TypeDefinition, true);
            if (registerPropertyWithDefaultValueInvokerMethod is null)
            {
                return false;
            }

            RegisterPropertyWithDefaultValueInvoker = module.ImportReference(registerPropertyWithDefaultValueInvokerMethod);
            RegisterPropertyWithoutDefaultValueInvoker = module.ImportReference(FindRegisterPropertyMethod(TypeDefinition, false));
            GetValueInvoker = module.ImportReference(RecursiveFindMethod(TypeDefinition, "GetValue", new[] { "property" }, true));

            string[] parameterNames;

            switch (Version)
            {
                case CatelVersion.v4:
                    parameterNames = new[] { "property", "value" };
                    break;

                case CatelVersion.v5:
                    parameterNames = new[] { "property", "value", "notifyOnChange" };
                    break;

                case CatelVersion.v6:
                default:
                    parameterNames = new[] { "property", "value", "notifyOnChange" };
                    break;
            }

            SetValueInvoker = module.ImportReference(RecursiveFindMethod(TypeDefinition, "SetValue", parameterNames, false));

            // Introduced in Catel 5.12 / 6.0
            var genericSetValue = RecursiveFindMethod(TypeDefinition, "SetValue", parameterNames, true);
            if (genericSetValue != null)
            {
                SetValueGenericInvoker = module.ImportReference(genericSetValue);
            }

            return true;
        }

        private void DetermineProperties()
        {
            Properties = new List<CatelTypeProperty>();
            NoWeavingProperties = new List<PropertyDefinition>();
            var typeProperties = TypeDefinition.Properties;

            foreach (var typeProperty in typeProperties)
            {
                if (typeProperty.IsDecoratedWithAttribute("NoWeavingAttribute"))
                {
                    typeProperty.RemoveAttribute("NoWeavingAttribute");
                    NoWeavingProperties.Add(typeProperty);
                }
                else if (typeProperty.SetMethod != null && !typeProperty.SetMethod.IsStatic)
                {
                    Properties.Add(new CatelTypeProperty(TypeDefinition, typeProperty));
                }
            }
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
                    // v4: public static PropertyData RegisterProperty<TValue>(string name, Type type, TValue defaultValue, EventHandler<AdvancedPropertyChangedEventArgs> propertyChangedEventHandler = null, bool includeInSerialization = true, bool includeInBackup = true, bool setParent = true)
                    // v5: public static PropertyData RegisterProperty<TValue>(string name, Type type, TValue defaultValue, EventHandler<AdvancedPropertyChangedEventArgs> propertyChangedEventHandler = null, bool includeInSerialization = true, bool includeInBackup = true)
                    // v6: public static PropertyData RegisterProperty<TValue>(string name, Type type, TValue defaultValue, EventHandler<AdvancedPropertyChangedEventArgs> propertyChangedEventHandler = null, bool includeInSerialization = true, bool includeInBackup = true)

                    int argumentCount;

                    switch (Version)
                    {
                        case CatelVersion.v4:
                            argumentCount = 7;
                            break;

                        case CatelVersion.v5:
                            argumentCount = 6;
                            break;

                        case CatelVersion.v6:
                        default:
                            argumentCount = 6;
                            break;
                    }

                    methods = (from method in currentTypeDefinition.Methods
                               where method.Name == "RegisterProperty" &&
                                     method.IsPublic &&
                                     method.Parameters.Count == argumentCount &&
                                     method.HasGenericParameters &&
                                     method.GenericParameters.Count == 1 &&
                                     method.Parameters[0].ParameterType.FullName.Contains("System.String") &&
                                     !method.Parameters[2].ParameterType.FullName.Contains("System.Func")
                               select method).ToList();
                }
                else
                {
                    // Search for this method:
                    // public static PropertyData RegisterProperty(string name, Type type, object defaultValue, EventHandler<AdvancedPropertyChangedEventArgs> propertyChangedEventHandler = null, bool includeInSerialization = true, bool includeInBackup = true, bool setParent = true)
                    methods = (from method in currentTypeDefinition.Methods
                               where method.Name == "RegisterProperty" &&
                                     method.IsPublic &&
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
                if (baseType is null || baseType.FullName == "System.Object")
                {
                    return null;
                }

                currentTypeDefinition = baseType.ResolveType();
            }
            while (true);

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
                if (baseType is null || baseType.FullName == "System.Object")
                {
                    return null;
                }

                currentTypeDefinition = baseType.ResolveType();
            }
            while (true);

            return methodDefinition.GetMethodReference(typeDefinitions);
        }

        private bool FindMethodDefinition(TypeDefinition type, string methodName, string[] parameterNames, bool findGenericDefinition, out MethodDefinition methodDefinition)
        {
            List<MethodDefinition> methodDefinitions;

            if (!findGenericDefinition)
            {
                methodDefinitions = type.Methods.Where(x => x.Name == methodName).OrderBy(definition => definition.Parameters.Count).ToList();
            }
            else
            {
                methodDefinitions = (from method in type.Methods where method.Name == methodName && method.HasGenericParameters select method).ToList();
            }

            if (parameterNames != null)
            {
                methodDefinitions = methodDefinitions.Where(x => x.Parameters.Select(y => y.Name).ToArray().SequenceEqual(parameterNames)).ToList();
            }

            methodDefinition = methodDefinitions.FirstOrDefault();

            return methodDefinition != null;
        }

        public IEnumerable<PropertyDefinition> GetDependentPropertiesFrom(PropertyDefinition property)
        {
            var dependentPropertyDefinitions = (from dependentPropertyDefinition in TypeDefinition.Properties
                                                where dependentPropertyDefinition != property && ExistPropertyDependencyBetween(dependentPropertyDefinition, property)
                                                select dependentPropertyDefinition).ToList();
            for (var i = 0; i < dependentPropertyDefinitions.Count; i++)
            {
                foreach (var propertyDefinition in GetDependentPropertiesFrom(dependentPropertyDefinitions[i]))
                {
                    if (!dependentPropertyDefinitions.Contains(propertyDefinition))
                    {
                        dependentPropertyDefinitions.Add(propertyDefinition);
                    }
                }
            }

            return dependentPropertyDefinitions;
        }

        public bool ExistPropertyDependencyBetween(PropertyDefinition dependentPropertyDefinition, PropertyDefinition property)
        {
            if (dependentPropertyDefinition.HasParameters)
            {
                return false;
            }

            var found = false;
            var getMethodDefinition = dependentPropertyDefinition?.GetMethod;
            if (getMethodDefinition != null && getMethodDefinition.HasBody)
            {
                var processor = getMethodDefinition.Body.GetILProcessor();

                var idx = 0;
                while (!found && idx < processor.Body.Instructions.Count)
                {
                    var instruction = processor.Body.Instructions[idx];

                    MethodDefinition methodDefinition;
                    if (instruction.OpCode == OpCodes.Call && (methodDefinition = instruction.Operand as MethodDefinition) != null && methodDefinition.DeclaringType.IsAssignableFrom(TypeDefinition) && methodDefinition.Name == string.Format(CultureInfo.InvariantCulture, "get_{0}", property.Name))
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        idx++;
                    }
                }
            }

            return found;
        }
    }
}
