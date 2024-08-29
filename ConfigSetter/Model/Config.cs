namespace ConfigSetter.Model
{
    public class JsonPatch
    {
        public required string Op { get; set; }
        public required string Path { get; set; }
    }
    public class Setting
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required List<JsonPatch> JsonPatch { get; set; }
    }
    public class Config
    {
        public required List<string> Environments { get; set; }
        public required List<Setting> Settings { get; set; }
    }
}