using ImagePerfect.Repository.IRepository;
using ImagePerfect.Repository;
using ImagePerfect.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace ImagePerfect.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {

            //appsettings.json
            IConfiguration config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

            // Get a configuration section
            IConfigurationSection section = config.GetSection("ConnectionStrings");

            collection.AddMySqlDataSource(section["DefaultConnection"]);
            collection.AddScoped<IUnitOfWork, UnitOfWork>();
            collection.AddTransient<MainWindowViewModel>();
        }
    }
}
