// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggingWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.Logging
{
    using System;
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class LoggingWeaver
    {
        private const string CatelLoggingClass = "Catel.Logging.ILog";

        private readonly TypeDefinition _type;

        public LoggingWeaver(TypeDefinition type)
        {
            _type = type;
        }

        public void Execute()
        {
            var loggingFields = (from field in _type.Fields
                                 where field.IsStatic && string.Equals(field.FieldType.FullName, CatelLoggingClass)
                                 select field).ToList();

            if (loggingFields.Count == 0)
            {
                return;
            }

            var staticConstructor = _type.GetStaticConstructor();
            if (staticConstructor == null)
            {
                FodyEnvironment.LogInfo(string.Format("Cannot weave ILog fields without a static constructor, ignoring type '{0}'", _type.FullName));
                return;
            }

            var body = staticConstructor.Body;
            body.SimplifyMacros();

            foreach (var loggingField in loggingFields)
            {
                try
                {
                    UpdateStaticLogDefinition(loggingField, body);
                }
                catch (Exception ex)
                {
                    FodyEnvironment.LogWarning(string.Format("Failed to update static log definition '{0}.{1}', '{2}'",
                        _type.FullName, loggingField.Name, ex.Message));
                }
            }

            body.OptimizeMacros();
        }

        private void UpdateStaticLogDefinition(FieldDefinition logField, MethodBody ctorBody)
        {
            // Convert this:
            //
            // call class [Catel.Core]Catel.Logging.ILog [Catel.Core]Catel.Logging.LogManager::GetCurrentClassLogger()
            // stsfld class [Catel.Core]Catel.Logging.ILog Catel.Fody.TestAssembly.LoggingClass::AutoLog
            //
            // into this:
            //
            // ldtoken Catel.Fody.TestAssembly.LoggingClass
            // call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
            // call class [Catel.Core]Catel.Logging.ILog [Catel.Core]Catel.Logging.LogManager::GetLogger(class [mscorlib]System.Type)
            // stsfld class [Catel.Core]Catel.Logging.ILog Catel.Fody.TestAssembly.LoggingClass::ManualLog

            var type = logField.DeclaringType;
            var instructions = ctorBody.Instructions;

            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (instruction.OpCode == OpCodes.Stsfld)
                {
                    if (instruction.Operand == logField)
                    {
                        var logFieldName = string.Format("{0}.{1}", type.FullName, logField.Name);

                        FodyEnvironment.LogInfo(string.Format("Weaving auto log to specific log for '{0}'", logFieldName));

                        var previousInstruction = instructions[i - 1];
                        var getCurrentClassLoggerMethod = (MethodReference)previousInstruction.Operand;
                        if (!string.Equals(getCurrentClassLoggerMethod.Name, "GetCurrentClassLogger"))
                        {
                            // This is already a manual logging
                            return;
                        }

                        var getLoggerMethod = GetGetLoggerMethod(getCurrentClassLoggerMethod.DeclaringType);
                        if (getLoggerMethod == null)
                        {
                            FodyEnvironment.LogWarningPoint(string.Format("Cannot change method call for log '{0}', the GetLogger(type) method does not exist on the calling type (try to use LogManager.GetCurrentClassLogger())", logFieldName), previousInstruction.SequencePoint);
                            return;
                        }

                        var getTypeFromHandle = type.Module.GetMethodAndImport("GetTypeFromHandle");

                        instructions.Insert(i, new []
                        {
                            Instruction.Create(OpCodes.Ldtoken, type),
                            Instruction.Create(OpCodes.Call, getTypeFromHandle),
                            Instruction.Create(OpCodes.Call, type.Module.Import(getLoggerMethod))
                        });

                        instructions.RemoveAt(i - 1);
                        return;
                    }
                }
            }
        }

        private MethodDefinition GetGetLoggerMethod(TypeReference logManagerType)
        {
            var typeDefinition = logManagerType.Resolve();

            return (from method in typeDefinition.Methods
                    where method.IsStatic && string.Equals(method.Name, "GetLogger") && method.Parameters.Count == 1
                    select method).FirstOrDefault();
        }
    }
}