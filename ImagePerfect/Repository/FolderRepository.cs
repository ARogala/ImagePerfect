using MySqlConnector;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Generic;

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
        public async Task<Folder?> GetRootFolder()
        {
            string sql = @"SELECT * FROM folders WHERE IsRoot = True";
            return await _connection.QuerySingleOrDefaultAsync<Folder>(sql);
        }
        public async Task<bool> AddFolderCsv(string filePath)
        {
            int rowsEffected = 0;
            var bulkLoader = new MySqlBulkLoader(_connection)
            {
                FileName = filePath,
                TableName = "folders",
                CharacterSet = "UTF8",
                NumberOfLinesToSkip = 1,
                FieldTerminator = ",",
                FieldQuotationCharacter = '"',
                FieldQuotationOptional = true,
                Local = true,
            };
            rowsEffected = await bulkLoader.LoadAsync();
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<List<Folder>> GetFoldersInDirectory(string directoryPath)
        {
            string sql = @"SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, '" + directoryPath + "');";
            List<Folder> folders = (List<Folder>)await _connection.QueryAsync<Folder>(sql);
            return folders;
        }
    }
}
