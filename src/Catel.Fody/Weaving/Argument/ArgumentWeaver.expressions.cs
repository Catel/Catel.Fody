// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgumentWeaver.expressions.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.Argument
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Mono.Collections.Generic;

    public partial class ArgumentWeaver
    {
        private bool IsSupportedExpressionArgumentCheck(Instruction instruction)
        {
            var methodBeingCalled = instruction.Operand as MethodReference;
            if (methodBeingCalled != null)
            {
                if (methodBeingCalled.DeclaringType.FullName.Contains("Catel.Argument"))
                {
                    var finalKey = methodBeingCalled.GetFullName();
                    if (!ArgumentMethodCallWeaverBase.WellKnownWeavers.ContainsKey(finalKey))
                    {
                        return false;
                    }

                    var firstParameter = methodBeingCalled.Parameters.FirstOrDefault();
                    if (firstParameter != null)
                    {
                        if (firstParameter.ParameterType.FullName.Contains("System.Linq.Expressions."))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void RemoveObsoleteCodeForArgumentExpression(MethodDefinition method, Collection<Instruction> instructions, TypeDefinition displayClassType)
        {
            if (method.DeclaringType.NestedTypes.Contains(displayClassType))
            {
                method.DeclaringType.NestedTypes.Remove(displayClassType);
            }

            // Remove display class - variables
            for (var i = 0; i < method.Body.Variables.Count; i++)
            {
                var variable = method.Body.Variables[i];
                if (string.Equals(variable.VariableType.Name, displayClassType.Name))
                {
                    method.Body.Variables.RemoveAt(i--);
                }
            }

            // Remove display class creation
            //L_0000: newobj instance void Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass1a::.ctor()
            //L_0005: stloc.0 

            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];
                if (innerInstruction.OpCode == OpCodes.Newobj)
                {
                    if (string.Equals(((MethodDefinition) innerInstruction.Operand).DeclaringType.Name, displayClassType.Name))
                    {
                        // Delete 2 instructions, same location since remove will move everything 1 place up
                        instructions.RemoveAt(i);
                        instructions.RemoveAt(i);

                        break;
                    }
                }
            }

            // Remove display class allocation
            //L_0014: ldloc.0 
            //L_0015: ldarg.3 
            //L_0016: stfld object Catel.Fody.TestAssembly.ArgumentChecksAsExpressionsClass/<>c__DisplayClass28::myObject3

            for (var i = 0; i < instructions.Count; i++)
            {
                var innerInstruction = instructions[i];

                if (innerInstruction.OpCode == OpCodes.Stfld)
                {
                    if (string.Equals(((FieldDefinition) innerInstruction.Operand).DeclaringType.Name, displayClassType.Name))
                    {
                        // Remove the previous 3 operations
                        instructions.RemoveAt(i);
                        instructions.RemoveAt(i - 1);
                        instructions.RemoveAt(i - 2);

                        break;
                    }
                }
            }
        }

        private TypeDefinition RemoveArgumentWeavingCall(MethodDefinition method, Collection<Instruction> instructions, Instruction instruction)
        {
            TypeReference displayClassType = null;

            var index = instructions.IndexOf(instruction);
            for (var i = index; i >= 0; i--)
            {
                // Remove everything until the first ldloc.0 call
                var innerInstruction = instructions[i];

                instructions.RemoveAt(i);

                if (innerInstruction.OpCode == OpCodes.Ldtoken)
                {
                    if (displayClassType == null)
                    {
                        // First call to ldtoken with FieldDefinition contains the display class type
                        var fieldDefinition = innerInstruction.Operand as FieldDefinition;
                        if (fieldDefinition != null)
                        {
                            displayClassType = fieldDefinition.DeclaringType;
                        }
                    }
                }

                if ((innerInstruction.OpCode == OpCodes.Ldloc_0) ||
                    ((innerInstruction.OpCode == OpCodes.Ldloc) && (innerInstruction.Operand.ToString() == "V_0")))
                {
                    break;
                }
            }

            return displayClassType.Resolve();
        }

        private ParameterDefinition GetParameterForExpressionArgumentCheck(MethodDefinition method, Collection<Instruction> instructions, Instruction instruction)
        {
            TypeReference displayClassType = null;
            FieldDefinition parameterFieldDefinition = null;

            // This logic is used for finding the parameter:

            // 1) Find the display class
            // 2) Find the field used
            // 3) Check with which parameter the field is assigned

            var index = instructions.IndexOf(instruction);
            for (var i = index; i >= 0; i--)
            {
                var innerInstruction = instructions[i];

                if (innerInstruction.OpCode == OpCodes.Ldtoken)
                {
                    if (displayClassType == null)
                    {
                        // First call to ldtoken with FieldDefinition contains the display class type
                        var fieldDefinition = innerInstruction.Operand as FieldDefinition;
                        if (fieldDefinition != null)
                        {
                            parameterFieldDefinition = fieldDefinition;
                            displayClassType = fieldDefinition.DeclaringType;
                        }
                    }
                }

                if (innerInstruction.OpCode == OpCodes.Stfld)
                {
                    if (innerInstruction.Operand == parameterFieldDefinition)
                    {
                        // We found it, now check 1 instruction back for the actual parameter
                        var finalInstruction = instructions[i - 1];
                        return (ParameterDefinition)finalInstruction.Operand;
                    }
                }
            }

            return null;
        }

        private CustomAttribute CreateAttributeForExpressionArgumentCheck(MethodDefinition method, Collection<Instruction> instructions, Instruction instruction)
        {
            var fullMethodName = ((MethodReference)instruction.Operand).GetFullName();

            switch (fullMethodName)
            {
                case "Catel.Argument.IsNotNull":
                    return CreateCustomAttribute("Catel.Fody.NotNullAttribute");

                case "Catel.Argument.IsNotNullOrEmpty":
                    return CreateCustomAttribute("Catel.Fody.NotNullOrEmptyAttribute");

                case "Catel.Argument.IsNotNullOrWhitespace":
                    return CreateCustomAttribute("Catel.Fody.NotNullOrWhitespaceAttribute");
            }

            return null;
        }

        private CustomAttribute CreateCustomAttribute(string attributeTypeName, params object[] parameters)
        {
            var typeDefinition = _typeDefinition.Module.FindType("Catel.Fody.Attributes", attributeTypeName);

            var typeList = new List<TypeDefinition>();
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.GetType();
                if (parameterType.IsClass)
                {
                    parameterType = typeof(object);
                }

                typeList.Add(_msCoreReferenceFinder.GetCoreTypeReference(parameterType.FullName).Resolve());
            }

            var constructor = typeDefinition.FindConstructor(typeList).Resolve();
            var attribute = new CustomAttribute(constructor);
            for (var i = 0; i < constructor.Parameters.Count; i++)
            {
                attribute.ConstructorArguments.Add(new CustomAttributeArgument(typeList[i], parameters[i]));
            }

            return attribute;
        }
    }
}