namespace Catel.Fody.TestAssembly
{
    using Catel;

    public class CTL908
    {
        public CTL908(object obj)
        {
            Argument.IsNotNull(() => obj);
        }
    }

    public class CTL908_Expected
    {
        public CTL908_Expected(object obj)
        {
            Argument.IsNotNull("obj", obj);
        }
    }
}