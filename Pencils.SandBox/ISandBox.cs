namespace Pencils.SandBox;

public interface ISandBox
{
    void Init(int width, int height);
    void Renderer(float time);
    void Update(float time);
}