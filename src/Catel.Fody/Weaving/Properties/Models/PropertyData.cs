// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyData.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Mono.Cecil;

public class PropertyData
{
    public FieldReference BackingFieldReference;
    public PropertyDefinition PropertyDefinition;
    public MethodReference ChangeCallbackReference;
}