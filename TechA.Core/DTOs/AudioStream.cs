namespace TechA.Core.DTOs;

public class AudioStream
{
    public const string SectionName = "AudioStream";

    public string SttServiceUrl { get; set; } = string.Empty;

    public int BufferSize { get; set; } = 4 * 1024;

    public string SttApiToken { get; set; } = string.Empty;
}
