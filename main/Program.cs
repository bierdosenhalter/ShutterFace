namespace ShutterFace
{
    using ShutterFace.Resources;
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

            var savedCulture = Resources.CultureConfig.LoadDefaultCulture();
            if (savedCulture != null)
            {
                SetCulture(savedCulture);
            }

            Application.Run(new VideoPlayer());
        }

        public static void SetCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            ControlResourceManager.Culture = culture;

            if (Application.OpenForms["VideoPlayer"] is VideoPlayer player)
            {
                player.ApplyLanguage();
            }
        }
    }
}
