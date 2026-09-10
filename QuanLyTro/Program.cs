using System;
using System.Windows.Forms;

namespace QuanLyTro;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
