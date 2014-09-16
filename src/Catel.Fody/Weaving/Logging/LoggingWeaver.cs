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
                FodyEnvironment.LogDebug(string.Format("Cannot weave ILog fields without a static constructor, ignoring type '{0}'", _type.FullName));
                return;
            }

            var body = staticConstructor.Body;
            body.SimplifyMacros();

            try
            {
                UpdateCallsToGetCurrentClassLogger(body);
            }
            catch (Exception ex)
            {
                FodyEnvironment.LogWarning(string.Format("Failed to update static log definition in '{0}', '{1}'",
                    _type.FullName, ex.Message));
            }

            body.OptimizeMacros();
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

            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                var methodReference = instruction.Operand as MethodReference;
                if (methodReference != null)
                {
                    FodyEnvironment.LogDebug(string.Format("Weaving auto log to specific log for '{0}'", type.FullName));

                    if (string.Equals(methodReference.Name, "GetCurrentClassLogger"))
                    {
                        // We have a possible match
                        var getLoggerMethod = GetGetLoggerMethod(methodReference.DeclaringType);
                        if (getLoggerMethod == null)
                        {
                            var point = instruction.SequencePoint ?? instruction.SequencePoint;

                            var message = string.Format("Cannot change method call for log '{0}', the GetLogger(type) method does not exist on the calling type (try to use LogManager.GetCurrentClassLogger())", type.FullName);

                            if (point != null)
                            {
                                FodyEnvironment.LogWarningPoint(message, point);
                            }
                            else
                            {
                                FodyEnvironment.LogWarning(message);
                            }
                            continue;
                        }

                        var getTypeFromHandle = type.Module.GetMethodAndImport("GetTypeFromHandle");

                        instructions.RemoveAt(i);

                        instructions.Insert(i, new[]
                        {
                            Instruction.Create(OpCodes.Ldtoken, type),
                            Instruction.Create(OpCodes.Call, getTypeFromHandle),
                            Instruction.Create(OpCodes.Call, type.Module.Import(getLoggerMethod))
                        });
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