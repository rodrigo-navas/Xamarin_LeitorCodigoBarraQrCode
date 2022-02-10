using Leitor.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Leitor.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}