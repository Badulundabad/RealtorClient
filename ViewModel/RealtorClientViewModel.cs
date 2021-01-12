using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using RealtorClient.Model;
using RealtyModel;

namespace RealtorClient.ViewModel
{
    class RealtorClientViewModel : INotifyPropertyChanged
    {
        Client client;
        String name;
        String password;
        String checkPassword;
        Boolean isLoggedIn;

        public String Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public String Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }
        public String CheckPassword
        {
            get => checkPassword;
            set
            {
                checkPassword = value;
                OnPropertyChanged();
            }
        }
        public Boolean IsLoggedIn
        {
            get => isLoggedIn;
            private set
            {
                isLoggedIn = value;
                OnPropertyChanged();
            }
        }
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ObservableCollection<LogMessage> Log { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public RealtorClientViewModel()
        {
            client = new Client(Dispatcher.CurrentDispatcher);
            client.ConnectAsync(IPAddress.Loopback);
            Log = new ObservableCollection<LogMessage>();

            UpdateLog("Connected");

            client.Log.CollectionChanged += (sender, e) => UpdateLog(e.NewItems);
            client.IncomingOperations.CollectionChanged += (sender, e) => HandleOperation(e.NewItems);

            LoginCommand = new CustomCommand((obj) =>
            {
                Operation login = new Operation()
                {
                    IpAddress = "",
                    OperationNumber = new Guid(),
                    OperationType = OperationType.LOGIN,
                    Data = JsonSerializer.Serialize<User>
                    (
                        new User() { Role = Role.User, Name = this.Name, Password = this.Password }
                    )
                };
                client.SendMessage(login);
            });
            RegisterCommand = new CustomCommand((obj) =>
            {
                if (Password == CheckPassword)
                {
                    Operation register = new Operation()
                    {
                        IpAddress = "",
                        OperationNumber = new Guid(),
                        OperationType = OperationType.REGISTER,
                        Data = JsonSerializer.Serialize<User>
                       (
                           new User() { Role = Role.User, Name = this.Name, Password = this.Password }
                       )
                    };
                    client.SendMessage(register);
                }
                else UpdateLog("Passwords doesn't match");
            });
        }

        private void UpdateLog(String message)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                Log.Add(new LogMessage(DateTime.Now.ToString("dd:MM:yy hh:mm"), message));
            }));
        }
        private void UpdateLog(IList messages)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                foreach (LogMessage message in messages)
                {
                    try
                    {
                        Log.Add(message);
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(ex.Message);
                    }
                }
            }));
        }
        private void HandleOperation(IList messages)
        {
            foreach (Operation operation in messages)
            {
                try
                {
                    if (operation.OperationType == OperationType.REGISTER)
                    {
                        if (operation.IsSuccessfully)
                        {
                            UpdateLog("Registration is successfull");
                        }
                        else UpdateLog("Registration is not successfull");
                    }
                    else if (operation.OperationType == OperationType.LOGIN)
                    {
                        if (operation.IsSuccessfully)
                        {
                            IsLoggedIn = true;
                            UpdateLog("Log in is successfull");
                        }
                        else UpdateLog("Log in is not successfull");
                    }
                }
                catch (Exception ex)
                {
                    UpdateLog(ex.Message);
                }
            }
        }
        private void OnPropertyChanged([CallerMemberName] String prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
