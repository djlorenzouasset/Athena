using System.Runtime.InteropServices;

namespace Athena.Services;

public static partial class MessageService
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

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    public static int Show(string title, string text, uint flags)
    {
        return MessageBox(GetConsoleWindow(), text, title, flags);
    }
}