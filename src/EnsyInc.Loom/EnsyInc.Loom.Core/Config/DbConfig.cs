
using EnsyNet.Core.Configurations;

namespace EnsyInc.Loom.Core.Config;

public sealed record DbConfig : IConfig
{
    public static string ConfigName => "Db";

    public required string ConnectionString { get; init; }

    public bool IsValid()
        => !string.IsNullOrWhiteSpace(ConnectionString);
}
