using System;

namespace Dropio.Core
{
	public class Subscription
	{

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of subscription.
        /// </summary>
        /// <value>The contents.</value>
        public string Type { get; set; }

		/// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The contents.</value>
        public string Username { get; set; }

		/// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The contents.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the drop.
        /// </summary>
        /// <value>The drop.</value>
        public Drop Drop { get; set; }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            return ServiceProxy.Instance.UpdateSubscription(this);
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            return ServiceProxy.Instance.DeleteSubscription(this);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            Subscription s2 = obj as Subscription;

            if (s2 == null) return false;
            if (this.Id == s2.Id) return true;
            if (this == s2) return true;
            
            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Id;
        }
	}
}
