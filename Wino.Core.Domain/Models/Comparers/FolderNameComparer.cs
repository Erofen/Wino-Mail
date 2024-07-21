﻿using System.Collections.Generic;
using Wino.Domain.Entities;

namespace Wino.Domain.Models.Comparers
{
    public class FolderNameComparer : IComparer<MailItemFolder>
    {
        public int Compare(MailItemFolder x, MailItemFolder y)
        {
            return x.FolderName.CompareTo(y.FolderName);
        }
    }
}
