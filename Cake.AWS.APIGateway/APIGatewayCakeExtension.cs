using Amazon.Runtime;
using Cake.Core;
using Cake.Core.Annotations;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public static class APIGatewayCakeExtension
{
    [CakeMethodAlias]
    public static async Task<string> DeployRestApi(this ICakeContext context, DeployApiGatewayConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        config.EnsureValid();

        context.Log.Write(
            Cake.Core.Diagnostics.Verbosity.Normal,
            Cake.Core.Diagnostics.LogLevel.Information,
            $"Deploying api {config.RestApiId} on {config.StageName} stage"
        );

        var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);

        using (var client = new Amazon.APIGateway.AmazonAPIGatewayClient(credentials, config))
        {
            var response = await client.CreateDeploymentAsync(new Amazon.APIGateway.Model.CreateDeploymentRequest()
            {
                RestApiId = config.RestApiId,
                StageName = config.StageName,
            });

            var meta = string.Join(" | ", response.ResponseMetadata.Metadata.Select(q => $"{q.Key}={q.Value}"));

            if ((int)response.HttpStatusCode < 300)
            {
                context.Log.Write(
                    Cake.Core.Diagnostics.Verbosity.Normal,
                    Cake.Core.Diagnostics.LogLevel.Information,
                    $"Deployment for api {config.RestApiId} succeeded on stage {config.StageName} with http code {response.HttpStatusCode}; {meta}"
                );

                return response.Id;
            }
            else
            {
                var message = $"Deployment for api {config.RestApiId} failed on stage {config.StageName} with http code {response.HttpStatusCode}; {meta}";
                
                throw new Exception(message);
            }
        }
    }
    
    [CakeMethodAlias]
    public static async Task<string> CreateOrChangeRestApi(this ICakeContext context, PublishApiGatewayConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        config.EnsureValid();

        var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);

        using (var reader = new FileStream(config.SwaggerApiFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var mm = new MemoryStream())
        using (var client = new Amazon.APIGateway.AmazonAPIGatewayClient(credentials, config))
        {
            await reader.CopyToAsync(mm);
            mm.Position = 0;

            string restApiId;

            if (string.IsNullOrWhiteSpace(config.RestApiId))
            {
                context.Log.Write(
                    Cake.Core.Diagnostics.Verbosity.Normal,
                    Cake.Core.Diagnostics.LogLevel.Information,
                    $"Creating new api"
                );

                var api = await client.ImportRestApiAsync(new Amazon.APIGateway.Model.ImportRestApiRequest()
                {
                    Body = mm,
                    FailOnWarnings = config.FailOnWarnings
                });

                context.Log.Write(
                    Cake.Core.Diagnostics.Verbosity.Normal,
                    Cake.Core.Diagnostics.LogLevel.Information,
                    $"New api with id {api.Id} has been created"
                );

                restApiId = api.Id;
            }
            else
            {
                context.Log.Write(
                    Cake.Core.Diagnostics.Verbosity.Normal,
                    Cake.Core.Diagnostics.LogLevel.Information,
                    $"Overwriting api {config.RestApiId}"
                );

                await client.PutRestApiAsync(new Amazon.APIGateway.Model.PutRestApiRequest()
                {
                    Body = mm,
                    FailOnWarnings = config.FailOnWarnings,
                    Mode = config.PutMode,
                    RestApiId = config.RestApiId
                });

                context.Log.Write(
                    Cake.Core.Diagnostics.Verbosity.Normal,
                    Cake.Core.Diagnostics.LogLevel.Information,
                    $"Api {config.RestApiId} has been overriden"
                );

                restApiId = config.RestApiId;
            }

            return restApiId;
        }
    }
}