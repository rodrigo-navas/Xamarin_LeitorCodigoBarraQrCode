using System.Collections.ObjectModel;

namespace Leitor.Interfaces
{
    public interface IBth
	{
		void Start(string name, int sleepTime, bool readAsCharArray);
		void Cancel();
		ObservableCollection<string> PairedDevices();
	}
}
