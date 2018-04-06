using Amazon.Lambda.Core;
using ApiContract;
using System;
using System.Threading.Tasks;

namespace LambdaTest
{
    public class LambdaHandler
    {
        [RestMethod(Microsoft.OpenApi.Models.OperationType.Post, "/azaza")]
        [AmazonLambda(
            "TestFunction", 
            "arn:aws:apigateway:us-east-2:lambda:path/2015-03-31/functions/arn:aws:lambda:us-east-2:408795339721:function:TestFunction/invocations"
        )]
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<SomeModel> XXX(SomeModel request)
        {
            await Task.Delay(100);
            return new SomeModel
            {
                Email = request.Email + "@@@@@"
            };
        }
    }

    public class SomeModel
    {
        [RestModelProperty("email")]
        public string Email { get; set; }
    }
}
