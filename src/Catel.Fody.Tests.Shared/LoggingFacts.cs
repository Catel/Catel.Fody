namespace Catel.Fody.Tests;

using System;
using System.ComponentModel;
using NUnit.Framework;

#if !CATEL_7_OR_HIGHER

[TestFixture]
public class LoggingFacts
{
    [TestCase]
    public void InheritanceWorks()
    {
        // Instantiating is sufficient
        var type = AssemblyWeaver.Instance.Assembly.GetType("Catel.Fody.TestAssembly.LoggingClass");
        var obj = (dynamic)Activator.CreateInstance(type);
    }
}
#endif
