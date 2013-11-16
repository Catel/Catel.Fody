// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsNotNullOrEmptyArgumentMethodCallWeaver.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody.Weaving.Argument
{
    public sealed class IsNotNullOrEmptyArgumentMethodCallWeaver : ArgumentMethodCallWeaverBase
    {
        #region Constructors
        public IsNotNullOrEmptyArgumentMethodCallWeaver()
            : base("IsNotNullOrEmpty")
        {
        }
        #endregion
    }
}