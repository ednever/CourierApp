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

namespace CourierApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AppDbContext _context;
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(AppDbContext context)
    {
        InitializeComponent();
        _context = context;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var user = new User { Name = "Jane Doe", Age = 25 };
        _context.Users.Add(user);
        _context.SaveChanges();
    }
}