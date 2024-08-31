
namespace ConfigSetter.Model;
public class UpdateConfigParameters
{
    public required FileInfo Configuration { get; set; }
    public required FileInfo InputSettings { get; set; }
    public FileInfo? OutputFile { get; set; }
    public string OutputFormat { get; set; } = "yaml";
    public string Prefix { get; set; } = "DEV";
}