using System.Windows;

namespace br.dev.optimus.hermes.app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            dotenv.net.DotEnv.Load();

            base.OnStartup(e);
            log.Info("Startup");
        }
    }
}
