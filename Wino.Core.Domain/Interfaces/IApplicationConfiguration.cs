﻿namespace Wino.Domain.Interfaces
{
    /// <summary>
    /// Singleton object that holds the application data folder path and the publisher shared folder path.
    /// Load the values before calling any service.
    /// App data folder is used for storing files.
    /// Pubhlisher cache folder is only used for database file so other apps can access it in the same package by same publisher.
    /// </summary>
    public interface IApplicationConfiguration
    {
        /// <summary>
        /// Application data folder.
        /// </summary>
        string ApplicationDataFolderPath { get; set; }

        /// <summary>
        /// Publisher shared folder path.
        /// </summary>
        string PublisherSharedFolderPath { get; set; }
    }
}
