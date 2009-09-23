using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Dropio.Core.Types;

namespace Dropio.Core
{

    public enum Status
    {
        Unconverted,
        Converted
    }

    public class Asset
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public Status Status { get; set; }

        /// <summary>
        /// Gets or sets the filesize.
        /// </summary>
        /// <value>The filesize.</value>
        public int Filesize { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>The created at.</value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the file URL.
        /// </summary>
        /// <value>The file URL.</value>
        public string FileUrl { get; set; }

        /// <summary>
        /// Gets or sets the converted file URL.
        /// </summary>
        /// <value>The converted file URL.</value>
        public string ConvertedFileUrl { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail URL.
        /// </summary>
        /// <value>The thumbnail URL.</value>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets the drop.
        /// </summary>
        /// <value>The drop.</value>
        public Drop Drop { get; set; }
		
		/// <summary>
		/// Gets or sets the hidden url.
		/// </summary>
		/// <value>The hidden URL</value>
		public string HiddenUrl { get; set; }

        /// <summary>
        /// Gets the display type.
        /// </summary>
        /// <value>The display type.</value>
        public string DisplayType
        {
            get
            {
                return this.GetType().Name;
            }
        }

        private List<Comment> _comments;

        /// <summary>
        /// Gets a list of comments.
        /// </summary>
        /// <value>The comments.</value>
        public List<Comment> Comments
        {
            get
            {
                if (this._comments == null)
                {
                    this._comments = ServiceProxy.Instance.FindComments(this);
                }

                return this._comments;
            }
        }

        #endregion

        #region Create/Read

        /// <summary>
        /// Finds the specified asset name.
        /// </summary>
        /// <param name="assetUrl">The asset URL.</param>
        /// <returns></returns>
        public static Asset Find(Drop drop, string name)
        {
            return ServiceProxy.Instance.FindAsset(drop, name);
        }

        /// <summary>
        /// Creates the note.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static Note CreateNote(Drop drop, string title, string content)
        {
            return ServiceProxy.Instance.CreateNote(drop, title, content);
        }

        /// <summary>
        /// Creates the link.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static Link CreateLink(Drop drop, string title, string description, string url)
        {
            return ServiceProxy.Instance.CreateLink(drop, title, description, url);
        }

        /// <summary>
        /// Creates a Comment.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public Comment CreateComment(string contents)
        {
            Comment c = ServiceProxy.Instance.CreateComment(this, contents);

            if((c != null) && (!this.Comments.Contains(c)))
            {
                this.Comments.Add(c);
            }

            return c; 
        }

        #endregion

        #region Update/Delete

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            return ServiceProxy.Instance.UpdateAsset(this);
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            return ServiceProxy.Instance.DeleteAsset(this);
        }

        #endregion

        #region Actions
		
		/// <summary>
		/// Copies the asset to the given drop and returns the new asset.
		/// </summary>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool Copy(Drop targetDrop)
		{
			return ServiceProxy.Instance.CopyAsset(this, targetDrop);
		}
		
		/// <summary>
		/// Moves the asset to the given drop.
		/// </summary>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool Move(Drop targetDrop)
		{
			return ServiceProxy.Instance.MoveAsset(this, targetDrop);
		}
		
		/// <summary>
		/// Gets the embed code for the Asset.
		/// </summary>
		/// <returns></returns>
		public string GetEmbedCode()
		{
			return ServiceProxy.Instance.GetAssetEmbedCode(this);
		}

        /// <summary>
        /// Saves the file to the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public void SaveFile(string path)
        {
            this.SaveFile(path, null);
        }

        /// <summary>
        /// Saves the file to the given path.
        /// </summary>
        /// <param name="path">The filepathparam>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public void SaveFile(string path, ServiceAdapter.TransferProgressHandler handler)
        {
            if (handler != null)
            {
                ServiceProxy.Instance.ServiceAdapter.OnTransferProgress += handler;
            }

            ServiceProxy.Instance.SaveFile(this, path);

            if (handler != null)
            {
                ServiceProxy.Instance.ServiceAdapter.OnTransferProgress -= handler;
            }
        }

        /// <summary>
        /// Generates the authenticated URL.
        /// </summary>
        /// <returns></returns>
        public string GenerateAuthenticatedUrl()
        {
            return ServiceProxy.Instance.GenerateAuthenticatedAssetUrl(this);
        }

        /// <summary>
        /// Determines whether this instance can fax.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can fax; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanFax()
        {
            return false;
        }

        /// <summary>
        /// Sends to fax.
        /// </summary>
        /// <param name="faxNumber">The fax number.</param>
        public void SendToFax(string faxNumber)
        {
            ServiceProxy.Instance.SendToFax(this, faxNumber);
        }

        /// <summary>
        /// Sends to email.
        /// </summary>
        /// <param name="emails">The emails.</param>
        /// <param name="message">The message.</param>
        public void SendToEmails(List<string> emails, string message)
        {
            ServiceProxy.Instance.SendToEmails(this, emails, message);
        }

        /// <summary>
        /// Sends to drop.
        /// </summary>
        /// <param name="dropName">Name of the drop.</param>
        public void SendToDrop(string dropName)
        {
            this.SendToDrop(dropName, string.Empty);
        }
		
		/// <summary>
		/// Sends to drop.
		/// </summary>
		/// <param name="dropName">Name of the drop</param>
		/// <param name="dropToken">Drop token.</param>
		public void SendToDrop(string dropName, string dropToken)
		{
			ServiceProxy.Instance.SendToDrop(this, dropName, dropToken);
		}

        #endregion

    }
}
