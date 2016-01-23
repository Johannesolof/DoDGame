using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

// This game controller script is supposed to be staying between all scene changes.
// This way, we can easily keep important things between all scenes.
public class GameController : MonoBehaviour {

	static private GameController _instance;
	static public GameController Instance {
		get
		{
			if(_instance == null)
			{
				var go = GameObject.FindWithTag("GameController");
				_instance = go.GetComponent<GameController>();
			}
			return _instance;
		}
		private set{_instance = Instance;}
	}

	public Canvas mainCanvas;
	public Canvas hostCanvas;
	public Canvas joinCanvas;

	// For networking
	public string playerName = "";
	public int port = -1;
	public string adress = "";
	public bool isServer = false;

	public const int defaultPort = 47624;


	Scene currentScene;
	Canvas currentCanvas;


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
			if(port == -1) port = defaultPort;
			SceneManager.LoadScene("NetScene");
		}
	}

	public void JoinGame()
	{
		if(playerName != "" && adress != "")
		{
			if(port == -1) port = defaultPort;
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
	}
}
