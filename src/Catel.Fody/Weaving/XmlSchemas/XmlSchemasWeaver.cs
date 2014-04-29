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

    public class XmlSchemasWeaver
    {
        private readonly MsCoreReferenceFinder _msCoreReferenceFinder;

        public XmlSchemasWeaver(MsCoreReferenceFinder msCoreReferenceFinder)
        {
            _msCoreReferenceFinder = msCoreReferenceFinder;
        }

        public void Execute(CatelType catelType)
        {
            if (catelType.TypeDefinition.IsAbstract)
            {
                return;
            }

            if (catelType.TypeDefinition.IsEnum)
            {
                return;
            }

            if (!catelType.TypeDefinition.ImplementsCatelModel())
            {
                return;
            }

            if (catelType.TypeDefinition.ImplementsViewModelBase())
            {
                return;
            }

            FodyEnvironment.LogInfo("\t\t Adding xml schema for type " + catelType.TypeDefinition.FullName);

            if (AddXmlSchemaProviderAttribute(catelType))
            {
                AddGetXmlSchemaMethod(catelType);
            }
        }

        private bool AddXmlSchemaProviderAttribute(CatelType catelType)
        {
            var catelTypeDefinition = catelType.TypeDefinition;

            var methodName = GetXmlSchemaMethodName(catelType);

            var existingCustomAttribute = (from attribute in catelTypeDefinition.CustomAttributes
                                           where string.Equals(attribute.AttributeType.Name, "XmlSchemaProviderAttribute")
                                           select attribute).FirstOrDefault();
            if (existingCustomAttribute != null)
            {
                var constructorArgument = existingCustomAttribute.ConstructorArguments[0];
                if (string.Equals(constructorArgument.Value, methodName))
                {
                    return false;
                }
            }

            var xmlSchemaProviderAttribute = catelTypeDefinition.Module.FindType("System.Xml", "System.Xml.Serialization.XmlSchemaProviderAttribute");

            var attributeConstructor = catelTypeDefinition.Module.Import(xmlSchemaProviderAttribute.Resolve().Constructor(false));
            var customAttribute = new CustomAttribute(attributeConstructor);
            customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(catelTypeDefinition.Module.TypeSystem.String, methodName));

            catelTypeDefinition.CustomAttributes.Add(customAttribute);

            return true;
        }

        private void AddGetXmlSchemaMethod(CatelType catelType)
        {
            var catelTypeDefinition = catelType.TypeDefinition;

            var methodName = GetXmlSchemaMethodName(catelType);

            var alreadyHandled = (from method in catelTypeDefinition.Methods
                                  where method.Name == methodName
                                  select method).Any();
            if (alreadyHandled)
            {
                return;
            }

            var getTypeFromHandle = catelTypeDefinition.Module.GetMethod("GetTypeFromHandle");
            var importedGetTypeFromHandle = catelTypeDefinition.Module.Import(getTypeFromHandle);

            var xmlSchemaManager = (TypeDefinition)catelTypeDefinition.Module.FindType("Catel.Core", "Catel.Runtime.Serialization.Xml.XmlSchemaManager");
            if (xmlSchemaManager == null)
            {
                // Support versions before 3.8
                xmlSchemaManager = (TypeDefinition)catelTypeDefinition.Module.FindType("Catel.Core", "Catel.Runtime.Serialization.XmlSchemaManager");
            }

            var getXmlSchemaMethodOnXmlSchemaManager = catelTypeDefinition.Module.Import(xmlSchemaManager.Methods.First(x => x.IsStatic && x.Name == "GetXmlSchema"));

            //public static XmlQualifiedName GetXmlSchema(XmlSchemaSet schemas)
            //{
            //    Type callingType = typeof(int);
            //    return XmlSchemaManager.GetXmlSchema(callingType, schemas);
            //}

            var getXmlSchemaMethod = new MethodDefinition(methodName, MethodAttributes.Public | MethodAttributes.Static, catelTypeDefinition.Module.Import(_msCoreReferenceFinder.XmlQualifiedName));

            getXmlSchemaMethod.Parameters.Add(new ParameterDefinition("xmlSchemaSet", ParameterAttributes.None, catelTypeDefinition.Module.Import(_msCoreReferenceFinder.XmlSchemaSet)));

            var ldloc1Instruction = Instruction.Create(OpCodes.Ldloc_1);

            var instructions = getXmlSchemaMethod.Body.Instructions;
            instructions.Insert(0, Instruction.Create(OpCodes.Nop),
                                Instruction.Create(OpCodes.Ldtoken, catelTypeDefinition),
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
            getXmlSchemaMethod.Body.Variables.Add(new VariableDefinition("callingType", catelTypeDefinition.Module.Import(catelTypeDefinition.Module.FindType("mscorlib", "System.Type"))));
            getXmlSchemaMethod.Body.Variables.Add(new VariableDefinition(catelTypeDefinition.Module.Import(_msCoreReferenceFinder.XmlQualifiedName)));

            catelTypeDefinition.Methods.Add(getXmlSchemaMethod);

            getXmlSchemaMethod.MarkAsCompilerGenerated(_msCoreReferenceFinder);
        }

        private string GetXmlSchemaMethodName(CatelType catelType)
        {
            var methodName = string.Format("GetXmlSchemaFor{0}", catelType.TypeDefinition.FullName);

            return methodName.Replace(".", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty).Replace("`", string.Empty);
        }
    }
}