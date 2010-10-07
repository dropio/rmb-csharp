using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

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
        /// <exception cref="System.NullReferenceException">Thrown when the Drop object is not set to an instance of Drop</exception> 
        public List<Asset> Assets()
        {
            return this.Assets(1);
        }

        /// <summary>Gets the specified page of drop assets in the default order of oldest assets first. If the
        /// specified page does not exist there is no error, 0 assets will be returned.</summary>
        /// <param name="page">An <see cref="int"/> type specifying the page number.</param>
        /// <returns>A List of <see cref="Asset"/> objects containing the specified page of returned assets (up to 30,
        /// in the default order, oldest first)</returns>
        /// <exception cref="System.NullReferenceException">Thrown when the Drop object is not set to an instance of Drop</exception> 
        public List<Asset> Assets(int page)
        {
            return this.Assets(page,Order.Oldest);
        }
		
		/// <summary>Gets the specified page of drop assets in the specified order. If the specified page does not
		/// exist there is no error, 0 assets will be returned.</summary>
        /// <param name="page">An <see cref="int"/> type specifying the page number.</param>
        /// <param name="order">An <see cref="Order"/> enumeration constant specifying in which order to return the
        /// assets.</param>
        /// <returns>A List of <see cref="Asset"/> objects containing the specified page of returned assets (up to
        /// 30, in the specified order)</returns>
        /// <exception cref="System.NullReferenceException">Thrown when the Drop object is not set to an instance of Drop</exception> 
        public List<Asset> Assets(int page, Order order)
        {
            return ServiceProxy.Instance.FindAssets(this, page, order);
        }

		/// <summary>Get the pingback subscriptions associated with the drop, using the default of the first page of
		/// results.</summary>
        /// <returns>A List of <see cref="Subscription"/> objects containing the first page of returned pingback
        /// subscriptions (up to 30)</returns>
        /// <exception cref="System.NullReferenceException">Thrown when the Drop object is not set to an instance of Drop</exception> 
        public List<Subscription> Subscriptions()
        {
            return this.Subscriptions(1);
        }
		
		/// <summary>Get the specified page of pingback subscriptions associated with the account. If the page number
		/// does not exist there is no error, 0 assets will be returned.</summary>
        /// <param name="page">An <see cref="int"/> type specifying the page number</param>
        /// <returns>A List of <see cref="Subscription"/> objects containing the specified page of returned pingback
        /// subscriptions (up to 30)</returns>
        /// <exception cref="System.NullReferenceException">Thrown when the Drop object is not set to an instance of Drop</exception> 
        public List<Subscription> Subscriptions (int page)
        {
        	return ServiceProxy.Instance.FindSubscriptions (this, page);
        }
		
        #endregion

        #region Create / Read

        /// <summary>Finds the drop by name.</summary>
        /// <param name="name">A <see cref="string"/> type specifying the drop to find</param>
        /// <returns>A <see cref="Drop"/> object of the found drop.</returns>
        /// <exception cref="Dropio.Core.ServiceException">thrown when the drop does not exist</exception>
        public static Drop Find(string name)
        {
            return ServiceProxy.Instance.FindDrop(name);
        }
	
		/// <summary>Create a new drop with a random name</summary>
		/// <returns>A <see cref="Drop"/> object of the newly created drop.</returns>
		public static Drop Create()
		{
			return ServiceProxy.Instance.CreateDrop( new Hashtable() );
		}
		
        /// <summary>Creates a drop with the given options.</summary>
        /// <param name="dropAttributes">A <see cref="Hashtable"/> that contains key-value pairs that correspond to
        /// options for drop creation. All parameters are optional, so if none are specified use the overloaded form of
        /// Create() that has no parameters
        /// Options are:
        ///	name: name of drop
        /// description: description for the drop
        /// email_key: security key tagged onto the email address created for a drop
        /// max_size: max size of drop, in megabytes (default is 100)
        /// chat_password: the password used for 3rd party and javascript clients to connect to real-time stream of
        ///		events in your drop. Valid characters are alphanumeric (a-z, A-Z, 0-9)
        ///
        /// All keys and values must be created as <see cref="string"/> types.
        /// </param>
        /// <returns>A <see cref="Drop"/> object of the newly created drop.</returns>
        /// <exception cref="Dropio.Core.ServiceException">Thrown if the drop name given is not available</exception>
        public static Drop Create( Hashtable dropAttributes )
        {
            return ServiceProxy.Instance.CreateDrop( dropAttributes );
        }
        
        /// <summary>Creates a pingback subscription. When the events happen, the url will be sent a POST request with the
		/// pertinent data.</summary>
		/// <param name="url">A <see cref="string"> type specifying the url to send the pingback to</param>
		/// <param name="events">The events that a pingback should be sent for. Multiple events can be specifed by using the
		/// bitwise OR operator.
		/// <example><code>AssetEvents.AssetCreated | AssetEvents.AssetDeleted</code></example>
		/// </param>
		/// <returns>A <see cref="Subscription"> object of the newly created pingback subscription</returns>
        /// <exception cref="System.NullReferenceException">Thrown when the Drop object is not set to an instance of Drop</exception> 
		public Subscription CreatePingbackSubscription (string url, AssetEvents events)
		{
			return ServiceProxy.Instance.CreatePingbackSubscription (this, url, events);
		}
		
		/// <summary>Get the first page of the list of drops associated with the RMB account</summary>
		/// <returns>A List of <see cref="Drop"> objects (up to 30)</returns>
		public static List<Drop> FindAll()
		{
			return FindAll(1);
		}
		
		/// <summary>Get the specified results page of the list of drops associated with the RMB account. If the page
		/// specified does not exist there is no error, 0 results will be returned.</summary>
		/// <param name="page">An <see cref="int"> type specifying the page of results to return</summary>
		/// <returns>A List of <see cref="Drop"> objects of the returned drops</returns>
		public static List<Drop> FindAll(int page)
		{
			return ServiceProxy.Instance.FindAll(page);
		}

        #endregion

        #region Update / Delete
        
        /// <summary>
        /// Change the name of a drop
        /// </summary>
        ///	<remarks>If you have changed any other properties of the Drop object, those changes will be erased if this
        /// method returns true because it will reload the drop object with all the drop information (not just the new
        /// name. If are changing other properties of the drop, either call Update() first before changing the name, or
        /// change the other properties after calling ChangeName()</remarks>
        /// <param name="newName">
        /// A <see cref="System.String"/> type specifying the new drop name
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/> indicating the success of the update
        /// </returns>
        /// <exception cref="Dropio.Core.ServiceException">Thrown when the drop cannot be updated (for example, the name
        /// is not available</exception>
        public bool ChangeName( string newName )
        {
        	return ServiceProxy.Instance.UpdateDrop( this, newName );
        }
		
        /// <summary>Update information associated with a drop. Update the properties you want to change on the drop
        /// object itself, then call Update(). Note that you CANNOT change the name using this method, trying to do so
        /// will result in a "drop not found" exception. To change the name, use ChangeName(). If Update() returns true,
        /// the drop object will be updated with the new drop properties
        /// Properties that can be changed are description, email_key, chat_password and max_size.
        /// <returns>A <see cref="bool"> indicating whether the action sucessfully completed</returns>
        /// <exception cref="Dropio.Core.ServiceException">Thrown when the drop cannot be updated (for example, the name
        /// is not available</exception>
        public bool Update()
        {
			return ServiceProxy.Instance.UpdateDrop( this, string.Empty );
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
		/// <remarks>When using a FileUpload control in ASP.NET there is no way to retrieve the file path of the file
		/// to be uploaded. This overloaded method allows a <see cref="HttpPostedFile"> object (which FileUpload provides
		/// to be passed in</remarks>
        /// <param name="file">A <see cref="HttpPostedFile"> object of the file to upload</param>
        /// <returns>An <see cref="Asset"> object of the newly created asset</returns>
		public Asset AddFile ( HttpPostedFile file )
		{
			return this.AddFile (file, string.Empty, string.Empty, string.Empty, string.Empty, null);
		}
		
		/// <summary>Add a file to a drop</summary>
		/// <remarks>When using a FileUpload control in ASP.NET there is no way to retrieve the file path of the file
		/// to be uploaded. This overloaded method allows a <see cref="HttpPostedFile"> object (which FileUpload provides
		/// to be passed in.
		/// All parameters listed are optional, except for file. Pass in string.Empty for any you do not want to specify
		/// (or null for handler). If you don't need to specify anything but the file, use the single parameter overload</remarks>	
		/// <param name="file">A <see cref="HttpPostedFile"> object of the file to upload</param>
		/// <param name="description">A <see cref="string"> type specifying a description for the file.</param>
		/// <param name="conversion">A <see cref="string"> type sepcifying what conversions you want. Currently the only
		/// possible value is "BASE", which will cause the default drop.io conversions to be performed. Passing an empty string
		/// will cause no conversions to be performed</param>
		/// <param name="pingbackUrl">A <see cref="string"> type that specifies a fully qualified url to which a request will
		/// be sent once the conversion is complete</param>
		/// <param name="outputLocations">A <see cref="string"> type that specifies a comma separated list of output locations</param>
		/// <param name="handler">A <see cref="ServiceAdapter.TransferProgressHandler"/> instance that can be used to
		/// keep track of the file transfer progress</param>
		/// <returns>An <see cref="Asset"/> object of the newly created asset</returns>		
		public Asset AddFile (HttpPostedFile file, string description, string conversion, string pingbackUrl, string outputLocations, ServiceAdapter.TransferProgressHandler handler)
		{
			if (handler != null)
				ServiceProxy.Instance.ServiceAdapter.OnTransferProgress += handler;
			
			Asset a = ServiceProxy.Instance.AddFile (this, file, description, conversion, pingbackUrl, outputLocations);
			
			if (handler != null)
				ServiceProxy.Instance.ServiceAdapter.OnTransferProgress -= handler;
			
			return a;
		}
		
		/// <summary>Add a file to a drop (using a string type to specify the filepath)</summary>
		/// <param name="file">A <see cref="string"> type specifying the path to the file</param>
		/// <returns>An <see cref="Asset"> object of the newly created asset</returns	
		public Asset AddFile (string file)
		{
			return this.AddFile (file, string.Empty, string.Empty, string.Empty, string.Empty, null);
		}

		/// <summary>Add a file to a drop (using a string type to specify the filepath)</summary>
		/// <remarks>All parameters listed are optional, except for file. Pass in string.Empty for any you do not want to specify
		/// (or null for handler). If you don't need to specify anything but the file, use the single parameter overload</remarks>	
        /// <param name="file">A <see cref="string"> type specifying the path to the file</param>
		/// <param name="description">A <see cref="string"> type specifying a description for the file.</param>
		/// <param name="conversion">A <see cref="string"> type sepcifying what conversions you want. Currently the only
		/// possible value is "BASE", which will cause the default drop.io conversions to be performed. Passing an empty string
		/// will cause no conversions to be performed</param>
		/// <param name="pingbackUrl">A <see cref="string"> type that specifies a fully qualified url to which a request will
		/// be sent once the conversion is complete</param>
		/// <param name="outputLocations">A <see cref="string"> type that specifies a comma separated list of output locations</param>
		/// <param name="handler">A <see cref="ServiceAdapter.TransferProgressHandler"/> instance that can be used to
		/// keep track of the file transfer progress</param>
		/// <returns>An <see cref="Asset"/> object of the newly created asset</returns>		
		public Asset AddFile (string file, string description, string conversion, string pingbackUrl, string outputLocations, ServiceAdapter.TransferProgressHandler handler)
		{
			if (handler != null)
				ServiceProxy.Instance.ServiceAdapter.OnTransferProgress += handler;

            Asset a = ServiceProxy.Instance.AddFile (this, file, description, conversion, pingbackUrl, outputLocations);

            if (handler != null)
				ServiceProxy.Instance.ServiceAdapter.OnTransferProgress -= handler;
			
			return a;
		}

        /// <summary>
        /// returns a string that contain the entire <script> needed to embed an uploadify uploader on a web page.
        /// The uploader is created using the following defaults:
        ///		'uploader':'uploadify/uploadify.swf'
        ///		'script':'http://assets.drop.io/upload'
        ///		'multi':true
        ///		'cancelImg':'uploadify/cancel.png'
        ///		'auto':true
        ///		'scriptData': (dynamically generated on each call)
        /// You can set (or override) any of these options by passing a Hashtable object to the method that specifies what
        /// options you want (though overriding "scriptData" is not recommended and will probably break uploadify). Make 
        /// sure the value is formatted correctly (if the value requires quote marks, make sure you add them to the string).
        /// Do not add any quote marks for the keys, just the values where required.
        /// </summary>
        /// <param name="uploadifyOptions">
        /// A <see cref="Hashtable"/> that contains any options to add to uploadify. Can also be used to override default values.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that contains all the javascript needed for the uploadify uploader. Can be
        /// directly embedded in the page or added using <see cref="ClientScriptManager"/>.
        /// </returns>
        public string GetUploadifyForm( Hashtable uploadifyOptions )
        {
        	return ServiceProxy.Instance.GetUploadifyForm(this, uploadifyOptions );
        }
        
        /// <summary>
        /// Returns a string that contains the entire <script> needed to embed an uploadify uploader on a web page,
        /// using only the default uploadify options.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that contains all the javascript needed for the uploadify uploader. Can be
        /// directly embedded in the page or added using <see cref="ClientScriptManager"/>.
        /// </returns>
        public string GetUploadifyForm()
        {
        	return this.GetUploadifyForm( null );
        }
        
        #endregion
    }
}
