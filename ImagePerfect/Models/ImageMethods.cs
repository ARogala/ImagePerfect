﻿using ImagePerfect.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Models
{
    public class ImageMethods
    {
        private readonly IUnitOfWork _unitOfWork;

        public ImageMethods(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
