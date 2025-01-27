using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //T -- ToDoList or any model/class we want to interact with dbContext
        //that is why T is a generic class
        //where T : class (just means T is a class)

        //https://stackoverflow.com/questions/14458566/making-interface-implementations-async
        Task<IEnumerable<T>> GetAll();
        /// <summary>
        /// Gets all database rows for column where value is a string type
        /// 
        /// SELECT * FROM table WEHRE column = value
        /// </summary>
        /// <param name="column">The name of the data table column</param>
        /// <param name="value"> The string type value to filter on</param>
        /// <returns>A List of all the returned rows</returns>
        Task<IEnumerable<T>> GetAllWhere(string column, string value);
        Task<T> GetById(int? id);
        Task<T> GetRandomRow();
        Task<bool> Add(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(int? id);
        Task<bool> DeleteAll();
        /// <summary>
        /// Delete all database rows for column where value is a string type
        /// 
        /// DELETE FROM table WEHRE column = value
        /// </summary>
        /// <param name="column">The name of the data table column</param>
        /// <param name="value"> The string type value to filter on</param>
        /// <returns>bool true if success</returns>
        Task<bool> DeleteAllWhere(string column, string value);
    }
}
