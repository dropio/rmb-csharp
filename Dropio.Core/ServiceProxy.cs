using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
//using Dropio.Core.Types;

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
        /// Gets an original file download url.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public string OriginalFileUrl(Asset asset)
        {
            return this.ServiceAdapter.OriginalFileUrl(asset);
        }

		/// <summary>
		/// Create a drop
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="password">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="description">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="emailKey">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="maxSize">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="chatPassword">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="Drop"/>
		/// </returns>
        public Drop CreateDrop(string name, string description, string emailKey, int maxSize, string chatPassword)
        {
            return this.ServiceAdapter.CreateDrop(name, description, emailKey, maxSize, chatPassword);
        }

        /// <summary>
        /// Finds the drop.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Drop FindDrop(string name) //, string token)
        {
            return this.ServiceAdapter.FindDrop(name); //, token);
        }
		
		/// <summary>
		/// Gets a paginated list of drops with the Manager Account. Requires Manager API Token.
		/// </summary>
		/// <param name="page">The page.</param>
		/// <returns></returns>
		public List<Drop> FindManagerDrops(int page)
		{
			return this.ServiceAdapter.FindManagerDrops(page);
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
        /// <param name="drop">The drop.</param>
        /// <returns></returns>
        public bool UpdateDrop(Drop drop, string name, string chatPassword)
        {
			Console.WriteLine( "Serviceproxy: " + name);
            return this.ServiceAdapter.UpdateDrop(drop, name, chatPassword);
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
        /// Creates the note.
        /// </summary>
        /// <param name="drop">The drop.</param>
        /// <param name="title">The title.</param>
        /// <param name="contents">The content.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public Asset CreateNote(Drop drop, string title, string contents, string description)
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
        public Asset CreateLink(Drop drop, string title, string description, string url)
        {
            return this.ServiceAdapter.CreateLink(drop, title, description, url);
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