using System;
using UnityEngine;
using System.Collections;

public class SpawnDice : MonoBehaviour
{

    public GameObject[] dice;

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
            
	        var pos = hit.point - ray.direction;
	        if (dice != null)
	        {
                GameObject ins = (GameObject) Instantiate(dice[UnityEngine.Random.Range(0, dice.Length-1)] , ray.origin + ray.direction*2, Quaternion.identity);
	            var rg = ins.GetComponent<Rigidbody>();
	            rg.velocity = ray.direction*12;
	            float rotValue = 5f;
                rg.angularVelocity = new Vector3(UnityEngine.Random.Range(-rotValue, rotValue), UnityEngine.Random.Range(-rotValue, rotValue), UnityEngine.Random.Range(-rotValue, rotValue));
	        }
	    }
	}
}
