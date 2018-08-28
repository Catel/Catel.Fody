namespace Catel.Fody
{
    using System;

    /// <summary>
    /// Excludes a specific Catel property from the backup logic. Note that this
    /// can only be applied to properties being processed by Catel.Fody.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExcludeFromBackupAttribute : Attribute
    {
    }
}
