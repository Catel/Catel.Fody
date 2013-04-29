// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeNodeBuilder.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;

    public class CatelTypeNodeBuilder
    {
        private readonly ModuleWeaver _moduleWeaver;

        private readonly List<TypeDefinition> _allClasses;

        private ModuleDefinition _moduleDefinition;

        public List<CatelTypeNode> Nodes;

        public List<CatelTypeNode> NotifyNodes;

        public CatelTypeNodeBuilder(ModuleWeaver moduleWeaver, List<TypeDefinition> allTypes)
        {
            _moduleWeaver = moduleWeaver;

            _allClasses = allTypes.Where(x => x.IsClass).ToList();
        }

        public void Execute()
        {
            _moduleDefinition = _moduleWeaver.ModuleDefinition;
            Nodes = new List<CatelTypeNode>();
            NotifyNodes = new List<CatelTypeNode>();

            // allClasses = new List<TypeDefinition>();
            WalkAllTypeDefinitions();

            foreach (var typeDefinition in _allClasses.ToList())
            {
                AddClass(typeDefinition);
            }

            var typeNodes = Nodes;
            PopulateCatelModelNodes(typeNodes);
        }

        private void PopulateCatelModelNodes(List<CatelTypeNode> typeNodes)
        {
            foreach (var node in typeNodes)
            {
                if (node.TypeDefinition.ImplementsCatelModel())
                {
                    NotifyNodes.Add(node);
                    continue;
                }

                PopulateCatelModelNodes(node.Nodes);
            }
        }

        private CatelTypeNode AddClass(TypeDefinition typeDefinition)
        {
            if (typeDefinition == null)
            {
                return null;
            }

            _allClasses.Remove(typeDefinition);
            var typeNode = new CatelTypeNode { TypeDefinition = typeDefinition };

            if (typeDefinition.BaseType == null)
            {
                return null;
            }

            if (Nodes.Contains(typeNode))
            {
                return typeNode;
            }

            if (typeDefinition.BaseType.Scope.Name != _moduleDefinition.Name)
            {
                Nodes.Add(typeNode);
            }
            else
            {
                var baseType = typeDefinition.BaseType.ResolveType();
                var parentNode = FindClassNode(baseType, Nodes);
                if (parentNode == null)
                {
                    parentNode = AddClass(baseType);
                }

                parentNode.Nodes.Add(typeNode);
            }

            return typeNode;
        }

        private CatelTypeNode FindClassNode(TypeDefinition type, IEnumerable<CatelTypeNode> typeNode)
        {
            foreach (var node in typeNode)
            {
                if (type == node.TypeDefinition)
                {
                    return node;
                }

                var findNode = FindClassNode(type, node.Nodes);
                if (findNode != null)
                {
                    return findNode;
                }
            }

            return null;
        }

        public void WalkAllTypeDefinitions()
        {
            // First is always module so we will skip that;
            GetTypes(_moduleDefinition.Types.Skip(1));
        }

        private void GetTypes(IEnumerable<TypeDefinition> typeDefinitions)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                GetTypes(typeDefinition.NestedTypes);

                if (typeDefinition.IsClass)
                {
                    _allClasses.Add(typeDefinition);
                }
            }
        }
    }
}