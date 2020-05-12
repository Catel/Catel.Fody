namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public abstract class PropertyWeaverBase
    {
        protected readonly Dictionary<string, string> _cachedFieldInitializerNames = new Dictionary<string, string>();
        protected readonly Dictionary<string, string> _cachedFieldNames = new Dictionary<string, string>();

        protected readonly CatelType _catelType;
        protected readonly CatelTypeProperty _propertyData;
        protected readonly ModuleWeaver _moduleWeaver;
        protected readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public PropertyWeaverBase(CatelType catelType, CatelTypeProperty propertyData, ModuleWeaver moduleWeaver,
            MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _catelType = catelType;
            _propertyData = propertyData;
            _moduleWeaver = moduleWeaver;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        protected string GetChangeNotificationHandlerFieldName(PropertyDefinition property)
        {
            var key = $"{property.DeclaringType.FullName}|{property.Name}";
            if (_cachedFieldNames.ContainsKey(key))
            {
                return _cachedFieldNames[key];
            }

            var counter = 2; // start at 2
            while (true)
            {
                var fieldName = $"CS$<>9__CachedAnonymousMethodDelegate{counter}";
                if (GetFieldDefinition(property.DeclaringType, fieldName, false) is null)
                {
                    _cachedFieldNames[key] = fieldName;
                    return fieldName;
                }

                counter++;
            }
        }

        protected string GetBackingFieldName(PropertyDefinition property)
        {
            return $"<{property.Name}>k__BackingField";
        }

        protected GenericInstanceType GetEventHandlerAdvancedPropertyChangedEventArgs(PropertyDefinition property)
        {
            var genericHandlerType = _msCoreReferenceFinder.GetCoreTypeReference("System.EventHandler`1");
            if (genericHandlerType is null)
            {
                FodyEnvironment.WriteError("Expected to find EventHandler<T>, but type was not  found");
                return null;
            }

            var advancedPropertyChangedEventArgsType = property.Module.FindType("Catel.Core", "Catel.Data.AdvancedPropertyChangedEventArgs");

            var handlerType = new GenericInstanceType(genericHandlerType);
            handlerType.GenericArguments.Add(advancedPropertyChangedEventArgsType);

            return handlerType;
        }

        protected bool HasBackingField(PropertyDefinition property)
        {
            var fieldName = GetBackingFieldName(property);

            var field = GetFieldReference(property.DeclaringType, fieldName, false);
            return (field != null);
        }

        protected static FieldDefinition GetFieldDefinition(TypeDefinition declaringType, string fieldName, bool allowGenericResolving)
        {
            var fieldReference = GetFieldReference(declaringType, fieldName, allowGenericResolving);
            return fieldReference != null ? fieldReference.Resolve() : null;
        }

        protected static FieldReference GetFieldReference(TypeDefinition declaringType, string fieldName, bool allowGenericResolving)
        {
            var field = (from x in declaringType.Fields
                         where x.Name == fieldName
                         select x).FirstOrDefault();
            if (field is null)
            {
                return null;
            }

            FieldReference fieldReference = field;

            if (declaringType.HasGenericParameters && allowGenericResolving)
            {
                fieldReference = field.MakeGeneric(declaringType);
            }

            return fieldReference;
        }

        protected static MethodReference GetMethodReference(TypeDefinition declaringType, string methodName, bool allowGenericResolving)
        {
            var method = (from x in declaringType.Methods
                          where x.Name == methodName
                          select x).FirstOrDefault();
            if (method is null)
            {
                return null;
            }

            MethodReference methodReference = method;

            if (declaringType.HasGenericParameters && allowGenericResolving)
            {
                methodReference = method.MakeGeneric(declaringType);
            }

            return methodReference;
        }
    }
}
