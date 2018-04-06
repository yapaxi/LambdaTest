using ApiContract;
using LambdaTest;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiGen
{
    class Program
    {
        static Type UnwrapTaskType(Type type)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return type.GetGenericArguments().Single();
            }
            else
            {
                return type;
            }
        }

        static OpenApiSchema GetSchema(Type type)
        {
            var schema = new OpenApiSchema();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var name = prop.GetCustomAttribute<RestModelPropertyAttribute>()?.Name ?? prop.Name;

                if (prop.PropertyType == typeof(string))
                {
                    schema.Properties[name] = new OpenApiSchema() { Type = "string" };
                }
                else if (prop.PropertyType == typeof(Guid))
                {
                    schema.Properties[name] = new OpenApiSchema() { Type = "string" };
                }
                else if (prop.PropertyType.IsPrimitive)
                {
                    schema.Properties[name] = new OpenApiSchema() { Type = "integer" };
                }
                else
                {
                    schema.Properties[name] = GetSchema(prop.PropertyType);
                }
            }
            return schema;
        }

        static void Main(string[] args)
        {
            var methods = (from q in typeof(LambdaHandler).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                           let lambda = q.GetCustomAttribute<AmazonLambdaAttribute>()
                           let rest = q.GetCustomAttribute<RestMethodAttribute>()
                           let input = q.GetParameters().SingleOrDefault()?.ParameterType
                           let output = q.ReturnType == null ? null : UnwrapTaskType(q.ReturnType)
                           where lambda != null && rest != null
                           select new
                           {
                               Lambda = lambda,
                               Rest = rest,
                               InputType = input,
                               OutputType = output
                           });

            var paths = new OpenApiPaths();

            foreach (var method in methods)
            {
                paths[method.Rest.Path] = new OpenApiPathItem()
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>()
                    {
                        [method.Rest.HttpMethod] = new OpenApiOperation()
                        {
                            RequestBody = method.InputType == null ? null : new OpenApiRequestBody()
                            {
                                Content =
                                {
                                    ["application/json"] = new OpenApiMediaType()
                                    {
                                        Schema = GetSchema(method.InputType)
                                    }
                                }
                            },
                            Responses = new OpenApiResponses()
                            {
                                ["200"] = new OpenApiResponse()
                                {
                                    Description = "OK",
                                    Content = method.OutputType == null ? null : new Dictionary<string, OpenApiMediaType>()
                                    {
                                        ["application/json"] = new OpenApiMediaType()
                                        {
                                            Schema = GetSchema(method.OutputType)
                                        }
                                    }
                                }
                            },
                            Extensions =
                            {
                                ["x-amazon-apigateway-integration"] = new AmazonApiGatewayExtentions(
                                    credentials: "arn:aws:iam::408795339721:role/apigateway-to-lambda",
                                    lambdaUri: method.Lambda.Uri, 
                                    httpMethod: method.Rest.HttpMethod.ToString().ToUpper()
                                )
                            }
                        }
                    }
                };
            }

            var document = new OpenApiDocument()
            {
                Info = new OpenApiInfo()
                {
                    Version = args[2],
                    Title = args[1],
                },
                Paths = paths
            };

            using (var writer = new StreamWriter(args[0], false, Encoding.UTF8))
            {
                var jsonWriter = new OpenApiJsonWriter(writer);
                document.SerializeAsV2(jsonWriter);
            }
        }
    }
}
