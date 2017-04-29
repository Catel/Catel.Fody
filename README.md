Catel.Fody
==========

[![Join the chat at https://gitter.im/Catel/Catel.Fody](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Catel/Catel.Fody?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

![License](https://img.shields.io/github/license/catel/catel.fody.svg)
![NuGet downloads](https://img.shields.io/nuget/dt/catel.fody.svg)
![Version](https://img.shields.io/nuget/v/catel.fody.svg)
![Pre-release version](https://img.shields.io/nuget/vpre/catel.fody.svg)

# Introduction

Catel.Fody is an addin for Fody (see https://github.com/Fody/Fody), which
is an extensible tool for weaving .net assemblies. This addin will rewrite simple properties to the dependency-property alike properties that are used inside Catel.

It will rewrite all properties on the *ModelBase* and *ViewModelBase*. So, a
property that is written as this:

    public string FirstName { get; set; }

will be weaved into

    public string FirstName
    {
        get { return GetValue<string>(FirstNameProperty); }
        set { SetValue(FirstNameProperty, value); }
    }

    public static readonly PropertyData FirstNameProperty = RegisterProperty("FirstName", typeof(string));

but if a readonly computed property like this one exists:

    public string FullName
    {
        get { return string.Format("{0} {1}", FirstName, LastName).Trim(); }
    }

the *OnPropertyChanged* method will be also weaved into

	protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (e.PropertyName.Equals("FirstName"))
		{
			base.RaisePropertyChanged("FullName");
		}

		if (e.PropertyName.Equals("LastName"))
		{
			base.RaisePropertyChanged("FullName");
		}
	}


## Enabling Catel.Fody

To enable Catel.Fody to weave assemblies, you need to perform the following steps:

1. Install the Catel.Fody NuGet package
2. Update FodyWeavers.xml and make sure it contains <Catel />

> Note that the FodyWeavers.xml should be updated automatically when
 

## Disabling weaving for specific types or properties

To disable the weaving of types or properties of a type, decorate it with the 
*NoWeaving* attribute as shown in the example below:

	[NoWeaving]
	public class MyClass : ModelBase
	{
	    ...
	}

## Configuring Catel.Fody

Though we recommend to leave the default settings (great for most people), it is possible to configure the weaver. Below is a list of options that can be configured.

To configure an option, modify *FodyWeavers.xml* by adding the property and value to the Catel element. For example, the example below will disable argument and logging weaving:

	<Catel WeaveArguments="false" WeaveLogging="false" />

### WeaveProperties

Weave all regular properties on classes that inherit (directly or indirectly) from Catel.Data.ModelBase into Catel properties.

> Default value is *true*


### WeaveExposedProperties

Weave all Catel properties decorated with both the Catel.MVVM.Model attribute and Fody.Expose attribute as automatic mappings,

> Default value is *true*


### WeaveArguments

Weave all Argument check attributes into actual argument checks.

> Default value is *true*


### WeaveLogging

Weave all calls to *LogManager.GetCurrentClassLogger()* into *LogManager.GetLogger(typeof(classname))*.

> Default value is *true*


### GenerateXmlSchemas

Generate xml schemas for all classes that inherit (directly or indirectly) from *Catel.Data.ModelBase*.

> Default value is *false*


## Weaving properties

The *PropertyChanged.Fody* plugin for Fody already supports Catel out of the box, but only for property change notifications. However, with the *Catel.Fody* plugin, it is possible to automatically weave a simple property into a Catel property.

The following property definition:

	public string Name { get; set; }

will be weaved into:

	public string Name
	{
	    get { return GetValue<string>(NameProperty); }
	    set { SetValue(NameProperty, value); }
	}
	 
	public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string));


### Support for computed properties

If  a computed property like this one exists:

	public string FullName
	{
	    get { return string.Format("{0} {1}", FirstName, LastName).Trim(); }
	}

the *OnPropertyChanged* method will be also weaved into

	protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
	{
	    base.OnPropertyChanged(e);
	 
	    if (e.PropertyName.Equals("FirstName"))
	    {
	        base.RaisePropertyChanged("FullName");
	    }
	 
	    if (e.PropertyName.Equals("LastName"))
	    {
	        base.RaisePropertyChanged("FullName");
	    }
	}

In order to avoid this behavior, you can use the *NoWeavingAttribute* on the computed property, just like this:

	[NoWeaving]
	public string FullName
	{
	    get { return string.Format("{0} {1}", FirstName, LastName).Trim(); }
	}

In the background, *Catel.Fody* will handle the following workflow:

1. Find all types in the assembly deriving from *ModelBase* (thus also *ViewModelBase*)
2. Check if the type has an automatic property backing field (only those properties are weaved)
3. Add the *PropertyData* field for the property
4. Instantiate the *PropertyData* field in the static constructor of the type
5. Replace the content of the getter and setter with the appropriate calls to *GetValue* and *SetValue*
 
> Note that this feature is automatically disabled for classes that already override the *OnPropertyChanged* method. It is too complex to determine where the logic should be added so the end-developer is responsible for implementing this feature when overriding *OnPropertyChanged*


### Automatically excluded properties

By default, Catel.Fody ignores the following properties and types by default because they shouldn't be weaved:

1. All properties of type *ICommand*
2. Properties without an automatically generated backing field


### Specifying default values for weaved properties

By default, Catel uses *null* as default values for reference types. For value types, it will use *default(T)*. To specify the default value of a weaved property, use the *DefaultValue* attribute as shown in the example below:

	public class Person : ModelBase
	{
	    [DefaultValue("Geert")]
	    public string FirstName { get; set; }
	  
	    public string LastName { get; set; }
	}

This will be weaved into:

	public class Person : ModelBase
	{
	    public string FirstName
	    {
	        get { return GetValue<string>(FirstNameProperty); }
	        set { SetValue(FirstNameProperty, value); }
	    }
	 
	    public static readonly PropertyData FirstNameProperty = RegisterProperty("FirstName", typeof(string), "Geert");
	 
	    public string LastName
	    {
	        get { return GetValue<string>(LastNameProperty); }
	        set { SetValue(LastNameProperty, value); }
	    }
	 
	    public static readonly PropertyData LastNameProperty = RegisterProperty("LastName", typeof(string), null);
	}

### How to get automatic change notifications

The Fody plugin for Catel automatically searches for the *On[PropertyName]Changed* methods. If a method is found, it will automatically be called when the property has changed. For example, the *OnNameChanged* is automatically called when the *Name* property is changed in the example below:

	public string Name { get; set; }
	 
	private void OnNameChanged()
	{
	    // this method is automatically called when the Name property changes
	}


## Weaving argument checks

With the Catel.Fody plugin, it is possible to automatically weave a method implementation with its own argument check operations declared via attributes.

### Automatic performance improvements

The latest version of Catel.Fody automatically converts all expression argument checks to faster calls. For example, the code below:

	Argument.IsNotNull(() => myString);

Will automatically be weaved into this:

	Argument.IsNotNull("myString", myString);

This is much faster because the expression doesn't have to be parsed at runtime. This is a very noticeable performance boost if the expression check is used more than 50 times per second.

> When using the latest version of Catel.Fody, the team recommends using expressions above the regular argument checks (with name and value specified separately) because it will result in cleaner code. With this feature, there is no longer a performance penalty for using the expressions version


### Argument checking via attributes

The following method definition:

	public void DoSomething([NotNullOrEmpty] string myString. [NotNull] object myObject)
	{
	}

Will be weaved into:

	public void DoSomething(string myString, object myObject)
	{
	    Argument.IsNotNullOrEmpty("myString", myString);
	    Argument.IsNotNull("myObject", myObject);
	}

In the background, Catel.Fody will handle the following workflow:

1. Find all types in the assembly
2. Find all method of each type
3. Find all annotated method parameter of each method
4. Insert as first instructions of the method body the calls to Argument check corresponding methods.

### Available argument check Catel.Fody attributes

- NotNull => Argument.IsNotNull
- NotNullOrEmptyArray => Argument.IsNotNullOrEmptyArray
- NotNullOrEmpty => Argument.IsNotNullOrEmpty
- NotNullOrWhitespace => Argument.IsNotNullOrWhitespace
- Match => Argument.IsMatch
- NotMatch => Argument.IsNotMatch
- NotOutOfRange => Argument.IsNotOutOfRange
- Maximum => Argument.IsMaximum
- Minimal => Argument.IsMinimal
- OfType => Argument.IsOfType
- ImplementsInterface => Argument.ImplementsInterface
- InheritsFrom => Argument.InheritsFrom


## Exposing properties on view models

The way to expose properties of a model to the view model in Catel is the *ViewModelToModelAttribute*. The goal of these attributes is to easily map properties from a model to the view model so as much of the plumbing (setting/getting properties, rechecking validation, etc) is done automatically for the developer.

Using the *ViewModelToModelAttribute*, this is the syntax to map properties automatically:

	/// <summary>
	/// Gets or sets the person.
	/// </summary>
	[Model]
	public Person Person
	{
	    get { return GetValue<Person>(PersonProperty); }
	    private set { SetValue(PersonProperty, value); }
	}
	  
	/// <summary>
	/// Register the Person property so it is known in the class.
	/// </summary>
	public static readonly PropertyData PersonProperty = RegisterProperty("Person", typeof(Person));
	  
	/// <summary>
	/// Gets or sets the first name.
	/// </summary>
	[ViewModelToModel("Person")]
	public string FirstName
	{
	    get { return GetValue<string>(FirstNameProperty); }
	    set { SetValue(FirstNameProperty, value); }
	}
	  
	/// <summary>
	/// Register the FirstName property so it is known in the class.
	/// </summary>
	public static readonly PropertyData FirstNameProperty = RegisterProperty("FirstName", typeof(string));

However, if you only define the *FirstName* property just to protect your model, then why should you define the whole property? This is where the *ExposeAttribute* property comes in very handy. This attribute internally registers a new dynamic property on the view model, and then uses the same technique as the *ViewModelToModelAttribute*.

Below is the new way you can easily expose properties of a model and protect other properties of the model from the view:

	/// <summary>
	/// Gets or sets the person.
	/// </summary>
	[Model]
	[Expose("FirstName")]
	[Expose("MiddleName")]
	[Expose("LastName")]
	private Person Person
	{
	    get { return GetValue<Person>(PersonProperty); }
	    set { SetValue(PersonProperty, value); }
	}
	  
	/// <summary>
	/// Register the Person property so it is known in the class.
	/// </summary>
	public static readonly PropertyData PersonProperty = RegisterProperty("Person", typeof(Person));

This is a very cool feature that allows you to protect your model without having to re-define all the properties on the view model. Also, the validation in the model is automatically synchronized with the view model when you use this attribute.

In combination with the automatic property weaving, this could be written as clean as the code below:

	[Model]
	[Expose("FirstName")]
	[Expose("MiddleName")]
	[Expose("LastName")]
	private Person Person { get; set; }


## XmlSchema generation

to start of metadata

The .NET framework supports *XmlSchema* attributes to allow static members to define a custom schema method required for WCF serialization. Unfortunately this cannot be implemented in Catel itself because it would required too much reflection and the method is static. Therefore this feature included in Catel.Fody.

Starting with Catel.Fody 2.0, this feature is disabled by default. To enabled it, use the following option in *FodyWeavers.xml*:

	<Catel GenerateXmlSchemas="true" />

When the *XmlSchemaProvider* is available on the target platform where Catel is used, the changes will be made to classes deriving from *ModelBase*:

1. Decorate the class with *XmlSchemaProvider* attribute:


	[XmlSchemaProvider("GetXmlSchemaForCatelFodyTestAssemblyInheritedClass")]
	public class InheritedClass : BaseClass
	{
	    // rest of the class definition
	}

2. Implement the class specific GetXmlSchema method:

	[CompilerGenerated]
	public static XmlQualifiedName GetXmlSchemaForCatelFodyTestAssemblyInheritedClass(XmlSchemaSet xmlSchemaSet)
	{
	    Type type = typeof(InheritedClass);
	    return XmlSchemaManager.GetXmlSchema(type, xmlSchemaSet);
	}

