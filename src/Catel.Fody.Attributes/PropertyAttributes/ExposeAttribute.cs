﻿namespace Catel.Fody
{
    using System;

    /// <summary>
    /// The expose attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExposeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExposeAttribute" /> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException">The <paramref name="propertyName" /> is <c>null</c> or whitespace.</exception>
        /// <remarks>
        /// Do not create default values for attribute constructors! For more information, see:
        /// http://stackoverflow.com/questions/8290853/attribute-argument-must-be-a-constant-error-when-using-an-optional-parameter-in
        /// </remarks>
        public ExposeAttribute(string propertyName)
            : this(propertyName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExposeAttribute" /> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyNameOnModel">The property name configuration model.</param>
        /// <exception cref="ArgumentException">The <paramref name="propertyName" /> is <c>null</c> or whitespace.</exception>
        public ExposeAttribute(string propertyName, string propertyNameOnModel)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("The argument cannot be null or whitespace", "propertyName");
            }

            PropertyName = propertyName;
            PropertyNameOnModel = propertyNameOnModel ?? propertyName;
            //Mode = ViewModelToModelMode.TwoWay;
        }

        /// <summary>
        ///   Gets the name of the property that should be automatically created.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; private set; }

        /// <summary>
        ///   Gets or sets the name of the property on the model. If the <see cref = "PropertyName" /> is not the
        ///   same as the name of the property on the model, this can be used to map the properties.
        ///   <para />
        ///   By default, the value is the same as the <see cref="PropertyName"/>.
        ///   <example>
        ///     In this example, the name of the property to map on the model is <c>first_name</c>, but
        ///     it must be available as <c>FirstName</c> on the view model.
        ///     <code>
        ///       <![CDATA[
        ///         [Model]
        ///         [Expose("FirstName", "first_name")]
        ///         public Person Person { get; set; }
        ///       ]]>
        ///     </code>
        ///   </example>
        /// </summary>
        /// <value>The property name on model.</value>
        public string PropertyNameOnModel { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this property is read only.
        /// </summary>
        /// <value><c>true</c> if this property is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly { get; set; }

        ///// <summary>
        ///// Gets or sets the mode of the mapping.
        ///// <para />
        ///// The default value is <see cref="ViewModelToModelMode.TwoWay"/>.
        ///// </summary>
        ///// <value>The mode.</value>
        //public ViewModelToModelMode Mode { get; set; }
    }
}
