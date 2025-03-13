using CourierApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;



namespace CourierApp
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private IServiceScope _scope;

        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public App()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(@"Data Source=..\..\..\MyDatabase.db"));
            services.AddScoped<MainWindow>();
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _scope = _serviceProvider.CreateScope();
            var mainWindow = _scope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _scope?.Dispose();
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }

    }
}