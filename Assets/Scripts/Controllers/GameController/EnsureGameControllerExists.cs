using UnityEngine;
using System.Collections;

// Drag this onto any game object in the scene, 
// preferrably the camera
public class EnsureGameControllerExists : MonoBehaviour 
{
	void Awake () {
		if(GameController.Instance == null)
		{
			GameObject game = new GameObject("Game");
			game.AddComponent<GameController>();
			game.tag = "GameController";
		}
	}
}