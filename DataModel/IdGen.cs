namespace RaylibNodeLibrary.DataModel;

public static class IdGen
{
    private static int id = -1;

    public static int GetNewID()
    {
        id++;
        return id;
    }
}
