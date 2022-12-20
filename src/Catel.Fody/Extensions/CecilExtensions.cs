namespace Catel.Fody
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public static partial class CecilExtensions
    {
        private static AssemblyNameReference SystemRuntimeRef;

        private static readonly Dictionary<string, TypeDefinition> CachedTypeDefinitions = CacheHelper.GetCache<Dictionary<string, TypeDefinition>>("CecilExtensions");

        public static void FixPrivateCorLibScope(this TypeReference type)
        {
            var scope = type.Scope;
            if (!scope.Name.Contains("System.Private.CoreLib.dll"))
            {
                return;
            }

            if (SystemRuntimeRef is null)
            {
                var systemRuntimeAssembly = type.Module.ResolveAssembly("System.Runtime");

                SystemRuntimeRef = new AssemblyNameReference(systemRuntimeAssembly.Name.Name, systemRuntimeAssembly.GetVersion())
                {
                    PublicKeyToken = new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a }
                };
            }

            type.Scope = SystemRuntimeRef;
        }

        public static CatelVersion GetCatelVersion(this TypeReference type)
        {
            var baseType = type;

            while (true)
            {
                if (baseType is null)
                {
                    break;
                }

                var assemblyName = baseType.Module.Assembly.Name;
                if (assemblyName.Name.Equals("Catel.Core"))
                {
                    var version = assemblyName.Version;

                    switch (version.Major)
                    {
                        case 5:
                            return CatelVersion.v5;

                        case 6:
                            return CatelVersion.v6;

                        default:
                            return CatelVersion.Unknown;
                    }
                }

                baseType = baseType.Resolve()?.BaseType;
            }

            return CatelVersion.Unknown;
        }

        public static bool IsBoxingRequired(this TypeReference typeReference, TypeReference expectedType)
        {
            if (expectedType.IsValueType && string.Equals(typeReference.FullName, expectedType.FullName))
            {
                // Boxing is never required if type is expected
                return false;
            }

            if (typeReference.IsValueType || typeReference.IsGenericParameter)
            {
                return true;
            }

            return false;
        }

        public static TypeReference Import(this TypeReference typeReference, bool checkForNullableValueTypes = false)
        {
            var module = FodyEnvironment.ModuleDefinition;

            if (checkForNullableValueTypes)
            {
                var nullableValueType = typeReference.GetNullableValueType();
                if (nullableValueType is not null)
                {
                    return module.ImportReference(nullableValueType);
                }
            }

            return module.ImportReference(typeReference);
        }

        public static MethodReference FindConstructor(this TypeDefinition typeReference, List<TypeDefinition> types)
        {
            foreach (var ctor in typeReference.GetConstructors())
            {
                if (ctor.Parameters.Count == types.Count)
                {
                    var isValid = true;

                    for (var i = 0; i < ctor.Parameters.Count; i++)
                    {
                        var parameter = ctor.Parameters[i];
                        if (!string.Equals(parameter.ParameterType.FullName, types[i].FullName))
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        return ctor;
                    }
                }
            }

            return null;
        }

        public static TypeReference MakeGenericIfRequired(this TypeReference typeReference)
        {
            if (typeReference.HasGenericParameters)
            {
                var genericDeclaringType = new GenericInstanceType(typeReference);

                foreach (var genericParameter in typeReference.GenericParameters)
                {
                    genericDeclaringType.GenericArguments.Add(genericParameter);
                }

                typeReference = genericDeclaringType;
            }

            return typeReference;
        }

        public static TypeReference GetNullableValueType(this TypeReference typeReference)
        {
            if (!typeReference.IsGenericInstance)
            {
                return null;
            }

            if (!typeReference.FullName.Contains("System.Nullable`1"))
            {
                return null;
            }

            if (!(typeReference is GenericInstanceType genericInstanceType))
            {
                return null;
            }

            if (genericInstanceType.GenericArguments.Count != 1)
            {
                return null;
            }

            var genericParameter = genericInstanceType.GenericArguments[0];
            if (!genericParameter.IsValueType)
            {
                return null;
            }

            return genericParameter.GetElementType();
        }

        public static bool IsNullableValueType(this TypeReference typeReference)
        {
            return GetNullableValueType(typeReference) is not null;
        }

        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType, self.DeclaringType.MakeGenericInstanceType(arguments))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParameter in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));
            }

            return reference;
        }

        public static AssemblyDefinition ResolveAssembly(this ModuleDefinition moduleDefinition, string assemblyName)
        {
            var assemblyWithoutExtension = moduleDefinition.Name.Substring(0, moduleDefinition.Name.LastIndexOf("."));
            if (string.Equals(assemblyWithoutExtension, assemblyName))
            {
                return moduleDefinition.Assembly;
            }

            var assemblyResolver = moduleDefinition.AssemblyResolver;
            var resolvedAssembly = assemblyResolver.Resolve(assemblyName);
            return resolvedAssembly;
        }

        public static TypeDefinition FindType(this ModuleDefinition moduleDefinition, string assemblyName, string typeName)
        {
            var cacheKey = $"{typeName}, {assemblyName}|{moduleDefinition.Name}";
            if (CachedTypeDefinitions.ContainsKey(cacheKey))
            {
                return CachedTypeDefinitions[cacheKey];
            }

            var resolvedAssembly = moduleDefinition.ResolveAssembly(assemblyName);
            if (resolvedAssembly is null)
            {
                return null;
            }

            foreach (var module in resolvedAssembly.Modules)
            {
                var allTypes = module.GetAllTypeDefinitions().OrderBy(x => x.FullName).ToList();

                var type = (from typeDefinition in allTypes
                            where typeDefinition.FullName == typeName
                            select typeDefinition).FirstOrDefault();
                if (type is null)
                {
                    type = (from typeDefinition in allTypes
                            where typeDefinition.Name == typeName
                            select typeDefinition).FirstOrDefault();
                }

                if (type is not null)
                {
                    CachedTypeDefinitions[cacheKey] = type;
                    return type;
                }
            }

            return null;
        }

        public static PropertyReference GetProperty(this TypeReference typeReference, string propertyName)
        {
            if (!(typeReference is TypeDefinition typeDefinition))
            {
                typeDefinition = typeReference.Resolve();
            }

            return GetProperty(typeDefinition, propertyName);
        }

        public static PropertyReference GetProperty(this TypeDefinition typeDefinition, string propertyName)
        {
            var type = typeDefinition;
            while (type is not null && !type.FullName.Contains("System.Object"))
            {
                var propertyDefinition = (from property in type.Properties
                                          where string.Equals(propertyName, property.Name)
                                          select property).FirstOrDefault();

                if (propertyDefinition is not null)
                {
                    return propertyDefinition;
                }

                type = type.BaseType?.Resolve();
            }

            return null;
        }

        public static MethodReference GetMethodAndImport(this ModuleDefinition module, string methodName)
        {
            var method = GetMethod(module, methodName);
            if (method is null)
            {
                return method;
            }

            return module.ImportReference(method);
        }

        public static MethodReference GetMethod(this ModuleDefinition module, string methodName)
        {
            var resolver = module.AssemblyResolver;
            foreach (var assemblyReference in module.AssemblyReferences)
            {
                var assembly = resolver.Resolve(assemblyReference.Name);
                if (assembly is not null)
                {
                    foreach (var type in assembly.MainModule.GetAllTypeDefinitions())
                    {
                        var methodReference = (from method in type.Methods
                                               where method.Name == methodName
                                               select method).FirstOrDefault();
                        if (methodReference is not null)
                        {
                            return methodReference;
                        }
                    }
                }
            }

            return null;
        }

        public static string GetName(this PropertyDefinition propertyDefinition)
        {
            return $"{propertyDefinition.DeclaringType.FullName}.{propertyDefinition.Name}";
        }

        public static bool IsCall(this OpCode opCode)
        {
            return (opCode.Code == Code.Call) || (opCode.Code == Code.Callvirt);
        }

        public static string GetName(this MethodDefinition methodDefinition)
        {
            return $"{methodDefinition.DeclaringType.FullName}.{methodDefinition.Name}";
        }

        public static MethodDefinition Constructor(this TypeDefinition typeDefinition, bool isStatic)
        {
            return (from method in typeDefinition.Methods
                    where method.IsConstructor && method.IsStatic == isStatic
                    select method).FirstOrDefault();
        }

        public static FieldReference GetGeneric(this FieldDefinition definition)
        {
            if (definition.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(definition.DeclaringType);

                foreach (var parameter in definition.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }

                return new FieldReference(definition.Name, definition.FieldType, declaringType);
            }

            return definition;
        }

        public static MethodReference GetGeneric(this MethodReference reference)
        {
            if (reference.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(reference.DeclaringType);
                foreach (var parameter in reference.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }
                var methodReference = new MethodReference(reference.Name, reference.MethodReturnType.ReturnType, declaringType);
                foreach (var parameterDefinition in reference.Parameters)
                {
                    methodReference.Parameters.Add(parameterDefinition);
                }
                methodReference.HasThis = reference.HasThis;
                return methodReference;
            }

            return reference;
        }

        public static CustomAttribute GetAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
        {
            return attributes.FirstOrDefault(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
        }

        public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
        {
            return attributes.Any(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
        }

        public static List<TypeDefinition> GetAllTypeDefinitions(this ModuleDefinition moduleDefinition)
        {
            var definitions = new List<TypeDefinition>();
            //First is always module so we will skip that;
            GetTypes(moduleDefinition.Types.Skip(1), definitions);
            return definitions;
        }

        private static void GetTypes(IEnumerable<TypeDefinition> typeDefinitions, List<TypeDefinition> definitions)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                GetTypes(typeDefinition.NestedTypes, definitions);
                definitions.Add(typeDefinition);
            }
        }

        public static IEnumerable<TypeReference> GetBaseTypes(this TypeDefinition type, bool includeIfaces)
        {
            var result = new List<TypeReference>();

            var current = type;

            var mappedFromSuperType = new List<TypeReference>();

            var previousGenericArgsMap = GetGenericArgsMap(type, new Dictionary<string, TypeReference>(), mappedFromSuperType);

            do
            {
                var currentBase = current.BaseType;
                if (currentBase is null)
                {
                    break;
                }

                if (currentBase is GenericInstanceType instanceType)
                {
                    previousGenericArgsMap = GetGenericArgsMap(current.BaseType, previousGenericArgsMap, mappedFromSuperType);

                    if (mappedFromSuperType.Any())
                    {
                        currentBase = instanceType.ElementType.MakeGenericInstanceType(previousGenericArgsMap.Select(x => x.Value).ToArray()).Import();
                        mappedFromSuperType.Clear();
                    }
                }
                else
                {
                    previousGenericArgsMap = new Dictionary<string, TypeReference>();
                }

                result.Add(currentBase);

                current = current.BaseType.Resolve();

                if (includeIfaces)
                {
                    var interfaces = BuildInterfaces(current, previousGenericArgsMap);
                    result.AddRange(interfaces);
                }
            } while (current.BaseType is not null);

            return result;
        }

        private static IEnumerable<TypeReference> BuildInterfaces(TypeDefinition type, IDictionary<string, TypeReference> genericArgsMap)
        {
            var mappedFromSuperType = new List<TypeReference>();

            // We need to map any generic parameters of the implementing type, for example when we want to build such interfaces:
            // KeyValuePair<TKey, TValue> : ICustomKey<TKey>, IEquatable<KeyValuePair<TKey, TValue>>
            var fullTypeArgsMap = GetGenericArgsMap(type, genericArgsMap, mappedFromSuperType);

            foreach (var item in fullTypeArgsMap)
            {
                if (!genericArgsMap.ContainsKey(item.Key))
                {
                    genericArgsMap[item.Key] = item.Value;
                }
            }

            foreach (var iface in type.Interfaces)
            {
                var result = iface.InterfaceType;

                try
                {
                    if (iface.InterfaceType is GenericInstanceType genericIface)
                    {
                        var map = GetGenericArgsMap(genericIface, genericArgsMap, mappedFromSuperType);

                        if (mappedFromSuperType.Any())
                        {
                            var genericInstance = genericIface.ElementType.MakeGenericInstanceType(map.Select(x => x.Value).ToArray());
                            result = genericInstance.Import();
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }

                if (result is not null)
                {
                    yield return result;
                }
            }
        }

        private static IDictionary<string, TypeReference> GetGenericArgsMap(TypeReference type, IDictionary<string, TypeReference> superTypeMap,
                                                                            IList<TypeReference> mappedFromSuperType)
        {
            var result = new Dictionary<string, TypeReference>();

            if (type is GenericInstanceType == false)
            {
                return result;
            }

            var genericArgs = ((GenericInstanceType)type).GenericArguments;
            var genericParameters = ((GenericInstanceType)type).ElementType.Resolve().GenericParameters;

            /*

         * Now genericArgs contain concrete arguments for the generic
         * parameters (genericPars).
         *
         * However, these concrete arguments don't necessarily have
         * to be concrete TypeReferences, these may be referenced to
         * generic parameters from super type.
         *
         * Example:
         *
         *      Consider following hierarchy:
         *          StringMap<T> : Dictionary<string, T>
         *
         *          StringIntMap : StringMap<int>
         *
         *      What would happen if we walk up the hierarchy from StringIntMap:
         *          -> StringIntMap
         *              - here don't have any generic agrs or params for StringIntMap.
         *              - but when we resolve StringIntMap we get a
         *                  reference to the base class StringMap<int>,
         *          -> StringMap<int>
         *              - this reference will have one generic argument
         *                  System.Int32 and it's ElementType,
         *                which is StringMap<T>, has one generic argument 'T'.
         *              - therefore we need to remember mapping T to System.Int32
         *              - when we resolve this class we'll get StringMap<T> and it's base
         *              will be reference to Dictionary<string, T>
         *          -> Dictionary<string, T>
         *              - now *genericArgs* will be System.String and 'T'
         *              - genericPars will be TKey and TValue from Dictionary
         *                  declaration Dictionary<TKey, TValue>
         *              - we know that TKey is System.String and...
         *              - because we have remembered a mapping from T to
         *                  System.Int32 and now we see a mapping from TValue to T,
         *                  we know that TValue is System.Int32, which bring us to
         *                  conclusion that StringIntMap is instance of
         *          -> Dictionary<string, int>
         */

            for (var i = 0; i < genericArgs.Count; i++)
            {
                var arg = genericArgs[i];

                var param = genericParameters[i];

                if (arg is GenericParameter)
                {
                    if (superTypeMap.TryGetValue(arg.Name, out var mapping))
                    {
                        mappedFromSuperType.Add(mapping);

                        result.Add(param.Name, mapping);
                    }
                    //else
                    //{
                    //            throw new Exception(string.Format(
                    //"GetGenericArgsMap: A mapping from supertype was not found. " +
                    //"Program searched for generic argument of name {0} in supertype generic arguments map " +
                    //"as it should server as value form generic argument for generic parameter {1} in the type {2}",
                    //arg.Name,
                    //param.Name,
                    //type.FullName));
                    //}
                }
                else
                {
                    // Note: this could be generic, resolve first. This is to solve a case where we have a nested generic interface implementation:
                    // KeyValuePair<TKey, TValue> : ICustomKey<TKey>, IEquatable<KeyValuePair<TKey, TValue>> // Resolving KeyValuePair<TKey, TValue> of the IEquatable interface here
                    var finalArgument = arg;
                    if (finalArgument.IsGenericInstance)
                    {
                        var generic = finalArgument as GenericInstanceType;

                        var genericArguments = new TypeReference[generic.GenericArguments.Count];
                        var success = true;

                        for (var j = 0; j < genericArguments.Length; j++)
                        {
                            var genericArgument = generic.GenericArguments[j];
                            if (!superTypeMap.TryGetValue(genericArgument.Name, out var mappedValue))
                            {
                                success = false;
                                break;
                            }

                            genericArguments[j] = mappedValue;
                        }

                        if (!success)
                        {
                            // Ignore this type
                            continue;
                        }

                        finalArgument = generic.ElementType.MakeGenericInstanceType(genericArguments);
                    }

                    result.Add(param.Name, finalArgument);
                }
            }

            return result;
        }
    }
}
