using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class SaveClearBuildProcessor : IPreprocessBuildWithReport
{
	public int callbackOrder { get { return 0; } }


	public void OnPreprocessBuild(BuildReport report)
	{
		Debug.Log("SaveClearBuildProcessor running, clearing save data");
		WorldState.DeleteSave();
		Debug.Log("Save data cleared!");
	}

}
