namespace Pratyaksh.Core;

public static class Clipboard
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    public static string GetClipboardText()
    {
        const uint CF_UNICODETEXT = 13;
        if (!OpenClipboard(IntPtr.Zero)) return string.Empty;
        try
        {
            IntPtr handle = GetClipboardData(CF_UNICODETEXT);
            if (handle == IntPtr.Zero) return string.Empty;
            return System.Runtime.InteropServices.Marshal.PtrToStringUni(handle) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            CloseClipboard();
        }
    }
}
