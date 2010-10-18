using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Rmb.Core
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

        /// <summary>Deletes the asset from the drop</summary>
        /// <returns>a <see cref="bool"/> indicating the success of the update</returns>
        public bool Destroy()
        {
            return ServiceProxy.Instance.DeleteAsset(this);
        }

        #endregion

        #region Actions
		
		/// <summary>Copies the asset to the given drop and returns the new asset.</summary>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns>a <see cref="bool"/> indicating the success of the copy</returns>
		public bool CopyTo(Drop targetDrop)
		{
			return ServiceProxy.Instance.CopyAsset(this, targetDrop);
		}
		
		/// <summary>Moves the asset to the given drop.</summary>
		/// <param name="targetDrop">The target drop.</param>
		/// <returns>A <see cref="bool"/> indicating the success of the move</returns>
		public bool MoveTo (Drop targetDrop)
		{
			return ServiceProxy.Instance.MoveAsset (this, targetDrop);
		}

		/// <summary>Gets an original file download url.</summary>
		/// <returns>A <see cref="string"/> containing a url that can be used to access the "original_content" role of
		/// the asset</returns>
		public string OriginalFileUrl()
		{
			return ServiceProxy.Instance.GenerateOriginalFileUrl(this);
		}
		
        #endregion
        
        /// <summary>Convert the asset. This is a convenience function for Job.Convert(), and is an overloaded form that
        /// takes a Hashtable for the outputs (instead of a Hashtable List</summary>
        /// <remarks>When creating the output parameters "asset_id" does not have to be included if you are adding this
        /// converted output to the same asset (which is usually what you want to do). If you want to save it to a
        /// different asset then you must include "asset_id" in the output hash</remarks>
        /// <param name="output">A <see cref="Hashtable"/> containing the output parameters for the conversion you want
        /// performed</param>
        /// <param name="plugin">A <see cref="System.String"/> type object that specifies what converter plugin to use</param>
        /// <returns>A <see cref="System.Boolean"/> indicated whether a Job Resource was returned. Note that this does not
        /// indicate if the job was successful! If you want to check if the job succeeded, update the Asset object and
        /// check the status for the role you just created for the conversion</returns>
        /// <exception cref="Dropio.Core.ServiceException">Throw when there is a problem processing the job request</exception>
        /// <exception cref="ArgumentNullException">Thrown when the Asset object is null</exception>
        public bool Convert ( Hashtable output, string plugin )
        {
        	// convert to List<Hashtable>
        	List<Hashtable> outputHashTable = new List<Hashtable>();
        	outputHashTable.Add( output );
        	return this.Convert( outputHashTable, plugin );
        }
        
        /// <summary>Convert the asset. This is a convenience function for Job.Convert()</summary>
        /// <remarks>When creating the output parameters, "asset_id" does not have to be included if you are adding this
        /// converted output to the same asset (which is usually what you want to do). If you want to save it to a
        /// different asset then you must include "asset_id" in the output hash</remarks>
        /// <param name="outputs">A <see cref="List<Hashtable>"/> containing Hashtables with the output parameters for
        /// each conversion you want</param>
        /// <param name="plugin">A <see cref="System.String"/> type object that specifies what converter plugin to use</param>
        /// <returns>A <see cref="System.Boolean"/> indicated whether a Job Resource was returned. Note that this does not
        /// indicate if the job was successful! If you want to check if the job succeeded, update the Asset object and
        /// check the status for the role you just created for the conversion</returns>
        /// <exception cref="Dropio.Core.ServiceException">Throw when there is a problem processing the job request</exception>
        /// <exception cref="ArgumentNullException">Thrown when the Asset object is null</exception> 
        public bool Convert ( List<Hashtable> outputs, string plugin )
        {
       		return ServiceProxy.Instance.ConvertAsset( this, outputs, plugin, string.Empty );
        }
        
        /// <summary>Convert the asset. This is a convenience function for Job.Convert() and is an overloaded form that
        /// takes a Hashtable for the outputs (instead of a Hashtable List)</summary>
        /// <remarks>When creating the output parameters, "asset_id" does not have to be included if you are adding this
        /// converted output to the same asset (which is usually what you want to do). If you want to save it to a
        /// different asset then you must include "asset_id" in the output hash</remarks>
        /// <param name="outputs">A <see cref="List<Hashtable>"/> containing Hashtables with the output parameters for
        /// each conversion you want</param>
        /// <param name="plugin">A <see cref="System.String"/> type object that specifies what converter plugin to use</param>
        /// <param name="pingbackUrl">A <see cref="string"/> type object that specifies a pingback url</param>
        /// <returns>A <see cref="System.Boolean"/> indicated whether a Job Resource was returned. Note that this does not
        /// indicate if the job was successful! If you want to check if the job succeeded, update the Asset object and
        /// check the status for the role you just created for the conversion</returns>
        /// <exception cref="Dropio.Core.ServiceException">Throw when there is a problem processing the job request</exception>
        /// <exception cref="ArgumentNullException">Thrown when the Asset object is null</exception>
        public bool Convert ( Hashtable outputs, string plugin, string pingbackUrl)
        {
        	// convert to List<Hashtable>
        	List<Hashtable> outputHashTable = new List<Hashtable>();
        	outputHashTable.Add( outputs );
        	return this.Convert( outputHashTable, plugin, pingbackUrl);
        }
        
        /// <summary>Convert the asset. This is a convenience function for Job.Convert()</summary>
        /// <remarks>When creating the output parameters, "asset_id" does not have to be included if you are adding this
        /// converted output to the same asset (which is usually what you want to do). If you want to save it to a
        /// different asset then you must include "asset_id" in the output hash</remarks>
        /// <param name="outputs">A <see cref="List<Hashtable>"/> containing Hashtables with the output parameters for
        /// each conversion you want</param>
        /// <param name="plugin">A <see cref="System.String"/> type object that specifies what converter plugin to use</param>
        /// <param name="pingbackUrl">A <see cref="string"/> type object that specifies a pingback url</param>
        /// <returns>A <see cref="System.Boolean"/> indicated whether a Job Resource was returned. Note that this does not
        /// indicate if the job was successful! If you want to check if the job succeeded, update the Asset object and
        /// check the status for the role you just created for the conversion</returns>
        /// <exception cref="Dropio.Core.ServiceException">Throw when there is a problem processing the job request</exception>
        /// <exception cref="ArgumentNullException">Thrown when the Asset object is null</exception>
        public bool Convert ( List<Hashtable> outputs, string plugin, string pingbackUrl )
        {
        	return ServiceProxy.Instance.ConvertAsset( this, outputs, plugin, pingbackUrl);
        
        }

    }
}