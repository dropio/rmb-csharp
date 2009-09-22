using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Specialized;
using Dropio.Core.Types;
using System.Security.Cryptography;
using System.Web;

namespace Dropio.Core
{
    public abstract class ServiceAdapter
    {
        public const string DROPS = "drops/";
        public const string ASSETS = "/assets/";
        public const string COMMENTS = "/comments/";
        public const string SEND_TO = "/send_to/";
        public const string FROM_API = "/from_api";
		public const string EMBED_CODE = "/embed_code";
        public const string VERSION = "2.0";

        public abstract string BaseUrl { get; }
        public abstract string ApiBaseUrl { get; }
        public abstract string UploadUrl { get; }
        public string ApiKey { get; set; }

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

        /// <summary>
        /// Generates the authenticated drop URL.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public string GenerateAuthenticatedDropUrl(Drop drop)
        {
            string unixTime = GenerateUnixTimestamp().ToString();
            string signature = GenerateSignature(drop, unixTime);
            StringBuilder sb = new StringBuilder();
            sb.Append(this.BaseUrl);
            sb.Append(drop.Name);
            sb.Append(FROM_API);
            sb.Append("?&signature=");
            sb.Append(signature);
            sb.Append("&expires=");
            sb.Append(unixTime);
            return sb.ToString(); 
        }

        /// <summary>
        /// Generates the authenticated asset URL.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public string GenerateAuthenticatedAssetUrl(Asset asset)
        {
            string unixTime = GenerateUnixTimestamp().ToString();
            string signature = GenerateSignature(asset.Drop, unixTime);
            StringBuilder sb = new StringBuilder();
            sb.Append(this.BaseUrl);
            sb.Append(asset.Drop.Name);
            sb.Append("/asset/");
            sb.Append(asset.Name);
            sb.Append(FROM_API);
            sb.Append("?&signature=");
            sb.Append(signature);
            sb.Append("&expires=");
            sb.Append(unixTime);
            return sb.ToString();
        }

        /// <summary>
        /// Generates the signature.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="expires">The expires.</param>
        /// <returns></returns>
        protected string GenerateSignature(Drop drop, string expires)
        {
            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            string before = expires + "+" + token + "+" + drop.Name;
            
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] input = Encoding.UTF8.GetBytes(before);
            byte[] result = sha1.ComputeHash(input);
            string hex = BitConverter.ToString(result);
            hex = hex.Replace("-", "");

            return hex;
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
        public Drop CreateDrop(string name, bool guestsCanAdd, bool guestsCanComment, bool guestsCanDelete, ExpirationLength expirationLength, string password, string adminPassword, string premiumCode)
        {
            Drop d = null;

            NameValueCollection parameters = new NameValueCollection();

            parameters.Add("name", name);
            parameters.Add("guests_can_add", guestsCanAdd.ToString().ToLower());
            parameters.Add("guests_can_comment", guestsCanComment.ToString().ToLower());
            parameters.Add("guests_can_delete", guestsCanDelete.ToString().ToLower());
            parameters.Add("expiration_length", this.MapExpirationLength(expirationLength));
            parameters.Add("password", password);
            parameters.Add("admin_password", adminPassword);
            parameters.Add("premium_code", premiumCode);

            HttpWebRequest request = this.CreatePostRequest(this.CreateDropUrl(string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => d = this.CreateAndMapDrop(doc));
            });

            return d;
        }

        /// <summary>
        /// Finds a drop by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Drop FindDrop(string name, string token)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "The given name can't be blank.");

            Drop d = null;

