using Dapper;
using ImagePerfect.Repository.IRepository;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MySqlConnection _connection;
        public Repository(MySqlConnection db)
        {
            _connection = db;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            string tableName = GetTableName();
            string sql = $"SELECT * FROM {tableName}";
            List<T> allDbRows = (List<T>)await _connection.QueryAsync<T>(sql);
            await _connection.CloseAsync();
            return allDbRows;
        }

        public async Task<IEnumerable<T>> GetAllWhere(string column, string value)
        {
            string tableName = GetTableName();
            string sql = $"SELECT * FROM {tableName} WHERE {column} = @Parameter";
            List<T> allDbRows = (List<T>)await _connection.QueryAsync<T>(sql, new { Parameter = value });
            await _connection.CloseAsync();
            return allDbRows;
        }
        public async Task<T> GetById(int? id)
        {
            string tableName = GetTableName();
            string keyColumn = GetKeyColumnName();
            string keyProperty = GetKeyPropertyName();
            //string sql = $"SELECT * FROM {tableName} WHERE {keyColumn} = @{keyProperty}";
            string sql = $"SELECT * FROM {tableName} WHERE {keyColumn} = @id";
            T singleDbRow = await _connection.QuerySingleAsync<T>(sql, new { id });
            await _connection.CloseAsync();
            return singleDbRow;
        }

        public async Task<T> GetRandomRow()
        {
            string tableName = GetTableName();
            string sql = $"SELECT * FROM {tableName} ORDER BY RAND() LIMIT 1;";
            T singleDbRow = await _connection.QuerySingleAsync<T>(sql);
            await _connection.CloseAsync();
            return singleDbRow;
        }

        public async Task<bool> Add(T entity)
        {
            int rowsEffected = 0;
            string tableName = GetTableName();
            string columns = GetColumns(excludeKey: true);
            string properties = GetPropertyNames(excludeKey: true);
            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({properties})";

            rowsEffected = await _connection.ExecuteAsync(query, entity);
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> Update(T entity)
        {
            int rowsEffected = 0;

            string tableName = GetTableName();
            string keyColumn = GetKeyColumnName();
            string keyProperty = GetKeyPropertyName();

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {tableName} SET ");

            foreach (var property in GetProperties(true))
            {
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                string propertyName = property.Name;
                string columnName = columnAttr.Name;

                query.Append($"{columnName} = @{propertyName},");
            }
            //remove last , in query -- UPDATE table_name SET column1 = value1, column2 = value2, WHERE id = @id
            query.Remove(query.Length - 1, 1);

            query.Append($" WHERE {keyColumn} = @{keyProperty}");
            //https://github.com/DapperLib/Dapper/issues/540
            rowsEffected = await _connection.ExecuteAsync(query.ToString(), entity);

            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> Delete(int? id)
        {
            int rowsEffected = 0;

            string tableName = GetTableName();
            string keyColumn = GetKeyColumnName();
            string query = $"DELETE FROM {tableName} WHERE {keyColumn} = @Parameter";

            rowsEffected = await _connection.ExecuteAsync(query, new { Parameter = id });

            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> DeleteAll()
        {
            int rowsEffected = 0;
            string tableName = GetTableName();
            string keyColumn = GetKeyColumnName();

            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string query1 = $"DELETE FROM {tableName} WHERE {keyColumn} >= 1";
            string query2 = $"ALTER TABLE {tableName} AUTO_INCREMENT = 1";
            rowsEffected = await _connection.ExecuteAsync(query1, transaction: txn);
            await _connection.ExecuteAsync(query2, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> DeleteAllWhere(string column, string value)
        {
            int rowsEffected = 0;
            string tableName = GetTableName();
            string sql = $"DELETE FROM {tableName} WHERE {column} = @Parameter";
            rowsEffected = await _connection.ExecuteAsync(sql, new { Parameter = value });
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }
        private static string GetTableName()
        {
            string tableName = "";
            Type type = typeof(T);
            var tableAttr = type.GetCustomAttribute<TableAttribute>();

            tableName = tableAttr.Name;
            return tableName;
        }

        private static string GetKeyColumnName()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object[] keyAttributes = property.GetCustomAttributes(typeof(KeyAttribute), true);

                if (keyAttributes != null && keyAttributes.Length > 0)
                {
                    object[] columnAttributes = property.GetCustomAttributes(typeof(ColumnAttribute), true);

                    if (columnAttributes != null && columnAttributes.Length > 0)
                    {
                        ColumnAttribute columnAttribute = (ColumnAttribute)columnAttributes[0];
                        return columnAttribute.Name;
                    }
                    else
                    {
                        return property.Name;
                    }
                }
            }
            return null;
        }

        private static string GetColumns(bool excludeKey = false)
        {
            //https://stackoverflow.com/questions/2439142/c-sharp-linq-statements-with-or-clauses
            /*
             * p.IsDefined(typeof(KeyAttribute) returns true for key column and false otherwise
             * thus the Where() will return all non key column when exclued key is true
             * 
             * also builds string with propery names if Column attribute is not defined 
             */
            Type type = typeof(T);
            string columns = string.Join(", ", type.GetProperties()
                .Where(p => !excludeKey || !p.IsDefined(typeof(KeyAttribute))).Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null)
                .Select(p =>
                {
                    var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
                    return columnAttr != null ? columnAttr.Name : p.Name;
                }));

            return columns;
        }

        private static string GetPropertyNames(bool excludeKey = false)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => !excludeKey || p.GetCustomAttribute<KeyAttribute>() == null).Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null);

            var values = string.Join(", ", properties.Select(p =>
            {
                return $"@{p.Name}";
            }));

            return values;
        }

        private static IEnumerable<PropertyInfo> GetProperties(bool excludeKey = false)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => !excludeKey || p.GetCustomAttribute<KeyAttribute>() == null).Where(p=> p.GetCustomAttribute<NotMappedAttribute>() == null);

            return properties;
        }

        private static string GetKeyPropertyName()
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (properties.Any())
            {
                return properties.FirstOrDefault().Name;
            }

            return null;
        }
    }
}
