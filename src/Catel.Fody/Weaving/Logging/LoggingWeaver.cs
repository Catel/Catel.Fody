﻿namespace Catel.Fody.Weaving.Logging
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
            if (staticConstructor is null)
            {
                FodyEnvironment.WriteWarning($"Cannot weave ILog fields without a static constructor, ignoring type '{_type.FullName}'");
                return;
            }

            FodyEnvironment.WriteDebug($"\tExecuting '{GetType().Name}' for '{_type.FullName}'");

            var body = staticConstructor.Body;
            body.SimplifyMacros();

            try
            {
                UpdateCallsToGetCurrentClassLogger(body);
            }
            catch (Exception ex)
            {
                FodyEnvironment.WriteWarning($"Failed to update static log definition in '{_type.FullName}', '{ex.Message}'");
            }

            body.OptimizeMacros();
            staticConstructor.UpdateDebugInfo();
        }

        private void UpdateCallsToGetCurrentClassLogger(MethodBody ctorBody)
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

            var type = ctorBody.Method.DeclaringType;
            var instructions = ctorBody.Instructions;

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                if (instruction.Operand is MethodReference methodReference)
                {
                    if (string.Equals(methodReference.Name, "GetCurrentClassLogger"))
                    {
                        // We have a possible match
                        var getLoggerMethod = GetGetLoggerMethod(methodReference.DeclaringType);
                        if (getLoggerMethod is null)
                        {
                            var point = methodReference.Resolve().GetSequencePoint(instruction);

                            var message = $"Cannot change method call for log '{type.FullName}', the GetLogger(type) method does not exist on the calling type (try to use LogManager.GetCurrentClassLogger())";

                            if (point is not null)
                            {
                                FodyEnvironment.WriteWarningPoint(message, point);
                            }
                            else
                            {
                                FodyEnvironment.WriteWarning(message);
                            }
                            continue;
                        }

                        FodyEnvironment.WriteDebug($"Weaving auto log to specific log for '{type.FullName}'");

                        var getTypeFromHandle = type.Module.GetMethodAndImport("GetTypeFromHandle");

                        instructions.RemoveAt(i);

                        instructions.Insert(i,
                            Instruction.Create(OpCodes.Ldtoken, type),
                            Instruction.Create(OpCodes.Call, getTypeFromHandle),
                            Instruction.Create(OpCodes.Call, type.Module.ImportReference(getLoggerMethod)));
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
