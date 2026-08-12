using Pratyaksh.Node.Editor;

public class Program
{
    static void Main(string[] args)
    {
        NodeEditorEngine engine = new(1024, 576);
        engine.Start();
    }
}