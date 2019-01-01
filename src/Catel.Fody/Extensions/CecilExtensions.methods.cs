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
    using Mono.Cecil.Cil;

    public static partial class CecilExtensions
    {
        public static bool IsAsyncMethod(this MethodDefinition method)
        {
            if (!method.Name.Contains("MoveNext"))
            {
                return false;
            }

            var declaringType = method.DeclaringType;

            var setStateMachineMethod = declaringType?.Methods.FirstOrDefault(x => x.Name.Equals("SetStateMachine"));
            if (setStateMachineMethod == null)
            {
                return false;
            }

            return true;
        }

        public static int FindBaseConstructorIndex(this MethodDefinition method)
        {
            var declaringType = method.DeclaringType;
            var baseType = declaringType?.BaseType;
            if (baseType != null)
            {
                var instructions = method.Body.Instructions;

                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    if (instruction.IsOpCode(OpCodes.Call))
                    {
                        if (instruction.Operand is MethodReference methodReference)
                        {
                            if (methodReference.Name.Equals(".ctor"))
                            {
                                var ctorDeclaringType = methodReference.DeclaringType?.FullName;
                                if (baseType.FullName.Equals(ctorDeclaringType))
                                {
                                    return i;
                                }
                            }
                        }
                    }
                }
            }

            return -1;
        }

        public static SequencePoint GetFirstSequencePoint(this MethodDefinition method)
        {
            return method.DebugInformation.SequencePoints.FirstOrDefault();
        }

        public static SequencePoint GetSequencePoint(this MethodDefinition method, Instruction instruction)
        {
            var debugInfo = method.DebugInformation;
            return debugInfo.GetSequencePoint(instruction);
        }

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
            var methodReference = FodyEnvironment.ModuleDefinition.ImportReference(methodDefinition).GetGeneric();

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
