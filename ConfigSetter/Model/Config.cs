namespace ConfigSetter.Model;

public class JsonPatch
{
    public required string Op { get; set; }
    public required string Path { get; set; }
    public object? Value { get; set; }
}
public class Setting
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required List<JsonPatch> JsonPatch { get; set; }
}
public class Setup
{
    public required List<JsonPatch> JsonPatch { get; set; }
}
public class Config
{
    public required List<Setting> Settings { get; set; }
    public required Setup Setup { get; set; }
}