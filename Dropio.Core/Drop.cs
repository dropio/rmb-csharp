using System;
using System.Collections.Generic;
using System.Text;
//using Dropio.Core.Types;

namespace Dropio.Core
{
    public class Drop
    {
    
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the asset count.
        /// </summary>
        /// <value>The asset count.</value>
        public int AssetCount { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }
		
		/// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the admin token.
        /// </summary>
        /// <value>The admin token.</value>
        public string AdminToken { get; set; }

        /// <summary>
        /// Gets or sets the max bytes.
        /// </summary>
        /// <value>The max bytes.</value>
        public int MaxBytes { get; set; }

        /// <summary>
        /// Gets or sets the current bytes.
        /// </summary>
        /// <value>The current bytes.</value>
        public int CurrentBytes { get; set; }

        /// <summary>
        /// Gets or sets the length of the expiration.
        /// </summary>
        /// <value>The length of the expiration.</value>
        public ExpirationLength ExpirationLength { get; set; }
		
		/// <summary>
		/// Gets or sets the expiration date.
		/// </summary>
		/// <value>The expiration date.</value>
		public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the chat password.
        /// </summary>
        /// <value>The chat password.</value>
        public string ChatPassword { get; set; }

        /// <summary>
        /// Gets the assets, defaulting to page 1.
        /// </summary>
        /// <returns></returns>
        public List<Asset> GetAssets()
        {
            return this.GetAssets(1);
        }

        /// <summary>
        /// Gets a list of assets via a page number.s
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public List<Asset> GetAssets(int page)
        {
            return this.GetAssets(page,Order.Oldest);
        }
		
		/// <summary>
        /// Gets a list of assets via a page number.s
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public List<Asset> GetAssets(int page, Order order)
        {
            return ServiceProxy.Instance.FindAssets(this, page, order);
        }

		/// <summary>
        /// Gets the subscriptions.
        /// </summary>
        /// <returns></returns>
        public List<Subscription> GetSubscriptions()
        {
            return this.GetSubscriptions(1);
        }
		
		/// <summary>
        /// Gets the subscriptions.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public List<Subscription> GetSubscriptions(int page)
        {
            return ServiceProxy.Instance.FindSubscriptions(this, page);
        }
        #endregion

        #region Create / Read

        /// <summary>
        /// Finds the drop by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Drop Find(string name)
        {
            return ServiceProxy.Instance.FindDrop(name);
        }
	
		/// <summary>
		/// Create a new drop with a random name
		/// </summary>
		/// <returns>
		/// A <see cref="Drop"/>
		/// </returns>
		public static Drop Create()
		{
			return ServiceProxy.Instance.CreateDrop( string.Empty, string.Empty, string.Empty, 0, string.Empty);
		}
		
        /// <summary>
        /// Creates a drop with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Drop Create(string name)
        {
            return ServiceProxy.Instance.CreateDrop( name, string.Empty, string.Empty, 0, string.Empty );
        }

		/// <summary>
		/// Create a new drop (with more options). All parameters are optional, pass "string.Empty" for any that you
		/// don't want to specify (or 0 for "maxSize", which will cause the default to be used)
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/>. The name of the drop.
		/// </param>
		/// <param name="description">
		/// A <see cref="System.String"/>. The description for the drop
		/// </param>
		/// <param name="emailKey">
		/// A <see cref="System.String"/>. The security key for the drop's email address
		/// </param>
		/// <param name="maxSize">
		/// A <see cref="System.Int32"/>. The maximum size of the drop in megabytes. "0" will create a drop of default
		/// size.
		/// </param>
		/// <param name="chatPassword">
		/// A <see cref="System.String"/>. The chat password for the drop.
		/// </param>
		/// <returns>
		/// A <see cref="Drop"/> representing the newly created drop.
		/// </returns>
        public static Drop Create(string name, string description, string emailKey, int maxSize, string chatPassword)
        {
            return ServiceProxy.Instance.CreateDrop(name, description, emailKey, maxSize, chatPassword);
        }
		
		/// <summary>
		/// Gets a paginated list of drops with the Manager Account.
		/// </summary>
		/// <returns></returns>
		public static List<Drop> FindManagerDrops()
		{
			return FindManagerDrops(1);
		}
		
		/// <summary>
		/// Gets a paginated list of drops with the Manager Account. Requires Manager API Token.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <returns></returns
		public static List<Drop> FindManagerDrops(int page)
		{
			return ServiceProxy.Instance.FindManagerDrops(page);
		}

        #endregion

        #region Update / Delete

		
        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
			return ServiceProxy.Instance.UpdateDrop( this, string.Empty, string.Empty );
          //  return ServiceProxy.Instance.UpdateDrop(this, string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public bool ChangeChatPassword(string newPassword)
        {
            return ServiceProxy.Instance.UpdateDrop(this, string.Empty, newPassword );
        }
		
		public bool ChangeName(string newName)
		{
			return ServiceProxy.Instance.UpdateDrop(this, newName, string.Empty);
		}
		
		/// <summary>
		/// Empty the drop of all assets.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Empty()
		{
			return ServiceProxy.Instance.EmptyDrop(this);
		}

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            return ServiceProxy.Instance.DeleteDrop(this);
        }

        #endregion

        #region Actions
		
		/// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></return>
		public Asset AddFile(string file, string comment)
		{
			return this.AddFile(file, comment, null);
		}
		
		/// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="description">The description.</param>
        /// <returns></return>
		public Asset AddFile(string file, string comment, string description)
		{
			return this.AddFile(file, comment, description, null);
		}
		
		/// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="description">The description.</param>
        /// <param name="handler">The handler.</param>
        /// <returns></return>
		public Asset AddFile(string file, string comment, string description, ServiceAdapter.TransferProgressHandler handler)
		{
			if (handler != null)
            {
                ServiceProxy.Instance.ServiceAdapter.OnTransferProgress += handler;
            }

            Asset a = ServiceProxy.Instance.AddFile(this, file, comment, description);

            if (handler != null)
            {
                ServiceProxy.Instance.ServiceAdapter.OnTransferProgress -= handler;
            }
            return a;
		}

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public Asset AddFile(string file)
        {
            return this.AddFile(file, string.Empty, string.Empty, null);
        }

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public Asset AddFile(string file, ServiceAdapter.TransferProgressHandler handler)
        {
            return this.AddFile(file, string.Empty, string.Empty, handler);
        }

        /// <summary>
        /// Creates a note.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Asset CreateNote(string title, string contents, string description)
        {
            return ServiceProxy.Instance.CreateNote(this, title, contents, description);
        }

        /// <summary>
        /// Creates a link.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public Asset CreateLink(string title, string description, string url)
        {
            return ServiceProxy.Instance.CreateLink(this, title, description, url);
        }
		
		/// <summary>
		/// Creates a pingback subscription. When the events happen, the url will be sent a POST request with the pertinent data.
		/// </summary>
		/// <param name="url">The url.</param>
		/// <param name="events"> The events. </param>
		/// <returns></returns>
		public Subscription CreatePingbackSubscription(string url, AssetEvents events)
		{
			return ServiceProxy.Instance.CreatePingbackSubscription(this, url, events);
		}

        #endregion
    }
}
