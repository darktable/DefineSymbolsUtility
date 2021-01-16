using System.Collections;
using UnityEditor.Callbacks;
using Unity.EditorCoroutines.Editor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace DefineSymbolUtilities
{
	public static class RenderPipelineDefineSymbols
	{
		const string LWRP_PACKAGE = "com.unity.render-pipelines.lightweight";
		const string URP_PACKAGE = "com.unity.render-pipelines.universal";
		const string HDRP_PACKAGE = "com.unity.render-pipelines.high-definition";

		const string UNITY_BIRP = "UNITY_BIRP";
		const string UNITY_HDRP = "UNITY_HDRP";
		const string UNITY_SRP = "UNITY_SRP";
		const string UNITY_URP = "UNITY_URP";

		static readonly object coroutineObject = new object();

		[DidReloadScripts]
		private static void SetRenderPipelineDefines()
		{
			var symbols = DefineSymbols.LoadFromEditorPrefs();

			if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset == null)
			{
				symbols.SetSymbol(UNITY_BIRP);
				symbols.UnsetSymbol(UNITY_SRP);
				symbols.UnsetSymbol(UNITY_URP);
				symbols.UnsetSymbol(UNITY_HDRP);

				symbols.ApplySymbolsToProject();
				symbols.SaveToEditorPrefs();
			}
			else
			{
				symbols.UnsetSymbol(UNITY_BIRP);
				symbols.SetSymbol(UNITY_SRP);

				EditorCoroutineUtility.StartCoroutine(CheckRenderPipeline(symbols), coroutineObject);
			}

			IEnumerator CheckRenderPipeline(DefineSymbols ds)
			{
				bool urpPackageInstalled = false;
				bool hdrpPackageInstalled = false;

				ListRequest listRequest = Client.List(true);

				while (listRequest.Status == StatusCode.InProgress)
				{
					yield return null;
				}

				foreach (var info in listRequest.Result)
				{
					switch (info.name)
					{
						case LWRP_PACKAGE:
						case URP_PACKAGE:
							urpPackageInstalled = true;
							break;
						case HDRP_PACKAGE:
							hdrpPackageInstalled = true;
							break;
					}
				}

				ds.SetSymbol(UNITY_URP, urpPackageInstalled);
				ds.SetSymbol(UNITY_HDRP, hdrpPackageInstalled);

				ds.ApplySymbolsToProject();
				ds.SaveToEditorPrefs();
			}
		}
	}
}
