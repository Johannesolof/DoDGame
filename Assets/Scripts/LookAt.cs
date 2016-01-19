using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

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
    private RaycastHit _prevHit;
    private Vector3 _delta;

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
        switch (_zoom)
        {
            case Zooming.NotZoomed:
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100) && hit.transform.tag == "Zoomable")
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
                    Ray ray1 = _camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit1;
                    if (Physics.Raycast(ray1, out hit1, 100) && hit1.transform.tag == "Zoomable") //TODO: Fulkod som funkar dåligt
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            _offset = transform.position - hit1.point;
                            _prevHit = hit1;
                            _delta = Vector3.zero;
                        }

                        var delta = _prevHit.point - hit1.point;
                        

                        if (delta != Vector3.zero)
                            Debug.Log(String.Format("{0:F3}, {1:F3}, {2:F3}", delta.x, delta.y, delta.z));
                        _prevHit = hit1;
                        //transform.position = new Vector3(temp.x, transform.position.y, temp.z);

                        if ((delta.normalized-_delta.normalized).magnitude < 1)
                        transform.position += delta*2;

                        _delta = delta;


                        //if (Input.GetMouseButtonDown(0))
                        //{
                        //    _screenPoint = _camera.WorldToScreenPoint(hit1.transform.position);

                        //    _offset = hit1.transform.position - _camera.ScreenToWorldPoint(new Vector3(Screen.width - Input.mousePosition.x, Screen.height - Input.mousePosition.y, _screenPoint.z));
                        //}

                        //Vector3 curScreenPoint = new Vector3(Screen.width - Input.mousePosition.x, Screen.height - Input.mousePosition.y, _screenPoint.z);

                        //Vector3 curPosition = _camera.ScreenToWorldPoint(curScreenPoint) + _offset;
                        ////hit1.transform.position = curPosition;
                        //transform.position = new Vector3(curPosition.x, _camera.transform.position.y, curPosition.z);
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
