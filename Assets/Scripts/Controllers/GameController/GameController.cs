using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

// This game controller script is supposed to be staying between all scene changes.
// This way, we can easily keep important things between all scenes.
public class GameController 
{
	static private GameController _instance;
	static public GameController Instance {
		get {
			if (_instance == null) {
				_instance = new GameController ();
			}
			return _instance;
		}
		private set {

		}
	}

	public NetworkingInfo networkingInfo = new NetworkingInfo();

	public void resetNetworking ()
	{
		networkingInfo = new NetworkingInfo();
	}

	public void quitGame()
	{
		#if UNITY_EDITOR || UNITY_EDITOR_64
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	public void SceneLoader(string scene)
	{
		switch(scene)
		{
		case "MenuScene":
			SceneManager.LoadScene(scene);
			break;
		case "NetScene":
			SceneManager.LoadScene(scene);
			break;
		default:
			break;
		}
	}
}
