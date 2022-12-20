namespace Catel.Fody.TestAssembly.Bugs.GH0021
{
    public abstract class MyService
    {
        public abstract void MyMethod(object a, object b);

        public abstract void MyMethod_Expected(object a, object b);
    }
}