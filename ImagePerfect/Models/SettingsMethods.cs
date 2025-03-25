using ImagePerfect.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Models
{
    public class SettingsMethods
    {
        private readonly IUnitOfWork _unitOfWork;

        public SettingsMethods(IUnitOfWork unitOfWork) 
        { 
            _unitOfWork = unitOfWork; 
        }
    }
}
