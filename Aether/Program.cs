using Aether.States;
using System;
using System.Windows.Forms;

namespace Aether
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Application configuration
            ApplicationConfiguration.Initialize();

            // Program henüz başladığı an State sistemlerini tetikle ve hazırla
            ClientState.Initialize();
            PageState.Initialize();

            Application.Run(new Forms.MainForm());
        }
    }
}