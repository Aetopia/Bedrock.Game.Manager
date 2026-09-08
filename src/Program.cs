using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bedrock.Game.Manager.Interface;
using static System.Windows.Forms.UnhandledExceptionMode;

[assembly: SupportedOSPlatform("windows10.0.19041.0")]

namespace Bedrock.Game.Manager;

static class Program
{
    [STAThread]
    static async Task Main()
    {
        Application.EnableVisualStyles();

        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(CatchException);

        Application.SetColorMode(SystemColorMode.Dark);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        Application.Run(new MainForm());
    }
}