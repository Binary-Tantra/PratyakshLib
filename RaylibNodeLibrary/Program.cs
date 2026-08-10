using RaylibNodeLibrary;

public class Program
{
    static void Main(string[] args)
    {
        GEngine engine = new(1024, 576);
        engine.Start();
    }
}