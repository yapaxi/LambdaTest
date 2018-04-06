using System;

namespace ApiContract
{
    public class RestMethodAttribute : Attribute
    {
        public Microsoft.OpenApi.Models.OperationType HttpMethod { get; }
        public string Path { get; }

        public RestMethodAttribute(Microsoft.OpenApi.Models.OperationType httpMethod, string path)
        {
            HttpMethod = httpMethod;
            Path = path;
        }
    }
}
