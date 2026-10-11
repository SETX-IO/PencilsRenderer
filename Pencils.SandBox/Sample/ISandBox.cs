namespace Pencils.SandBox.Sample;

public interface ISandBox
{
    void Init(int width, int height);
    void Renderer(float time);
    void Update(float time);
}