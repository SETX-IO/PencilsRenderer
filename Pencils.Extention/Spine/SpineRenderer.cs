using Spine;

namespace Pencils.Extension.Spine;

public class SpineRenderer
{
    public SpineRenderer()
    {
        
    }

    public void Render()
    {
        TextureLoader tLoader = new PencilsTextureLoader();
        Atlas atlas = new Atlas("", tLoader);
        SkeletonBinary binary = new SkeletonBinary(atlas);
        
        
        SkeletonJson skeletonJson = new SkeletonJson(atlas);
        SkeletonData skeletonData = skeletonJson.ReadSkeletonData("");

        Skeleton skeleton = new Skeleton(skeletonData);
        
    }
}