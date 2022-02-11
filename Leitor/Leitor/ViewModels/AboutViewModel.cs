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
        public Command AddItemCommand { get; }
        public Command CameraCommand { get; }
        public Command ExcluirItemCommand { get; }

        private string _codigoBarra;
        public string CodigoBarra
        {
            get => _codigoBarra;
            set => SetProperty(ref _codigoBarra, value);
        }

        private decimal _totalProdutos;
        public decimal TotalProdutos
        {
            get => _totalProdutos;
            set => SetProperty(ref _totalProdutos, value);
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
            TotalProdutos = 0;
            Title = "Leitor";
            MinhaLista = new ObservableCollection<ProdutoLeitor>();

            AddItemCommand = new Command(() =>
            {
                if (string.IsNullOrEmpty(CodigoBarra))
                    return;

                AdicionarItemLista(CodigoBarra, "Produto Manual");
                CodigoBarra = string.Empty;
            });

            CameraCommand = new Command(async () =>
            {
                ZXingScannerPage scan = new ZXingScannerPage();
                await Application.Current.MainPage.Navigation.PushModalAsync(scan);

                scan.OnScanResult += (result) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.Navigation.PopModalAsync();
                        AdicionarItemLista(result.Text, "Produto Câmera");
                    });
                };
            });

            ExcluirItemCommand = new Command<ProdutoLeitor>(async (item) =>
            {
                var answer = await Application.Current.MainPage.DisplayAlert("Exclusão", "Deseja remover o item?", "Yes", "No");

                if (answer)
                    MinhaLista.Remove(item);

                EnviarMensagemFocusNovoProduto();
            });

            //IniciarConexaoBluetooth();
        }

        public void AdicionarItemColetor()
        {
            if (string.IsNullOrEmpty(CodigoBarra))
                return;

            AdicionarItemLista(CodigoBarra, "Produto Coletor");
            CodigoBarra = string.Empty;
        }

        private void AdicionarItemLista(string codigoBarras, string nomeProduto)
        {
            MinhaLista.Insert(0, new ProdutoLeitor
            {
                CodigoBarras = codigoBarras,
                NomeProduto = nomeProduto
            });

            EnviarMensagemFocusNovoProduto();
        }

        private void NotificarQuantidadeProdutos()
        {
            TotalProdutos = MinhaLista.Count;
        }

        private void EnviarMensagemFocusNovoProduto()
        {
            MessagingCenter.Send<App>((App)Application.Current, "NovoProduto");
            NotificarQuantidadeProdutos();
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