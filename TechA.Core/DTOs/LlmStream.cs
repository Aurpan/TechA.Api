namespace TechA.Core.DTOs;

public class LlmStream
{
    public const string SectionName = "LlmStream";

    public string BaseUrl { get; set; } = string.Empty;

    public string GenerateEndpoint { get; set; } = "/v1/llm/extract:expenses";

    public double Temperature { get; set; } = 0.3;

    public int MaxOutputTokens { get; set; } = 512;

    public string ApiToken { get; set; } = string.Empty;
}
