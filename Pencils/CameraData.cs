using System.Numerics;

namespace Pencils;

public struct CameraData
{
    public float fov;
    public float aspect;
    public float zoom;
    public Vector3 rotation;
    public Vector3 position;
    public CameraType cameraType;

    public CameraData(CameraType type)
    {
        fov = 0;
        aspect = 0;
        zoom = 1f;
        rotation = default;
        position = default;
        cameraType = type;
    }
}