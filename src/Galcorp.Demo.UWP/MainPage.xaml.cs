// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Galcorp.Auth;
using Galcorp.Auth.UWP;

namespace Galcorp.Demo.UWP
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                
                var a = new Authenticator(
                    new UWPWrapper(Credentials.ClientId, Credentials.ClientSecret, new UWPPlatform(Handler)));

                var c = a.Authenticate("google").Result;
            }).Start();
        }

        private void Handler(object sender, string text)
        {
        }
    }
}