            HttpWebRequest request = this.CreateGetRequest(this.CreateDropUrl(name), token);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => d = this.CreateAndMapDrop(doc));
            });

            return d;
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

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);

            HttpWebRequest request = this.CreateDeleteRequest(this.CreateDropUrl(drop.Name), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
        }

        /// <summary>
        /// Updates the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool UpdateDrop(Drop drop, string password, string adminPassword, string premiumCode)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            bool updated = false;

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);
            parameters.Add("guests_can_add", drop.GuestsCanAdd.ToString());
            parameters.Add("guests_can_comment", drop.GuestsCanComment.ToString());
            parameters.Add("guests_can_delete", drop.GuestsCanDelete.ToString());
            parameters.Add("expiration_length", this.MapExpirationLength(drop.ExpirationLength));
            parameters.Add("password", password);
            parameters.Add("admin_password", adminPassword);
            parameters.Add("premium_code", premiumCode);

            HttpWebRequest request = this.CreatePutRequest(this.CreateDropUrl(drop.Name), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, (XmlDocument doc) => this.MapDrop(drop, doc));
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

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;

            HttpWebRequest request = this.CreateGetRequest(this.CreateAssetUrl(drop.Name, name), token);

            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc) 
                {
                    XmlNodeList nodes = doc.SelectNodes("/asset");
                    a = this.CreateAndMapAsset(drop, nodes[0]);
                });
            });

            return a;
        }

        /// <summary>
        /// Finds the assets for a given drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public List<Asset> FindAssets(Drop drop, int page)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            List<Asset> assets = new List<Asset>();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            NameValueCollection parameters = new NameValueCollection();
            parameters["token"] = token;
            parameters["page"] = page.ToString();
            HttpWebRequest request = this.CreateGetRequest(this.CreateAssetUrl(drop.Name, string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/assets/asset");

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
        /// <returns></returns
		public List<Subscription> FindSubscriptions(Drop drop)
		{
			return null;
		}
		
		/// <summary>
		/// Creates a Twitter subscription
		/// </summary>
		/// <param name="drop">The drop.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password</param>
		/// <returns></returns>
		public Subscription CreateTwitterSubscription(Drop drop, string username, string password)
		{
			return null;
		}
		
		/// <summary>
		/// Creates an email subscription
		/// </summary>
		/// <param name="drop">The drop.</param>
		/// <param name="email">The email.</param>
		/// <param name="welcomeFrom">The welcome message from address.</param>
		/// <param name="welcomeSubject">The welcome message subject.</param>
		/// <param name="welcomeMessage">The welcome message.</param>
		/// <returns></returns>
		public Subscription CreateEmailSubscription(Drop drop, string email, string welcomeFrom, string welcomeSubject, string welcomeMessage)
		{
			return null;
		}
		
		/// <summary>
        /// Saves the subscription.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
		public bool UpdateSubscription(Subscription subscription)
		{
			return false;
		}
		
		/// <summary>
        /// Deletes the subscription.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
		public bool DeleteSubscription(Subscription subscription)
		{
			return false;
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
			
            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;

            HttpWebRequest request = this.CreateGetRequest(this.CreateAssetEmbedCodeUrl(drop.Name, asset.Name), token);

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
        /// <returns></returns>
        public Note CreateNote(Drop drop, string title, string contents)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            Note a = null;

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);
            parameters.Add("title", title);
            parameters.Add("contents", contents);

            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetUrl(drop.Name, string.Empty), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/asset");
                    a = this.CreateAndMapAsset(drop, nodes[0]) as Note;
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
        public Link CreateLink(Drop drop, string title, string description, string url)
        {
            if (drop == null)
                throw new ArgumentNullException("drop", "The given drop can't be null");

            Link a = null;

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);
            parameters.Add("title", title);
            parameters.Add("description", description);
            parameters.Add("url", url);

            HttpWebRequest request = this.CreatePostRequest(this.CreateAssetUrl(drop.Name, string.Empty), parameters);

            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/asset");
                    a = this.CreateAndMapAsset(drop, nodes[0]) as Link;
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

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);

            HttpWebRequest request = this.CreateDeleteRequest(this.CreateAssetUrl(drop.Name, asset.Name), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
        }

        /// <summary>
        /// Updates the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public bool UpdateAsset(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");

            bool updated = false;
            Drop drop = asset.Drop;

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);
            parameters.Add("name", asset.Name);

            this.AddTypedProperties(parameters, asset);

            HttpWebRequest request = this.CreatePutRequest(this.CreateAssetUrl(drop.Name, asset.Name), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/asset");

                    this.MapAsset(asset, drop, nodes[0]);
                    updated = true;
                });
            });

            return updated;
        }

        /// <summary>
        /// Adds the typed properties.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="asset">The asset.</param>
        private void AddTypedProperties(NameValueCollection parameters, Asset asset)
        {
            switch (asset.DisplayType)
            {
                case "Link":
                    parameters.Add("title", ((Link)asset).Title);
                    parameters.Add("description", ((Link)asset).Description);
                    parameters.Add("url", ((Link)asset).Url);
                    break;
                case "Note":
                    parameters.Add("title", ((Note)asset).Title);
                    parameters.Add("contents", ((Note)asset).Contents);
                    break;
            }
        }

        /// <summary>
        /// Deletes the comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public bool DeleteComment(Comment comment)
        {
            bool destroyed = false;
            Asset asset = comment.Asset;
            Drop drop = asset.Drop;

            NameValueCollection parameters = new NameValueCollection();

            string token = drop.AdminToken;
            parameters.Add("token", token);

            HttpWebRequest request = this.CreateDeleteRequest(this.CreateCommentUrl(drop.Name, asset.Name, comment.Id), parameters);
            CompleteRequest(request, (HttpWebResponse response) => { destroyed = true; });

            return destroyed;
        }

        /// <summary>
        /// Updates the comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public bool UpdateComment(Comment comment)
        {
            if (comment == null)
                throw new ArgumentNullException("comment", "The given comment can't be null");

            bool updated = false;
            Asset asset = comment.Asset;
            Drop drop = asset.Drop;

            NameValueCollection parameters = new NameValueCollection();

            string token = drop.AdminToken;
            parameters.Add("token", token);
            parameters.Add("contents", comment.Contents);

            HttpWebRequest request = this.CreatePutRequest(this.CreateCommentUrl(drop.Name, asset.Name, comment.Id), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/comment");

                    this.MapComment(asset, comment, nodes[0]);
                    updated = true;
                });
            });

            return updated;
        }

        /// <summary>
        /// Finds the comments.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public List<Comment> FindComments(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");

            List<Comment> comments = new List<Comment>();

            Drop drop = asset.Drop;

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;

            HttpWebRequest request = this.CreateGetRequest(this.CreateCommentUrl(drop.Name, asset.Name), token);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/comments/comment");

                    foreach (XmlNode node in nodes)
                    {
                        Comment c = this.CreateAndMapComment(asset, node);
                        comments.Add(c);
                    }
                });
            });

            return comments;
        }

        /// <summary>
        /// Creates the comment.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public Comment CreateComment(Asset asset, string contents)
        {
            if (asset == null)
                throw new ArgumentNullException("asset", "The given asset can't be null");

            Comment c = null;
            Drop drop = asset.Drop;

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);
            parameters.Add("contents", contents);

            HttpWebRequest request = this.CreatePostRequest(this.CreateCommentUrl(drop.Name, asset.Name), parameters);
            CompleteRequest(request, delegate(HttpWebResponse response)
            {
                ReadResponse(response, delegate(XmlDocument doc)
                {
                    XmlNodeList nodes = doc.SelectNodes("/comment");
                    c = this.CreateAndMapComment(asset, nodes[0]);
                });
            });

            return c;
        }

        /// <summary>
        /// Sends to fax.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="faxNumber">The fax number.</param>
        public void SendToFax(Asset asset, string faxNumber)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("medium", "fax");
            parameters.Add("fax_number", faxNumber);
            this.Send(asset, parameters);
        }

        /// <summary>
        /// Sends to emails.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="emails">The emails.</param>
        /// <param name="message">The message.</param>
        public void SendToEmails(Asset asset, List<string> emails, string message)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("medium", "email");
            
            StringBuilder emailList = new StringBuilder();
            foreach (string email in emails)
            {
                emailList.Append(email + ",");
            }

            parameters.Add("emails", emailList.ToString());
            parameters.Add("message", message);
            this.Send(asset, parameters);
        }

        /// <summary>
        /// Sends to drop.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="dropToken">Drop token.</param>
        public void SendToDrop(Asset asset, string dropName, string dropToken)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("medium", "drop");
            parameters.Add("drop_name", dropName);
			
			if (!string.IsNullOrEmpty(dropToken))
			{
				parameters.Add("drop_token", dropToken);
			}
			
            this.Send(asset, parameters);
        }

        /// <summary>
        /// Sends the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        protected void Send(Asset a, NameValueCollection parameters)
        {
            if (a == null)
                throw new ArgumentNullException("a", "The given asset can't be null");

            Drop drop = a.Drop;

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;

            parameters.Add("token", token);

            HttpWebRequest request = this.CreatePostRequest(this.CreateSendToUrl(drop.Name, a.Name), parameters);
            CompleteRequest(request, null);
        }
		
		/// <summary>
		/// Copies the asset to the given drop and returns the new asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="drop">The drop.</param>
		/// <returns></returns>
		public Asset CopyAsset(Asset asset, Drop drop)
		{
			return null;
		}
		
		/// <summary>
		/// Moves the asset to the given drop.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="drop">The drop.</param>
		/// <returns></returns>
		public void MoveAsset(Asset asset, Drop drop)
		{
		}

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public Asset AddFile(Drop drop, string file)
        {
            string requestUrl = this.UploadUrl;

            NameValueCollection parameters = new NameValueCollection();

            string token = string.IsNullOrEmpty(drop.AdminToken) ? drop.GuestToken : drop.AdminToken;
            parameters.Add("token", token);

            HttpWebRequest request = HttpWebRequest.Create(requestUrl) as HttpWebRequest;
            string boundary = "DROPIO_MIME_" + DateTime.Now.ToString("yyyyMMddhhmmss");

            request.Method = "POST";
            request.KeepAlive = true;
            request.Timeout = 30000000;
            request.ContentType = "multipart/form-data; boundary=" + boundary + "";
            request.Expect = "";

            parameters["api_key"] = this.ApiKey;
            parameters["format"] = "xml";
            parameters["drop_name"] = drop.Name;
            parameters["version"] = VERSION;

            StringBuilder sb = new StringBuilder();
            string fileName = Path.GetFileName(file);

            foreach (string key in parameters.AllKeys)
            {
                sb.Append("--" + boundary + "\r\n");
                sb.Append("Content-Disposition: form-data; name=\"" + key + "\"\r\n");
                sb.Append("\r\n");
                sb.Append(parameters[key] + "\r\n");
            }

            // File
            sb.Append("--" + boundary + "\r\n");
            sb.Append("Content-Disposition: form-data; name=\"file\"; filename=\"" + fileName + "\"\r\n");
            sb.Append("Content-Type: " + this.GetMimeType(file) + "\r\n");
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
                    XmlNodeList nodes = doc.SelectNodes("/asset");
                    a = this.CreateAndMapAsset(drop, nodes[0]) as Asset;
                });
            });

            return a;
        }

        /// <summary>
        /// Gets the type of the MIME.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        protected string GetMimeType(string fileName)
        {
            string mime = "application/octetstream";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (rk != null && rk.GetValue("Content Type") != null)
                mime = rk.GetValue("Content Type").ToString();
            return mime;
        }

        /// <summary>
        /// Saves the file to the given path.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="path">The path.</param>
        public void SaveFile(Asset asset, string path)
        {
            if (!string.IsNullOrEmpty(asset.FileUrl))
            {
                long bytesSoFar = 0;
                long totalBytes = 0;
                int bytesOut = 0;

                HttpWebRequest request = HttpWebRequest.Create(asset.FileUrl) as HttpWebRequest;
                request.AllowAutoRedirect = true;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                totalBytes = response.ContentLength;
                
                if (!Directory.Exists(path))
                {
                    throw new ArgumentException("Value given is not a complete path.", "path");
                }

                string contentHeader = response.GetResponseHeader("Content-Disposition");
                // Kinda hacky, I know, but it's after attachment; filename=*utf-8''
                string fileName = HttpUtility.UrlDecode(contentHeader.Substring(29));
                string fullPath = Path.Combine(path, fileName);


                using (Stream responseStream = response.GetResponseStream())
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    {
                        long size = Math.Min(Math.Max(totalBytes / 100, 50 * 1024), 1024 * 1024);
                        byte[] buffer = new byte[size];

                        while ((bytesOut = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            fs.Write(buffer, 0, bytesOut);
                            bytesSoFar += bytesOut;
                            if (OnTransferProgress != null)
                            {
                                OnTransferProgress(this, new TransferProgressEventArgs(bytesSoFar, totalBytes, false));
                            }
                        }

                        if (OnTransferProgress != null)
                        {
                            OnTransferProgress(this, new TransferProgressEventArgs(totalBytes, totalBytes, true));
                        }
                    }
                }
            }
            else
            {
                throw new ServiceException(ServiceError.NotAuthorized);
            }
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
        protected string MapExpirationLength(ExpirationLength expirationLength)
        {
            switch (expirationLength)
            {
                case ExpirationLength.OneDayFromLastView:
                    return "1_DAY_FROM_LAST_VIEW";
                case ExpirationLength.OneWeekFromLastView:
                    return "1_WEEK_FROM_LAST_VIEW";
                case ExpirationLength.OneMonthFromLastView:
                    return "1_MONTH_FROM_LAST_VIEW";
                case ExpirationLength.OneYearFromLastView:
                    return "1_YEAR_FROM_LAST_VIEW";
                case ExpirationLength.OneDayFromNow:
                    return "1_DAY_FROM_NOW";
                case ExpirationLength.OneWeekFromNow:
                    return "1_WEEK_FROM_NOW";
                case ExpirationLength.OneMonthFromNow:
                    return "1_MONTH_FROM_NOW";
                case ExpirationLength.OneYearFromNow:
                    return "1_YEAR_FROM_NOW";
            }

            return string.Empty;
        }

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
        protected void MapDrop(Drop d, XmlDocument doc)
        {
            XmlNode node = doc.SelectSingleNode("drop");
            d.Name = this.ExtractInnerText(node, "name");
            d.AssetCount = this.ExtractInt(node, "asset_count");
            d.AdminToken = this.ExtractInnerText(node, "admin_token");
            d.GuestToken = this.ExtractInnerText(node, "guest_token");
            d.CurrentBytes = this.ExtractInt(node, "current_bytes");
            d.MaxBytes = this.ExtractInt(node, "max_bytes");
            d.Voicemail = this.ExtractInnerText(node, "voicemail");
            d.Fax = this.ExtractInnerText(node, "fax");
            d.Conference = this.ExtractInnerText(node, "conference");
            d.Email = this.ExtractInnerText(node, "email");
            d.Rss = this.ExtractInnerText(node, "rss");
          
            d.GuestsCanAdd = this.ExtractBoolean(node, "guests_can_add");
            d.GuestsCanComment = this.ExtractBoolean(node, "guests_can_comment");
            d.GuestsCanDelete = this.ExtractBoolean(node, "guests_can_delete");

            d.ExpirationLength = this.ExtractExpirationLength(this.ExtractInnerText(node, "expiration_length"));

        }

        /// <summary>
        /// Creates and maps a drop.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        protected Drop CreateAndMapDrop(XmlDocument doc)
        {
            Drop d = new Drop();
            this.MapDrop(d, doc);
            return d;
        }

        /// <summary>
        /// Creates the and map asset.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected Asset CreateAndMapAsset(Drop d, XmlNode node)
        {
            string displayType = this.ExtractInnerText(node, "type");
            Asset a = this.CreateTypedAsset(displayType);
            this.MapAsset(a, d, node);
            return a;
        }

        /// <summary>
        /// Creates the typed asset.
        /// </summary>
        /// <param name="displayType">The display type.</param>
        /// <returns></returns>
        protected Asset CreateTypedAsset(string displayType)
        {
            switch (displayType)
            {
                case "audio":
                    return new Audio();
                case "document":
                    return new Document();
                case "note":
                    return new Note();
                case "image":
                    return new Image();
                case "movie":
                    return new Movie();
                case "link":
                    return new Link();
                default:
                    return new Asset();
            }
        }

        /// <summary>
        /// Maps the typed data.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="node">The node.</param>
        protected void MapTypedData(Asset asset, XmlNode node)
        {
            switch (asset.DisplayType)
            {
                case "Audio":
                    Audio a = asset as Audio;
                    a.Artist = this.ExtractInnerText(node, "artist");
                    a.Title = this.ExtractInnerText(node, "title");
                    a.Duration = this.ExtractInt(node, "duration");
                    break;
                case "Document":
                    Document d = asset as Document;
                    d.Pages = this.ExtractInt(node, "pages");
				    d.FaxStatus = this.ExtractFaxStatus(node, "fax_status");
                    break;
                case "Note":
                    Note n = asset as Note;
                    n.Title = this.ExtractInnerText(node, "title");
                    n.Contents = this.ExtractInnerText(node, "contents");
                    break;
                case "Image":
                    Image i = asset as Image;
                    i.Height = this.ExtractInt(node, "height");
                    i.Width = this.ExtractInt(node, "width");
                    break;
                case "Movie":
                    Movie m = asset as Movie;
                    m.Duration = this.ExtractInt(node, "duration");
                    break;
                case "Link":
                    Link l = asset as Link;
                    l.Title = this.ExtractInnerText(node, "title");
                    l.Description = this.ExtractInnerText(node, "description");
                    l.Url = this.ExtractInnerText(node, "url");
                    break;
            }
        }

        /// <summary>
        /// Maps the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="drop">The drop.</param>
        /// <param name="node">The node.</param>
        protected void MapAsset(Asset asset, Drop drop, XmlNode node)
        {
            asset.CreatedAt = this.ExtractDateTime(this.ExtractInnerText(node, "created_at"));
            asset.Filesize = this.ExtractInt(node, "filesize");
            asset.Status = (Status)Enum.Parse(typeof(Status), this.ExtractInnerText(node,"status"), true);
            asset.Name = this.ExtractInnerText(node, "name");
            asset.ThumbnailUrl = this.ExtractInnerText(node, "thumbnail");
            asset.FileUrl = this.ExtractInnerText(node, "file");
            asset.ConvertedFileUrl = this.ExtractInnerText(node, "converted");
			asset.HiddenUrl = this.ExtractInnerText(node, "hidden_url");
            asset.Drop = drop;

            this.MapTypedData(asset, node);
        }

        /// <summary>
        /// Creates the and map comment.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected Comment CreateAndMapComment(Asset asset, XmlNode node)
        {
            Comment c = new Comment();
            this.MapComment(asset, c, node);
            return c;
        }

        /// <summary>
        /// Maps the comment.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="c">The c.</param>
        /// <param name="node">The node.</param>
        protected void MapComment(Asset asset, Comment c, XmlNode node)
        {
            c.CreatedAt = this.ExtractDateTime(this.ExtractInnerText(node, "created_at"));
            c.Contents = this.ExtractInnerText(node, "contents");
            c.Id = this.ExtractInt(node, "id");
            c.Asset = asset;
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
		
		/// <summary>
        /// Extracts the fax status.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected FaxStatus ExtractFaxStatus(XmlNode node, string p)
        {
            string returnedStatus = this.ExtractInnerText(node,p);
			if (!string.IsNullOrEmpty(returnedStatus))
			{
				switch (returnedStatus)
				{
				case "pending":
					return FaxStatus.Pending;
				case "failed":
					return FaxStatus.Failed;
				case "success":
					return FaxStatus.Success;
				}
			}
            return FaxStatus.None;
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
        /// Creates the comment URL.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        protected string CreateCommentUrl(string dropName, string assetName)
        {
            return this.ApiBaseUrl + DROPS + dropName + ASSETS + assetName + COMMENTS;
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
        protected HttpWebRequest CreateDeleteRequest(string url, NameValueCollection parameters)
        {
            return this.CreateRequestWithParameters(url, "DELETE", parameters);
        }

        /// <summary>
        /// Creates a get request.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        protected HttpWebRequest CreateGetRequest(string url, string token)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters["token"] = token;
            return this.CreateGetRequest(url, parameters);
        }

        /// <summary>
        /// Creates the get request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected HttpWebRequest CreateGetRequest(string url, NameValueCollection parameters)
        {
            string newUrl = url + "?format=xml&version=" + VERSION + "&api_key=" + this.ApiKey;
            foreach (string key in parameters.Keys)
            {
                newUrl += "&" + key + "=" + parameters[key];
            }
            HttpWebRequest request = HttpWebRequest.Create(newUrl) as HttpWebRequest;
            return request;
        }

        /// <summary>
        /// Creates a post request.
        /// </summary>
        /// <param name="name">The url.</param>
        /// <param name="url">The parameters.</param>
        /// <returns></returns>
        protected HttpWebRequest CreatePostRequest(string url, NameValueCollection parameters)
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
        protected HttpWebRequest CreateRequestWithParameters(string url, string method, NameValueCollection parameters)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            parameters["api_key"] = this.ApiKey;
            parameters["format"] = "xml";
            parameters["version"] = VERSION;
            StringBuilder p = new StringBuilder();
            foreach (string key in parameters)
            {
                p.Append(HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(parameters[key]) + "&");
            }

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
        protected HttpWebRequest CreatePutRequest(string url, NameValueCollection parameters)
        {
            return this.CreateRequestWithParameters(url, "PUT", parameters);
        }

        #endregion

    }
}
