// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveArgumentWeavingCallResult.cs" company="Catel development team">
//   Copyright (c) 2008 - 2016 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.Argument.Models
{
    using Mono.Cecil;

    internal class RemoveArgumentWeavingCallResult
    {
        public RemoveArgumentWeavingCallResult(TypeDefinition displayClassTypeDefinition, int index, bool hasConstructorCall)
        {
            DisplayClassTypeDefinition = displayClassTypeDefinition;
            Index = index;
            HasConstructorCall = hasConstructorCall;
        }

        public TypeDefinition DisplayClassTypeDefinition { get; private set; }

        public int Index { get; private set; }

        public bool HasConstructorCall { get; private set; }
    }
}