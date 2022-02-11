using Android.Bluetooth;
using Android.OS;
using Java.IO;
using Java.Util;
using Leitor.Droid;
using Leitor.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(Bth))]
namespace Leitor.Droid
{
    public class Bth : IBth
    {
        private CancellationTokenSource _ct { get; set; }

        #region IBth implementation
        /// <summary>
        /// Start the "reading" loop 
        /// </summary>
        /// <param name="name">Name of the paired bluetooth device (also a part of the name)</param>
        public void Start(string name, int sleepTime = 200, bool readAsCharArray = false)
        {
            Task.Run(() => Loop(name, sleepTime, readAsCharArray));
        }

        private async Task Loop(string name, int sleepTime, bool readAsCharArray)
        {
            BluetoothDevice device = null;
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            BluetoothSocket BthSocket = null;

            _ct = new CancellationTokenSource();
            while (_ct.IsCancellationRequested == false)
            {
                try
                {
                    Thread.Sleep(sleepTime);

                    adapter = BluetoothAdapter.DefaultAdapter;

                    if (adapter == null)
                        System.Diagnostics.Debug.WriteLine("No Bluetooth adapter found.");
                    else
                        System.Diagnostics.Debug.WriteLine("Adapter found!!");

                    if (!adapter.IsEnabled)
                        System.Diagnostics.Debug.WriteLine("Bluetooth adapter is not enabled.");
                    else
                        System.Diagnostics.Debug.WriteLine("Adapter enabled!");

                    System.Diagnostics.Debug.WriteLine("Try to connect to " + name);

                    foreach (var bd in adapter.BondedDevices)
                    {
                        System.Diagnostics.Debug.WriteLine("Paired devices found: " + bd.Name.ToUpper());
                        if (bd.Name.ToUpper().IndexOf(name.ToUpper()) >= 0)
                        {

                            System.Diagnostics.Debug.WriteLine("Found " + bd.Name + ". Try to connect with it!");
                            device = bd;
                            break;
                        }
                    }

                    if (device == null)
                        System.Diagnostics.Debug.WriteLine("Named device not found.");
                    else
                    {
                        ParcelUuid[] uuids = null;
                        if (device.FetchUuidsWithSdp())
                            uuids = device.GetUuids();

                        if ((uuids != null) && (uuids.Length > 0))
                        {
                            foreach (var uuid in uuids)
                            {
                                try
                                {
                                    if ((int)Android.OS.Build.VERSION.SdkInt >= 10)
                                        BthSocket = device.CreateInsecureRfcommSocketToServiceRecord(uuid.Uuid);
                                    else
                                        BthSocket = device.CreateRfcommSocketToServiceRecord(uuid.Uuid);

                                    await BthSocket.ConnectAsync();
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(ex.Message);
                                }
                            }
                        }

                        if (BthSocket != null)
                        {
                            if (BthSocket.IsConnected)
                            {
                                System.Diagnostics.Debug.WriteLine("Connected!");
                                var mReader = new InputStreamReader(BthSocket.InputStream);
                                var buffer = new BufferedReader(mReader);

                                while (_ct.IsCancellationRequested == false)
                                {
                                    if (buffer.Ready())
                                    {
                                        char[] chr = new char[100];
                                        string barcode = "";
                                        if (readAsCharArray)
                                        {
                                            await buffer.ReadAsync(chr);
                                            foreach (char c in chr)
                                            {
                                                if (c == '\0')
                                                    break;
                                                barcode += c;
                                            }
                                        }
                                        else
                                            barcode = await buffer.ReadLineAsync();

                                        if (barcode.Length > 0)
                                        {
                                            System.Diagnostics.Debug.WriteLine("Letto: " + barcode);
                                            Xamarin.Forms.MessagingCenter.Send<App, string>((App)Xamarin.Forms.Application.Current, "Barcode", barcode);
                                        }
                                        else
                                            System.Diagnostics.Debug.WriteLine("No data");
                                    }
                                    else
                                        System.Diagnostics.Debug.WriteLine("No data to read");

                                    System.Threading.Thread.Sleep(sleepTime);

                                    if (!BthSocket.IsConnected)
                                    {
                                        System.Diagnostics.Debug.WriteLine("BthSocket.IsConnected = false, Throw exception");
                                        throw new Exception();
                                    }
                                }

                                System.Diagnostics.Debug.WriteLine("Exit the inner loop");
                            }
                        }
                        else
                            System.Diagnostics.Debug.WriteLine("BthSocket = null");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("EXCEPTION: " + ex.Message);
                }
                finally
                {
                    if (BthSocket != null)
                        BthSocket.Close();
                    device = null;
                    adapter = null;
                }
            }

            System.Diagnostics.Debug.WriteLine("Exit the external loop");
        }

        /// <summary>
        /// Cancel the Reading loop
        /// </summary>
        /// <returns><c>true</c> if this instance cancel ; otherwise, <c>false</c>.</returns>
        public void Cancel()
        {
            if (_ct != null)
            {
                System.Diagnostics.Debug.WriteLine("Send a cancel to task!");
                _ct.Cancel();
            }
        }

        public ObservableCollection<string> PairedDevices()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            ObservableCollection<string> devices = new ObservableCollection<string>();

            foreach (var bd in adapter.BondedDevices)
                devices.Add(bd.Name);

            return devices;
        }
        #endregion
    }
}