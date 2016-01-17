using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class PlayerLog : MonoBehaviour 
{
	// Private VARS
	NetworkingTest networkingTest;
	List<string> Eventlog = new List<string>();
	string guiTextToShow = "";

	string chatMessage = "";
	bool autoScroll = true;

	float rectWidth;
	float rectHeight;
	float rectX;
	float rectY;

	bool showConsole = true;

	Vector2 scrollPos = new Vector2 (0, 0);
	Rect consoleViewRect;

	Texture2D border;
	GUIStyle wooden_guiStyle;

	// Public VARS
	public int maxLines = 255;


	void Start () {
		networkingTest = GetComponent<NetworkingTest>();

		border = Resources.Load("Images/border") as Texture2D;

		wooden_guiStyle = new GUIStyle();
		wooden_guiStyle.normal.background = border;
		wooden_guiStyle.border = new RectOffset(4, 4, 4, 4);

	}

	void OnGUI ()
	{
		if(showConsole)
		{
			rectWidth = Screen.width / 5;
			rectHeight = Screen.height / 5;
			rectX = 0f;
			rectY = Screen.height * 3 / 5;
			consoleViewRect = new Rect(rectX, rectY, rectWidth, rectHeight);

			GUILayout.BeginArea(consoleViewRect);
			{
				GUILayout.BeginVertical(wooden_guiStyle);
				{
					GUILayout.Space(2);
					autoScroll = GUILayout.Toggle (autoScroll, "Autoscroll");
					GUILayout.Space(2);
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical(wooden_guiStyle);
				{
					GUILayout.Space(4);
					scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(rectWidth - 3));
					{
						GUI.skin.textArea.normal.background = null;
						GUI.skin.textArea.active.background = null;
						GUI.skin.textArea.focused.background = null;
						GUI.skin.textArea.hover.background = null;
						GUI.skin.textArea.alignment = TextAnchor.LowerLeft;
						GUILayout.TextArea(guiTextToShow);
					}
					GUILayout.EndScrollView();
					GUILayout.Space(4);
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical(wooden_guiStyle);
				{
					Event e = Event.current;
					if (e.keyCode == KeyCode.Return &&
						GUI.GetNameOfFocusedControl() == "chatbox" &&
						chatMessage != "")
					{
						networkingTest.SendChatMessage(chatMessage);
						chatMessage = "";
					}
					GUILayout.Space(2);
					GUI.skin.textField.normal.background = null;
					GUI.skin.textField.active.background = null;
					GUI.skin.textField.focused.background = null;
					GUI.skin.textField.hover.background = null;
					GUI.SetNextControlName("chatbox");
					chatMessage = GUILayout.TextField(chatMessage);
					GUILayout.Space(2);
				}
				GUILayout.EndVertical();

			}
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
	}


	void AddEvent(string eventString, bool sendToDebugLog = false, bool sendOverNetwork = false)
	{
		if(sendToDebugLog)
			Debug.Log(eventString);

		if (autoScroll)
			scrollPos = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);

		string time = System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second;
		Eventlog.Add("[" + time + "] " + eventString);

		if (Eventlog.Count > maxLines)
			Eventlog.RemoveAt(0);

		recalcDisplay();
	}

	public void AddTaggedEvent(string tag, string eventString, bool sendToDebugLog = false, bool sendOverNetwork = false)
	{
		if( tag == "" )
			AddEvent(eventString, sendToDebugLog, sendOverNetwork);
		else
			AddEvent("[" +  tag + "] " + eventString, sendToDebugLog, sendOverNetwork);
	}

	void recalcDisplay () {

		guiTextToShow = "";

		foreach (string s in Eventlog) {
			guiTextToShow += s;
			guiTextToShow += "\n";
		}
	}
}