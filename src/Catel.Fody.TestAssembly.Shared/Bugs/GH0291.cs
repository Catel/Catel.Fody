namespace Catel.Fody.TestAssembly.Bugs.GH0291
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Catel.Data;

    public class Address : ModelBase
    {
        public string City { get; set; }
        public List<Person> People { get; set; }
        public virtual List<Person> People2 { get; set; }
        public bool IsUsed => People.Any();
        public bool IsUsed2 => People2.Any();
    }

    public class Person : ModelBase
    {
        public Person()
        {
            Address = new Address();
            Address2 = new Address();
        }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string ComposedName => $"{Name} {Surname}";
        public string ComplAddress => $"{ComposedName}, {Address.City}";
        public string ComplAddress2 => $"{ComposedName}, {Address2.City}";

        public virtual Address Address { get; set; }
        public Address Address2 { get; set; }
        public bool HasAddress => Address != null;
        public bool HasAddress2 => Address2 != null;
    }
}
