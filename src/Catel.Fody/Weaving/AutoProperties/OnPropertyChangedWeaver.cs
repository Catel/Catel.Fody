namespace Catel.Fody.Weaving.AutoProperties
{
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    internal class OnPropertyChangedWeaver
    {
        #region Fields
        private readonly CatelType _catelType;

        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        #endregion

        #region Constructors
        public OnPropertyChangedWeaver(CatelType catelType, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _catelType = catelType;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }
        #endregion

        #region Methods
        public void Execute()
        {
            foreach (var propertyDefinition in _catelType.AllProperties)
            {
                AddOrUpdateOnPropertyChangedMethod(propertyDefinition);
            }
        }

        private void AddOrUpdateOnPropertyChangedMethod(PropertyDefinition property)
        {
            MethodReference getMethodReference = _catelType.TypeDefinition.Module.Import(_catelType.AdvancedPropertyChangedEventArgsType.GetProperty("PropertyName").Resolve().GetMethod);
            MethodReference stringEqualsMethodReference = _catelType.TypeDefinition.Module.Import(GetSystemObjectEqualsMethodReference(_catelType.TypeDefinition.Module));

            List<PropertyDefinition> dependentProperties = _catelType.GetDependentPropertiesFrom(property).ToList();

            if (dependentProperties.Count > 0)
            {
                MethodDefinition onPropertyChangedMethod = EnsureOnPropertyChangedMethod();
                int idx = onPropertyChangedMethod.Body.Instructions.ToList().FindLastIndex(instruction => instruction.OpCode == OpCodes.Ret);

                if (idx > -1)
                {
                    TypeReference booleanTypeReference = _catelType.TypeDefinition.Module.Import(_msCoreReferenceFinder.GetCoreTypeReference("Boolean"));
                    if (onPropertyChangedMethod.Body.Variables.ToList().FirstOrDefault(definition => definition.VariableType != booleanTypeReference) == null)
                    {
                        onPropertyChangedMethod.Body.Variables.Add(new VariableDefinition(booleanTypeReference));
                        onPropertyChangedMethod.Body.InitLocals = true;
                    }

                    foreach (PropertyDefinition propertyDefinition in dependentProperties)
                    {
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldarg_1));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Callvirt, getMethodReference));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldstr, property.Name));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Callvirt, stringEqualsMethodReference));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldc_I4_0));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ceq));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Stloc_0));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldloc_0));

                        int jmpIdx = idx++;
                        onPropertyChangedMethod.Body.Instructions.Insert(jmpIdx, Instruction.Create(OpCodes.Nop));

                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Nop));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldarg_0));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Ldstr, propertyDefinition.Name));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Call, _catelType.RaisePropertyChangedInvoker));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Nop));
                        onPropertyChangedMethod.Body.Instructions.Insert(idx++, Instruction.Create(OpCodes.Nop));

                        onPropertyChangedMethod.Body.Instructions[jmpIdx] = Instruction.Create(OpCodes.Brtrue_S, onPropertyChangedMethod.Body.Instructions[idx - 1]);
                    }
                }
            }
        }

        private static MethodReference GetSystemObjectEqualsMethodReference(ModuleDefinition moduleDefinition)
        {
            TypeReference typeReference = moduleDefinition.GetTypeReferences().Single(t => t.FullName == "System.String");

            TypeDefinition typeDefinition = typeReference.Resolve();

            MethodDefinition methodDefinition = typeDefinition.Methods.Single(m => m.Name == "Equals" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "String");

            return methodDefinition;
        }

        private MethodDefinition EnsureOnPropertyChangedMethod()
        {
            TypeDefinition type = _catelType.TypeDefinition;
            MethodDefinition methodDefinition = type.Methods.FirstOrDefault(definition => definition.Name == "OnPropertyChanged" && definition.HasParameters && definition.Parameters[0].ParameterType == _catelType.AdvancedPropertyChangedEventArgsType);
            if (methodDefinition == null)
            {
                TypeReference voidType = _msCoreReferenceFinder.GetCoreTypeReference("Void");

                methodDefinition = new MethodDefinition("OnPropertyChanged", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual, type.Module.Import(voidType));
                methodDefinition.Parameters.Add(new ParameterDefinition("e", ParameterAttributes.None, _catelType.AdvancedPropertyChangedEventArgsType));

                MethodBody body = methodDefinition.Body;

                body.SimplifyMacros();

                body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                body.Instructions.Add(Instruction.Create(OpCodes.Call, _catelType.BaseOnPropertyChangedInvoker));
                body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                body.OptimizeMacros();

                type.Methods.Add(methodDefinition);
            }

            return methodDefinition;
        }
        #endregion
    }
}