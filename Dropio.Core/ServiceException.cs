using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{
    
    public class ServiceException : Exception
    {
        /// <summary>
        /// Gets or sets the service error.
        /// </summary>
        /// <value>The service error.</value>
        public ServiceError serviceError { get; set; }
        
		/// <summary>
		/// Gets or sets the service message (the message that is sent back from drop.io)
		/// </summary>
        public string serviceMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        public ServiceException(ServiceError error)
        {
            this.serviceError = error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="message">The message.</param>
        public ServiceException(ServiceError error, string message):base(message)
        {
            this.serviceError = error;
            this.serviceMessage = message;
        }
    }
}
