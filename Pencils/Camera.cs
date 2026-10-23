using System;
using System.Numerics;

namespace Pencils;

public enum CameraType
{
    Perspective,
    Orthographic
}

public struct MvpData(Matrix4x4 proj, Matrix4x4 view, Matrix4x4 model = default)
{
    public Matrix4x4 modelMatrix = Matrix4x4.Transpose(model == default ? Matrix4x4.Identity : model);
    public Matrix4x4 viewMatrix = Matrix4x4.Transpose(view);
    public Matrix4x4 projMatrix = Matrix4x4.Transpose(proj);
}

public class Camera
{
    private static Vector3 _front = Vector3.UnitZ;
    private static Vector3 _up = Vector3.UnitY;
    
    private CameraData _data;

    public Matrix4x4 View => _data.cameraType switch
    {
        // CameraType.Perspective => Matrix4x4.CreateLookAtLeftHanded(_data.position, Vector3.UnitZ, _up),
        CameraType.Perspective => Matrix4x4.CreateLookAtLeftHanded(_data.position, _data.position + _front, _up),
        CameraType.Orthographic => Matrix4x4.Identity,
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public Matrix4x4 Projection => _data.cameraType switch
    {
        CameraType.Perspective => Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(float.DegreesToRadians(_data.fov),
            _data.aspect, 1, 1000),
        CameraType.Orthographic => Matrix4x4.CreateOrthographicLeftHanded(_data.zoom * _data.aspect, _data.zoom, -100f, 100f),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public Vector3 Position
    {
        get => _data.position;
        set => _data.position = value;
    }
    
    public Vector3 Rotation { 
        get => _data.rotation;
        set => _data.rotation = value;
    }

    public float Fov
    {
        get => _data.fov;
        set => _data.fov = value;
    }
    
    public float Zoom
    {
        get => _data.zoom; 
        set => _data.zoom = value;
    }
    
    public MvpData SceneData => new MvpData(Projection, View);
    
    public Camera(int width, int height, CameraType type = CameraType.Perspective)
    {
        _data.cameraType = type;
        
        _data.aspect = (float)width / height;
        
        _data.fov = 45f;
    }

    public Camera(CameraType type = CameraType.Perspective)
    {
        _data.cameraType = type;
        _data.aspect = 16f / 9f;
        _data.fov = 45f;
    }

    public void SetAspect(int width, int height) => _data.aspect = (float)width / height;
    
}