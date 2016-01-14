using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerLog : MonoBehaviour 
{
	// Private VARS
//	private int displayElementsPerScreen = 11;
//	private int displayScrollSpeed = 4;

	private List<string> Eventlog = new List<string>();
	private string guiTextToShow = "";
	private string[] DisplayArray;

	private bool autoScroll;

	private float rectWidth = Screen.width / 5;
	private float rectHeight = Screen.height / 5;
	private float rectX = 0f;
	private float rectY = Screen.height * 3 / 5;

	private bool showConsole = true;

//	private int displayOffset = 0;

	private Vector2 scrollPos = new Vector2 (0, 0);
	private Rect scrollRect;
	private Rect scrollViewRect;

	// Public VARS
	public int maxLines = 5;
//	public Vector2 scrollPosition;


	void Start () {
		scrollRect = new Rect(rectX, rectY, rectWidth, rectHeight);
		scrollViewRect = scrollRect; // new Rect(0, 0, 800, 600);
	}

	void OnGUI ()
	{
		if(showConsole)
		{
			GUILayout.BeginArea(scrollViewRect);
//			Vector2 pos = scrollPosition;
//			if (Mathf.Approximately(scrollPosition.x, 1f)) {
//				pos.x = new Vector2 (1f, 1f);
//			}
			
			scrollPos = GUILayout.BeginScrollView(scrollPos);

			Debug.Log ("ScrollView slider position is at: " + scrollPos);

			GUILayout.Label(guiTextToShow);

			GUILayout.EndScrollView();

			autoScroll = GUILayout.Toggle (true, "Autoscroll");

			GUILayout.EndArea();
		}
	}


	void Update () {
		if (Input.GetKeyDown(KeyCode.Q))
		{
			showConsole = !showConsole;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			AddEvent("This is a sample message!", false);
		}
//		if (Input.GetKeyDown(KeyCode.PageUp))
//		{
//			displayOffset += displayScrollSpeed;
//			if(displayOffset > Eventlog.Count - 1) displayOffset = Eventlog.Count - 1;
//			recalcDisplay();
//		}
//		if (Input.GetKeyDown(KeyCode.PageDown))
//		{
//			displayOffset -= displayScrollSpeed;
//			if(displayOffset < 0) displayOffset = 0;
//			recalcDisplay();
//		}
	}


	public void AddEvent(string eventString, bool sendToDebugLog)
	{
		if(sendToDebugLog)
			Debug.Log(eventString);

		if (autoScroll)
			scrollPos = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);

		string time = System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second;
		Eventlog.Add("[" + time + "] " + eventString);

		if (Eventlog.Count > maxLines)
			Eventlog.RemoveAt(0);

//		if (displayOffset != 0)
//			++displayOffset;

		recalcDisplay();
	}


	private void recalcDisplay () {
//		DisplayArray = new string[displayElementsPerScreen];
//
//		for (int i = 0; i < displayElementsPerScreen; ++i) {
//			if (i == Eventlog.Count) {
//				break;
//			}
//			if (Eventlog.Count - displayOffset - i - 1 < 0) {
//				DisplayArray [displayElementsPerScreen - i - 1] = "";
//			} else {
//				DisplayArray [displayElementsPerScreen - i - 1] = Eventlog [Eventlog.Count - displayOffset - i - 1];
//			}
//		}

		guiTextToShow = "";

		foreach (string s in Eventlog) {
			guiTextToShow += s;
			guiTextToShow += "\n";
		}

//		for (int i = Mathf.Max(Eventlog.Count - displayElementsPerScreen - displayOffset, 0); i < Eventlog.Count - displayOffset; ++i) {
//			guiTextToShow += DisplayArray[i];
//			guiTextToShow += "\n";
//		}
	}
}