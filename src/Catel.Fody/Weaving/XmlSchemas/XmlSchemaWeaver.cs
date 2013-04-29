// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlSchemaWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.XmlSchemas
{
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public class XmlSchemaWeaver
    {
        private readonly ModuleWeaver _moduleWeaver;
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public XmlSchemaWeaver(ModuleWeaver moduleWeaver, MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _moduleWeaver = moduleWeaver;
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute(CatelTypeNode catelTypeNode)
        {
            if (catelTypeNode.TypeDefinition.IsAbstract)
            {
                return;
            }

            if (catelTypeNode.TypeDefinition.IsEnum)
            {
                return;
            }

            if (!catelTypeNode.TypeDefinition.ImplementsCatelModel())
            {
                return;
            }

            _moduleWeaver.LogInfo("\t\t Adding xml schema for type " + catelTypeNode.TypeDefinition.FullName);

            if (AddXmlSchemaProviderAttribute(catelTypeNode))
            {
                AddGetXmlSchemaMethod(catelTypeNode);
            }

            foreach (var childNode in catelTypeNode.Nodes)
            {
                Execute(childNode);
            }
        }

        private bool AddXmlSchemaProviderAttribute(CatelTypeNode catelTypeNode)
        {
            var catelType = catelTypeNode.TypeDefinition;

            var methodName = GetXmlSchemaMethodName(catelTypeNode);

            var existingCustomAttribute = (from attribute in catelType.CustomAttributes
                                           where attribute.AttributeType.Name == "XmlSchemaProviderAttribute"
                                           select attribute).FirstOrDefault();
            if (existingCustomAttribute != null)
            {
                var constructorArgument = existingCustomAttribute.ConstructorArguments[0];
                if (string.Equals(constructorArgument.Value, methodName))
                {
                    return false;
                }
            }

            var xmlSchemaProviderAttribute = catelType.Module.FindType("System.Xml", "System.Xml.Serialization.XmlSchemaProviderAttribute");

            var attributeConstructor = catelType.Module.Import(xmlSchemaProviderAttribute.Resolve().Constructor(false));
            var customAttribute = new CustomAttribute(attributeConstructor);
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(catelType.Module.TypeSystem.String, methodName));

            catelType.CustomAttributes.Add(customAttribute);

            return true;
        }

        private void AddGetXmlSchemaMethod(CatelTypeNode catelTypeNode)
        {
            var catelType = catelTypeNode.TypeDefinition;

            var methodName = GetXmlSchemaMethodName(catelTypeNode);

            var alreadyHandled = (from method in catelType.Methods
                                  where method.Name == methodName
                                  select method).Any();
            if (alreadyHandled)
            {
                return;
            }

            var getTypeFromHandle = catelType.Module.GetMethod("GetTypeFromHandle");
            var importedGetTypeFromHandle = catelType.Module.Import(getTypeFromHandle);

            var xmlSchemaManager = (TypeDefinition)catelType.Module.FindType("Catel.Core", "Catel.Runtime.Serialization.XmlSchemaManager");
            var getXmlSchemaMethodOnXmlSchemaManager = catelType.Module.Import(xmlSchemaManager.Methods.First(x => x.IsStatic && x.Name == "GetXmlSchema"));

            //public static XmlQualifiedName GetXmlSchema(XmlSchemaSet schemas)
            //{
            //    Type callingType = typeof(int);
            //    return XmlSchemaManager.GetXmlSchema(callingType, schemas);
            //}

            var getXmlSchemaMethod = new MethodDefinition(methodName, MethodAttributes.Public | MethodAttributes.Static, catelType.Module.Import(_msCoreReferenceFinder.XmlQualifiedName));

            var compilerGeneratedAttribute = catelType.Module.FindType("mscorlib", "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            getXmlSchemaMethod.CustomAttributes.Add(new CustomAttribute(catelType.Module.Import(compilerGeneratedAttribute.Resolve().Constructor(false))));

            getXmlSchemaMethod.Parameters.Add(new ParameterDefinition("xmlSchemaSet", ParameterAttributes.None, catelType.Module.Import(_msCoreReferenceFinder.XmlSchemaSet)));

            var ldloc1Instruction = Instruction.Create(OpCodes.Ldloc_1);

            var instructions = getXmlSchemaMethod.Body.Instructions;
            instructions.Insert(0, Instruction.Create(OpCodes.Nop),
                                Instruction.Create(OpCodes.Ldtoken, catelType),
                                Instruction.Create(OpCodes.Call, importedGetTypeFromHandle),
                                Instruction.Create(OpCodes.Stloc_0),
                                Instruction.Create(OpCodes.Ldloc_0),
                                Instruction.Create(OpCodes.Ldarg_0),
                                Instruction.Create(OpCodes.Call, getXmlSchemaMethodOnXmlSchemaManager),
                                Instruction.Create(OpCodes.Stloc_1),
                                Instruction.Create(OpCodes.Br_S, ldloc1Instruction),
                                ldloc1Instruction,
                                Instruction.Create(OpCodes.Ret));

            getXmlSchemaMethod.Body.InitLocals = true;
            getXmlSchemaMethod.Body.Variables.Add(new VariableDefinition("callingType", catelType.Module.Import(catelType.Module.FindType("mscorlib", "System.Type"))));
            getXmlSchemaMethod.Body.Variables.Add(new VariableDefinition(catelType.Module.Import(_msCoreReferenceFinder.XmlQualifiedName)));

            catelType.Methods.Add(getXmlSchemaMethod);
        }

        private string GetXmlSchemaMethodName(CatelTypeNode catelTypeNode)
        {
            var methodName = string.Format("GetXmlSchemaFor{0}", catelTypeNode.TypeDefinition.FullName);

            return methodName.Replace(".", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty).Replace("`", string.Empty);
        }
    }
}