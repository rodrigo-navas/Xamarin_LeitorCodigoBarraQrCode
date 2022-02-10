using Leitor.ViewModels;
using Xamarin.Forms;

namespace Leitor.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            BindingContext = new AboutViewModel();

            Device.BeginInvokeOnMainThread(() =>
            {
                txtCodBarra.Focus();
            });
        }

        private void txtCodBarra_Unfocused(object sender, FocusEventArgs e)
        {
            (BindingContext as AboutViewModel).AdicionarItemCommand(null);
        }

        protected override void OnAppearing()
        {
            MessagingCenter.Subscribe<App>((App)Application.Current, "NovoProduto", (obj) =>
            {
                txtCodBarra.Focus();
            });
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<App>((App)Application.Current, "NovoProduto");
        }
    }
}