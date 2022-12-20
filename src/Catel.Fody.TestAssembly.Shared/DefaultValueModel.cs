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

        [DefaultValue(null)]
        public int? NullableIntDefaultNullValue { get; set; }

        [DefaultValue(0)]
        public int? NullableIntDefault0Value { get; set; }

        [DefaultValue(1)]
        public int? NullableIntDefault1Value { get; set; }

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
        public static readonly IPropertyData BoolValueCatelProperty = RegisterProperty<bool>("BoolValueCatel", true);
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
        public static readonly IPropertyData NullableBoolDefaultNullValueCatelProperty = RegisterProperty<bool?>("NullableBoolDefaultNullValueCatel", (bool?)null);
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
        public static readonly IPropertyData NullableBoolDefaultTrueValueCatelProperty = RegisterProperty<bool?>("NullableBoolDefaultTrueValueCatel", true);
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
        public static readonly IPropertyData NullableBoolDefaultFalseValueCatelProperty = RegisterProperty<bool?>("NullableBoolDefaultFalseValueCatel", false);
#endif

        public int? NullableIntDefaultNullValueCatel
        {
            get { return GetValue<int?>(NullableIntDefaultNullValueCatelProperty); }
            set { SetValue(NullableIntDefaultNullValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData NullableIntDefaultNullValueCatelProperty = RegisterProperty(nameof(NullableIntDefaultNullValueCatel), typeof(int?), null);
#elif CATEL_6
        public static readonly IPropertyData NullableIntDefaultNullValueCatelProperty = RegisterProperty<int?>(nameof(NullableIntDefaultNullValueCatel));
#endif

        public int? NullableIntDefault0ValueCatel
        {
            get { return GetValue<int?>(NullableIntDefault0ValueCatelProperty); }
            set { SetValue(NullableIntDefault0ValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData NullableIntDefault0ValueCatelProperty = RegisterProperty(nameof(NullableIntDefault0ValueCatel), typeof(int?), 0);
#elif CATEL_6
        public static readonly IPropertyData NullableIntDefault0ValueCatelProperty = RegisterProperty<int?>(nameof(NullableIntDefault0ValueCatel), 0);
#endif

        public int? NullableIntDefault1ValueCatel
        {
            get { return GetValue<int?>(NullableIntDefault1ValueCatelProperty); }
            set { SetValue(NullableIntDefault1ValueCatelProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData NullableIntDefault1ValueCatelProperty = RegisterProperty(nameof(NullableIntDefault1ValueCatel), typeof(int?), 1);
#elif CATEL_6
        public static readonly IPropertyData NullableIntDefault1ValueCatelProperty = RegisterProperty<int?>(nameof(NullableIntDefault1ValueCatel), 1);
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
        public static readonly IPropertyData IntValueCatelProperty = RegisterProperty<int>("IntValueCatel", 42);
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
        public static readonly IPropertyData LongValueCatelProperty = RegisterProperty<long>("LongValueCatel", 42L);
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
        public static readonly IPropertyData DoubleValueCatelProperty = RegisterProperty<double>("DoubleValueCatel", default(double));
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
        public static readonly IPropertyData FloatValueCatelProperty = RegisterProperty<float>("FloatValueCatel", 42f);
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
        public static readonly IPropertyData EnumValueCatelProperty = RegisterProperty<ExampleEnum>("EnumValueCatel", ExampleEnum.B);
#endif
    }
}
