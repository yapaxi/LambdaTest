using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;

public class AmazonApiGatewayExtentions : IOpenApiExtension
{
    public AmazonApiGatewayExtentions(string credentials, string lambdaUri, string httpMethod)
    {
        Credentials = credentials;
        LambdaUri = lambdaUri;
        HttpMethod = httpMethod;
    }

    public string Credentials { get; }
    public string LambdaUri { get; }
    public string HttpMethod { get; }

    public void Write(IOpenApiWriter writer)
    {
        writer.WriteStartObject();

        if (!string.IsNullOrWhiteSpace(Credentials))
        {
            writer.WriteProperty("credentials", Credentials);
        }

        writer.WritePropertyName("responses");
        {
            writer.WriteStartObject();
            writer.WritePropertyName("default");
            {
                writer.WriteStartObject();
                writer.WriteProperty("statusCode", "200");
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        writer.WriteProperty("uri", LambdaUri);
        writer.WriteProperty("passthroughBehavior", "when_no_templates");
        writer.WriteProperty("httpMethod", HttpMethod);
        writer.WriteProperty("type", "aws");
        writer.WriteEndObject();
    }
}
