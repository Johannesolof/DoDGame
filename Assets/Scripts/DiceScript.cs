using System;
using UnityEngine;
using System.Collections;
using System.Linq;

public class DiceScript : MonoBehaviour
{
    private Renderer _renderer;
    private Rigidbody _rigidbody;
    private Transform _transform;


    public int Sides;

	// Use this for initialization
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _transform = GetComponent<Transform>();
        SetFadeColors();
    }




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
	        ;
	    }

	}
}
