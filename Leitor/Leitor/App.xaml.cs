using Leitor.Services;
using Xamarin.Forms;

namespace Leitor
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            MessagingCenter.Send<App>(this, "Sleep"); // When app sleep, send a message so I can "Cancel" the connection
        }

        protected override void OnResume()
        {
            MessagingCenter.Send<App>(this, "Resume"); // When app resume, send a message so I can "Resume" the connection
        }
    }
}
