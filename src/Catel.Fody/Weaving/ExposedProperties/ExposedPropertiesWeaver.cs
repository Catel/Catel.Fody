// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExposedPropertiesWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.Weaving.ExposedProperties
{
    public class ExposedPropertiesWeaver
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public ExposedPropertiesWeaver(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        public void Execute()
        {
            //foreach (var type )
        }
    }
}