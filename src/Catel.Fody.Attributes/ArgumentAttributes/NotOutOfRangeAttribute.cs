// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotNullAttribute.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Catel.Fody
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class NotOutOfRangeAttribute : Attribute
    {
        public NotOutOfRangeAttribute(int minValue, int maxValue)
        {
        }

        public NotOutOfRangeAttribute(long minValue, long maxValue)
        {
        }   

        public NotOutOfRangeAttribute(double minValue, double maxValue)
        {
        }
        
        public NotOutOfRangeAttribute(float minValue, float maxValue)
        {
        }
        
        public NotOutOfRangeAttribute(string minValue, string maxValue)
        {
        }
    }
}