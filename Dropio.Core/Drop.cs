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
            return ServiceProxy.Instance.FindAssets(this, page);
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
        /// Generates the authenticated URL.
        /// </summary>
        /// <returns></returns>
        public string GenerateAuthenticatedUrl()
        {
            return ServiceProxy.Instance.GenerateAuthenticatedDropUrl(this);
        }

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public Asset AddFile(string file)
        {
            return ServiceProxy.Instance.AddFile(this, file);
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

        #endregion

    }
}
