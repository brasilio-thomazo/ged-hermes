using log4net;
using System.Windows;

namespace br.dev.optimus.hermes.app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILog log = LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Finish(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Load page of Execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadExecute(object sender, RoutedEventArgs e)
        {
            if (mainFrame.Content != null && mainFrame.Content.GetType().Name == typeof(PageExecute).Name) return;
            var page = new PageExecute();
            mainFrame.Content = page;
        }

        /// <summary>
        /// Load page of tests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestHandler(object sender, RoutedEventArgs e)
        {
            if (mainFrame.Content != null && mainFrame.Content.GetType().Name == typeof(TestPage).Name) return;
            mainFrame.Content = new TestPage();
        }
    }
}
