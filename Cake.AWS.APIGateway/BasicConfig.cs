using System;

public class BasicConfig : Amazon.APIGateway.AmazonAPIGatewayConfig
{
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string RestApiId { get; set; }

    internal virtual void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(AccessKey))
        {
            throw new Exception($"{nameof(AccessKey)} cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new Exception($"{nameof(SecretKey)} cannot be null or empty");
        }
    }
}