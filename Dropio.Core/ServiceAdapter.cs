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

namespace Dropio.Core
{
    public abstract class ServiceAdapter
    {
		public const string ACCOUNTS = "accounts/";
        public const string DROPS = "drops/";
		public const string EMPTY_DROP = "/empty";
		public const string PROMOTE_NICK = "/promote";
        public const string ASSETS = "/assets/";
        public const string COMMENTS = "/comments/";
        public const string SUBSCRIPTIONS = "/subscriptions/";
        public const string SEND_TO = "/send_to/";
        public const string FROM_API = "/from_api";
		public const string EMBED_CODE = "/embed_code";
		public const string UPLOAD_CODE = "/upload_code";
		public const string DOWNLOAD_ORIGINAL = "/download/original";
		public const string COPY = "/copy";
		public const string MOVE = "/move";
        public const string VERSION = "3.0";

        public abstract string BaseUrl { get; }
        public abstract string ApiBaseUrl { get; }
        public abstract string UploadUrl { get; }
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
                response = request.GetResponse() as HttpWebResponse;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (respond != null)
                    {
                        respond(response);
                    }
                }
                else
                {
                    throw new ServiceException(ServiceError.BadRequest, "There was a problem with your request.");
                }
            }
            catch (WebException exc)
            {
                this.HandleException(exc);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }

                response = null;
            }
        }

        /// <summary>
        /// Reads the response.
        /// </summary>
        /// <param name="response">The response.</param>
        private void ReadResponse(HttpWebResponse response, ReadDocument read)
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
            
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                if (read != null)
                {
                    read(doc);
                }
            }
        }
		
		public void SignIfNeeded( ref Hashtable parameters )
		{
			// only sign if a secret key has been set
			if( !String.IsNullOrEmpty( this.ApiSecret ) )
			{
				// add the timestamp to our parameters
				string timestamp = GenerateUnixTimestamp().ToString();
				parameters.Add( "timestamp", timestamp );
				
				// the parameters must be in alpha order before signing
				// create an array from the hash keys, and use that to sort the parameters
				ArrayList ParameterKeys = new ArrayList( parameters.Keys );
				ParameterKeys.Sort();
				
				// concatenate the parameters and values together then add the secret and sign it
				StringBuilder StringToSign = new StringBuilder();
				foreach( object key in ParameterKeys )
				{
					StringToSign.Append( key + "=" + parameters[key] );
				}
				
				string signature = GenerateSignature( StringToSign.Append( this.ApiSecret ).ToString() );

				// Add signature as a parameter
				parameters.Add( "signature", signature );
				
			}
		}
		
		/// <summary>
        /// Gets an original file download url.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public string OriginalFileUrl(Asset asset)
        {
			Hashtable parameters = new Hashtable();
            StringBuilder sb = new StringBuilder();
			
            sb.Append(this.ApiBaseUrl + DROPS + asset.Drop.Name + ASSETS + asset.Name + DOWNLOAD_ORIGINAL);
			
			AddCommonParameters( ref parameters );
			SignIfNeeded( ref parameters );
			
			sb.Append("?");
			
			sb.Append( BuildParameterString( parameters ));

            return sb.ToString();
        }

        /// <summary>
        /// Generates the signature.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="expires">The expires.</param>
        /// <returns></returns>
        protected string GenerateSignature( string StringToSign )
        {
            
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] input = Encoding.UTF8.GetBytes( StringToSign );
            byte[] result = sha1.ComputeHash(input);
			// returned hash has a dash between each byte, remove and convert to lowercase
            return BitConverter.ToString(result).Replace( "-", "" ).ToLower();


        }

        /// <summary>
        /// Generates the unix timestamp.
        /// </summary>
        /// <returns></returns>
        protected long GenerateUnixTimestamp()
        {
            TimeSpan ts = (DateTime.UtcNow.AddMinutes(10) - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long) ts.TotalSeconds;
        }

		// STAY
        /// <summary>
        /// Creates a Drop.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="guestsCanAdd">if set to <c>true</c> [guests can add].</param>
        /// <param name="guestsCanComment">if set to <c>true</c> [guests can comment].</param>
        /// <param name="guestsCanDelete">if set to <c>true</c> [guests can delete].</param>
        /// <param name="expirationLength">Length of the expiration.</param>
        /// <param name="password">The password.</param>
        /// <param name="adminPassword">The admin password.</param>
        /// <param name="premiumCode">The premium code.</param>
        /// <returns></returns>
        public Drop CreateDrop(string name, string description, string emailKey, int maxSize, string chatPassword ) 
        {
            Drop d = null;

			Hashtable parameters = new Hashtable();

			// Since all these parameters are options, we only want to add the ones that have been explicitly specified
			if( name != string.Empty)
	            parameters.Add("name", name);
			if( description != string.Empty)
				parameters.Add("description", description);
			if( emailKey != string.Empty)
				parameters.Add("email_key", emailKey);
			if( maxSize > 0 )
				parameters.Add("max_size", maxSize.ToString() );
			if( chatPassword != string.Empty)
				parameters.Add("chat_password", chatPassword);

            HttpWebRequest request = this.CreatePostRequest(this.CreateDropUrl(string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => d = this.CreateAndMapDrop(doc.SelectSingleNode("drop")));
            });

            return d;
        }

        /// <summary>
        /// Finds a drop by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Drop FindDrop(string name) // , string token)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "The given name can't be blank.");

            Drop d = null;

            HttpWebRequest request = this.CreateGetRequest(this.CreateDropUrl(name)); //, token);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => d = this.CreateAndMapDrop(doc.SelectSingleNode("drop")));
            });

            return d;
        }
		
		/// <summary>
		/// Gets a paginated list of drops with the Manager Account. Requires Manager API Token.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <param name="managerApiToken">The manager API token. </param>
		/// <returns></returns>
		public List<Drop> FindManagerDrops(int page)
		{
			
			List<Drop> drops = new List<Drop>();

			Hashtable parameters = new Hashtable();
			parameters.Add( "page", page.ToString() );
			
            HttpWebRequest request = this.CreateGetRequest(this.CreateManagerDropsUrl()); //, parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/drops/drop");

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
        /// Empties the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool EmptyDrop(Drop drop)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            bool emptied = false;

            HttpWebRequest request = this.CreatePutRequest(this.CreateEmptyDropUrl(drop.Name), new Hashtable() );
            CompleteRequest(request, (HttpWebResponse response) => { emptied = true; });

            return emptied;
        }

        /// <summary>
        /// Deletes the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool DeleteDrop(Drop drop)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            bool destroyed = false;

            Hashtable parameters = new Hashtable();

            HttpWebRequest request = this.CreateDeleteRequest(this.CreateDropUrl(drop.Name), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
        }

        /// <summary>
        /// Updates the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool UpdateDrop(Drop drop, string name, string chatPassword)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            bool updated = false;
			
            Hashtable parameters = new Hashtable();

			if( !String.IsNullOrEmpty( name ))
				parameters.Add("name", name);
			if( !String.IsNullOrEmpty( chatPassword ))
				parameters.Add("chat_password", chatPassword);
			
            HttpWebRequest request = this.CreatePutRequest(this.CreateDropUrl(drop.Name), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => this.MapDrop(drop, doc.SelectSingleNode("drop")));
                updated = true;
            });

            return updated;
        }

        /// <summary>
        /// Finds the asset.
        /// </summary>
        /// <param name="dropUrl">The drop name.</param>
        /// <param name="name">The asset name.</param>
        /// <returns></returns>
        public Asset FindAsset(Drop drop, string name)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            if (name == null)
                throw new ArgumentNullException("name", "The given drop name can't be null");

            Asset a = null;

            HttpWebRequest request = this.CreateGetRequest(this.CreateAssetUrl(drop.Name, name)); //, token);

            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc) 
                {
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
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            List<Asset> assets = new List<Asset>();

			Hashtable parameters = new Hashtable();
			parameters.Add( "page", page.ToString() );
            parameters.Add( "order", (order == Order.Newest ) ? "newest" : "older" );
            HttpWebRequest request = this.CreateGetRequest(this.CreateAssetUrl(drop.Name, string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
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
        /// <returns></returns
		public List<Subscription> FindSubscriptions(Drop drop, int page)
		{
			if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            List<Subscription> subscriptions = new List<Subscription>();

			Hashtable parameters = new Hashtable();
			parameters.Add( "page", page.ToString() );
            HttpWebRequest request = this.CreateGetRequest(this.CreateSubscriptionsUrl(drop.Name)); //, parameters);
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

            HttpWebRequest request = this.CreateDeleteRequest(this.CreateSubscriptionUrl(drop.Name, subscription.Id), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
		}
		
		/// <summary>
		/// Gets the embed code for the asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <returns></returns>
		public string GetAssetEmbedCode(Asset asset)
		{
			if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");

			Drop drop = asset.Drop;
			string embed_code = string.Empty;
			
            HttpWebRequest request = this.CreateGetRequest(this.CreateAssetEmbedCodeUrl(drop.Name, asset.Name)); //, token);

            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc) 
                {
                    XmlNodeList nodes = doc.SelectNodes("/response");
                    embed_code = this.ExtractInnerText(nodes[0],"embed_code");
                });
            });

            return embed_code;
		}

        /// <summary>
        /// Creates the note.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="title">The title.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Asset CreateNote(Drop drop, string title, string contents, string description)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            Asset a = null;

            Hashtable parameters = new Hashtable();

            parameters.Add("title", title);
            parameters.Add("contents", contents);
			parameters.Add("description", description);

            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetUrl(drop.Name, string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNode node = doc.SelectSingleNode( "/asset" );
					a = this.CreateAndMapAsset(drop, node);
                });
            });

            return a;
        }

        /// <summary>
        /// Creates the link.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public Asset CreateLink(Drop drop, string title, string description, string url)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            Asset a = null;

            Hashtable parameters = new Hashtable();

            parameters.Add("title", title);
            parameters.Add("description", description);
            parameters.Add("url", url);

            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetUrl(drop.Name, string.Empty), parameters);

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

            HttpWebRequest request = this.CreateDeleteRequest(this.CreateAssetUrl(drop.Name, asset.Name), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
        }

        /// <summary>
        /// Updates the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public bool UpdateAsset(Asset asset, string newName, string newDescription)
        {
            if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");

            bool updated = false;
            Drop drop = asset.Drop;

            Hashtable parameters = new Hashtable();
				
			if( !string.IsNullOrEmpty( newName ))
				parameters.Add("name", newName);
			if( !string.IsNullOrEmpty( newDescription ))
				parameters.Add("description", newDescription );
			//if( !string.IsNullOrEmpty( asset.))
			
            HttpWebRequest request = this.CreatePutRequest(this.CreateAssetUrl(drop.Name, asset.Name), parameters);
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
        /// Sends to drop.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="dropToken">Drop token.</param>
        public void SendToDrop(Asset asset, string dropName, string dropToken)
        {
            this.Copy(asset, dropName);
        }
		
		protected bool Copy(Asset asset, string dropName)
		{
			if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");
			
			if (asset == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

			bool copied = false;
            Drop drop = asset.Drop;

            Hashtable parameters = new Hashtable();

            parameters.Add("drop_name", dropName);

            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetCopyUrl(drop.Name, asset.Name), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { copied = true; });

            return copied;
		}

        /// <summary>
        /// Sends the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        protected void Send(Asset a, Hashtable parameters)
        {
            if (a == null)
                throw new ArgumentNullException("a", "The given asset can't be null");

            Drop drop = a.Drop;

            HttpWebRequest request = this.CreatePostRequest(this.CreateSendToUrl(drop.Name, a.Name), parameters);
            CompleteRequest(request, null);
        }
		
		/// <summary>
		/// Copies the asset to the given drop and returns the new asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool CopyAsset(Asset asset, Drop targetDrop)
		{
			return this.Copy(asset, targetDrop.Name);
		}
		
		/// <summary>
		/// Moves the asset to the given drop.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool MoveAsset(Asset asset, Drop targetDrop)
		{
			if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");
			
			if (asset == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

			bool moved = false;
            Drop drop = asset.Drop;

            Hashtable parameters = new Hashtable();

            parameters.Add("drop_name", targetDrop.Name);

            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetMoveUrl(drop.Name, asset.Name), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { moved = true; });

            return moved;
		}
		
		/// <summary>
        /// Adds a file via a url.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="url">The url.</param>
        /// <param name="description">The description.</param>
        /// <returns></return>
//		public Asset AddFileFromUrl(Drop drop, string url, string description)
//		{
//			if (drop == null)
//                throw new ArgumentNullException("drop", "The given drop can't be null");
//
//            Asset a = null;
//
//            Hashtable parameters = new Hashtable();
//
//            parameters.Add("file_url", url);
//			parameters.Add("description", description);
//
//            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetUrl(drop.Name, string.Empty), parameters);
//            CompleteRequest(request, delegate(HttpWebResponse response)
//            {
//                ReadResponse(response, delegate(XmlDocument doc)
//                {
//                    XmlNodeList nodes = doc.SelectNodes("/asset");
////                    a = this.CreateAndMapAsset(drop, nodes[0]) as Asset;
//                });
//            });
//
//            return a;
//		}

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="file">The file.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Asset AddFile(Drop drop, string file, string comment, string description)
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
			parameters.Add( "description", description );
			
			AddCommonParameters( ref parameters );
			SignIfNeeded( ref parameters );
			
            StringBuilder sb = new StringBuilder();
            string fileName = Path.GetFileName(file);

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

            request.ContentLength = postContents.Length + new FileInfo(file).Length + postFooter.Length;

            request.AllowWriteStreamBuffering = false;
            Stream resStream = request.GetRequestStream();
            resStream.Write(postContents, 0, postContents.Length);

            if (OnTransferProgress != null)
            {
                OnTransferProgress(this, new TransferProgressEventArgs(postContents.LongLength, request.ContentLength, false));
            }

            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
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
        /// Maps the length of the expiration.
        /// </summary>
        /// <param name="expirationLength">Length of the expiration.</param>
        /// <returns></returns>
//        protected string MapExpirationLength(ExpirationLength expirationLength)
//        {
//            switch (expirationLength)
//            {
//                case ExpirationLength.OneDayFromLastView:
//                    return "1_DAY_FROM_LAST_VIEW";
//                case ExpirationLength.OneWeekFromLastView:
//                    return "1_WEEK_FROM_LAST_VIEW";
//                case ExpirationLength.OneMonthFromLastView:
//                    return "1_MONTH_FROM_LAST_VIEW";
//                case ExpirationLength.OneYearFromLastView:
//                    return "1_YEAR_FROM_LAST_VIEW";
//                case ExpirationLength.OneDayFromNow:
//                    return "1_DAY_FROM_NOW";
//                case ExpirationLength.OneWeekFromNow:
//                    return "1_WEEK_FROM_NOW";
//                case ExpirationLength.OneMonthFromNow:
//                    return "1_MONTH_FROM_NOW";
//                case ExpirationLength.OneYearFromNow:
//                    return "1_YEAR_FROM_NOW";
//            }
//
//            return string.Empty;
//        }

        /// <summary>
        /// Extracts the length of the expiration.
        /// </summary>
        /// <param name="expirationLength">Length of the expiration.</param>
        /// <returns></returns>
        protected ExpirationLength ExtractExpirationLength(string expirationLength)
        {
            switch (expirationLength)
            {
                case "1_DAY_FROM_LAST_VIEW":
                    return ExpirationLength.OneDayFromLastView;
                case "1_WEEK_FROM_LAST_VIEW":
                    return ExpirationLength.OneWeekFromLastView;
                case "1_MONTH_FROM_LAST_VIEW":
                    return ExpirationLength.OneMonthFromLastView;
                case "1_YEAR_FROM_LAST_VIEW":
                    return ExpirationLength.OneYearFromLastView;
                case "1_DAY_FROM_NOW":
                    return ExpirationLength.OneDayFromNow;
                case "1_WEEK_FROM_NOW":
                    return ExpirationLength.OneWeekFromNow;
                case "1_MONTH_FROM_NOW":
                    return ExpirationLength.OneMonthFromNow;
                case "1_YEAR_FROM_NOW":
                    return ExpirationLength.OneYearFromNow;
            }

            return ExpirationLength.OneYearFromLastView;
        }

        /// <summary>
        /// Maps the drop.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        protected void MapDrop(Drop d, XmlNode node)
        {
			XmlNode dropNode = node;
            d.Name = this.ExtractInnerText(dropNode, "name");
            d.AssetCount = this.ExtractInt(dropNode, "asset_count");
            d.CurrentBytes = this.ExtractInt(dropNode, "current_bytes");
            d.MaxBytes = this.ExtractInt(dropNode, "max_bytes");
            d.Email = this.ExtractInnerText(dropNode, "email");
			//d.ExpiresAt = this.ExtractDateTime(this.ExtractInnerText(dropNode, "expires_at"));
            d.Description = this.ExtractInnerText(dropNode, "description");
			d.ChatPassword = this.ExtractInnerText(dropNode, "chat_password");
            d.ExpirationLength = this.ExtractExpirationLength(this.ExtractInnerText(dropNode, "expiration_length"));

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
        protected string CreateManagerDropsUrl()
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
        /// Creates the promote nick URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <returns></returns>
        protected string CreatePromoteNickUrl(string dropName)
        {
            return this.ApiBaseUrl + DROPS + dropName + PROMOTE_NICK;
        }
		
		/// <summary>
        /// Creates the drop upload code URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <returns></returns>
        protected string CreateDropUploadCodeUrl(string dropName)
        {
            return this.ApiBaseUrl + DROPS + dropName + UPLOAD_CODE;
        }

        /// <summary>
        /// Creates the asset URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateAssetUrl(string dropName, string assetName)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName;
        }
		
		/// <summary>
        /// Creates the asset embed code URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateAssetEmbedCodeUrl(string dropName, string assetName)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + EMBED_CODE;
        }
		
		/// <summary>
        /// Creates the asset embed code URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateAssetCopyUrl(string dropName, string assetName)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + COPY;
        }
		
		/// <summary>
        /// Creates the asset embed code URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateAssetMoveUrl(string dropName, string assetName)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + MOVE;
        }

        /// <summary>
        /// Creates the comment URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateCommentsUrl(string dropName, string assetName)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + COMMENTS;
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
        /// Creates the comment URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <param name="commentId">The comment id.</param>
        /// <returns></returns>
        protected string CreateCommentUrl(string dropName, string assetName, int commentId)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + COMMENTS + commentId.ToString();
        }

        /// <summary>
        /// Creates the send to URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateSendToUrl(string dropName, string assetName)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + SEND_TO;
        }

        /// <summary>
        /// Creates a delete request.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
