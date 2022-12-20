namespace Catel.Fody
{
    using System.Collections.Generic;
    using System.Linq;

    public class CodeGenTypeCleaner
    {
        private readonly CatelTypeNodeBuilder _catelTypeNodeBuilder;

        public CodeGenTypeCleaner(CatelTypeNodeBuilder catelTypeNodeBuilder)
        {
            _catelTypeNodeBuilder = catelTypeNodeBuilder;
        }

        private void Process(List<CatelType> catelTypes)
        {
            foreach (var catelType in catelTypes.ToList())
            {
                var customAttributes = catelType.TypeDefinition.CustomAttributes;
                if (customAttributes.ContainsAttribute("System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                {
                    catelTypes.Remove(catelType);
                    continue;
                }
            }
        }

        public void Execute()
        {
            Process(_catelTypeNodeBuilder.CatelTypes);
        }
    }
}