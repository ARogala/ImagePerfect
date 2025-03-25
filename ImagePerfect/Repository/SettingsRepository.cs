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
    public class SettingsRepository : Repository<Settings>, ISettingsRepository
    {
        private readonly MySqlConnection _connection;

        public SettingsRepository(MySqlConnection db) : base(db) 
        { 
            _connection = db;
        }

        //any Settings model specific database methods here
    }
}
