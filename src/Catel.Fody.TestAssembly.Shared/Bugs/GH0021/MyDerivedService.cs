namespace Catel.Fody.TestAssembly.Bugs.GH0021
{
    public class MyDerivedService : MyService
    {
        public override void MyMethod(object a, object b)
        {
            Argument.IsNotNull(() => b);

            var model = (ModelBaseTest)a;
            model.Name = "test";
        }

        public override void MyMethod_Expected(object a, object b)
        {
            Argument.IsNotNull("b", b);

            var model = (ModelBaseTest)a;
            model.Name = "test";
        }
    }
}