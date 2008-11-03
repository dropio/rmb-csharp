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
        public int Bytes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadProgressEventArgs"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="complete">if set to <c>true</c> [complete].</param>
        public UploadProgressEventArgs(int bytes, bool complete)
        {
            this.Bytes = bytes;
            this.UploadComplete = complete;
        }
    }

}
