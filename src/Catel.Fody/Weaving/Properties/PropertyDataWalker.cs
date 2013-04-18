// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyDataWalker.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class PropertyDataWalker
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public PropertyDataWalker(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelTypeNode> notifyNodes)
        {
            foreach (var node in notifyNodes)
            {
                foreach (var property in node.TypeDefinition.Properties)
                {
                    if (property.CustomAttributes.ContainsAttribute("PropertyChanged.DoNotNotifyAttribute"))
                    {
                        continue;
                    }

                    if (property.SetMethod == null)
                    {
                        continue;
                    }

                    if (property.SetMethod.IsStatic)
                    {
                        continue;
                    }

                    GetPropertyData(property, node);
                }
                Process(node.Nodes);
            }
        }

        private void GetPropertyData(PropertyDefinition propertyDefinition, CatelTypeNode node)
        {
            var backingFieldReference = node.Mappings.First(x => x.PropertyDefinition == propertyDefinition).FieldDefinition;
            if (node.GetValueInvoker == null || node.SetValueInvoker == null)
            {
                return;
            }

            node.PropertyDatas.Add(new PropertyData
                                       {
                                           BackingFieldReference = backingFieldReference,
                                           PropertyDefinition = propertyDefinition,
                                           ChangeCallbackReference = GetOnChangedNotification(propertyDefinition)
                                       });
        }

        private MethodReference GetOnChangedNotification(PropertyDefinition propertyDefinition)
        {
            string methodName = string.Format("On{0}Changed", propertyDefinition.Name);

            var changedMethod = (from method in propertyDefinition.DeclaringType.Methods
                                 where method.Name == methodName
                                 select method).FirstOrDefault();

            return changedMethod;
        }

        public void Execute()
        {
            Process(_catelTypeNodeBuilder.NotifyNodes);
        }
    }
}