﻿using System.Runtime.InteropServices;

namespace Athena.Services;

public static class Message
{
    // buttons
    public static readonly uint MB_OK = 0x00000000;
    public static readonly uint MB_YESNO = 0x00000004;
    public static readonly uint MB_DEFBUTTON1 = 0x00000000;

    // selected buttons
    public static readonly int BT_OK = 0x6;

    // icons
    public static readonly uint MB_ICONERROR = 0x00000010;
    public static readonly uint MB_ICONINFORMATION = 0x00000040;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hInstance, string lpText, string lpCaption, uint type);

    [DllImport("kernel32.dll")]
    public static extern int GetConsoleWindow();
}