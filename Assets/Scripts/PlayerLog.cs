using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerLog : MonoBehaviour 
{
	// Private VARS
	private NetworkingTest networkingTest;
	private List<string> Eventlog = new List<string>();
	private string guiTextToShow = "";
	private string[] DisplayArray;

	private string chatMessage = "";
	private bool autoScroll = true;

	private float rectWidth;
	private float rectHeight;
	private float rectX;
	private float rectY;

	private bool showConsole = true;

	private Vector2 scrollPos = new Vector2 (0, 0);
	private Rect consoleViewRect;
	private Rect scrollViewRect;

	private Texture2D border;
	private GUIStyle wooden_guiStyle;

	// Public VARS
	public int maxLines = 5;


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
						AddEvent(chatMessage, false, true);
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


	public void AddEvent(string eventString, bool sendToDebugLog = false, bool sendOverNetwork = false)
	{
		if(sendToDebugLog)
			Debug.Log(eventString);

		if(sendOverNetwork)
			networkingTest.BroadcastStringToConsole(eventString);

		if (autoScroll)
			scrollPos = new Vector2 (float.PositiveInfinity, float.PositiveInfinity);

		string time = System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second;
		Eventlog.Add("[" + time + "] " + eventString);

		if (Eventlog.Count > maxLines)
			Eventlog.RemoveAt(0);

		recalcDisplay();
	}


	private void recalcDisplay () {

		guiTextToShow = "";

		foreach (string s in Eventlog) {
			guiTextToShow += s;
			guiTextToShow += "\n";
		}
	}
}