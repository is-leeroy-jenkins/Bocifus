

namespace Bocifus
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class VersionWindow
    {
        /// <summary>
        /// The busy
        /// </summary>
        private protected bool _busy;

        /// <summary>
        /// The path
        /// </summary>
        private protected object _entry = new object();

        /// <summary>
        /// The seconds
        /// </summary>
        private protected int _seconds;

        /// <summary>
        /// The time
        /// </summary>
        private protected int _time;

        /// <summary>
        /// The timer
        /// </summary>
        private protected Timer _timer;

        /// <summary>
        /// The timer
        /// </summary>
        private protected TimerCallback _timerCallback;

        /// <summary>
        /// The status update
        /// </summary>
        private protected Action _statusUpdate;

        /// <summary>
        /// The theme
        /// </summary>
        private protected readonly DarkMode _theme = new DarkMode();

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="VersionWindow"/> class.
        /// </summary>
        public VersionWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            try
            {
                VersionText.Text = "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch (Exception ex)
            {
                ModernWpf.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Called when [ok button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the KeyDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        /// <summary>
        /// Called when [version text mouse left button down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/>
        /// instance containing the event data.</param>
        private void OnVersionTextMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var exeConfigurationPath = 
                ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

            Clipboard.SetText(exeConfigurationPath);
            var _msg = "Configuration file path copied to clipboard.";
            var _b = "Information";
            ModernWpf.MessageBox.Show(_msg, _b, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Called when [github URL text mouse left button down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/>
        /// instance containing the event data.</param>
        private void OnGithubUrlTextMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var _a = "Open the URL in the browser?";
            var _b = "Confirmation";
            var result = ModernWpf.MessageBox.Show(_a, _b, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo(GithubUrlText.Text) { UseShellExecute = true });
            }
        }
    }
}
