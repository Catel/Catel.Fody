// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingFinder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class MappingFinder
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public MappingFinder(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelTypeNode> notifyNodes)
        {
            foreach (var node in notifyNodes)
            {
                var typeDefinition = node.TypeDefinition;
                node.Mappings = GetMappings(typeDefinition).ToList();
                Process(node.Nodes);
            }
        }

        public static IEnumerable<MemberMapping> GetMappings(TypeDefinition typeDefinition)
        {
            foreach (var property in typeDefinition.Properties)
            {
                var fieldDefinition = TryGetField(typeDefinition, property);
                yield return new MemberMapping
                                 {
                                     PropertyDefinition = property,
                                     FieldDefinition = fieldDefinition
                                 };
            }
        }

        private static FieldDefinition TryGetField(TypeDefinition typeDefinition, PropertyDefinition property)
        {
            var propertyName = property.Name;
            var fieldsWithSameType = typeDefinition.Fields.Where(x => x.DeclaringType == typeDefinition).ToList();
            foreach (var field in fieldsWithSameType)
            {
                //AutoProp
                if (field.Name == string.Format("<{0}>k__BackingField", propertyName))
                {
                    return field;
                }
            }

            foreach (var field in fieldsWithSameType)
            {
                //diffCase
                var upperPropertyName = propertyName.ToUpper();
                var fieldUpper = field.Name.ToUpper();
                if (fieldUpper == upperPropertyName)
                {
                    return field;
                }
                //underScore
                if (fieldUpper == "_" + upperPropertyName)
                {
                    return field;
                }
            }
            return GetSingleField(property);
        }

        private static FieldDefinition GetSingleField(PropertyDefinition property)
        {
            var fieldDefinition = GetSingleField(property, Code.Stfld, property.SetMethod);
            if (fieldDefinition != null)
            {
                return fieldDefinition;
            }
            return GetSingleField(property, Code.Ldfld, property.GetMethod);
        }

        private static FieldDefinition GetSingleField(PropertyDefinition property, Code code, MethodDefinition methodDefinition)
        {
            if (methodDefinition == null)
            {
                return null;
            }
            if (methodDefinition.Body == null)
            {
                return null;
            }
            FieldReference fieldReference = null;
            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode.Code == code)
                {
                    //if fieldReference is not null then we are at the second one
                    if (fieldReference != null)
                    {
                        return null;
                    }
                    var field = instruction.Operand as FieldReference;
                    if (field != null)
                    {
                        if (field.DeclaringType != property.DeclaringType)
                        {
                            continue;
                        }
                        if (field.FieldType != property.PropertyType)
                        {
                            continue;
                        }
                        fieldReference = field;
                    }
                }
            }
            if (fieldReference != null)
            {
                return fieldReference.Resolve();
            }
            return null;
        }

        public void Execute()
        {
            var notifyNodes = _catelTypeNodeBuilder.NotifyNodes;
            Process(notifyNodes);
        }
    }
}