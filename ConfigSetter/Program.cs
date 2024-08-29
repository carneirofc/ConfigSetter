using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using ConfigSetter.Commands;

int status = new Root().Invoke(args);
return status;