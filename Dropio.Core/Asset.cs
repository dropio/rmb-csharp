using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Dropio.Core
{
    /// <summary>
    /// Struct to store asset roles and locations. Field names that are returned are not consistent so we must use a hash
    /// </summary>
	public struct AssetRoleAndLocations
	{
		public Hashtable Role;
		public List<Hashtable> Locations;
		
	}
    
	/// <summary>
	/// Class for working with assets
	/// </summary>
    public class Asset
    {
		
        #region Properties

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; internal set; }

		/// <summary>Gets or sets the created at.</summary>
        public DateTime CreatedAt { get; internal set; }

		/// <summary>Sets and gets the file type.</summary>
		public AssetType Type { get; internal set; }
		
        /// <summary>Gets or sets the filesize.</summary>
        public int Filesize { get; internal set; }		
		
		/// <summary>Gets or sets the description.</summary>
		public string Description { get; set; }		
		
		/// <summary>Gets or sets the title of the Asset.</summary>
		public string Title { get; set; }
		
		/// <summary>Gets or sets the asset id</summary>
		public string Id { get; internal set; }
		
		/// <summary>Gets or sets the name of the drop the asset belongs to</summary>
		public string DropName { get; internal set; }
		
        /// <summary>Gets or sets the drop.</summary>
        public Drop Drop { get; internal set; }
		
		/// <summary>Get or set the roles</summary>
		public List<AssetRoleAndLocations> Roles { get; internal set; }

        #endregion

        #region Create/Read

        /// <summary>Finds the specified asset name.</summary>
        /// <param name="drop">A <see cref="Drop"/> object containing the drop in which to retrieve the asset</param>
        /// <param name="name">A <see cref="string"/> object specifying the ID of the asset to get</param>
        /// <returns>An <see cref="Asset"/> object of the returned asset</param>
        /// <exception cref="Dropio.Core.ServiceException">Thrown when the asset does not exist</exception>
		/// </returns>
        public static Asset Find(Drop drop, string assetId)
        {
            return ServiceProxy.Instance.FindAsset(drop, assetId);
        }

        #endregion

        #region Update/Delete
        
        /// <summary>Saves this instance of the asset</summary>
        /// <remarks>To update information about the asset, change the properties on the asset object itself then
        /// call this.
        /// Currently the only updatable properties are "title" and "description"</remarks>
        /// <returns>a <see cref="bool"/> indicating the sucess of the update</returns>
        public bool Save()
        {
            return ServiceProxy.Instance.UpdateAsset(this);
        }

        /// <summary>
        /// Deletes the asset from the drop
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        public bool Destroy()
        {
            return ServiceProxy.Instance.DeleteAsset(this);
        }

        #endregion

        #region Actions
		
		/// <summary>
		/// Copies the asset to the given drop and returns the new asset.
		/// </summary>
		/// <param name="targetDrop">
		/// The target drop.
		/// </param>
		/// <returns>
		/// 
		/// </returns>
		public bool CopyTo(Drop targetDrop)
		{
			return ServiceProxy.Instance.CopyAsset(this, targetDrop);
		}
		
		/// <summary>
		/// Moves the asset to the given drop.
		/// </summary>
		/// <param name="targetDrop">
		/// The target drop.
		/// </param>
		/// <returns>
		/// 
		/// </returns>
		public bool MoveTo (Drop targetDrop)
		{
			return ServiceProxy.Instance.MoveAsset (this, targetDrop);
		}

		/// <summary>
		/// Gets an original file download url.
		/// </summary>
		/// <returns></returns>
		public string OriginalFileUrl()
		{
			return ServiceProxy.Instance.GenerateOriginalFileUrl(this);
		}
		
        #endregion
        
        public bool Convert (AssetType type, List<Hashtable> inputs, List<Hashtable> outputs, string plugin, string pingback_url)
        {
        	return ServiceProxy.Instance.CreateJob( type, inputs, outputs, plugin, pingback_url);
        }

    }
}