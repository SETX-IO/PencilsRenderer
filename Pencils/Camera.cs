using System.Numerics;

namespace Pencils;

public class Camera
{
    private static Vector3 _front = Vector3.UnitZ;
    private static Vector3 _up = Vector3.UnitY;
    
    private float _fov;
    private float _aspect;
    private Vector3 _rotation;
    private Vector3 _position;

    private Matrix4x4 _view => Matrix4x4.CreateLookAtLeftHanded(_position,  _position + _front, _up);
    private Matrix4x4 _projection => Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(float.DegreesToRadians(_fov), _aspect, 1, 100);

    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }
    
    public Vector3 Rotation { 
        get => _rotation;
        set => _rotation = value;
    }

    public float Fov
    {
        get => _fov;
        set => _fov = value;
    }
    
    public Matrix4x4 CameraMatrix => _view * _projection;
    
    public Camera(int width, int height)
    {
        _aspect = (float)width / height;
        _fov = 45f;
    }

    public Camera()
    {
        _aspect = 16f / 9f;
        _fov = 45f;
    }

    public void SetAspect(int width, int height) => _aspect = (float)width / height;
    
}