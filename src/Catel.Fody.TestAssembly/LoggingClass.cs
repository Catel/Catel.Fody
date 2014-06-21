// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassWithLogging.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody.TestAssembly
{
    using Logging;

    public class LoggingClass
    {
        #region Constants
        private static readonly ILog AutoLog = LogManager.GetCurrentClassLogger();
        private static readonly ILog ManualLog = LogManager.GetLogger(typeof (LoggingClass));
        #endregion

        public LoggingClass()
        {
            AutoLog.Info("Autolog test");
            ManualLog.Info("ManualLog test");
        }
    }
}