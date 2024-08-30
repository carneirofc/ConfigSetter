using ConfigSetter.Commands;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

int status = new RootCommand().Invoke(args);
return status;