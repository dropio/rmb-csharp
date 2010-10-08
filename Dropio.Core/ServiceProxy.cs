using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;

namespace Dropio.Core
{
	/// <summary>
	/// 
	/// </summary>
    public sealed class ServiceProxy
    {
        private static volatile ServiceProxy instance;
        private static object syncRoot = new Object();

		/// <summary>
		/// 
		/// </summary>
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
        /// Gets an original file download url.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public string GenerateOriginalFileUrl(Asset asset)
        {
            return this.ServiceAdapter.GenerateOriginalFileUrl(asset);
        }

		/// <summary>
		/// Create a drop
		/// </summary>
		/// <param name="dropAttributes">
		/// A <see cref="Hashtable"/>
		/// </param>
		/// <returns>
		/// A <see cref="Drop"/>
		/// </returns>
        public Drop CreateDrop( Hashtable dropAttributes )
        {
            return this.ServiceAdapter.CreateDrop( dropAttributes );
        }

        /// <summary>
        /// Finds the drop.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public Drop FindDrop(string name)
        {
            return this.ServiceAdapter.FindDrop(name);
        }
		
		/// <summary>
		/// Gets a paginated list of drops with the Manager Account. Requires Manager API Token.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <returns></returns>
		public List<Drop> FindAll(int page)
		{
			return this.ServiceAdapter.FindAll(page);
		}

        /// <summary>
        /// Deletes the drop.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool DestroyDrop(Drop drop)
        {
            return this.ServiceAdapter.DestroyDrop(drop);
        }
		
		/// <summary>
		/// Empty the drop of all assets
		/// </summary>
		/// <param name="drop">
		/// A <see cref="Drop"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool EmptyDrop(Drop drop)
		{
			return this.ServiceAdapter.EmptyDrop(drop);
		}

        /// <summary>
        /// Updates the drop.
        /// </summary>
        /// <param name="drop">
        /// The drop.
        /// </param>
        /// <param name="name">
        /// 
        /// </param>
        /// <param name="chatPassword">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
//        public bool UpdateDrop(Drop drop, string newName, string newDescription, string newChatPassword, int newMaxSize)
        public bool UpdateDrop(Drop drop, string newName )
        {
            return this.ServiceAdapter.UpdateDrop(drop, newName );
        }

        /// <summary>
        /// Finds the asset.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="assetId">The asset id.</param>
        /// <returns></returns>
        public Asset FindAsset(Drop drop, string assetId)
        {
            return this.ServiceAdapter.FindAsset(drop, assetId);
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

        /// <summary>Saves the asset.</summary>
        /// <returns>A <see cref="bool"/> indicating the sucess of the update</returns>
        public bool UpdateAsset(Asset asset)
        {
            return this.ServiceAdapter.UpdateAsset(asset);
        }
		
		/// <summary>
        /// Finds the subscriptions.
        /// </summary>
        /// <param name="drop">
        /// The drop.
        /// </param>
        /// <param name="page">
        /// The page.
        /// </param>
        /// <returns>
        /// 
        /// </returns>
		public List<Subscription> FindSubscriptions(Drop drop, int page)
		{
			return this.ServiceAdapter.FindSubscriptions(drop,page);
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
        /// Adds the file to the drop..
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="file">The file.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Asset AddFile (Drop drop, string file, string description, string conversion, string pingbackUrl, string outputLocations )
        {
        	return this.ServiceAdapter.AddFileInit (drop, file, description, conversion, pingbackUrl, outputLocations);
		}
		
		public Asset AddFile (Drop drop, HttpPostedFile file, string description, string conversion, string pingbackUrl, string outputLocations )
		{
			return this.ServiceAdapter.AddFileInit (drop, file, description, conversion, pingbackUrl, outputLocations );
		}
		
		/// <summary>
		/// Copies the asset to the given drop and returns the new asset.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool CopyAsset(Asset asset, Drop targetDrop)
		{
			return this.ServiceAdapter.CopyAsset(asset, targetDrop, true);
		}
		
		/// <summary>
		/// Moves the asset to the given drop.
		/// </summary>
		/// <param name="asset">The asset.</param>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns></returns>
		public bool MoveAsset(Asset asset, Drop targetDrop)
		{
			return this.ServiceAdapter.CopyAsset(asset, targetDrop, false);
		}
		
        public bool CreateJob (AssetType type, List<Hashtable> inputs, List<Hashtable> outputs, string plugin, string pingback_url)
        {
        	return this.ServiceAdapter.CreateJob( type, inputs, outputs, plugin, pingback_url);
        }
        
        /// <summary>
        /// Gets an uploadify form
        /// </summary>
        /// <param name="drop">
        /// A <see cref="Drop"/>
        /// </param>
        /// <param name="uploadifyOptions">
        /// A <see cref="Hashtable"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/>
        /// </returns>
        public string GetUploadifyForm (Drop drop, Hashtable uploadifyOptions)
        {
        	return this.ServiceAdapter.GetUploadifyForm (drop, uploadifyOptions);
        }
        
		#endregion
    }
}