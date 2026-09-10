using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StackContract.Core;

public static class ContractLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    public static ContractDocument Load(string path)
    {
        var yaml = File.ReadAllText(path);
        return Deserialize(yaml);
    }

    public static ContractDocument Deserialize(string yaml)
    {
        var doc = Deserializer.Deserialize<ContractDocument>(yaml)
                  ?? throw new InvalidOperationException("Contract YAML deserialized to null.");
        if (doc.Version <= 0) doc.Version = 1;
        doc.Profiles ??= new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        if (!doc.Profiles.ContainsKey("default"))
            doc.Profiles["default"] = new List<string>();
        return doc;
    }

    public static string Serialize(ContractDocument doc) => Serializer.Serialize(doc);

    public static void Save(string path, ContractDocument doc)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, Serialize(doc));
    }
}
