using System;

public class PublishApiGatewayConfig : BasicConfig
{
    public string SwaggerApiFilePath { get; set; }
    public bool FailOnWarnings { get; set; }
    public Amazon.APIGateway.PutMode PutMode { get; set; }

    internal override void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(SwaggerApiFilePath))
        {
            throw new Exception($"{nameof(SwaggerApiFilePath)} cannot be null or empty");
        }

        if (!string.IsNullOrWhiteSpace(RestApiId))
        {
            if (PutMode == null)
            {
                throw new Exception($"{nameof(PutMode)} must be specified, if {nameof(RestApiId)} is specified");
            }
        }

        base.EnsureValid();
    }
}
