using System;

namespace ApiContract
{

    public class AmazonLambdaAttribute : Attribute
    {
        public AmazonLambdaAttribute(string name, string uri)
        {
            Name = name;
            Uri = uri;
        }

        public string Name { get; }
        public string Uri { get; }
    }
}
