using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using RealtyModel;
using System.Collections;

namespace RealtorClient.Model
{
    public class Client : INotifyPropertyChanged
    {
        Socket socket;
        Boolean isConnected;
        Dispatcher uiDispatcher;

        internal Boolean IsConnected
        {
            get => isConnected;
            private set
            {
                isConnected = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<LogMessage> Log { get; private set; }
        public ObservableCollection<Operation> IncomingOperations { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public Client(Dispatcher dispatcher)
        {
            uiDispatcher = dispatcher;
            Log = new ObservableCollection<LogMessage>();
            IncomingOperations = new ObservableCollection<Operation>();
        }

        public async void ConnectAsync(IPAddress ipAddress)
        {
            await Task.Run(() =>
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 8005);
                    socket.Connect(iPEndPoint);

                    IsConnected = true;
                    while (IsConnected) ReceiveMessage();
                }
                catch (Exception ex)
                {
                    UpdateLog(ex.Message);
                }
                finally
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            });
        }
        public void SendMessage(Operation operation)
        {
            try
            {
                String json = JsonSerializer.Serialize<Operation>(operation);
                Byte[] data = Encoding.UTF8.GetBytes(json);
                socket.Send(data);
            }
            catch (Exception ex)
            {
                UpdateLog(ex.Message);
            }
        }
        internal void Disconnect()
        {
            uiDispatcher.BeginInvoke(new Action(() =>
            {
                IsConnected = false;
            }));
        }
        private void ReceiveMessage()
        {
            Int32 byteCount = 0;
            Byte[] buffer = new Byte[256];
            StringBuilder message = new StringBuilder();

            do
            {
                byteCount = socket.Receive(buffer);
                message.Append(Encoding.UTF8.GetString(buffer), 0, byteCount);
            }
            while (socket.Available > 0);

            if (!String.IsNullOrWhiteSpace(message.ToString()))
            {
                Operation operation = JsonSerializer.Deserialize<Operation>(message.ToString());
                uiDispatcher.BeginInvoke(new Action(() =>
                {
                    IncomingOperations.Add(operation);
                }));
            }
        }
        private void UpdateLog(String message)
        {
            uiDispatcher.BeginInvoke(new Action(() =>
            {
                Log.Add(new LogMessage(DateTime.Now.ToString("dd:MM:yy hh:mm"), message));
            }));
        }
        private void OnPropertyChanged([CallerMemberName] String prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
