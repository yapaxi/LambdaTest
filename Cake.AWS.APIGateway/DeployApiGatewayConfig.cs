public class DeployApiGatewayConfig : BasicConfig
{
    public string StageName { get; set; }

    internal override void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(StageName))
        {
            throw new System.Exception($"{nameof(StageName)} cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(RestApiId))
        {
            throw new System.Exception($"{nameof(RestApiId)} cannot be null or empty");
        }

        base.EnsureValid();
    }
}
