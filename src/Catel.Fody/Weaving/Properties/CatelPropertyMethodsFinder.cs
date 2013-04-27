// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelPropertyMethodsFinder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Properties
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class CatelPropertyMethodsFinder
    {
        private readonly MethodGenerifier _methodGenerifier;
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public CatelPropertyMethodsFinder(MethodGenerifier methodGenerifier, CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _methodGenerifier = methodGenerifier;
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void ProcessChildNode(CatelTypeNode node)
        {
            var module = node.TypeDefinition.Module;

            node.RegisterPropertyInvoker = module.Import(FindRegisterPropertyMethod(node.TypeDefinition));
            node.GetValueInvoker = module.Import(RecursiveFindMethod(node.TypeDefinition, "GetValue", true).GetGeneric());
            node.SetValueInvoker = module.Import(RecursiveFindMethod(node.TypeDefinition, "SetValue"));
            node.PropertyDataType = module.Import(node.TypeDefinition.Module.FindType("Catel.Core", "PropertyData"));

            foreach (var childNode in node.Nodes)
            {
                ProcessChildNode(childNode);
            }
        }

        private MethodReference FindRegisterPropertyMethod(TypeDefinition typeDefinition)
        {
            var typeDefinitions = new Stack<TypeDefinition>();
            MethodDefinition methodDefinition;
            var currentTypeDefinition = typeDefinition;

            do
            {
                typeDefinitions.Push(currentTypeDefinition);

                var methods = (from method in currentTypeDefinition.Methods
                               where method.Name == "RegisterProperty" && method.IsPublic
                               select method).ToList();

                if (methods.Count > 0)
                {
                    // We now we have to use the last one
                    methodDefinition = methods[methods.Count - 1];
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

        private MethodReference RecursiveFindMethod(TypeDefinition typeDefinition, string methodName, bool findGenericDefinition = false)
        {
            var typeDefinitions = new Stack<TypeDefinition>();
            MethodDefinition methodDefinition;
            var currentTypeDefinition = typeDefinition;

            do
            {
                typeDefinitions.Push(currentTypeDefinition);

                if (FindMethodDefinition(currentTypeDefinition, methodName, findGenericDefinition, out methodDefinition))
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

            return _methodGenerifier.GetMethodReference(typeDefinitions, methodDefinition);
        }

        private bool FindMethodDefinition(TypeDefinition type, string methodName, bool findGenericDefinition, out MethodDefinition methodDefinition)
        {
            if (!findGenericDefinition)
            {
                methodDefinition = type.Methods
                                       .Where(x => x.Name == methodName)
                                       .OrderBy(definition => definition.Parameters.Count)
                                       .FirstOrDefault();
            }
            else
            {
                methodDefinition = (from method in type.Methods
                                    where method.Name == methodName &&
                                          method.HasGenericParameters
                                    select method).FirstOrDefault();
            }

            return methodDefinition != null;
        }

        public void Execute()
        {
            foreach (var notifyNode in _catelTypeNodeBuilder.NotifyNodes)
            {
                ProcessChildNode(notifyNode);
            }
        }
    }
}