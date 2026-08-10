namespace Pratyaksh.Core;

public static class IdGen
{
    private static int id = -1;

    public static int GetNewID()
    {
        id++;
        return id;
    }

    public static void SetCurrentId(int maxId)
    {
        id = maxId;
    }

    public static int CurrentId => id;
}
