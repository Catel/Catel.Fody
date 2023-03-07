namespace Catel.Fody.TestAssembly
{
    using MVVM;

    public class GH0012 : ViewModelBase
    {
        public GH0012()
        {

        }

        public void a(object o)
        {
            Argument.IsNotNull(() => o);
        }
    }
}