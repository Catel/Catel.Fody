// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultValueModel.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using System.ComponentModel;
    using Catel.Data;

    public enum ExampleEnum
    {
        A,

        B,

        C
    }

    public class DefaultValueModel : ModelBase
    {
        [DefaultValue("Geert")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [DefaultValue(true)]
        public bool BoolValue { get; set; }

        [DefaultValue(null)]
        public bool? NullableBoolDefaultNullValue { get; set; }

        [DefaultValue(true)]
        public bool? NullableBoolDefaultTrueValue { get; set; }

        [DefaultValue(false)]
        public bool? NullableBoolDefaultFalseValue { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool BoolValueCatel
        {
            get { return GetValue<bool>(BoolValueCatelProperty); }
            set { SetValue(BoolValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData BoolValueCatelProperty = RegisterProperty("BoolValueCatel", typeof(bool), true);
#elif CATEL_6
        public static readonly IPropertyData BoolValueCatelProperty = RegisterProperty("BoolValueCatel", typeof(bool), true);
#endif

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool? NullableBoolDefaultNullValueCatel
        {
            get { return GetValue<bool?>(NullableBoolDefaultNullValueCatelProperty); }
            set { SetValue(NullableBoolDefaultNullValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData NullableBoolDefaultNullValueCatelProperty = RegisterProperty("NullableBoolDefaultNullValueCatel", typeof(bool?), null);
#elif CATEL_6
        public static readonly IPropertyData NullableBoolDefaultNullValueCatelProperty = RegisterProperty("NullableBoolDefaultNullValueCatel", typeof(bool?), null);
#endif

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool? NullableBoolDefaultTrueValueCatel
        {
            get { return GetValue<bool?>(NullableBoolDefaultTrueValueCatelProperty); }
            set { SetValue(NullableBoolDefaultTrueValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData NullableBoolDefaultTrueValueCatelProperty = RegisterProperty("NullableBoolDefaultTrueValueCatel", typeof(bool?), true);
#elif CATEL_6
        public static readonly IPropertyData NullableBoolDefaultTrueValueCatelProperty = RegisterProperty("NullableBoolDefaultTrueValueCatel", typeof(bool?), true);
#endif

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool? NullableBoolDefaultFalseValueCatel
        {
            get { return GetValue<bool?>(NullableBoolDefaultFalseValueCatelProperty); }
            set { SetValue(NullableBoolDefaultFalseValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData NullableBoolDefaultFalseValueCatelProperty = RegisterProperty("NullableBoolDefaultFalseValueCatel", typeof(bool?), false);
#elif CATEL_6
        public static readonly IPropertyData NullableBoolDefaultFalseValueCatelProperty = RegisterProperty("NullableBoolDefaultFalseValueCatel", typeof(bool?), false);
#endif

        [DefaultValue(42)]
        public int IntValue { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int IntValueCatel
        {
            get { return GetValue<int>(IntValueCatelProperty); }
            set { SetValue(IntValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData IntValueCatelProperty = RegisterProperty("IntValueCatel", typeof(int), 42);
#elif CATEL_6
        public static readonly IPropertyData IntValueCatelProperty = RegisterProperty("IntValueCatel", typeof(int), 42);
#endif

        [DefaultValue(42L)]
        public long LongValue { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public long LongValueCatel
        {
            get { return GetValue<long>(LongValueCatelProperty); }
            set { SetValue(LongValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData LongValueCatelProperty = RegisterProperty("LongValueCatel", typeof(long), 42L);
#elif CATEL_6
        public static readonly IPropertyData LongValueCatelProperty = RegisterProperty("LongValueCatel", typeof(long), 42L);
#endif

        [DefaultValue(42d)]
        public double DoubleValue { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double DoubleValueCatel
        {
            get { return GetValue<double>(DoubleValueCatelProperty); }
            set { SetValue(DoubleValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData DoubleValueCatelProperty = RegisterProperty("DoubleValueCatel", typeof(double));
#elif CATEL_6
        public static readonly IPropertyData DoubleValueCatelProperty = RegisterProperty("DoubleValueCatel", typeof(double));
#endif

        [DefaultValue(42f)]
        public float FloatValue { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public float FloatValueCatel
        {
            get { return GetValue<float>(FloatValueCatelProperty); }
            set { SetValue(FloatValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData FloatValueCatelProperty = RegisterProperty("FloatValueCatel", typeof(float), 42f);
#elif CATEL_6
        public static readonly IPropertyData FloatValueCatelProperty = RegisterProperty("FloatValueCatel", typeof(float), 42f);
#endif

        [DefaultValue(ExampleEnum.B)]
        public ExampleEnum EnumValue { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ExampleEnum EnumValueCatel
        {
            get { return GetValue<ExampleEnum>(EnumValueCatelProperty); }
            set { SetValue(EnumValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData EnumValueCatelProperty = RegisterProperty("EnumValueCatel", typeof(ExampleEnum), ExampleEnum.B);
#elif CATEL_6
        public static readonly IPropertyData EnumValueCatelProperty = RegisterProperty("EnumValueCatel", typeof(ExampleEnum), ExampleEnum.B);
#endif
    }
}
