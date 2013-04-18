Catel.Fody readme
=================

Catel.Fody is an addin for Fody (see https://github.com/Fody/Fody), which
is an extensible tool for weaving .net assemblies. 

This addin will rewrite simple properties to the dependency-property alike 
properties that are used inside Catel.

It will rewrite all properties on the DataObjectBase and ViewModelBase. So, a
property that is written as this:

* public string FirstName { get; set; }

will be weaved into

* public string FirstName
* {
* 	get { return GetValue<string>(FirstNameProperty); }
* 	set { SetValue(FirstNameProperty, value); }
* }
* 
* public static readonly PropertyData FirstNameProperty = RegisterProperty("FirstName", typeof(string));

For more information about Catel.Fody, visit http://catelfody.codeplex.com