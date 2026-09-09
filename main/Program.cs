namespace ShutterFace
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new VideoPlayer());
        }

        public static void SetCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            if (Application.OpenForms["VideoPlayer"] is VideoPlayer player)
            {
                player.ApplyLanguage();
            }
        }
    }
}
