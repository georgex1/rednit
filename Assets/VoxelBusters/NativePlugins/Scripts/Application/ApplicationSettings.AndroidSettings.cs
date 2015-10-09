﻿using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	public partial class ApplicationSettings 
	{
		/// <summary>
		/// Application Settings specific to Android platform.
		/// </summary>
		[System.Serializable]
		public class AndroidSettings
		{
			#region Properties 

			[SerializeField, Tooltip("Identifier used to identify your app in Google Play Store.")]
			private string			m_storeIdentifier;
			/// <summary>
			/// Gets or sets the store identifier.
			/// </summary>
			/// <value>The store identifier for this application.</value>
			public string			StoreIdentifier
			{
				get
				{
					return m_storeIdentifier;
				}
				
				set
				{
					m_storeIdentifier	= value;
				}
			}

			#endregion
		}
	}
}