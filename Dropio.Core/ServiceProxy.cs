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
        /// Finds the assets.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public List<Asset> FindAssets(Drop drop, int page)
        {
            return this.ServiceAdapter.FindAssets(drop, page);
        }

        

        /// <summary>
        /// Creates the note.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="title">The title.</param>
        /// <param name="contents">The content.</param>
        /// <returns></returns>
        public Note CreateNote(Drop drop, string title, string contents)
        {
            return this.ServiceAdapter.CreateNote(drop, title, contents);
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
        /// <returns></returns>
        public List<Comment> FindComments(Asset asset)
        {
            return this.ServiceAdapter.FindComments(asset);
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
        public void SendToDrop(Asset asset, string dropName)
        {
            this.ServiceAdapter.SendToDrop(asset, dropName);
        }

        /// <summary>
        /// Adds the file to the drop..
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public Asset AddFile(Drop drop, string file)
        {
            return this.ServiceAdapter.AddFile(drop, file);
        }
        #endregion

    }
}