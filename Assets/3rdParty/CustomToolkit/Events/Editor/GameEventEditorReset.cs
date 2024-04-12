using System.Collections.Generic;
using CustomToolkit.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomToolkit.Events
{
	[InitializeOnLoad]
	public static class GameEventEditorReset
	{
		static GameEventEditorReset()
		{
			EditorApplication.update += ResetOnLoad;
		}

		private static void ResetOnLoad()
		{
			EditorApplication.update -= ResetOnLoad;
		
			if (!SessionState.GetBool("FirstUpdateDone", false))
			{
				ResetEvents();
				SessionState.SetBool("FirstUpdateDone", true);
			}
		}
	
		/////////////////////////////////////////////////////////////////////////////
		[MenuItem("Custom Tools/Game Event/Reset Events")]
		private static void ResetEvents()
		{
			List<GameEvent> gameEvents = AssetDatabaseExtensions.GetAssetsByType<GameEvent>();

			foreach (GameEvent gameEvent in gameEvents)
				gameEvent.ClearListeners();	
		}
	}
}
