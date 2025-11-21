using System.Runtime.InteropServices;

namespace Athena.Services;

public static class MessageService
{
    // buttons
    public const uint MB_OK = 0x00000000;
    public const uint MB_YESNO = 0x00000004;
    public const uint MB_DEFBUTTON1 = 0x00000000;

    // selected buttons
    public const uint BT_OK = 0x1;
    public const uint BT_YES = 0x6;

    // icons
    public const uint MB_ICONERROR = 0x00000010;
    public const uint MB_ICONWARNING = 0x00000030;
    public const uint MB_ICONINFORMATION = 0x00000040;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hInstance, string lpText, string lpCaption, uint type);

    [DllImport("kernel32.dll")]
    private static extern int GetConsoleWindow();

    public static int Show(string title, string caption, uint flags)
    {
        return MessageBox(GetConsoleWindow(), caption, title, flags);
    }
}