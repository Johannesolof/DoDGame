using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

// This game controller script is supposed to be staying between all scene changes.
// This way, we can easily keep important things between all scenes.
public class GameController 
{
	// Singleton pattern. It is thread safe and the user can always assume that the object exist.
	// It will also not be instantiated until first use, i.e. it "hold" default values
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

	// "GLOBALS"
	// Some shared data between scenes, for networking
	public RPC.NetworkingInfo networkingInfo = new RPC.NetworkingInfo();

	// PUBLIC FUNCTIONS
	// These can be used from wherever in the codebase
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
		// Acknowledge the scene, and maybe do some special treatment?
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
