using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Dropio.Core.Types;

namespace Dropio.Core
{
    public sealed class ServiceProxy
    {
        private static volatile ServiceProxy instance;
        private static object syncRoot = new Object();

        public ServiceAdapter ServiceAdapter { get; set; }

        private ServiceProxy() { }

        /// <summary>
        /// Gets the instance of the Service Proxy.
        /// </summary>
        /// <value>The instance.</value>
        public static ServiceProxy Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new ServiceProxy();
                            instance.ServiceAdapter = new ProductionService();
                        }
                    }
                }

                return instance;
            }
        }

        #region Service Abstractions

        /// <summary>
        /// Generates the authenticated drop URL.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public string GenerateAuthenticatedDropUrl(Drop drop)
        {
            return this.ServiceAdapter.GenerateAuthenticatedDropUrl(drop);
        }

        /// <summary>
        /// Generates the authenticated asset URL.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public string GenerateAuthenticatedAssetUrl(Asset asset)
        {
            return this.ServiceAdapter.GenerateAuthenticatedAssetUrl(asset);
        }
		
		/// <summary>
        /// Gets an original file download url.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public string OriginalFileUrl(Asset asset)
        {
            return this.ServiceAdapter.OriginalFileUrl(asset);
        }

        /// <summary>
        /// Creates the drop.
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
            return this.ServiceAdapter.CreateDrop(name, guestsCanAdd, guestsCanComment, guestsCanDelete, expirationLength, password, adminPassword, premiumCode);
        }

        /// <summary>
        /// Finds the drop.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Drop FindDrop(string name, string token)
        {
            return this.ServiceAdapter.FindDrop(name, token);
        }

        /// <summary>
        /// Deletes the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool DeleteDrop(Drop drop)
        {
            return this.ServiceAdapter.DeleteDrop(drop);
        }

        /// <summary>
        /// Updates the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool UpdateDrop(Drop drop, string password, string adminPassword, string premiumCode)
        {
            return this.ServiceAdapter.UpdateDrop(drop, password, adminPassword, premiumCode);
        }
		
		/// <summary>
		/// Gets the drops upload code.
		/// </summary>
		/// <param name="drop"></param>
		/// <returns></returns>
		public string GetDropUploadCode(Drop drop)
		{
			return this.ServiceAdapter.GetDropUploadCode(drop);
		}
		
		/// <summary>
		/// Promotes the nick in chat.
		/// </summary>
		/// <param name="drop">The drop.</param>
		/// <param name="nick">The nick.</param>
		/// <returns></returns>
		public bool PromoteNick(Drop drop, string nick)
		{
			return this.ServiceAdapter.PromoteNick(drop, nick);
		}

        /// <summary>
        /// Finds the asset.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="assetUrl">The asset name.</param>
        /// <returns></returns>
        public Asset FindAsset(Drop drop, string assetUrl)
        {
            return this.ServiceAdapter.FindAsset(drop, assetUrl);
        }
		
		/// <summary>
		/// Gets the embed code for the asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <returns></returns>
		public string GetAssetEmbedCode(Asset asset)
		{
			return this.ServiceAdapter.GetAssetEmbedCode(asset);
		}

        /// <summary>
        /// Deletes the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public bool DeleteAsset(Asset asset)
        {
            return this.ServiceAdapter.DeleteAsset(asset);
        }

        /// <summary>
        /// Saves the asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public bool UpdateAsset(Asset asset)
        {
            return this.ServiceAdapter.UpdateAsset(asset);
        }
		
		/// <summary>
        /// Finds the subscriptions.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns
		public List<Subscription> FindSubscriptions(Drop drop, int page)
		{
			return this.ServiceAdapter.FindSubscriptions(drop,page);
		}
		
		/// <summary>
		/// Creates a Twitter subscription
		/// </summary>
		/// <param name="drop">The drop.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password</param>
		/// <param name="message">The message,</param>
		/// <param name="events">The events.</param>
		/// <returns></returns>
		public Subscription CreateTwitterSubscription(Drop drop, string username, string password, string message, AssetEvents events)
		{
			return this.ServiceAdapter.CreateTwitterSubscription(drop, username, password, message, events);
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
			return this.ServiceAdapter.CreatePingbackSubscription(drop, url, events);
		}
		
		/// <summary>
		/// Creates an email subscription
		/// </summary>
		/// <param name="drop">The drop.</param>
		/// <param name="email">The email.</param>
		/// <param name="message">The message,</param>
		/// <param name="welcomeFrom">The welcome message from address.</param>
		/// <param name="welcomeSubject">The welcome message subject.</param>
		/// <param name="welcomeMessage">The welcome message.</param>
		/// <param name="events">The events.</param>
		/// <returns></returns>
		public Subscription CreateEmailSubscription(Drop drop, string email, string message, string welcomeFrom, string welcomeSubject, string welcomeMessage, AssetEvents events)
		{
			return this.ServiceAdapter.CreateEmailSubscription(drop, email, message, welcomeFrom, welcomeSubject, welcomeMessage, events);
		}
		
		/// <summary>
        /// Deletes the subscription.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
		public bool DeleteSubscription(Subscription subscription)
		{
			return this.ServiceAdapter.DeleteSubscription(subscription);
		}

        /// <summary>
        /// Finds the assets.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="page">The page.</param>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public List<Asset> FindAssets(Drop drop, int page, Order order)
        {
            return this.ServiceAdapter.FindAssets(drop, page, order);
        }

        

        /// <summary>
        /// Creates the note.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="title">The title.</param>
        /// <param name="contents">The content.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Note CreateNote(Drop drop, string title, string contents, string description)
        {
            return this.ServiceAdapter.CreateNote(drop, title, contents, description);
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
            return this.ServiceAdapter.CreateLink(drop, title, description, url);
        }

        /// <summary>
        /// Finds the comments.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public List<Comment> FindComments(Asset asset, int page)
        {
            return this.ServiceAdapter.FindComments(asset, page);
        }

        /// <summary>
        /// Creates the comment.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public Comment CreateComment(Asset asset, string contents)
        {
            return this.ServiceAdapter.CreateComment(asset, contents);
        }

        /// <summary>
        /// Updates the comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public bool UpdateComment(Comment comment)
        {
            return this.ServiceAdapter.UpdateComment(comment);
        }

        /// <summary>
        /// Deletes the comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        public bool DeleteComment(Comment comment)
        {
            return this.ServiceAdapter.DeleteComment(comment);
        }

        /// <summary>
        /// Sends to fax.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="faxNumber">The fax number.</param>
        public void SendToFax(Asset asset, string faxNumber)
        {
            this.ServiceAdapter.SendToFax(asset, faxNumber);
        }

        /// <summary>
        /// Sends to emails.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="emails">The emails.</param>
        /// <param name="message">The message.</param>
        public void SendToEmails(Asset asset, List<string> emails, string message)
        {
            this.ServiceAdapter.SendToEmails(asset, emails, message);
        }

        /// <summary>
        /// Sends to drop.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="dropName">Name of the drop.</param>
        /// <param name="dropToken">Drop token.</param>
        public void SendToDrop(Asset asset, string dropName, string dropToken)
        {
            this.ServiceAdapter.SendToDrop(asset, dropName, dropToken);
        }
		
		/// <summary>
        /// Adds a file via a url.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="url">The url.</param>
        /// <param name="description">The description.</param>
        /// <returns></return>
		public Asset AddFileFromUrl(Drop drop, string url, string description)
		{
			return this.ServiceAdapter.AddFileFromUrl(drop, url, description);
		}

        /// <summary>
        /// Adds the file to the drop..
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="file">The file.</param>
        /// <param name="comment">The comment. </param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Asset AddFile(Drop drop, string file, string comment, string description)
        {
            return this.ServiceAdapter.AddFile(drop, file, comment, description);
        }
		
		/// <summary>
		/// Copies the asset to the given drop and returns the new asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool CopyAsset(Asset asset, Drop targetDrop)
		{
			return this.ServiceAdapter.CopyAsset(asset, targetDrop);
		}
		
		/// <summary>
		/// Moves the asset to the given drop.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool MoveAsset(Asset asset, Drop targetDrop)
		{
			return this.ServiceAdapter.MoveAsset(asset, targetDrop);
		}
		
        #endregion
    }
}