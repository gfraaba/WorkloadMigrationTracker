using System;

namespace Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValidationIgnoreAttribute : Attribute
    {
    }
}