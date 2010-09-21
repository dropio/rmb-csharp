using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{
	/// <summary>
	/// 
	/// </summary>
    public class Drop
    {
    
        #region Properties

        /// <summary>Gets or sets the name of the drop.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the asset count.</summary>
        public int AssetCount { get; set; }

        /// <summary>
        /// Gets or sets the email address of the drop. This is usually just /<name_of_drop/>@drop.io unless
        /// a security key has been set.
        /// </summary>
        public string Email { get; set; }
		
		/// <summary>Gets or sets the description.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the maximum size of the drop, in bytes.</summary>
        public int MaxBytes { get; set; }

        /// <summary>Gets or sets the current size of the drop, in bytes.</summary>
        public int CurrentBytes { get; set; }

        /// <summary>Gets or sets the chat password.</summary>>
        public string ChatPassword { get; set; }

        /// <summary>Gets the assets contained in a drop, using defaults of first page, oldest assets first.</summary>
        /// <returns>A List of <see cref="Asset"/> objects containing the first page of returned assets (up to 30).
        /// </returns>
        public List<Asset> Assets()
        {
            return this.Assets(1);
        }

        /// <summary>Gets the specified page of drop assets in the default order of oldest assets first.</summary>
        /// <param name="page">An <see cref="int"/> type specifying the page number.</param>
        /// <returns>A List of <see cref="Asset"/> objects containing the specified page of returned assets (up to 30,
        /// in the default order, oldest first)</returns> 
        public List<Asset> Assets(int page)
        {
            return this.Assets(page,Order.Oldest);
        }
		
		/// <summary>Gets the specified page of drop assets in the specified order.</summary>
        /// <param name="page">An <see cref="int"/> type specifying the page number.</param>
        /// <param name="order">An <see cref="Order"/> enumeration constant specifying in which order to return the
        /// assets.</param>
        /// <returns>A List of <see cref="Asset"/> objects containing the specified page of returned assets (up to
        /// 30, in the specified order)</returns>
        public List<Asset> Assets(int page, Order order)
        {
            return ServiceProxy.Instance.FindAssets(this, page, order);
        }

		/// <summary>Get the pingback subscriptions associated with the drop, using the default of the first page of
		/// results.</summary>
        /// <returns>A List of <see cref="Subscription"/> objects containing the first page of returned pingback
        /// subscriptions (up to 30)</returns>
        public List<Subscription> Subscriptions()
        {
            return this.Subscriptions(1);
        }
		
		/// <summary>Get the specified page of pingback subscriptions associated with the account.</summary>
        /// <param name="page">An <see cref="int"/> type specifying the page number</param>
        /// <returns>A List of <see cref="Subscription"/> objects containing the specified page of returned pingback
        /// subscriptions (up to 30)</returns>
        public List<Subscription> Subscriptions (int page)
        {
        	return ServiceProxy.Instance.FindSubscriptions (this, page);
        }
		
        #endregion

        #region Create / Read

        /// <summary>Finds the drop by name.</summary>
        /// <param name="name">A <see cref="string"/> type specifying the drop to find</param>
        /// <returns>A <see cref="Drop"/> object of the found drop.</returns>
        public static Drop Find(string name)
        {
            return ServiceProxy.Instance.FindDrop(name);
        }
	
		/// <summary>Create a new drop with a random name</summary>
		/// <returns>A <see cref="Drop"/> object of the newly created drop.</returns>
		public static Drop Create()
		{
			return ServiceProxy.Instance.CreateDrop( string.Empty, string.Empty, string.Empty, 0, string.Empty);
		}
		
        /// <summary>Creates a drop with the given name.</summary>
        /// <param name="name">A <see cref="string"/> type specifying the name for the drop</param>
        /// <returns>A <see cref="Drop"/> object of the newly created drop.</returns>
        /// <exception cref="ServiceException">If the <paramref name="name"/> given is not available</exception>
        public static Drop Create(string name)
        {
            return ServiceProxy.Instance.CreateDrop( name, string.Empty, string.Empty, 0, string.Empty );
        }

		/// <summary>Create a new drop (with more options). All parameters are optional, pass "string.Empty" for any
		/// that you don't want to specify (or 0 for "maxSize", which will cause the default to be used).</summary>
		/// <param name="name">A <see cref="string"/> type specifying the name to use for the drop</param>
		/// <param name="description">A <see cref="string"/> specifying a description for the drop</param>
		/// <param name="emailKey">A <see cref="string"/> type specifying a key to use for the drop's email address</param>
		/// <param name="maxSize">A <see cref="int"/> specifying the size of the drop, in megabytes. "0" will create a drop
		/// with the default size</param>
		/// <param name="chatPassword">A <see cref="string"/> type specifying the chat password for the drop</param>
		/// <returns>A <see cref="Drop"/> object of the newly created drop</returns>
		/// <exception cref="ServiceException">If the <paramref name="name"/> given is not available</exception>
        public static Drop Create(string name, string description, string emailKey, int maxSize, string chatPassword)
        {
            return ServiceProxy.Instance.CreateDrop(name, description, emailKey, maxSize, chatPassword);
        }
		
		/// <summary>Get the first page of the list of drops associated with the RMB account</summary>
		/// <returns>A List of <see cref="Drop"> objects (up to 30)</returns>
		public static List<Drop> FindManagerDrops()
		{
			return FindManagerDrops(1);
		}
		
		/// <summary>Get the specified results page of the list of drops associated with the RMB account</summary>
		/// <param name="page">An <see cref="int"> type specifying the page of results to return</summary>
		/// <returns>A List of <see cref="Drop"> objects of the returned drops</returns>
		public static List<Drop> FindManagerDrops(int page)
		{
			return ServiceProxy.Instance.FindManagerDrops(page);
		}

        #endregion

        #region Update / Delete
		
        /// <summary>Update information associated with a drop. All parameters are optional, pass <see cref="string.Empty">
		/// to any you do not want to set.</summary>
		/// <param name="newName">A <see cref="string"> type specifying the new name for the drop</param>
		/// <param name="newDescription">A <see cref="string"> type specifying the new description for the drop</param>
		/// <param name="newChatPassword">A <see cref="string"> type specifying the new chat password for the drop</param>
        /// <returns>A <see cref="bool"> indicating whether the action sucessfully completed</returns>
        public bool Update()
        {
			return ServiceProxy.Instance.UpdateDrop( this, string.Empty, string.Empty );
        }

		/// <summary>Empty the drop of all assets</summary>
		/// <returns>A <see cref="bool"/> indicating whether the drop was sucessfully emptied</returns>
		public bool Empty()
		{
			return ServiceProxy.Instance.EmptyDrop(this);
		}

        /// <summary>Destroy the drop. The drop will no longer be accessible</summary>
        /// <returns>A <see cref="bool"> indicating whether the drop was sucessfully destroyed</returns>
        public bool Destroy()
        {
            return ServiceProxy.Instance.DestroyDrop(this);
        }

        #endregion

        #region Actions
		
		/// <summary>Add a file to a drop</summary>
        /// <param name="file">A <see cref="string"> type specifying the path to the file</param>
        /// <param name="description">A <see cref="string"> type specifying a description for the file. Pass
		/// <see cref="string.Empty"/> if you don't want to set a description</param>
        /// <returns>An <see cref="Asset"> object of the newly created asset</returns>
		public Asset AddFile(string file, string description)
		{
			return this.AddFile(file, description, null);
		}
		
		/// <summary>Add a file to a drop</summary>
        /// <param name="file">A <see cref="string"> type specifying the path to the file</param>
        /// <param name="description">A <see cref="string"> type specifying a description for the file. Pass
		/// <see cref="string.Empty"/> if you don't want to set a description</param>
        /// <param name="handler">A <see cref="ServiceAdapter.TransferProgressHandler"/> object to keep track of bytes
		/// transfered. If you don't want to specify this just use the AddFile(string, string) prototype</param>
        /// <returns>An <see cref="Asset"/> object of the newly created asset</returns>
		public Asset AddFile(string file, string description, ServiceAdapter.TransferProgressHandler handler)
		{
			if (handler != null)
            {
                ServiceProxy.Instance.ServiceAdapter.OnTransferProgress += handler;
            }

            Asset a = ServiceProxy.Instance.AddFile(this, file, description);

            if (handler != null)
            {
                ServiceProxy.Instance.ServiceAdapter.OnTransferProgress -= handler;
            }
            return a;
		}

		/// <summary>Creates a pingback subscription. When the events happen, the url will be sent a POST request with the
		/// pertinent data.</summary>
		/// <param name="url">A <see cref="string"> type specifying the url to send the pingback to</param>
		/// <param name="events">The events that a pingback should be sent for. Multiple events can be specifed by using the
		/// bitwise OR operator.
		/// <example><code>AssetEvents.AssetCreated | AssetEvents.AssetDeleted</code></example>
		/// </param>
		/// <returns>A <see cref="Subscription"> object of the newly created pingback subscription</returns>
		public Subscription CreatePingbackSubscription(string url, AssetEvents events)
		{
			return ServiceProxy.Instance.CreatePingbackSubscription(this, url, events);
		}

        #endregion
    }
}
