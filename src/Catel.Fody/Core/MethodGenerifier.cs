// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodGenerifier.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    public class MethodGenerifier
    {
        private readonly ModuleWeaver _moduleWeaver;

        public MethodGenerifier(ModuleWeaver moduleWeaver)
        {
            _moduleWeaver = moduleWeaver;
        }

        public MethodReference GetMethodReference(Stack<TypeDefinition> typeDefinitions, MethodDefinition methodDefinition)
        {
            var methodReference = _moduleWeaver.ModuleDefinition.Import(methodDefinition).GetGeneric();

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
                    methodReference = MakeGeneric(definition.BaseType, methodReference);
                }
            }
            return methodReference;
        }

        public static MethodReference MakeGeneric(TypeReference declaringType, MethodReference self)
        {
            var reference = new MethodReference(self.Name, self.ReturnType)
                                {
                                    DeclaringType = declaringType,
                                    HasThis = self.HasThis,
                                    ExplicitThis = self.ExplicitThis,
                                    CallingConvention = self.CallingConvention,
                                };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            return reference;
        }
    }
}