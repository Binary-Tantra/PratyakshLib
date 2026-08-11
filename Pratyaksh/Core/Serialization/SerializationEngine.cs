namespace Pratyaksh.Core.Serialization;

public interface ISerializationEngine
{
    string Serialize<T>(T obj) where T : BaseDTO;
    T? Deserialize<T>(string input) where T : BaseDTO;

    string GetExtension();
}