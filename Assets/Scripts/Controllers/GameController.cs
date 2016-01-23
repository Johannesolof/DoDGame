using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

// This game controller script is supposed to be staying between all scene changes.
// This way, we can easily keep important things between all scenes.
public class GameController : MonoBehaviour {

	public Canvas mainCanvas;
	public Canvas hostCanvas;
	public Canvas joinCanvas;

	Scene currentScene;
	Canvas currentCanvas;

	// For networking
	string playerName = "";
	int port = 0;
	string adress = "";
	bool isServer = false;

	void Awake ()
	{
		DontDestroyOnLoad(this);
	}

	void Start ()
	{
		currentScene = SceneManager.GetActiveScene();

		switch(currentScene.name)
		{
		case "MenuScene":
			currentCanvas = mainCanvas;
			break;
		default:
			Debug.Log("Unknown scene: " + currentScene.name);
			break;
		}
	}

	void Update () 
	{
		
	}

	public void displayMainMenu()
	{
		currentCanvas.sortingOrder = 0;
		mainCanvas.sortingOrder = 10;
		currentCanvas = mainCanvas;
	}

	public void hostGameMenu()
	{
		currentCanvas.sortingOrder = 0;
		hostCanvas.sortingOrder = 10;
		currentCanvas = hostCanvas;
	}

	public void joinGameMenu()
	{
		currentCanvas.sortingOrder = 0;
		joinCanvas.sortingOrder = 10;
		currentCanvas = joinCanvas;
	}

	public void HostGame()
	{
		if(playerName != "")
		{
			isServer = true;
			SceneManager.LoadScene("NetScene");
		}
	}

	public void JoinGame()
	{
		if(playerName != "" && adress != "")
		{
			SceneManager.LoadScene("NetScene");
		}
	}

	public void quitGame()
	{
		#if UNITY_EDITOR || UNITY_EDITOR_64
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	public void SetName(string s)
	{
		playerName = s;
	}

	public void SetAdress(string s)
	{
		adress = s;
	}

	public void SetPort(string s)
	{
		port = int.Parse(s);
		Debug.Log("Port parsed to: " + port);
	}
}
