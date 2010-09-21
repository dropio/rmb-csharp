using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{
    /// <summary>
    /// Event arguments for a <see cref="ServiceAdapter.OnTransferProgress"/> event.
    /// </summary>
    public class TransferProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether [transfer completed].
        /// </summary>
        /// <value><c>true</c> if [transfer complete]; otherwise, <c>false</c>.</value>
        public bool TransferCompleted { get; set; }

        /// <summary>
        /// Gets or sets the bytes uploaded so far.
        /// </summary>
        /// <value>The bytes.</value>
        public long Bytes { get; set; }

        /// <summary>
        /// Gets or sets the total number of bytes in file going uploaded.
        /// </summary>
        /// <value>The bytes.</value>
        public long Total { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferProgressEventArgs"/> class.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <param name="totalBytes">
        /// 
        /// </param>
        /// <param name="completed">
        /// if set to <c>true</c> [completed].
        /// </param>
        public TransferProgressEventArgs(long bytes, long totalBytes, bool completed)
        {
            this.Bytes = bytes;
            this.Total = totalBytes;
            this.TransferCompleted = completed;
        }
    }

}
