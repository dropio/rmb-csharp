using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
//using Dropio.Core.Types;

namespace Dropio.Core
{

    public enum Status
    {
        Unconverted,
        Converted
    }
    
	public struct AssetRoleAndLocations
	{
		public Hashtable Role;
		public List<Hashtable> Locations;
		
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
        /// Gets or sets the created at.
        /// </summary>
        /// <value>The created at.</value>
        public DateTime CreatedAt { get; set; }

		/// <summary>
		/// Sets and gets the file type.
		/// </summary>
		/// <value>The file type</value>
		public string Type { get; set; }
		
        /// <summary>
        /// Gets or sets the filesize.
        /// </summary>
        /// <value>The filesize.</value>
        public int Filesize { get; set; }		
		
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }		
		
		/// <summary>
		/// Gets or sets the title of the Asset.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get; set; }
		
        /// <summary>
        /// Gets or sets the drop.
        /// </summary>
        /// <value>The drop.</value>
        public Drop Drop { get; set; }
		
		/// <summary>
		/// Get or set the roles
		/// </summary>
		/// <value>List of roles of the asset</value>
		public List<AssetRoleAndLocations> Roles { get; set; }

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
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static Asset CreateNote(Drop drop, string title, string content, string description)
        {
            return ServiceProxy.Instance.CreateNote(drop, title, content, description);
        }

        /// <summary>
        /// Creates the link.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static Asset CreateLink(Drop drop, string title, string description, string url)
        {
            return ServiceProxy.Instance.CreateLink(drop, title, description, url);
        }

        #endregion

        #region Update/Delete

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public bool Update()
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
        /// Gets an original file download url.
        /// </summary>
        /// <returns></returns>
		public string OriginalFileUrl()
		{
			return ServiceProxy.Instance.OriginalFileUrl(this);
		}

        #endregion

    }
}
