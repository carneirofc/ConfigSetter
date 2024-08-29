using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Energisa.DevOps.ConfigSetter.Commands;

int status = new Root().Invoke(args);
return status;