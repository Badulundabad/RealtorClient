using System.Windows;
using RealtorClient.ViewModel;

namespace RealtorClient.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new RealtorClientViewModel();
        }
    }
}
