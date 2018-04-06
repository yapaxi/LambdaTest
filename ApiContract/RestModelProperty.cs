using System;
using System.Collections.Generic;
using System.Text;

namespace ApiContract
{
    public class RestModelPropertyAttribute : Attribute
    {
        public string Name { get; }

        public RestModelPropertyAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
