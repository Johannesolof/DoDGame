using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class MainMenu : MonoBehaviour
{
	GameObject canvasMain;
	GameObject canvasHost;
	GameObject canvasJoin;
	GameObject currentCanvas;

	string cpath = "Panel/";

	void Start ()
	{
//		menuState = MenuState.MainMenu;
//
//		border = Resources.Load("Images/border") as Texture2D;
//		background = Resources.Load("Images/black-background-04") as Texture2D;
//		woodenButton = new GUIStyle();
//		woodenButton.normal.background = border;
//		woodenButton.border = new RectOffset(4, 4, 4, 4);
//		woodenButton.normal.textColor = Color.white;
//		woodenButton.alignment = TextAnchor.MiddleCenter;
//		buttonHeight = Screen.height/20;
//		textSize = Screen.height/40;
//		woodenButton.fontSize = textSize;
//
//		fullscreenViewRect = new Rect(0, 0, Screen.width, Screen.height);
//		float f = Screen.width/2;
//		verticalButtonRect = new Rect(f - (Screen.width/6), 2*Screen.height/3, 2*(Screen.width/6), Screen.height/3);

		canvasMain = GameObject.FindGameObjectWithTag("MainCanvas");
		canvasHost = GameObject.FindGameObjectWithTag("HostCanvas");
		canvasJoin = GameObject.FindGameObjectWithTag("JoinCanvas");

		setupMainCanvas(canvasMain);
		setupHostCanvas(canvasHost);
		setupJoinCanvas(canvasJoin);

		currentCanvas = canvasMain;
	}

	void setupMainCanvas ( GameObject canvas )
	{
		// Buttons
		getUIComponent<Button>(canvas, cpath + "HostButton").onClick.AddListener( () => { gotoCanvas( canvasHost ); });
		getUIComponent<Button>(canvas, cpath + "JoinButton").onClick.AddListener( () => { gotoCanvas( canvasJoin ); });
		getUIComponent<Button>(canvas, cpath + "SettingsButton").onClick.AddListener( () => {  });
		getUIComponent<Button>(canvas, cpath + "ExitButton").onClick.AddListener( () => { GameController.Instance.quitGame(); } );
		getUIComponent<Button>(canvas, cpath + "EditorButton").onClick.AddListener( () => {  });
	}

	void setupHostCanvas ( GameObject canvas )
	{
		// Input fields
		getUIComponent<InputField>(canvas, cpath + "NameInput").onValueChanged.AddListener( (string s) => { UpdateName(s); } );
		getUIComponent<InputField>(canvas, cpath + "PortInput").onValueChanged.AddListener( (string s) => { UpdatePort(s); } );

		// Buttons
		getUIComponent<Button>(canvas, cpath + "HostButton").onClick.AddListener( () => { HostSession(); });
		getUIComponent<Button>(canvas, cpath + "BackButton").onClick.AddListener( () => { gotoCanvas( canvasMain ); });
	}

	void setupJoinCanvas ( GameObject canvas )
	{
		// Input fields
		getUIComponent<InputField>(canvas, cpath + "NameInput").onValueChanged.AddListener( (string s) => { UpdateName(s); } );
		getUIComponent<InputField>(canvas, cpath + "IPInput").onValueChanged.AddListener( (string s) => { UpdateAdress(s); } );
		getUIComponent<InputField>(canvas, cpath + "PortInput").onValueChanged.AddListener( (string s) => { UpdatePort(s); } );

		// Buttons
		getUIComponent<Button>(canvas, cpath + "JoinButton").onClick.AddListener( () => { JoinSession(); });
		getUIComponent<Button>(canvas, cpath + "BackButton").onClick.AddListener( () => { gotoCanvas( canvasMain ); });
	}
		
	T getUIComponent<T> (GameObject root, string path)
	{
//		Debug.Log("root, path = " + root + ", " + path);
		return root.transform.FindChild(path).gameObject.GetComponent<T>();
	}

	void gotoCanvas(GameObject canvas)
	{
		currentCanvas.GetComponent<Canvas>().sortingOrder = 0;
		currentCanvas = canvas;
		currentCanvas.GetComponent<Canvas>().sortingOrder = 10;

		if( currentCanvas == canvasHost || currentCanvas == canvasJoin )
		{
			GameController.Instance.networkingInfo.reset();
		}
	}

	void HostSession()
	{
		Debug.Log("on HostSession()");
		GameController.Instance.networkingInfo.isServer = true;
		GameController.Instance.SceneLoader("NetScene");
	}

	void JoinSession()
	{
		GameController.Instance.networkingInfo.isServer = false;
		GameController.Instance.SceneLoader("NetScene");
	}

	void UpdateName(string s)
	{
		GameController.Instance.networkingInfo.playerName = s;
	}

	void UpdateAdress(string s)
	{
		GameController.Instance.networkingInfo.address = s;
	}

	void UpdatePort(string s)
	{
		if( !int.TryParse(s, out GameController.Instance.networkingInfo.port) ) 
		{
			GameController.Instance.networkingInfo.port = -1;
		}
	}
}