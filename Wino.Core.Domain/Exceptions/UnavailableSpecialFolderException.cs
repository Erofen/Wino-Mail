﻿using System;
using Wino.Domain.Enums;

namespace Wino.Domain.Exceptions
{
    /// <summary>
    /// Emitted when special folder is needed for an operation but it couldn't be found.
    /// </summary>
    public class UnavailableSpecialFolderException : Exception
    {
        public UnavailableSpecialFolderException(SpecialFolderType specialFolderType, Guid accountId)
        {
            SpecialFolderType = specialFolderType;
            AccountId = accountId;
        }

        public SpecialFolderType SpecialFolderType { get; }
        public Guid AccountId { get; set; }
    }
}
