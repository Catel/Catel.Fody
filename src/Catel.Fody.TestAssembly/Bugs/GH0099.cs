namespace Catel.Fody.TestAssembly.Bugs.GH0099
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Catel.MVVM;

    public class TestViewModel : ViewModelBase
    {
        [Model]
        [Expose("ID")]
        public Model Model { get; set; }
    }

    public class Model : BaseModel<Model>
    {

    }

    public class BaseModel<T> : EntityBaseWithID<int, T>
        where T : BaseModel<T>
    {

    }


    public class EntityBaseWithID<T, S>
        where S : EntityBaseWithID<T, S>
    {
        public T ID { get; set; }
    }
}
