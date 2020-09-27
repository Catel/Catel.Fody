// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericPropertyModel.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System;
    using Catel.Data;

    public class GenericPropertyModelAsInt : GenericPropertyModel<int>
    {
        public GenericPropertyModelAsInt()
            : base()
        {
            var i = 42;

            Console.WriteLine(i);
        }
    }

    public class GenericPropertyModelAsObject : GenericPropertyModel<object>
    {
        public GenericPropertyModelAsObject()
            : base()
        {
            var i = new object();

            Console.WriteLine(i);
        }
    }

    public class GenericPropertyModel<TModel> : ModelBase
    {
        #region Properties
        public TModel MyModel { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public TModel MyModelCatel
        {
            get { return GetValue<TModel>(MyModelCatelProperty); }
            set { SetValue(MyModelCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData MyModelCatelProperty = RegisterProperty("MyModelCatel", typeof(TModel), null);
#elif CATEL_6
        public static readonly IPropertyData MyModelCatelProperty = RegisterProperty("MyModelCatel", typeof(TModel), null);
#endif
        #endregion
    }
}
