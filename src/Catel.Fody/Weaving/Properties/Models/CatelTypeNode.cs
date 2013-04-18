// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatelTypeNode.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Mono.Cecil;

public class CatelTypeNode
{
    public CatelTypeNode()
    {
        Nodes = new List<CatelTypeNode>();
        Mappings = new List<MemberMapping>();
        PropertyDatas = new List<PropertyData>();
    }

    public TypeDefinition TypeDefinition;
    public List<CatelTypeNode> Nodes;
    public List<MemberMapping> Mappings;

    public TypeReference PropertyDataType;

    public MethodReference RegisterPropertyInvoker;
    public MethodReference SetValueInvoker;
    public MethodReference GetValueInvoker;

    public List<PropertyData> PropertyDatas;
    public List<PropertyDefinition> AllProperties;
}