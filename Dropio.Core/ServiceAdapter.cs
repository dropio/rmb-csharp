using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Specialized;
using System.Collections;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace Dropio.Core
{
	/// <summary>
	/// 
	/// </summary>
    public abstract class ServiceAdapter
    {
		/// <summary>
		/// The following consts are for creating the various API urls
		/// </summary>
		public const string ACCOUNTS = "accounts/";
        public const string DROPS = "drops/";
		public const string EMPTY_DROP = "/empty";
        public const string ASSETS = "/assets/";
        public const string SUBSCRIPTIONS = "/subscriptions/";
		public const string DOWNLOAD_ORIGINAL = "/download/original";
		public const string COPY = "/copy";
		public const string MOVE = "/move";
		public const string JOBS = "jobs/";
		/// <summary>
		/// 
		/// </summary>
        public const string VERSION = "3.0";

		/// <summary>
		/// getters for the base API urls
		/// </summary>
        public abstract string BaseUrl { get; }
        public abstract string ApiBaseUrl { get; }
        public abstract string UploadUrl { get; }
        
		/// <summary>
		/// getters for API key(s)
		/// </summary>
        public string ApiKey { get; set; }
		public string ApiSecret { get; set; }

        delegate void GetResponse(HttpWebResponse response);
        delegate void ReadDocument(XmlDocument doc);

        /// <summary>
        /// Delegate
        /// </summary>
        public delegate void TransferProgressHandler(object sender, TransferProgressEventArgs e);

        /// <summary>
        /// UploadProgressHandler is fired during a synchronous transfer process to signify that 
        /// a segment of transfer has been completed. The total transfered is recorded in the 
        /// <see cref="TransferProgressEventArgs"/> class.
        /// </summary>
        public event TransferProgressHandler OnTransferProgress;

        #region Implementation

        /// <summary>
        /// Completes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="respond">The respond action.</param>
        private void CompleteRequest(HttpWebRequest request, GetResponse respond)
        {
            HttpWebResponse response = null;
            
            try
            {	
            	// get the request response
                response = request.GetResponse() as HttpWebResponse;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                	// request response returned status code OK, procede if a delagate was specified 
                    if (respond != null)
                    {
                        respond(response);
                    }
                }
                else
                {
                	// status code was anything but OK
                    throw new ServiceException(ServiceError.BadRequest, "There was a problem with your request.");
                }
            }
            catch (WebException exc)
            {
                this.HandleException(exc);
            }
            finally
            {
            	// close the connection (if we had a respose to read)
                if (response != null)
                {
                    response.Close();
                }

                response = null;
            }
        }

        /// <summary>
        /// Reads a <see cref="HttpWebResponse"/> response.
        /// </summary>
        /// <param name="response">
        /// The <see cref="HttpWebResponse"/> object being read.
        /// </param>
        /// <param name="read">
        /// A <see cref="ReadDocument"/> delagate that can be used to put the response into a <see cref="XmlDocument"/>
        /// object.
		/// </param>
        private void ReadResponse(HttpWebResponse response, ReadDocument read)
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
            	// create new XmlDocument, load the XML response into it, then run the delagate if one is specified
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                if (read != null)
                {
                    read(doc);
                }
            }
        }

		/// <summary>
        /// Gets an original file download url.
        /// </summary>
        /// <param name="asset">An <see cref="Asset"/> object</param>
        /// <returns></returns>
        public string OriginalFileUrl (Asset asset)
        {
			// create the API call
            StringBuilder sb = new StringBuilder ();
            sb.Append (this.ApiBaseUrl + DROPS + asset.Drop.Name + ASSETS + asset.Id + DOWNLOAD_ORIGINAL);
            
            // we just need the "common" parameters for this request, and sign if secret was provided
			Hashtable parameters = new Hashtable ();
			AddCommonParameters( ref parameters );
			SignIfNeeded( ref parameters );
			
			// add the parameters to the end of the url
			sb.Append("?");
			sb.Append( BuildParameterString( parameters ));

			// return as a string
            return sb.ToString();
        }

        /// <summary>
        /// Generates a SHA1 hash signature for the specified string
        /// </summary>
		/// <param name="StringToSign">
		/// The <see cref="string"/> of parameters to sign.
		/// </param>
        /// <returns>
        /// 
        /// </returns>
        protected string GenerateSignature( string StringToSign )
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            
            // the string must be converted to an array of byte's before it can be encoded
            byte[] input = Encoding.UTF8.GetBytes( StringToSign );
            byte[] result = sha1.ComputeHash(input);
            
			// value returned from ComputeHash() has a dash between each byte, remove and convert to lowercase
            return BitConverter.ToString(result).Replace( "-", "" ).ToLower();
        }

        /// <summary>
        /// Generates a unix timestamp 10 minutes in the future
        /// </summary>
        /// <returns></returns>
        protected long GenerateUnixTimestamp()
        {
        	// now + 10 minutes
            TimeSpan ts = (DateTime.UtcNow.AddMinutes(10) - new DateTime(1970, 1, 1, 0, 0, 0));
            // return the time span as just the total seconds (since that's what we mean by unix time)
            return (long) ts.TotalSeconds;
        }

        /// <summary>
        /// Creates a Drop.
        /// </summary>
        /// <param name="dropAttributes">The name.</param>
        /// <returns>
        /// 
        /// </returns>
        public Drop CreateDrop( Hashtable dropAttributes ) 
        {
        	// drop object that will be returned
            Drop d = null;

			// add any specified parameters to a Hashtable. All parameters are optional, so only add if they've been specified
//			Hashtable parameters = new Hashtable();
//			if( name != string.Empty)
//	            parameters.Add("name", name);
//			if( description != string.Empty)
//				parameters.Add("description", description);
//			if( emailKey != string.Empty)
//				parameters.Add("email_key", emailKey);
//			if( maxSize > 0 )
//				parameters.Add("max_size", maxSize.ToString() );
//			if( chatPassword != string.Empty)
//				parameters.Add("chat_password", chatPassword);

			// do the request and load to response into the Drop object
            HttpWebRequest request = this.CreatePostRequest(this.CreateDropUrl(string.Empty), dropAttributes );
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => d = this.CreateAndMapDrop(doc.SelectSingleNode("drop")));
            });

            return d;
        }

        /// <summary>
        /// Find a drop by name.
        /// </summary>
        /// <param name="name">A <see cref="string"/> specifying the name of the drop to find.</param>
        /// <returns></returns>
        public Drop FindDrop(string name)
        {
        	// can't find a drop if we don't have a name...
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "The given name can't be blank.");

			// drop object to be returned
            Drop d = null;

			// do the request, load response into Drop object
            HttpWebRequest request = this.CreateUrlEncodedRequest("GET", this.CreateDropUrl(name));
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => d = this.CreateAndMapDrop(doc.SelectSingleNode("drop")));
            });

            return d;
        }
		
		/// <summary>
		/// Gets a paginated list of drops associated with the ApiKey
		/// </summary>
		/// <param name="page">An <see cref="int"/> specifying the page of results to get.</param>
		/// <returns></returns>
		public List<Drop> FindAll(int page)
		{
			// List<Drop> object to return
			List<Drop> drops = new List<Drop>();

			// add the page parameter
			Hashtable parameters = new Hashtable();
			parameters.Add( "page", page.ToString() );
			
			// do the request and load the response into the drop list
            HttpWebRequest request = this.CreateUrlEncodedRequest("GET", this.CreateAllDropsUrl(), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/drops/drop");
					
					// go through each returned drop XML node. load into Drop object and Add() to List
                    foreach (XmlNode node in nodes)
                    {
                        Drop d = this.CreateAndMapDrop(node);
                        drops.Add(d);
                    }
                });
            });

            return drops;
		}

		/// <summary>
        /// Empties a drop of all assets
        /// </summary>
        /// <param name="drop">The <see cref="Drop"/> to be emptied</param>
        /// <returns></returns>
        public bool EmptyDrop(Drop drop)
        {
        	// can't do much if we don't have a drop to act on...
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

			// bool to return
            bool emptied = false;

			// do request and change "emptied" to true if request succeeds 
            HttpWebRequest request = this.CreatePutRequest(this.CreateEmptyDropUrl(drop.Name), new Hashtable() );
            CompleteRequest(request, (HttpWebResponse response) => { emptied = true; });

            return emptied;
        }

        /// <summary>
        /// Deletes the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool DestroyDrop (Drop drop)
        {
        	// can't do much if we don't have a drop to act on...
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

			// bool to return
            bool destroyed = false;

			// do request and change "destroyed" to true if request succeeds
            HttpWebRequest request = this.CreateUrlEncodedRequest("DELETE",this.CreateDropUrl(drop.Name), new Hashtable() );
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
        }

        /// <summary>
        /// Updates the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="name"></param>
        /// <param name="chatPassword"></param>
        /// <returns></returns>
        public bool UpdateDrop (Drop drop, string newName, string newDescription, string newChatPassword, int newMaxSize)
        {
        	// can't do much if we don't have a drop to act on...
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

			// bool to return
            bool updated = false;
			
			// add the parameters (that were actually specified) to a hashtable of parameters
            Hashtable parameters = new Hashtable();
			if( !String.IsNullOrEmpty( newName ))
				parameters.Add("name", newName);
			if( !String.IsNullOrEmpty( newDescription ))
				parameters.Add("description", newDescription);
			if( !String.IsNullOrEmpty( newChatPassword ))
				parameters.Add("chat_password", newChatPassword);
			if( newMaxSize > 0 )
				parameters.Add( "max_size", newMaxSize.ToString());
			
			// do the request and change updated to "true" if request succeeded
            HttpWebRequest request = this.CreatePutRequest(this.CreateDropUrl(drop.Name), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
            	// the response includes the updated drop information, load it into the drop object we passed
                ReadResponse(response, (XmlDocument doc) => this.MapDrop(drop, doc.SelectSingleNode("drop")));
                updated = true;
            });

            return updated;
        }

        /// <summary>
        /// Finds an asset.
        /// </summary>
        /// <param name="drop"></param>
        /// <param name="name">The asset name.</param>
        /// <returns></returns>
        public Asset FindAsset(Drop drop, string assetId)
        {
        	// we can't do much without a drop or asset ID...
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");
            if (assetId == null)
                throw new ArgumentNullException("assetId", "The given asset ID can't be null");

			// asset object to return
            Asset a = null;

			// do the request
            HttpWebRequest request = this.CreateUrlEncodedRequest("GET", this.CreateAssetUrl(drop.Name, assetId, string.Empty));
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc) 
                {
                	// read the XML response into the Asset object
                    XmlNode node = doc.SelectSingleNode( "/asset");
                    a = this.CreateAndMapAsset(drop, node);
                });
            });

            return a;
        }

        /// <summary>
        /// Finds the assets for a given drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="page">The page.</param>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public List<Asset> FindAssets(Drop drop, int page, Order order)
        {
        	// can't do much without a drop to act on
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

			// the Asset list to return
            List<Asset> assets = new List<Asset>();

			// create a new hashtable and add the passed in parameters
			Hashtable parameters = new Hashtable();
			parameters.Add( "page", page.ToString() );
            parameters.Add( "order", (order == Order.Newest ) ? "latest" : "oldest" );
            
            // do the request
            HttpWebRequest request = this.CreateUrlEncodedRequest("GET", this.CreateAssetUrl(drop.Name, string.Empty, string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                	// get all the asset XML nodes, load each one into an Asset object and load into the Asset list
                    XmlNodeList nodes = doc.SelectNodes("//assets/asset");
                    foreach (XmlNode node in nodes)
                    {
                        Asset a = this.CreateAndMapAsset(drop, node);
                        assets.Add(a);
                    }
                });
            });
			
            return assets;
        }
		
		/// <summary>
        /// Finds the subscriptions.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
		public List<Subscription> FindSubscriptions(Drop drop, int page)
		{
			if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            List<Subscription> subscriptions = new List<Subscription>();

			Hashtable parameters = new Hashtable();
			parameters.Add( "page", page.ToString() );
            HttpWebRequest request = this.CreateUrlEncodedRequest("GET", this.CreateSubscriptionsUrl(drop.Name), parameters );
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/subscriptions/subscription");

                    foreach (XmlNode node in nodes)
                    {
                        Subscription s = this.CreateAndMapSubscription(drop, node);
                        subscriptions.Add(s);
                    }
                });
            });

            return subscriptions;
		}
				
		/// <summary>
		/// Creates a pingback subscription. When the events happen, the url will be sent a POST request with the pertinent data.
		/// </summary>
		/// <param name="drop">The drop.</param>
		/// <param name="url">The url.</param>
		/// <param name="events"> The events. </param>
		/// <returns></returns>
		public Subscription CreatePingbackSubscription(Drop drop, string url, AssetEvents events)
		{
			if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            Subscription s = null;

			Hashtable parameters = new Hashtable();

			parameters.Add("type", "pingback");
            parameters.Add("url", url);
			
			parameters.Add("asset_created", ((events & AssetEvents.AssetCreated) == AssetEvents.AssetCreated).ToString().ToLower());
			parameters.Add("asset_udpated", ((events & AssetEvents.AssetUpdated) == AssetEvents.AssetUpdated).ToString().ToLower());
			parameters.Add("asset_deleted", ((events & AssetEvents.AssetDeleted) == AssetEvents.AssetDeleted).ToString().ToLower());
			parameters.Add("job_started", ((events & AssetEvents.JobStarted) == AssetEvents.JobStarted).ToString().ToLower());
			parameters.Add("job_complete", ((events & AssetEvents.JobComplete) == AssetEvents.JobComplete).ToString().ToLower());
			parameters.Add("job_progress", ((events & AssetEvents.JobProgress) == AssetEvents.JobProgress).ToString().ToLower());

            HttpWebRequest request = this.CreatePostRequest(this.CreateSubscriptionsUrl(drop.Name), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNode node = doc.SelectSingleNode("/subscription");
                    s = this.CreateAndMapSubscription(drop, node);
                });
            });

            return s;
		}
				
		/// <summary>
        /// Deletes the subscription.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
		public bool DeleteSubscription(Subscription subscription)
		{
			bool destroyed = false;
            Drop drop = subscription.Drop;

            Hashtable parameters = new Hashtable();

            HttpWebRequest request = this.CreateUrlEncodedRequest("DELETE",this.CreateSubscriptionUrl(drop.Name, subscription.Id), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
		}

        /// <summary>
        /// Deletes the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public bool DeleteAsset(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");

            bool destroyed = false;
            Drop drop = asset.Drop;

            Hashtable parameters = new Hashtable();

            HttpWebRequest request = this.CreateUrlEncodedRequest("DELETE",this.CreateAssetUrl(drop.Name, asset.Name, string.Empty), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
        }

        /// <summary>
        /// Updates the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="newTitle"></param>
        /// <param name="newDescription"></param>
        /// <returns></returns>
        public bool UpdateAsset(Asset asset, string newTitle, string newDescription)
        {
            if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");

            bool updated = false;
            Drop drop = asset.Drop;

            Hashtable parameters = new Hashtable();
				
			if( !string.IsNullOrEmpty( newTitle ))
				parameters.Add("name", newTitle);
			if( !string.IsNullOrEmpty( newDescription ))
				parameters.Add("description", newDescription );
			//if( !string.IsNullOrEmpty( asset.))
			
            HttpWebRequest request = this.CreatePutRequest(this.CreateAssetUrl(drop.Name, asset.Name, string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNode node = doc.SelectSingleNode("/asset");
					CreateAndMapAsset( drop, node );
                    updated = true;
                });
            });

            return updated;
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="asset">
		/// A <see cref="Asset"/>
		/// </param>
		/// <param name="dropName">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool CopyAsset(Asset asset, Drop drop, bool keepOriginal)
		{
			if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");
			
			if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

			bool copied = false;

            Hashtable parameters = new Hashtable();

            parameters.Add("drop_name", drop.Name);

            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetUrl(asset.Drop.Name, asset.Id, keepOriginal == true ? COPY : MOVE ), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { copied = true; });

            return copied;
		}
	
		public Asset AddFileInit (Drop drop, string file, string description, bool conversion, string pingbackUrl, string outputLocations )
		{
			// get the name of the file
			string fileName = Path.GetFileName (file);
			
			// length of file in bytes
			long fileLength = new FileInfo (file).Length;
			
			// create Stream object for file access
			Stream fs = new FileStream (file, FileMode.Open, FileAccess.Read);
			
			return this.AddFile (drop, fileName, description, fileLength, fs, conversion, pingbackUrl, outputLocations);
		}
		
		public Asset AddFileInit (Drop drop, HttpPostedFile file, string description, bool conversion, string pingbackUrl, string outputLocations )
		{

			// get the name of the file
			string fileName = file.FileName;
			
			// length of file in bytes
			long fileLength = (long)file.ContentLength;
			
			// create Stream object for file access
			Stream fs = file.InputStream;
			
			return this.AddFile (drop, fileName, description, fileLength, fs, conversion, pingbackUrl, outputLocations);	
		}
		
		
        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="fileName">The file.</param>
        /// <param name="description">The description.</param>
        /// <param name="fileLength"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        //public Asset AddFile (Drop drop, string file, string description)
		public Asset AddFile (Drop drop, string fileName, string description, long fileLength, Stream fs, bool conversion, string pingbackUrl, string outputLoations )
        {

            string requestUrl = this.UploadUrl;

            Hashtable parameters = new Hashtable();

            HttpWebRequest request = HttpWebRequest.Create(requestUrl) as HttpWebRequest;
            string boundary = "DROPIO_MIME_" + DateTime.Now.ToString("yyyyMMddhhmmss");

            request.Method = "POST";
            request.KeepAlive = true;
            request.Timeout = 30000000;
            request.ContentType = "multipart/form-data; boundary=" + boundary + "";
            request.Expect = "";

			parameters.Add( "drop_name", drop.Name );
			if( !String.IsNullOrEmpty(description))
				parameters.Add( "description", description );
			if( conversion == true )
				parameters.Add( "conversion", "BASE" );
			if( !String.IsNullOrEmpty(pingbackUrl))
				parameters.Add( "pingback_url", pingbackUrl );
			if( !String.IsNullOrEmpty( outputLoations ))
				parameters.Add( "output_locations", outputLoations );
			
			AddCommonParameters( ref parameters );
			SignIfNeeded( ref parameters );
			
            StringBuilder sb = new StringBuilder();
            //string fileName = Path.GetFileName(file);
			//string fileName = file.FileName;

            foreach (DictionaryEntry parameter in parameters)
            {
                sb.Append("--" + boundary + "\r\n");
                sb.Append("Content-Disposition: form-data; name=\"" + parameter.Key + "\"\r\n");
                sb.Append("\r\n");
                sb.Append(parameter.Value + "\r\n");
            }

            // File
            sb.Append("--" + boundary + "\r\n");
            sb.Append("Content-Disposition: form-data; name=\"file\"; filename=\"" + fileName + "\"\r\n");
            sb.Append("\r\n");

            UTF8Encoding encoding = new UTF8Encoding();

            byte[] postContents = encoding.GetBytes(sb.ToString());
            byte[] postFooter = encoding.GetBytes("\r\n--" + boundary + "--\r\n");

            //request.ContentLength = postContents.Length + new FileInfo(file).Length + postFooter.Length;
			//request.ContentLength = postContents.Length + file.PostedFile.ContentLength + postFooter.Length;
			request.ContentLength = postContents.Length + fileLength + postFooter.Length;
			
            request.AllowWriteStreamBuffering = false;
            Stream resStream = request.GetRequestStream();
            resStream.Write(postContents, 0, postContents.Length);

            if (OnTransferProgress != null)
            {
                OnTransferProgress(this, new TransferProgressEventArgs(postContents.LongLength, request.ContentLength, false));
            }

            //FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
			//Stream fs = file.PostedFile.InputStream;
            long size = Math.Min(Math.Max(request.ContentLength / 100, 50 * 1024), 1024 * 1024);

            byte[] buffer = new byte[size];
            int bytesOut = 0;
            int bytesSoFar = 0;

            while ((bytesOut = fs.Read(buffer, 0, buffer.Length)) != 0)
            {
                resStream.Write(buffer, 0, bytesOut);
                bytesSoFar += bytesOut;
                if (OnTransferProgress != null)
                {
                    OnTransferProgress(this, new TransferProgressEventArgs(bytesSoFar, request.ContentLength, false));
                }
            }

            resStream.Write(postFooter, 0, postFooter.Length);

            if (OnTransferProgress != null)
            {
                OnTransferProgress(this, new TransferProgressEventArgs(request.ContentLength, request.ContentLength, true));
            }

            resStream.Close();
            fs.Close();

            Asset a = null;

            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNode node = doc.SelectSingleNode("/asset");
                    a = this.CreateAndMapAsset(drop, node);
                });
            });
			
            return a;
		}

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="exc">The exc.</param>
        protected void HandleException(WebException exc)
        {
            if (exc.Response != null)
            {
                if (exc.Response.Headers["Status"].StartsWith("404"))
                {
                    throw new ServiceException(ServiceError.NotFound, this.ExtractErrorMessage(exc.Response));
                }

                if (exc.Response.Headers["Status"].StartsWith("403"))
                {
                    throw new ServiceException(ServiceError.NotAuthorized, this.ExtractErrorMessage(exc.Response));
                }

                if (exc.Response.Headers["Status"].StartsWith("400"))
                {
                    throw new ServiceException(ServiceError.BadRequest, this.ExtractErrorMessage(exc.Response));
                }

                if (exc.Response.Headers["Status"].StartsWith("500"))
                {
                    throw new ServiceException(ServiceError.ServerError, "There was a problem connecting to Drop.io.");
                }
            }

            throw exc;
        }
        
        public bool CreateJob (AssetType type, List<Hashtable> inputs, List<Hashtable> outputs, string plugin, string pingback_url)
		{
			bool success = false;
			Hashtable parameters = new Hashtable ();
			
			parameters.Add ("job_type", type.ToString ().ToUpper ());
			parameters.Add ("using", plugin);
			
			parameters.Add ("inputs", inputs);
			parameters.Add ("outputs", outputs);
			
			//Console.Write( ToJson( parameters ) );
			
			HttpWebRequest request = this.CreatePostRequest (this.ApiBaseUrl + JOBS, parameters);
			CompleteRequest (request, (HttpWebResponse response) => { success = true; });
			
			return success;
			
		}
		
		public string GetUploadifyForm (Drop drop, Hashtable uploadifyOptions)
		{
			Hashtable parameters = new Hashtable ();
			
			AddCommonParameters (ref parameters);
			
			parameters.Add ("drop_name", drop.Name);
			
			StringBuilder sb = new StringBuilder ();
			
			
			sb.AppendLine ("<script type=\"text/javascript\" src=\"uploadify/jquery-1.3.2.min.js\"></script>");
			sb.AppendLine ("<script type=\"text/javascript\" src=\"uploadify/swfobject.js\"></script>");
			sb.AppendLine ("<script type=\"text/javascript\" src=\"uploadify/jquery.uploadify.v2.1.0.min.js\"></script>");
			sb.AppendLine ("<link rel=\"stylesheet\" type=\"text/css\" media=\"screen, projection\" href=\"uploadify/uploadify.css\" />");
			
			sb.AppendLine ("<script type=\"text/javascript\">// <![CDATA[");
			sb.AppendLine ("$(document).ready(function() {");
			sb.AppendLine ("$('#file').uploadify({");
			
			// UPLOADER
			if ((uploadifyOptions != null) && uploadifyOptions.Contains ("uploader")) {
				sb.AppendLine ("'uploader':" + uploadifyOptions["uploader"] + ",");
				uploadifyOptions.Remove ("uploader");
			} else
				sb.AppendLine ("'uploader':'uploadify/uploadify.swf',");
			
			// SCRIPT
			if ((uploadifyOptions != null) && uploadifyOptions.Contains ("script")) {
				sb.AppendLine ("'script':" + uploadifyOptions["script"] + ",");
				uploadifyOptions.Remove ("script");
			} else
				sb.AppendLine ("'script':'http://assets.drop.io/upload',");
			
			// MULTI
			if ((uploadifyOptions != null) && uploadifyOptions.Contains ("multi")) {
				sb.AppendLine ("'multi':" + uploadifyOptions["multi"] + ",");
				uploadifyOptions.Remove ("multi");
			} else
				sb.AppendLine ("'multi':true,");
			
			// SCRIPTDATA
			if ((uploadifyOptions != null) && uploadifyOptions.Contains ("scriptData")) {
				sb.AppendLine ("'scriptData':" + uploadifyOptions["scriptData"]);
				uploadifyOptions.Remove ("scriptData");
			} else
				sb.Append ("'scriptData': ").AppendLine (ToJson (parameters) + ",");
			
			// CANCELIMG
			if ((uploadifyOptions != null) && uploadifyOptions.Contains ("cancelImg")) {
				sb.AppendLine ("'cancelImg':" + uploadifyOptions["cancelImg"] + ",");
				uploadifyOptions.Remove ("cancelImg");
			} else
				sb.AppendLine ("'cancelImg':'uploadify/cancel.png',");
			
			// AUTO
			if ((uploadifyOptions != null) && uploadifyOptions.Contains ("auto")) {
				sb.AppendLine ("'auto':" + uploadifyOptions["auto"] + ",");
				uploadifyOptions.Remove ("auto");
			} else
				sb.AppendLine ("'auto':true,");
			
			// add any other options that don't have default options
			if (uploadifyOptions != null) {
				foreach (object obj in uploadifyOptions.Keys) {
					sb.AppendLine ("'" + obj + "':" + uploadifyOptions[obj] + ",");
				}
			}
			
			// ONCOMPLETE
			//sb.AppendLine ("'onComplete' : function(event, queueID, fileObj, response, data){ alert('all done'); }");
			sb.AppendLine ("});");
			sb.AppendLine ("});");
			sb.AppendLine ("// ]]></script>");
			
			return sb.ToString ();
			
		}

        /// <summary>
        /// Extracts the error message.
        /// </summary>
        /// <param name="errorResponse">The error response.</param>
        /// <returns></returns>
        protected string ExtractErrorMessage(WebResponse errorResponse)
        {
            string message = string.Empty;

            StreamReader reader = new StreamReader(errorResponse.GetResponseStream());

            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            XmlNodeList nodes = doc.SelectNodes("/response/message");

            message = nodes[0].InnerText;

            return message;
        }

        #endregion

        #region Mapping

        /// <summary>
        /// Maps the drop.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="node">The doc.</param>
        /// <returns></returns>
        protected void MapDrop(Drop d, XmlNode node)
        {
			XmlNode dropNode = node;
            d.Name = this.ExtractInnerText(dropNode, "name");
            d.AssetCount = this.ExtractInt(dropNode, "asset_count");
            d.CurrentBytes = this.ExtractInt(dropNode, "current_bytes");
            d.MaxBytes = this.ExtractInt(dropNode, "max_bytes");
            d.Email = this.ExtractInnerText(dropNode, "email");
            d.Description = this.ExtractInnerText(dropNode, "description");
			d.ChatPassword = this.ExtractInnerText(dropNode, "chat_password");
        }

        /// <summary>
        /// Creates and maps a drop.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected Drop CreateAndMapDrop(XmlNode node)
        {
            Drop d = new Drop();
            this.MapDrop(d, node);
            return d;
        }
		
		/// <summary>
        /// Creates the and map subscription.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected Subscription CreateAndMapSubscription(Drop d, XmlNode node)
        {
            Subscription s = new Subscription();
            this.MapSubscription(d, s, node);
            return s;
        }

        /// <summary>
        /// Creates the and map asset.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected Asset CreateAndMapAsset(Drop d, XmlNode node)
        {
            Asset a = new Asset();
            this.MapAsset(a, d, node);
            return a;
        }

        /// <summary>
        /// Maps the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="drop">The drop.</param>
        /// <param name="node">The node.</param>
        protected Asset MapAsset(Asset asset, Drop drop, XmlNode node)
        {			
            asset.CreatedAt = this.ExtractDateTime(this.ExtractInnerText(node, "created_at"));
            asset.Filesize = this.ExtractInt(node, "filesize");
			asset.Description = this.ExtractInnerText(node, "description");
			asset.Title = this.ExtractInnerText(node,"title");
            asset.Name = this.ExtractInnerText(node, "name");
            asset.Id = this.ExtractInnerText(node, "id");
            asset.DropName = this.ExtractInnerText(node, "drop_name");
            asset.Type = this.MapAssetType( this.ExtractInnerText(node, "type") );
			asset.Drop = drop;
			
			asset.Roles = new List<AssetRoleAndLocations>();
			
			XmlNodeList roles = node.SelectNodes( "roles/role");
			
			foreach( XmlNode roleNode in roles )
			{
				AssetRoleAndLocations rolesAndLocations = new AssetRoleAndLocations();
				rolesAndLocations.Role = new Hashtable();
				rolesAndLocations.Locations = new List<Hashtable>();
				
				foreach( XmlNode roleInfo in roleNode)
				{
					if ( roleInfo.Name == "locations")
					{
						XmlNodeList locations = roleInfo.SelectNodes( "location" );
						foreach( XmlNode locationNode in locations )
						{
							Hashtable temp = new Hashtable();
							foreach( XmlNode locationInfo in locationNode )
							{
								// PUT STUFF INTO LOCATION HASH
								temp.Add( locationInfo.Name, locationInfo.InnerText );
							}
							rolesAndLocations.Locations.Add( temp );
						}
					}
					else
					{
						rolesAndLocations.Role.Add( roleInfo.Name.ToString(), roleInfo.InnerText.ToString() );
					}
				}
				asset.Roles.Add( rolesAndLocations );
			}

            return asset;
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Type">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="AssetType"/>
		/// </returns>
		protected AssetType MapAssetType( string Type )
		{
			switch( Type )
			{
				case "image":
					return AssetType.Image;
				case "other":
					return AssetType.Other;
				case "audio":
					return AssetType.Audio;
				case "document":
					return AssetType.Document;
				case "movie":
					return AssetType.Movie;
				case "link":
					return AssetType.Link;
				default:
					throw new ArgumentException( "UnknownArgument", "Asset type " + Type + " is unknown"  );
			}
			
		}
		
		/// <summary>
        /// Maps the subscription.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <param name="drop">The drop.</param>
        /// <param name="node">The node.</param>
		protected void MapSubscription(Drop drop, Subscription subscription, XmlNode node)
		{
			subscription.Id = this.ExtractInt(node, "id");
			//subscription.Message = this.ExtractInnerText(node, "message");
			subscription.Type = this.ExtractInnerText(node, "type");
			//subscription.Username = this.ExtractInnerText(node, "username");
			subscription.Url = this.ExtractInnerText(node, "url");
			subscription.Drop = drop;
		}

        /// <summary>
        /// Extracts the boolean.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        protected bool ExtractBoolean(XmlNode node, string path)
        {
            bool result = false;
            bool.TryParse(this.ExtractInnerText(node, path), out result);
            return result;
        }

        /// <summary>
        /// Extracts the int.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        protected int ExtractInt(XmlNode node, string path)
        {
            string val = this.ExtractInnerText(node, path);

            if (!string.IsNullOrEmpty(val))
            {
                int result = 0;
                int.TryParse(val, out result);
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Extracts the inner text.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        protected string ExtractInnerText(XmlNode node, string path)
        {
            XmlNode result = node.SelectSingleNode(path);
            if (result != null)
            {
                return result.InnerText;
            }
            return string.Empty;
        }

        /// <summary>
        /// Extracts the date time.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected DateTime ExtractDateTime(string p)
        {
            p = p.Replace("UTC", string.Empty);
            DateTime extracted = DateTime.Now;
            try
            {
                extracted = DateTime.Parse(p);
            }
            catch (FormatException) { }
            return extracted;
        }
		
        #endregion

        #region HTTP Methods

        /// <summary>
        /// Creates the drop URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <returns></returns>
        protected string CreateDropUrl(string dropName)
        {
            return this.ApiBaseUrl + DROPS + dropName;
        }
		
		/// <summary>
        /// Creates the URL responsible for getting back a paginated list of Drops associated with the Manager Account.
        /// </summary>
        /// <returns></returns>
        protected string CreateAllDropsUrl()
        {
            return this.ApiBaseUrl + ACCOUNTS + DROPS;
        }
		
		/// <summary>
        /// Creates the drop empty URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <returns></returns>
        protected string CreateEmptyDropUrl(string dropName)
        {
            return this.ApiBaseUrl + DROPS + dropName + EMPTY_DROP;
        }

		/// <summary>
        /// Creates the asset embed code URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateAssetUrl (string dropName, string assetName, string action)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + action;
        }
		
		/// <summary>
        /// Creates the subscription URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <returns></returns>
        protected string CreateSubscriptionsUrl(string dropName)
        {
            return this.ApiBaseUrl + DROPS + dropName + SUBSCRIPTIONS;
        }

        /// <summary>
        /// Creates the subscription URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="subscriptionId">The subscription id.</param>
        /// <returns></returns>
        protected string CreateSubscriptionUrl(string dropName, int subscriptionId)
        {
            return this.ApiBaseUrl + DROPS + dropName + SUBSCRIPTIONS + subscriptionId.ToString();
        }

        /// <summary>
        /// Creates a get request.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        protected HttpWebRequest CreateUrlEncodedRequest(string method, string url)
        {
            return this.CreateUrlEncodedRequest(method, url, new Hashtable() );
        }

        /// <summary>
        /// Creates the get request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected HttpWebRequest CreateUrlEncodedRequest(string method, string url, Hashtable parameters )
        {

			this.AddCommonParameters( ref parameters );
			
			this.SignIfNeeded( ref parameters );
			
			StringBuilder sb = new StringBuilder( url );
			
			sb.Append( "?" );
			
			int index=0;
           	foreach (string key in parameters.Keys)
            {
            	index++;
            	sb.Append( HttpUtility.UrlEncode( key ) + "=" + HttpUtility.UrlEncode( parameters[key].ToString() ));
               	if( index < parameters.Count )
               		sb.Append( "&" );
            }

            HttpWebRequest request = HttpWebRequest.Create(sb.ToString()) as HttpWebRequest;
            request.Method = method;
            return request;
        }

        /// <summary>
        /// Creates a post request.
        /// </summary>
        /// <param name="url">
        /// The parameters.
        /// </param>
        /// <param name="parameters">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        protected HttpWebRequest CreatePostRequest(string url, Hashtable parameters)
        {
            return this.CreateRequestWithParameters(url, "POST", parameters);
        }

        /// <summary>
        /// Creates the request with parameters.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected HttpWebRequest CreateRequestWithParameters(string url, string method, Hashtable parameters)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

            request.Method = method;

			request.ContentType = "application/json";
			AddCommonParameters( ref parameters );
			SignIfNeeded( ref parameters );

            StringBuilder p = new StringBuilder( ToJson( parameters ));
            
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(p.ToString());
            request.ContentLength = bytes.Length;

            IAsyncResult result = request.BeginGetRequestStream(null, null);
            result.AsyncWaitHandle.WaitOne();

            using (Stream requestStream = request.EndGetRequestStream(result))
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }

            return request;
        }

        /// <summary>
        /// Creates a put request.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected HttpWebRequest CreatePutRequest(string url, Hashtable parameters)
        {
            return this.CreateRequestWithParameters(url, "PUT", parameters);
        }

        #endregion
		
		#region Helpers
		/// <summary>
		/// Add the parameters common to all api calls to a <see cref="HashTable"/> of parameters. Parameters added are "version",
		/// "format" and "api_key"
		/// </summary>
		/// <param name="parameters">
		/// A <see cref="Hashtable"/> reference
		/// </param>
		protected void AddCommonParameters( ref Hashtable parameters )
		{
			parameters.Add( "version", VERSION );
			parameters.Add( "format", "xml" );
			parameters.Add( "api_key", this.ApiKey );
		}
		
		/// <summary>
		/// Takes a <see cref="HashTable"/> containing parameters for an HTTP request and puts them in the form
		/// "k1=v1&k2=v2&...". Also url encodes all data
		/// </summary>
		/// <param name="parameters">
		/// A <see cref="Hashtable"/> containing the parameters to be used.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing the request string that can be added to the end of a url
		/// </returns>
		protected string BuildParameterString (Hashtable parameters)
		{
			StringBuilder paramString = new StringBuilder ();
			
			// iterate through each item in the hashtable...
			foreach (object parameter in parameters.Keys) {
				paramString.Append (HttpUtility.UrlEncode ( parameter.ToString() ) + "=" + HttpUtility.UrlEncode ( parameters[parameter].ToString() ) + "&");
			}
			
			return paramString.ToString();
		}

		/// <summary>
		/// Convenience function to convert a <see cref="Hashtable"/> of parameters to JSON format. Nested Hashtables are OK.
		/// </summary>
		/// <param name="parameters">
		/// A <see cref="Hashtable"/> containing the option to JSON-ify
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing valid JSON that represents the <see cref="Hashtable"/> that was given.
		/// </returns>
		protected string ToJson (Hashtable parameters)
		{
			// pretty simple stuff, C# already has a function to JSON-ify
			JavaScriptSerializer json = new JavaScriptSerializer ();
			return json.Serialize (parameters);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters">
		/// A <see cref="Hashtable"/>
		/// </param>
		public void SignIfNeeded (ref Hashtable parameters)
		{
			// only sign if a secret key has been set
			if (!String.IsNullOrEmpty (this.ApiSecret)) {
				// add the timestamp to our parameters
				string timestamp = GenerateUnixTimestamp ().ToString ();
				parameters.Add ("timestamp", timestamp);
				
				// the parameters must be in alpha order before signing
				// create an array from the hash keys, and use that to sort the parameters
				ArrayList ParameterKeys = new ArrayList (parameters.Keys);
				ParameterKeys.Sort ();
				
				// concatenate the parameters and values together then add the secret and sign it
				StringBuilder StringToSign = new StringBuilder ();
				foreach (object key in ParameterKeys) {
					StringToSign.Append (key + "=" + parameters[key]);
				}
				
				string signature = GenerateSignature (StringToSign.Append (this.ApiSecret).ToString ());
				
				// Add signature as a parameter
				parameters.Add ("signature", signature);
				
			}
		}
		#endregion
		
    }
}