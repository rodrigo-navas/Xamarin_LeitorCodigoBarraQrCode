using Leitor.Interfaces;
using Leitor.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Leitor.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private ObservableCollection<ProdutoLeitor> minhaLista;
        public ObservableCollection<ProdutoLeitor> MinhaLista { get => minhaLista; set => minhaLista = value; }
        public Command AddItem { get; }
        public Command Camera { get; }

        private string _codigoBarra;
        public string CodigoBarra
        {
            get => _codigoBarra;
            set => SetProperty(ref _codigoBarra, value);
        }

        #region [ Bluetooth ]
        public ObservableCollection<string> ListOfBarcodes { get; set; } = new ObservableCollection<string>();
        public string SelectedBthDevice { get; set; } = "";
        bool _isConnected { get; set; } = false;
        int _sleepTime { get; set; } = 250;
        ICommand DisconnectCommand;
        #endregion [ Bluetooth ]

        public AboutViewModel()
        {
            Title = "Leitor";
            MinhaLista = new ObservableCollection<ProdutoLeitor>();

            AddItem = new Command(AdicionarItemCommand);
            Camera = new Command(CameraCommand);

            //IniciarConexaoBluetooth();
        }

        public void AdicionarItemCommand(object obj)
        {
            if (string.IsNullOrEmpty(CodigoBarra))
            {
                //await Application.Current.MainPage.DisplayAlert("Código inválido", "", "Ok");
                return;
            }

            AdicionarItemLista(CodigoBarra, "Produto Manual");
            CodigoBarra = string.Empty;
        }

        private async void CameraCommand(object obj)
        {
            var scan = new ZXingScannerPage();
            await Application.Current.MainPage.Navigation.PushModalAsync(scan);

            scan.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                    AdicionarItemLista(result.Text, "Produto Câmera");
                });
            };
        }

        private void AdicionarItemLista(string codigoBarras, string nomeProduto)
        {
            MinhaLista.Add(new ProdutoLeitor
            {
                CodigoBarras = codigoBarras,
                NomeProduto = nomeProduto
            });

            MessagingCenter.Send<App>((App)Application.Current, "NovoProduto");
        }

        private void IniciarConexaoBluetooth()
        {
            MessagingCenter.Subscribe<App>(this, "Sleep", (obj) =>
            {
                // When the app "sleep", I close the connection with bluetooth
                if (_isConnected)
                    DependencyService.Get<IBth>().Cancel();

            });

            MessagingCenter.Subscribe<App>(this, "Resume", (obj) =>
            {
                // When the app "resume" I try to restart the connection with bluetooth
                if (_isConnected)
                    DependencyService.Get<IBth>().Start(SelectedBthDevice, _sleepTime, true);

            });

            this.DisconnectCommand = new Command(() =>
            {
                // Disconnect from bth device
                DependencyService.Get<IBth>().Cancel();
                MessagingCenter.Unsubscribe<App, string>(this, "Barcode");
                _isConnected = false;
            });

            // Try to connect to a bth device
            DependencyService.Get<IBth>().Start(SelectedBthDevice, _sleepTime, true);
            _isConnected = true;

            // Receive data from bth device
            MessagingCenter.Subscribe<App, string>(this, "Barcode", (sender, arg) =>
            {
                // Add the barcode to a list (first position)
                ListOfBarcodes.Insert(0, arg);
            });
        }
    }
}