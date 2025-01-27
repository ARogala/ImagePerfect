using MySqlConnector;
using ImagePerfect.Repository.IRepository;
//https://dotnettutorials.net/lesson/unit-of-work-csharp-mvc/

namespace ImagePerfect.Repository
{
    //goal is to use UnitOfWork to share the _connection
    //this passes down one connection throught the entire inheritance chain
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MySqlConnection _connection;
        public UnitOfWork(MySqlConnection db)
        {
            _connection = db;

        }
    }
}
