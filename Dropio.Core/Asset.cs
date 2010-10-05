using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Dropio.Core
{
    /// <summary>
    /// 
    /// </summary>
	public struct AssetRoleAndLocations
	{
		/// <summary>
		/// 
		/// </summary>
		public Hashtable Role;
		/// <summary>
		/// 
		/// </summary>
		public List<Hashtable> Locations;
		
	}
    
	/// <summary>
	/// 
	/// </summary>
    public class Asset
    {
		
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

		/// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        public DateTime CreatedAt { get; set; }

		/// <summary>
		/// Sets and gets the file type.
		/// </summary>
		/// <value>
		/// The file type
		/// </value>
		public AssetType Type { get; set; }
		
        /// <summary>
        /// Gets or sets the filesize.
        /// </summary>
        /// <value>
        /// The filesize.
        /// </value>
        public int Filesize { get; set; }		
		
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		public string Description { get; set; }		
		
		/// <summary>
		/// Gets or sets the title of the Asset.
		/// </summary>
		/// <value>
		/// The title.
		/// </value>
		public string Title { get; set; }
		
		public string Id { get; set; }
		
		public string DropName { get; set; }
		
        /// <summary>
        /// Gets or sets the drop.
        /// </summary>
        /// <value>
        /// The drop.
        /// </value>
        public Drop Drop { get; set; }
		
		/// <summary>
		/// Get or set the roles
		/// </summary>
		/// <value>
		/// List of roles of the asset
		/// </value>
		public List<AssetRoleAndLocations> Roles { get; set; }

        #endregion

        #region Create/Read

        /// <summary>
        /// Finds the specified asset name.
        /// </summary>
        /// <param name="drop">
        /// 
        /// </param>
        /// <param name="name">
        /// The asset URL.
        /// </param>
        /// <returns>
        /// 
		/// </returns>
        public static Asset Find(Drop drop, string name)
        {
            return ServiceProxy.Instance.FindAsset(drop, name);
        }

        #endregion

        #region Update/Delete
		
        /// <summary>
        /// Saves this instance.
        /// Do not use this to change the name of the asset, use ChangeName for that
        /// </summary>
        /// <param name="newTitle">
        /// 
        /// </param>
        /// <param name="newDescription">
        /// 
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public bool Update( string newTitle, string newDescription )
        {
            return ServiceProxy.Instance.UpdateAsset(this, newTitle, newDescription );
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
		public string GenerateOriginalFileUrl()
		{
			return ServiceProxy.Instance.OriginalFileUrl(this);
		}

        #endregion
        
        public bool Convert (AssetType type, List<Hashtable> inputs, List<Hashtable> outputs, string plugin, string pingback_url)
        {
        	return ServiceProxy.Instance.CreateJob( type, inputs, outputs, plugin, pingback_url);
        }

    }
}