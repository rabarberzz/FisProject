using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ControllerDevTool
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BLEServerService bleServerService;
        public MainPage()
        {
            this.InitializeComponent();

            // Set the initial window size
            ApplicationView.PreferredLaunchViewSize = new Size(800, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            bleServerService = App.SharedBleServerService;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            bleServerService.NotifyClientDevices(InputTextBox.Text);
        }

        private void GoToServerControl_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ServerControlPage));
        }
    }
}