//        protected HttpWebRequest CreateDeleteRequest(string url, Hashtable parameters)
//        {
//            return this.CreateRequestWithParameters(url, "DELETE", parameters);
//        }

        /// <summary>
        /// Creates a get request.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        protected HttpWebRequest CreateGetRequest(string url) //, string token)
        {
            //NameValueCollection null_parms = new NameValueCollection();
            //null_parms = null;
            return this.CreateGetRequest(url, new Hashtable() );
        }

        /// <summary>
        /// Creates the get request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected HttpWebRequest CreateGetRequest(string url, Hashtable parameters )
        {

			this.AddCommonParameters( ref parameters );
			
			this.SignIfNeeded( ref parameters );
			
			url += "?";
			
            if ( parameters != null )
			{
           		foreach (string key in parameters.Keys)
            	{
                	url += "&" + key + "=" + parameters[key];
            	}
			}
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            return request;
        }
		
		// this needs to be merged with "CreateGetRequest"
		// C# is complaining about not being allow to send data when you do a
		// DELETE, just like when you do a GET
		protected HttpWebRequest CreateDeleteRequest(string url, Hashtable parameters )
        {

			this.AddCommonParameters( ref parameters );
			
			this.SignIfNeeded( ref parameters );
			
			url += "?";
			
            if ( parameters != null )
			{
           		foreach (string key in parameters.Keys)
            	{
                	url += "&" + key + "=" + parameters[key];
            	}
			}
			
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
			request.Method = "DELETE";
            return request;
        }

        /// <summary>
        /// Creates a post request.
        /// </summary>
        /// <param name="name">The url.</param>
        /// <param name="url">The parameters.</param>
        /// <returns></returns>
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
            request.ContentType = "application/x-www-form-urlencoded";
			AddCommonParameters( ref parameters );
			SignIfNeeded( ref parameters );
            StringBuilder p = new StringBuilder( BuildParameterString( parameters ));
//            foreach (DictionaryEntry key in parameters)
//            {
//                p.Append(HttpUtility.UrlEncode(key.Key.ToString()) + "=" + HttpUtility.UrlEncode(key.Value.ToString()) + "&");
//            }

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
		
		protected void AddCommonParameters( ref Hashtable parameters )
		{
			parameters.Add( "version", VERSION );
			parameters.Add( "format", "xml" );
			parameters.Add( "api_key", this.ApiKey );
		}
		
		protected string BuildParameterString( Hashtable parameters )
		{
			StringBuilder paramString = new StringBuilder();
			
			//paramString.Append( "?" );
			
			foreach( DictionaryEntry key in parameters)
			{
				paramString.Append(HttpUtility.UrlEncode(key.Key.ToString()) + "=" + HttpUtility.UrlEncode(key.Value.ToString()) + "&");
			}
			
			return paramString.ToString();
		}

    }
}