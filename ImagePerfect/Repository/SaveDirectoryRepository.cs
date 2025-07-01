using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Repository
{
    public class SaveDirectoryRepository : Repository<SaveDirectory>, ISaveDirectoryRepository
    {
        private readonly MySqlConnection _connection;

        public SaveDirectoryRepository(MySqlConnection db) : base(db)
        {
            _connection = db;
        }

        //any SaveDirectory model specific database methods here
    }
}
