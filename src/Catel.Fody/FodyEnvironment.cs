// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FodyEnvironment.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;
    using System.Xml.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    public static class FodyEnvironment
    {
        /// <summary>
        /// Gets or sets the module definition.
        /// </summary>
        /// <value>The module definition.</value>
        public static ModuleDefinition ModuleDefinition { get; set; }

        /// <summary>
        /// Gets or sets the assembly resolver. Contains a  <seealso cref="Mono.Cecil.IAssemblyResolver"/> for resolving dependencies.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public static IAssemblyResolver AssemblyResolver { get; set; }

        /// <summary>
        /// Gets or sets the configuration element. Contains the full element from <c>FodyWeavers.xml</c>.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public static XElement Config { get; set; }

        public static Action<string> LogInfo { get; set; }

        public static Action<string> LogWarning { get; set; }

        public static Action<string, SequencePoint> LogWarningPoint { get; set; }

        public static Action<string> LogError { get; set; }

        public static Action<string, SequencePoint> LogErrorPoint { get; set; }

        public static bool IsCatelCoreAvailable { get; set; }

        public static bool IsCatelMvvmAvailable { get; set; }
    }
}