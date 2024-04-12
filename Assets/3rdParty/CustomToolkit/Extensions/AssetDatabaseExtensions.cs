using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomToolkit.Extensions
{
	public static class AssetDatabaseExtensions
	{
		/////////////////////////////////////////////////////////////////////////////
		public static List<T> GetAssetsByType<T>() where T : Object
		{
			List<T> assets = new List<T>();
			
			var assetGuids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

			for (int assetIndex = 0; assetIndex < assetGuids.Length; assetIndex++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[assetIndex]);
				var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

				if (asset != null)
					assets.Add(asset);
			}
			
			return assets;
		}
	}
}
