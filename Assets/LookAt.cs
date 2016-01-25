using System;
using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour
{

    enum Zooming
    {
        NotZoomed,
        Zoomed,
        ZoomingIn,
        ZoomingOut
    }

    private Zooming _zoom;
    private Camera _camera;
    private bool _draging;
    private RaycastHit _dragPrevPos;
    private CameraPosition _defaultTransform;
    private CameraPosition _zoomTransform;
    private Vector3 _screenPoint;
    private Vector3 _offset;
    private float _lerp;
    private float _defaultFov;
    private float _zoomFov;
    public float Duration = 1f;
    private Vector3 _prevPoint;
    private RaycastHit _hit;
    private bool _isHit;

    // Use this for initialization
    void Start()
    {
        _zoom = Zooming.NotZoomed;
        _camera = GetComponent<Camera>();
        _defaultTransform = new CameraPosition(transform);
        _defaultFov = _camera.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        _isHit = Physics.Raycast(ray, out hit, 100);


            switch (_zoom)
        {
            case Zooming.NotZoomed:
                if (Input.GetMouseButtonDown(0))
                {
                    if (_isHit && hit.transform.tag == "Zoomable")
                    {
                        GameObject temp = new GameObject();
                        //var tf = gameObject.AddComponent<Transform>();


                        temp.transform.position = hit.transform.up * 2.2f + hit.transform.position;


                        temp.transform.LookAt(hit.transform);
                        _zoomTransform = new CameraPosition(temp.transform);

                        _zoom = Zooming.ZoomingIn;
                        _lerp = 0;
                    }
                }
                break;
            case Zooming.ZoomingIn:

                _lerp += Time.deltaTime / Duration;

                transform.position = Vector3.Lerp(_defaultTransform.Position, _zoomTransform.Position, _lerp);
                transform.forward = Vector3.Lerp(_defaultTransform.Forward, _zoomTransform.Forward, _lerp);

                if (_lerp >= 1)
                    _zoom = Zooming.Zoomed;

                break;
            case Zooming.ZoomingOut:

                _lerp += Time.deltaTime / Duration;

                transform.position = Vector3.Slerp(_zoomTransform.Position, _defaultTransform.Position, _lerp);
                transform.forward = Vector3.Slerp(_zoomTransform.Forward, _defaultTransform.Forward, _lerp);
                _camera.fieldOfView = Mathf.Lerp(_zoomFov, _defaultFov, _lerp);

                if (_lerp >= 1)
                    _zoom = Zooming.NotZoomed;

                break;
            case Zooming.Zoomed:
                if (Input.GetMouseButton(0))
                {
                    if (_isHit && hit.transform.tag == "Zoomable")
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            _offset = transform.position - hit.point;
                            _prevPoint = hit.point;
                        }
                        Vector3 delta = _prevPoint - hit.point;
                        _camera.transform.position += new Vector3(delta.x, 0, delta.z);
                    }
                    break;
                }

                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    _camera.fieldOfView = Mathf.Min(_camera.fieldOfView + 2, 80);

                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    _camera.fieldOfView = Mathf.Max(_camera.fieldOfView - 2, 5);

                if (Input.GetMouseButtonDown(1))
                {
                    _zoomTransform = new CameraPosition(transform);
                    _zoomFov = _camera.fieldOfView;
                    _zoom = Zooming.ZoomingOut;
                    _lerp = 0;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100))
        {
            _prevPoint = hit.point;
        }
    }
}

public class CameraPosition
{
    public Vector3 Position;
    public Vector3 Forward;

    public CameraPosition(Transform transform)
    {
        Position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Forward = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);

    }
}
