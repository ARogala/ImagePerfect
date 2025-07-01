using ImagePerfect.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Models
{
    public class SaveDirectoryMethods
    {
        private readonly IUnitOfWork _unitOfWork;
        public SaveDirectoryMethods(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> UpdateSaveDirectory(SaveDirectory saveDirectory)
        {
            return await _unitOfWork.SaveDirectory.Update(saveDirectory);
        }

        public async Task<SaveDirectory> GetSavedDirectory()
        {
            return await _unitOfWork.SaveDirectory.GetById(1);
        }
    }
}
