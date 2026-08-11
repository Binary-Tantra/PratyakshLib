namespace Pratyaksh.Core.Serialization;

public class BaseDTO {}

public class BaseSerializer
{
    protected readonly ISerializationEngine engine;
    public string Extension { get => engine.GetExtension(); }

    public BaseSerializer(ISerializationEngine engine)
    {
        this.engine = engine;
    }
}
