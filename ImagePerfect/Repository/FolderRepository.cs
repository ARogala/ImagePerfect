using MySqlConnector;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;

namespace ImagePerfect.Repository
{
    public class FolderRepository : Repository<Folder>, IFolderRepository
    {
        private readonly MySqlConnection _connection;

        public FolderRepository(MySqlConnection db) : base(db) 
        {
            _connection = db;
        }
        //any Folder model specific database methods here
    }
}
