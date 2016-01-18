using UnityEngine;
using System.Collections.Generic;

public abstract class BaseDice : MonoBehaviour
{
    private Renderer _renderer;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private List<Normal> _normals;

    [HideInInspector]
    public Normal Closest;

    // Use this for initialization
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _transform = GetComponent<Transform>();
        Closest = new Normal();

        _normals = GetNormals(0);

        SetFadeColors();
    }

    protected abstract List<Normal> GetNormals(int sides);

    private bool _fadeInDone;
    private bool _fadeOut;
    public float Duration = 1f;
    private Color _startColor;
    private Color _endColor;
    private float _lerp;

    void SetFadeColors()
    {
        var color = _renderer.material.color;
        _endColor = color;
        color.a = 0;
        _startColor = color;
        _renderer.material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_fadeInDone)
        {
            _lerp += Time.deltaTime / Duration;
            _renderer.material.color = Color.Lerp(_startColor, _endColor, _lerp);
            if (_lerp >= 1)
            {
                _fadeInDone = true;
            }
        }

        if (_fadeOut)
        {
            _lerp += Time.deltaTime / Duration;
            _renderer.material.color = Color.Lerp(_endColor, _startColor, _lerp);
            if (_lerp >= 1)
            {
                Destroy(gameObject);
            }
        }

        Vector3 up = _transform.InverseTransformDirection(Vector3.up);
        Closest = new Normal() { Value = -1, Direction = new Vector3(99, 99, 99) };
        float angle = 360;
        foreach (var normal in _normals)
        {
            var a = Vector3.Angle(normal.Direction, up);
            if (a < angle)
            {
                Closest = normal;
                angle = a;
            }
        }
    }

    public void Destroy()
    {
        _lerp = 0;
        _fadeOut = true;
    }

    void OnGUI()
    {
        var point = Camera.main.WorldToScreenPoint(_transform.position);
        GUI.Label(new Rect(point.x, Camera.main.pixelHeight - point.y - 70, 200, 100), Closest.Value != -1 ? Closest.Value.ToString() : "");
    }
}

public class Normal
{
    public int Value { get; set; }
    public Vector3 Direction { get; set; }

    public Normal()
    {
        Value = -1;
        Direction = new Vector3();
    }
}
