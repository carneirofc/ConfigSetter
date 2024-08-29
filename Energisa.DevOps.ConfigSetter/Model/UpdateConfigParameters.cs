
namespace Energisa.DevOps.ConfigSetter.Model;
public class UpdateConfigParameters
{
    public required FileInfo Configuration { get; set; }
    public required FileInfo InputSettings { get; set; }
    public FileInfo? OutputSettings { get; set; }
}