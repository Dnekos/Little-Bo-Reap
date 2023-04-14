using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode.Examples.StateGraph;
using XNodeEditor;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace XNodeEditor.Examples {
	[CustomNodeGraphEditor(typeof(StateGraph))]
	public class StateGraphEditor : NodeGraphEditor {

		/// <summary> 
		/// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
	    /// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
		/// </summary>
		public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace == "XNode.Examples.StateGraph") {
				return base.GetNodeMenuName(type).Replace("X Node/Examples/State Graph/", "");
			} else return null;
		}

		public override void OnGUI()
		{
			base.OnGUI();
			StateGraph myScript = (StateGraph)target;

			myScript.LeftMost = myScript.FindLeftmostNode();
			//Debug.Log("Leftmost found. Leftmost node is " + myScript.LeftMost.name);
			EditorUtility.SetDirty(myScript);
			//NodeEditorUtilities.SetDirty(myScript);


		}
	}
}