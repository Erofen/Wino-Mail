﻿using System.Threading;
using System.Threading.Tasks;
using MailKit;
using Wino.Domain.Entities;
using Wino.Domain.Enums;
using Wino.Domain.Models.MailItem;
using Wino.Domain.Models.Synchronization;

namespace Wino.Domain.Interfaces
{
    public interface IBaseSynchronizer
    {
        /// <summary>
        /// Account that is assigned for this synchronizer.
        /// </summary>
        MailAccount Account { get; }

        /// <summary>
        /// Synchronizer state.
        /// </summary>
        AccountSynchronizerState State { get; }

        /// <summary>
        /// Queues a single request to be executed in the next synchronization.
        /// </summary>
        /// <param name="request">Request to queue.</param>
        void QueueRequest(IRequestBase request);

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns>Whether active synchronization is stopped or not.</returns>
        bool CancelActiveSynchronization();

        /// <summary>
        /// Performs a full synchronization with the server with given options.
        /// This will also prepares batch requests for execution.
        /// Requests are executed in the order they are queued and happens before the synchronization.
        /// Result of the execution queue is processed during the synchronization.
        /// </summary>
        /// <param name="options">Options for synchronization.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result summary of synchronization.</returns>
        Task<SynchronizationResult> SynchronizeAsync(SynchronizationOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a single MIME message from the server and saves it to disk.
        /// </summary>
        /// <param name="mailItem">Mail item to download from server.</param>
        /// <param name="transferProgress">Optional progress reporting for download operation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DownloadMissingMimeMessageAsync(IMailItem mailItem, ITransferProgress transferProgress, CancellationToken cancellationToken = default);
    }
}
