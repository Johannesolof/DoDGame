using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;

public abstract class BaseDice : MonoBehaviour
{
    private Renderer _renderer;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private List<Normal> _normals;

    // Use this for initialization
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _transform = GetComponent<Transform>();

        _normals = GetNormals(0);

        SetFadeColors();
    }

    protected abstract List<Normal> GetNormals(int sides);

    private bool _fadeInDone;
    public float Duration = 1f;
    private Color _startColor;
    private Color _endColor;
    private float _lerp = 0f;

    void SetFadeColors()
    {
        var color = _renderer.material.color;
        _endColor = color;
        color.a = 0;
        _startColor = color;
        _renderer.material.color = color;
    }

    // Update is called once per frame
	void Update ()
	{
	    if (!_fadeInDone)
	    {
	        _lerp += Time.deltaTime/Duration;
	        _renderer.material.color = Color.Lerp(_startColor, _endColor, _lerp);
	    }

	    if (_rigidbody.velocity.magnitude < 0.1)
	    {
	        switch (UpSide())
	        {
                case 0:
                    break;
                default:
                    break;
	        }
	        
	    }

	    



        

        
	}

    void OnGUI()
    {
        Vector3 up = _transform.InverseTransformDirection(Vector3.up);

        Normal closest = new Normal() {Value = 0, Direction = new Vector3(99, 99, 99)};
        foreach (var normal in _normals)
        {
            if (closest.Value == 0 || (normal.Direction - up).magnitude < closest.Direction.magnitude)
                closest = normal;
        }


        var point = Camera.main.WorldToScreenPoint(_transform.position);

        GUI.Label(new Rect(point.x - 100, Camera.main.pixelHeight - point.y -50, 200, 100), up + " | " + closest.Value);
    }

    int UpSide()
    {
        
        return 0;
    }
}

public class Normal
{
    public int Value { get; set; }
    public Vector3 Direction { get; set; }
}
