using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IFolderRepository Folder { get; }
        IImageRepository Image { get; }
    }
}
