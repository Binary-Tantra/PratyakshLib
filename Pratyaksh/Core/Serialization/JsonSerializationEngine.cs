using System.Text.Json;

namespace Pratyaksh.Core.Serialization;

public class JsonSerializationEngine : ISerializationEngine
{
    private JsonSerializerOptions? serializeOptions;
    private JsonSerializerOptions? deserializeOptions;

    public JsonSerializationEngine()
    {
        serializeOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        deserializeOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public string Serialize<T>(T obj) where T : BaseDTO
    {
        return JsonSerializer.Serialize(obj, serializeOptions);
    }

    public T? Deserialize<T>(string input) where T : BaseDTO
    {
        T? result = JsonSerializer.Deserialize<T>(input, deserializeOptions);

        if (result == null)
            Console.WriteLine($"Error: Deserialization of type {typeof(T).Name} returned null.");

        return result;
    }

    public JsonElement SerializeToElement<T>(T obj)
    {
        return JsonSerializer.SerializeToElement(obj);
    }

    public string GetExtension()
    {
        return "json";
    }
}
