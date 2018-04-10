// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyDerivedService.cs" company="Catel development team">
//   Copyright (c) 2008 - 2018 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly.Bugs.GH0021
{
    public class MyDerivedService : MyService
    {
        public override void MyMethod(object a, object b)
        {
            Argument.IsNotNull(() => b);

            var model = (ModelBaseTest)a;
            model.Name = "test";
        }
    }
}