namespace Catel.Fody
{
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

#if DEBUG
    using System.Diagnostics;
#endif

    public class ObservableObjectPropertyWeaver : PropertyWeaverBase
    {
        public ObservableObjectPropertyWeaver(CatelType catelType, CatelTypeProperty propertyData, ModuleWeaver moduleWeaver,
            MsCoreReferenceFinder msCoreReferenceFinder)
            : base(catelType, propertyData, moduleWeaver, msCoreReferenceFinder)
        {
        }

        public void Execute(bool force = false)
        {
            var property = _propertyData.PropertyDefinition;
            if (property is null)
            {
                FodyEnvironment.WriteWarning("Skipping an unknown property because it has no property definition");
                return;
            }

            if (!force && !HasBackingField(property))
            {
                FodyEnvironment.WriteDebug($"\t\tSkipping '{property.GetName()}' because it has no backing field");
                return;
            }

            if (!IsCleanSetter(property))
            {
                FodyEnvironment.WriteDebug($"\t\tSkipping '{property.GetName()}' because it has no clean setter (custom implementation?)");
                return;
            }

            FodyEnvironment.WriteDebug("\t\t" + property.Name);

            try
            {
                UpdateSetValueCall(property);
            }
            catch (Exception ex)
            {
                FodyEnvironment.WriteError($"\t\tFailed to handle property '{property.DeclaringType.Name}.{property.Name}'\n{ex.Message}\n{ex.StackTrace}");

#if DEBUG
                Debugger.Launch();
#endif
            }
        }

        private bool IsCleanSetter(PropertyDefinition property)
        {
            // A clean setter looks like this:
            // IL_0000: ldarg.0      // this
            // IL_0001: ldarg.1      // 'value'
            // IL_0002: stfld        bool Catel.Fody.TestAssembly.ObservableObjectTest_Expected/*0200004A*/::_onLastNameChangedCallbackCalled/*040000B7*/
            // IL_0007: ret

            var setter = property.SetMethod;
            if (setter is null)
            {
                return false;
            }

            var instructions = setter.Body.Instructions;

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                if (instruction.IsOpCode(OpCodes.Nop, OpCodes.Ret) ||
                    instruction.IsOpCode(OpCodes.Ldarg, OpCodes.Ldarg_0, OpCodes.Ldarg_1) ||
                    instruction.IsOpCode(OpCodes.Stfld))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void UpdateSetValueCall(PropertyDefinition property)
        {
            // We want to change this:
            //
            // set
            // {
            //     _myField = value;
            // }
            //
            // to
            // set
            // {
            //     _myField = value;
            //     OnMyFieldChanged();                     // Step 1, optional!
            //     RaisePropertyChanged(nameof(MyField));  // Step 2
            // }

            var newInstructions = new List<Instruction>();

            // Step 1: OnMyFieldChanged callbacks
            //
            // IL_0007: ldarg.0      // this
            // IL_0008: call instance void Catel.Fody.TestAssembly.ObservableObjectTest_Expected/*0200004A*/::OnFirstNameChanged()/*060001C0*/

            var changeCallbackReference = _propertyData.ChangeCallbackReference;
            if (changeCallbackReference != null)
            {
                newInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                newInstructions.Add(Instruction.Create(OpCodes.Call, changeCallbackReference));
            }

            // Step 2: RaisePropertyChanged
            //
            // IL_0007: ldarg.0      // this
            // IL_0008: ldstr        "ExistingProperty"
            // IL_000d: call instance void [Catel.Core/*23000003*/]Catel.Data.ObservableObject/*01000046*/::RaisePropertyChanged(string)/*0A000067*/

            newInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            newInstructions.Add(Instruction.Create(OpCodes.Ldstr, property.Name));
            newInstructions.Add(Instruction.Create(OpCodes.Call, _catelType.RaisePropertyChangedInvoker));

            // Final step: inject the instructions
            var body = property.SetMethod.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;

            var instructionIndex = instructions.Count - 1;

            for (var i = instructionIndex; i >= 0; i++)
            {
                var instruction = instructions[i];
                if (instruction.IsOpCode(OpCodes.Ret))
                {
                    break;
                }

                instructionIndex--;
            }

            instructions.Insert(instructionIndex, newInstructions);

            body.OptimizeMacros();
        }
    }
}
