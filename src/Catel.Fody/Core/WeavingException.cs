// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WeavingException.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Fody
{
    using System;

    public class WeavingException : Exception
    {
        public WeavingException(string message)
            : base(message)
        {
        }
    }
}