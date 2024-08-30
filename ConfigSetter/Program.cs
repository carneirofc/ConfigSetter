using ConfigSetter.Commands;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

int status = new Root().Invoke(args);
return status;