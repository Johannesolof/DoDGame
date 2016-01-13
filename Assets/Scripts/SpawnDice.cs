using UnityEngine;
using System.Collections;

public class SpawnDice : MonoBehaviour
{

    public GameObject dice;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButtonDown(0))
	    {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Casts the ray and get the first game object hit
            Physics.Raycast(ray, out hit);
            Debug.Log("This hit at " + hit.point);
	        var pos = hit.point;
	        pos.y += 2;
            if (dice != null)
                Instantiate(dice, pos, Quaternion.identity);
	    }
	}
}
