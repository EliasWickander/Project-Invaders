using System.Collections.Generic;
using CustomToolkit.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomToolkit.Variables
{
	public static class VariableResetManager
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnBeforeSceneLoad()
		{
			ResetVariables();
		}

		private static void ResetVariables()
		{
			List<Variable> allVariables = AssetDatabaseExtensions.GetAssetsByType<Variable>();

			foreach (Variable variable in allVariables)
			{
				variable.ResetToDefaultValue();
				variable.ClearListeners();		
			}
		}
	}
}
