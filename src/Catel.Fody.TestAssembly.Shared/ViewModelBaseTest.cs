// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBaseTest.cs" company="Catel development team">
//   Copyright (c) 2008 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.TestAssembly
{
    using Data;
    using MVVM;

    public class ViewModelBaseTest : MyViewModelBase
    {
        public ViewModelBaseTest()
        {
            TestCommand = new Command(OnTestCommandExecute);
            TestCommandWithInterface = new Command(OnTestCommandWithInterfaceExecute);
        }

        public string Name { get; set; }

        public string FullName
        {
            get { return GetValue<string>(FullNameProperty); }
            set { SetValue(FullNameProperty, value); }
        }

#if CATEL_5
        public static readonly PropertyData FullNameProperty = RegisterProperty("FullName", typeof(string));
#elif CATEL_6
        public static readonly IPropertyData FullNameProperty = RegisterProperty<string>("FullName");
#endif

        private void OnNameChanged()
        {

        }

        /// <summary>
        /// Gets the TestCommand command.
        /// </summary>
        public Command TestCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the TestCommand command is executed.
        /// </summary>
        private void OnTestCommandExecute()
        {
            // TODO: Handle command logic here
        }

        /// <summary>
        /// Gets the TestCommandWithInterface command.
        /// </summary>
        public ICatelCommand TestCommandWithInterface { get; private set; }

        /// <summary>
        /// Method to invoke when the TestCommandWithInterface command is executed.
        /// </summary>
        private void OnTestCommandWithInterfaceExecute()
        {
            // TODO: Handle command logic here
        }
    }
}
