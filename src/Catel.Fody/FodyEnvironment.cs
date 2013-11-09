// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FodyEnvironment.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using Mono.Cecil;

    public static class FodyEnvironment
    {
        /// <summary>
        /// Gets or sets the module definition.
        /// </summary>
        /// <value>The module definition.</value>
        public static ModuleDefinition ModuleDefinition { get; set; }
    }
}