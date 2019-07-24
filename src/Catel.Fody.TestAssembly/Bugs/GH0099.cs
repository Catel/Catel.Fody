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

        //this is why I'm using S
        public override bool Equals(object other)
        {
            if (other is S n)
            {
                return n.ID.Equals(ID);
            }

            return false;
        }

        //irrelevant for this example
        public override int GetHashCode()
        {
            var hashCode = 509650306;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(ID);
            hashCode = hashCode * -1521134295 + GetType().GetHashCode();
            return hashCode;
        }
    }
}
