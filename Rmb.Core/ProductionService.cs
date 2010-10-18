using System;
using System.Collections.Generic;
using System.Text;

namespace Rmb.Core
{
	/// <summary>
	/// 
	/// </summary>
    public class ProductionService : ServiceAdapter
    {
        private string _baseUrl;
        private string _apiBaseUrl;
        private string _uploadUrl;

		/// <summary>
		/// 
		/// </summary>
        public ProductionService ()
        {
        	this._baseUrl = "http://d.rmb.io/";
        	this._apiBaseUrl = "http://api.rmb.io/";
        	this._uploadUrl = "http://u.rmb.io/upload";
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        public override string BaseUrl
        {
            get { return this._baseUrl; }
        }

        /// <summary>
        /// Gets the API base URL.
        /// </summary>
        /// <value>The API base URL.</value>
        public override string ApiBaseUrl
        {
            get { return this._apiBaseUrl; }
        }

        /// <summary>
        /// Gets the upload URL.
        /// </summary>
        /// <value>The upload URL.</value>
        public override string UploadUrl
        {
            get { return this._uploadUrl; }
        }

        /// <summary>
        /// Sets the base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        public void SetBaseUrl(string baseUrl)
        {
            this._baseUrl = baseUrl;
        }

        /// <summary>
        /// Sets the API base URL.
        /// </summary>
        /// <param name="apiBaseUrl">The API base URL.</param>
        public void SetApiBaseUrl(string apiBaseUrl)
        {
            this._apiBaseUrl = apiBaseUrl;
        }

        /// <summary>
        /// Sets the upload URL.
        /// </summary>
        /// <param name="uploadUrl">The upload URL.</param>
        public void SetUploadUrl(string uploadUrl)
        {
            this._uploadUrl = uploadUrl;
        }
    }
}
