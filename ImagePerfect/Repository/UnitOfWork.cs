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

        public IFolderRepository Folder { get; private set; }
        public IImageRepository Image { get; private set; }
        public ISettingsRepository Settings { get; private set; }
        public ISaveDirectoryRepository SaveDirectory { get; private set; }
        public UnitOfWork(MySqlConnection db)
        {
            _connection = db;
            Folder = new FolderRepository(_connection);
            Image = new ImageRepository(_connection);
            Settings = new SettingsRepository(_connection);
            SaveDirectory = new SaveDirectoryRepository(_connection);
        }
    }
}
