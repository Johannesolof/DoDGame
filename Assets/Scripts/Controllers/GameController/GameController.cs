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
				if(go != null) _instance = go.GetComponent<GameController>();
			}
			return _instance;
		}
		private set{_instance = Instance;}
	}

	public NetworkingInfo networkingInfo = new NetworkingInfo();

	public void resetNetworking ()
	{
		networkingInfo = new NetworkingInfo();
	}

	void Awake ()
	{
		DontDestroyOnLoad(this);
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
