namespace Catel.Fody.Tests;

using System;
using Data;
using NUnit.Framework;

[TestFixture]
public class NoWeavingFacts
{
    [TestCase]
    public void IgnoresTypesWithNoWeavingAttribute()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoWeavingModelTest");

        Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"), Is.False);
    }

    [TestCase]
    public void IgnoresPropertiesWithNoWeavingAttribute()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoPropertyWeavingModelTest");

        Assert.That(PropertyDataManager.Default.IsPropertyRegistered(type, "FirstName"), Is.False);
    }

    [TestCase]
    public void Suppresses_Warning_For_Change_Callback_With_Parameters_Decorated_With_No_Weaving_Attribute()
    {
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.NoWeavingChangeCallbackWithParametersViewModel");
        var vm = (dynamic)Activator.CreateInstance(type);

        vm.SelectedItem = new object();

        Assert.That((bool)vm.WasCallbackInvoked, Is.True);
    }
}