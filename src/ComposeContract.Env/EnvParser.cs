namespace ComposeContract.Env;

public static class EnvParser
{
    public static EnvFile ParseFile(string path)
    {
        if (!File.Exists(path)) return new EnvFile();
        return Parse(File.ReadAllText(path));
    }

    public static EnvFile Parse(string content)
    {
        var file = new EnvFile();
        using var reader = new StringReader(content);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                line = line[7..].TrimStart();
            var eq = line.IndexOf('=');
            var key = eq >= 0 ? line[..eq].Trim() : line.Trim();
            if (key.Length == 0) continue;
            file.Keys.Add(key);
        }
        return file;
    }
}
