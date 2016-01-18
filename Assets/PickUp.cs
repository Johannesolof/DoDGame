using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PickUp : MonoBehaviour
{

    private Transform _startTransform;


	// Use this for initialization
	void Start ()
	{
	    _startTransform = transform;
        
	}
    
	// Update is called once per frame
	void Update () {
        
	}

    void OnMouseDown()
    {
        Camera camera = Camera.main;
        
        var ray = camera.ScreenPointToRay(new Vector3(camera.pixelWidth / 2f, camera.pixelHeight / 2f));
        var pos = ray.GetPoint(3);

        var renderer = GetComponent<Renderer>();
        

        


        transform.rotation = Quaternion.LookRotation(ray.direction);

        transform.position = pos;
    }
}
