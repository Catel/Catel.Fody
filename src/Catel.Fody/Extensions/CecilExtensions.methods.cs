// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CecilExtensions.methods.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public static partial class CecilExtensions
    {
        public static MethodReference MakeGeneric(this MethodReference method, TypeReference declaringType)
        {
            var reference = new MethodReference(method.Name, method.ReturnType)
            {
                DeclaringType = declaringType.MakeGenericIfRequired(),
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention,
            };

            foreach (var parameter in method.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            return reference;
        }

        public static MethodReference GetMethodReference(this MethodDefinition methodDefinition, Stack<TypeDefinition> typeDefinitions)
        {
            var methodReference = FodyEnvironment.ModuleDefinition.Import(methodDefinition).GetGeneric();

            if (methodDefinition.IsStatic)
            {
                return methodReference;
            }

            typeDefinitions.Pop();
            while (typeDefinitions.Count > 0)
            {
                var definition = typeDefinitions.Pop();

                // Only call lower class if possible
                var containsMethod = (from method in definition.Methods
                                      where method.Name == methodDefinition.Name
                                      select method).Any();
                if (containsMethod)
                {
                    methodReference = methodReference.MakeGeneric(definition.BaseType);
                }
            }

            return methodReference;
        }
    }
}