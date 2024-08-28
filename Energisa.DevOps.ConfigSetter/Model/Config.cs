namespace Energisa.DevOps.SettingsParser.Model
{
    public class JsonPatch
    {
        public string Op { get; set; }
        public string Path { get; set; }
    }
    public class Settings
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<JsonPatch> JsonPatch { get; set; }
    }
    public class Config
    {
        public List<string> Environments { get; set; }
        public Settings Settings { get; set; }
    }
}