// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalType.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System.Collections.ObjectModel;
    using Catel.Data;

    public interface IQuery
    {
        string String { get; set; }
    }

    public interface ISimpleModel
    {
        IQuery Query { get; set; }
        bool? IsOk { get; set; }
        ObservableCollection<bool> Items { get; set; }
    }

    public class SimpleModel : ModelBase, ISimpleModel
    {
        public IQuery Query { get; set; }
        public bool? IsOk { get; set; }
        public ObservableCollection<bool> Items { get; set; }
    }
}
