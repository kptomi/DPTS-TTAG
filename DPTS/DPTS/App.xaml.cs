using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

using DPTS.View;
using DPTS.ViewModel;

namespace DPTS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DPTSViewModel _ViewModel;
        private MainWindow _MainWindow;

        public App()
        {
            _ViewModel = new DPTSViewModel();
            _MainWindow = new MainWindow()
            {
                DataContext = _ViewModel
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // kultúrkörnyezet beállítása magyarra
            Thread.CurrentThread.CurrentCulture = new CultureInfo("hu-HU");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("hu-HU");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            base.OnStartup(e);
            _MainWindow.Show();
        }
    }
}
