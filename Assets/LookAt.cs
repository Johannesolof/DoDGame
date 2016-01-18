using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour {

    enum Zooming
    {
        NotZoomed, Zooming, Zoomed
    }

	// Use this for initialization
	void Start () {
	
	}


    private bool zoomed;

	// Update is called once per frame
	void Update () {

	    if (Input.GetMouseButtonDown(0) && !zoomed)
	    {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.transform.gameObject.name);
                transform.position = hit.transform.up*2 + hit.transform.position;
                transform.LookAt(hit.transform);
            }
        }

	}
}
