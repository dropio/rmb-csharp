using System;
using System.Collections.Generic;
using System.Text;
using Dropio.Core.Types;

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
        /// Gets or sets the voicemail.
        /// </summary>
        /// <value>The voicemail.</value>
        public string Voicemail { get; set; }

        /// <summary>
        /// Gets or sets the fax.
        /// </summary>
        /// <value>The fax.</value>
        public string Fax { get; set; }
		
		/// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the conference.
        /// </summary>
        /// <value>The conference.</value>
        public string Conference { get; set; }

        /// <summary>
        /// Gets or sets the admin token.
        /// </summary>
        /// <value>The admin token.</value>
        public string AdminToken { get; set; }

        /// <summary>
        /// Gets or sets the guest token.
        /// </summary>
        /// <value>The guest token.</value>
        public string GuestToken { get; set; }

        /// <summary>
        /// Gets or sets the RSS.
        /// </summary>
        /// <value>The RSS.</value>
        public string Rss { get; set; }

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
        /// Gets or sets a value indicating whether [guests can add].
        /// </summary>
        /// <value><c>true</c> if [guests can add]; otherwise, <c>false</c>.</value>
        public bool GuestsCanAdd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [guests can comment].
        /// </summary>
        /// <value><c>true</c> if [guests can comment]; otherwise, <c>false</c>.</value>
        public bool GuestsCanComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [guests can delete].
        /// </summary>
        /// <value><c>true</c> if [guests can delete]; otherwise, <c>false</c>.</value>
        public bool GuestsCanDelete { get; set; }

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
        /// Gets or sets the hidden upload url.
        /// </summary>
        /// <value>The hidden upload url.</value>
        public string HiddenUploadUrl { get; set; }

        /// <summary>
        /// Gets or sets the chat password.
        /// </summary>
        /// <value>The chat password.</value>
        public string ChatPassword { get; set; }

        /// <summary>
        /// Gets or sets the default view.
        /// </summary>
        /// <value>The default view.</value>
        public string DefaultView { get; set; }

        /// <summary>
        /// Gets or sets the upload url.
        /// </summary>
        /// <value>The upload url.</value>
        public string UploadUrl { get; set; }

		/// <summary>
        /// Gets or sets the admin email.
        /// </summary>
        /// <value>The admin email.</value>
        public string AdminEmail { get; set; }

        /// <summary>
        /// Gets or sets the email key.
        /// </summary>
        /// <value>The email key.</value>
        public string EmailKey { get; set; }

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

        #region Permissions

        /// <summary>
        /// Gets a value indicating whether you have admin access to this Drop.
        /// </summary>
        /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
        public bool IsAdmin
        {
            get
            {
                return !string.IsNullOrEmpty(this.AdminToken);
            }
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
            return Find(name, string.Empty);
        }

        /// <summary>
        /// Finds the drop by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public static Drop Find(string name, string token)
        {
            return ServiceProxy.Instance.FindDrop(name, token);
        }

        /// <summary>
        /// Creates a drop.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="adminPassword">The admin password.</param>
        /// <returns></returns>
        public static Drop Create(string name, string adminPassword)
        {
            return ServiceProxy.Instance.CreateDrop(name, true, true, false, ExpirationLength.OneYearFromLastView, string.Empty, adminPassword, string.Empty);
        }

        /// <summary>
        /// Creates the specified name.
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
        public static Drop Create(string name, bool guestsCanAdd, bool guestsCanComment, bool guestsCanDelete, ExpirationLength expirationLength, string password, string adminPassword, string premiumCode)
        {
            return ServiceProxy.Instance.CreateDrop(name, guestsCanAdd, guestsCanComment, guestsCanDelete, expirationLength, password, adminPassword, premiumCode);
        }

        #endregion

        #region Update / Delete

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            return ServiceProxy.Instance.UpdateDrop(this, string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public bool ChangePassword(string newPassword)
        {
            return ServiceProxy.Instance.UpdateDrop(this, newPassword, string.Empty, string.Empty);
        }

        /// <summary>
        /// Changes the admin password.
        /// </summary>
        /// <param name="adminPassword">The admin password.</param>
        /// <returns></returns>
        public bool ChangeAdminPassword(string adminPassword)
        {
            return ServiceProxy.Instance.UpdateDrop(this, string.Empty, adminPassword, string.Empty);
        }

        /// <summary>
        /// Applies the premium code.
        /// </summary>
        /// <param name="premiumCode">The premium code.</param>
        /// <returns></returns>
        public bool ApplyPremiumCode(string premiumCode)
        {
            return ServiceProxy.Instance.UpdateDrop(this, string.Empty, string.Empty, premiumCode);
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
		/// Promotes the nick in chat.
		/// </summary>
		/// <param name="nick">The nick.</param>
		/// <returns></returns>
		public bool PromoteNick(string nick)
		{
			return ServiceProxy.Instance.PromoteNick(this, nick);
		}
		
		/// <summary>
		/// Gets the upload code for the Drop.
		/// </summary>
		/// <returns></returns>
		public string GetUploadCode()
		{
			return ServiceProxy.Instance.GetDropUploadCode(this);
		}

        /// <summary>
        /// Generates the authenticated URL.
        /// </summary>
        /// <returns></returns>
        public string GenerateAuthenticatedUrl()
        {
            return ServiceProxy.Instance.GenerateAuthenticatedDropUrl(this);
        }
		
		/// <summary>
        /// Adds a file via a url.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <returns></return>
		public Asset AddFileFromUrl(string url)
		{
			return ServiceProxy.Instance.AddFileFromUrl(this, url);
		}
		
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
        /// <param name="handler">The handler.</param>
        /// <returns></return>
		public Asset AddFile(string file, string comment, ServiceAdapter.TransferProgressHandler handler)
		{
			if (handler != null)
            {
                ServiceProxy.Instance.ServiceAdapter.OnTransferProgress += handler;
            }

            Asset a = ServiceProxy.Instance.AddFile(this, file, comment);

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
            return this.AddFile(file, null, null);
        }

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public Asset AddFile(string file, ServiceAdapter.TransferProgressHandler handler)
        {
            return this.AddFile(file, string.Empty, handler);
        }

        /// <summary>
        /// Creates a note.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public Note CreateNote(string title, string contents)
        {
            return ServiceProxy.Instance.CreateNote(this, title, contents);
        }

        /// <summary>
        /// Creates a link.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public Link CreateLink(string title, string description, string url)
        {
            return ServiceProxy.Instance.CreateLink(this, title, description, url);
        }

		/// <summary>
        /// Creates a Twitter subscription.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
		public Subscription CreateTwitterSubscription(string username, string password)
		{
			return this.CreateTwitterSubscription(username, password, string.Empty, AssetEvents.AssetAdded);
		}
		
		/// <summary>
        /// Creates a Twitter subscription.
        /// There are variables which you can use to format the message that will be replaced with their values.
        /// Variables include: [item name], [item type], [item url], [item comment], [drop url]
        /// Example: "the [item type] [item name] was just added to [drop url]" yields "the image test.jpg was just added to http://drop.io/test_drop"
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="message">The message.</param>
        /// <param name="events">The events.</param>
        /// <returns></returns>
		public Subscription CreateTwitterSubscription(string username, string password, string message, AssetEvents events)
		{
			return ServiceProxy.Instance.CreateTwitterSubscription(this, username, password, message, events);
		}
		
		/// <summary>
        /// Creates an email subscription.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
		public Subscription CreateEmailSubscription(string email)
		{
			return this.CreateEmailSubscription(email, string.Empty, string.Empty, string.Empty, string.Empty, AssetEvents.AssetAdded);
		}
		
		/// <summary>
        /// Creates an email subscription.
        /// There are variables which you can use to format the message that will be replaced with their values.
        /// Variables include: [item name], [item type], [item url], [item comment], [drop url]
        /// Example: "the [item type] [item name] was just added to [drop url]" yields "the image test.jpg was just added to http://drop.io/test_drop"
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="message">The message.</param>
        /// <param name="events">The events.</param>
        /// <returns></returns>
		public Subscription CreateEmailSubscription(string email, string message, AssetEvents events)
		{
			return this.CreateEmailSubscription(email, message, string.Empty, string.Empty, string.Empty, events);
		}

		/// <summary>
        /// Creates an email subscription.
        /// There are variables which you can use to format the message that will be replaced with their values.
        /// Variables include: [item name], [item type], [item url], [item comment], [drop url]
        /// Example: "the [item type] [item name] was just added to [drop url]" yields "the image test.jpg was just added to http://drop.io/test_drop"
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="message">The message.</param>
		/// <param name="welcomeFrom">The welcome message from address.</param>
		/// <param name="welcomeSubject">The welcome message subject.</param>
		/// <param name="welcomeMessage">The welcome message.</param>
		/// <param name="events">The events.</param>
        /// <returns></returns>		
		public Subscription CreateEmailSubscription(string email, string message, string welcomeFrom, string welcomeSubject, string welcomeMessage, AssetEvents events)
		{
			return ServiceProxy.Instance.CreateEmailSubscription(this, email, message, welcomeFrom, welcomeSubject, welcomeMessage, events);
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
