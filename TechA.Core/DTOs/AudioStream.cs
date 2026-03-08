namespace TechA.Core.DTOs;

public class AudioStream
{
    public const string SectionName = "AudioStream";

    public string DownstreamServiceUrl { get; set; } = string.Empty;

    public int BufferSize { get; set; } = 4 * 1024;
}
