using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{
    public class ProductionService : ServiceAdapter
    {
        private string _baseUrl;
        private string _apiBaseUrl;

        public ProductionService()
        {
            this._baseUrl = "http://drop.io/";
            this._apiBaseUrl = "http://api.drop.io/";
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
    }
}
