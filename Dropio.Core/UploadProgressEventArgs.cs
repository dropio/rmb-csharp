using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{
    /// <summary>
    /// Event arguments for a <see cref="ServiceAdapter.OnUploadProgress"/> event.
    /// </summary>
    public class UploadProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether [upload complete].
        /// </summary>
        /// <value><c>true</c> if [upload complete]; otherwise, <c>false</c>.</value>
        public bool UploadComplete { get; set; }

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
        /// Initializes a new instance of the <see cref="UploadProgressEventArgs"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="complete">if set to <c>true</c> [complete].</param>
        public UploadProgressEventArgs(long bytes, long totalBytes, bool complete)
        {
            this.Bytes = bytes;
            this.Total = totalBytes;
            this.UploadComplete = complete;
        }
    }

}
