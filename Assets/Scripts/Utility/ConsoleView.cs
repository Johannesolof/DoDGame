using UnityEngine;
using System.Collections;

public class ConsoleView : MonoBehaviour {
	
	public Vector2 scrollPosition;
	public string longString = "This is a long-ish string";


	void OnGUI() {
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(100), GUILayout.Height(100));
		GUILayout.Label(longString);
		if (GUILayout.Button("Clear"))
			longString = "";

		GUILayout.EndScrollView();
		if (GUILayout.Button("Add More Text"))
			longString += "\nHere is another line";

	}
}
