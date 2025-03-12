using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace CourierApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    public MainWindow()
    {
        DatabaseHelper.InitializeDatabase();
        InitializeComponent();
        UsersListView.ItemsSource = DatabaseHelper.GetUsers();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        DatabaseHelper.AddUser("Edgar", 18);
        
    }
}