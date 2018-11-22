

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Galcorp.Demo.UWP
{
    using System.Threading;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Auth.UWP;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string clientID = "581786658708-r4jimt0msgjtp77b15lonfom92ko6aeg.apps.googleusercontent.com";
        private const string redirectURI = "pw.oauth2:/oauth2redirect";

        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(delegate
            {
                var a = new UWPWrapper(clientID, redirectURI);
                var c = a.GetToken().Result;

                TokenStore s = new TokenStore();
                s.Store("googleToken", c).Wait();

            })).Start();
        }
    }
}