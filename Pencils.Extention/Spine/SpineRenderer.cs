using Spine;

namespace Pencils.Extension.Spine;

public class SpineRenderer
{
    private SkeletonClipping _clipping;
    private float[] _worldVertices;
    public SpineRenderer()
    {
        _clipping = new SkeletonClipping(); 
    }

    public void Render()
    {
        TextureLoader tLoader = new PencilsTextureLoader();
        Atlas atlas = new Atlas("", tLoader);
        SkeletonBinary binary = new SkeletonBinary(atlas);
        
        
        SkeletonJson skeletonJson = new SkeletonJson(atlas);
        SkeletonData skeletonData = skeletonJson.ReadSkeletonData("");

        Skeleton skeleton = new Skeleton(skeletonData);

        ExposedList<Slot> pose = skeleton.DrawOrder.AppliedPose;
        foreach (var slot in pose)
        {
            Attachment attachment = slot.AppliedPose.Attachment;
            if (attachment != null)
            {
                _clipping.ClipEnd(slot);
                continue;
            }

            if ((slot.AppliedPose.GetColor().a == 0 || !slot.Bone.Active) && attachment is not ClippingAttachment)
            {
                _clipping.ClipEnd(slot);
                continue;
            }

            ref float[] worldVertices = ref _worldVertices;
            float[] vertices = worldVertices;
            float[] uvs = [];
            
            Color32F attachmentColor;
            uint verticesCount;
            int[] indices = [0, 1, 2, 2, 3, 0];
            int indicesCount = 6;
            
            if (attachment is RegionAttachment)
            {
                RegionAttachment? regionAttachment = (RegionAttachment?)attachment;
                attachmentColor = regionAttachment.GetColor();
                
                if (attachmentColor.a == 0)
                {
                    _clipping.ClipEnd(slot);
                    continue;
                }

                var sequence = regionAttachment.Sequence;
                var resolveIndex = sequence.ResolveIndex(slot.AppliedPose);
                TextureRegion region = sequence.GetRegion(resolveIndex);
                worldVertices = new float[8];
                regionAttachment.ComputeWorldVertices(slot, regionAttachment.GetOffsets(slot.AppliedPose), worldVertices, 0);
                verticesCount = 4;
                uvs = sequence.GetUVs(resolveIndex);
                // indices = [0, 1, 2, 2, 3, 0];
                // uint indicesCount = 6;
                // ITexture texture = region
            }
            else if (attachment is MeshAttachment)
            {
                MeshAttachment? mesh = (MeshAttachment?)attachment;
                attachmentColor = mesh.GetColor();
                
                if (attachmentColor.a == 0)
                {
                    _clipping.ClipEnd(slot);
                    continue;
                }
                
                var sequence = mesh.Sequence;
                var resolveIndex = sequence.ResolveIndex(slot.AppliedPose);
                TextureRegion region = sequence.GetRegion(resolveIndex);
                worldVertices = new float[mesh.WorldVerticesLength];
                mesh.ComputeWorldVertices(skeleton, slot, worldVertices);
                verticesCount = (uint)(mesh.WorldVerticesLength >> 1);
                uvs = sequence.GetUVs(resolveIndex);
                indices = mesh.Triangles;
                indicesCount = mesh.Triangles.Length;
                // ITexture texture = region
            }
            else if (attachment is ClippingAttachment)
            {
                ClippingAttachment clipping = (ClippingAttachment)slot.AppliedPose.Attachment;
                _clipping.ClipStart(skeleton, slot, clipping);
            }
            else continue;

            uint drakColor = 0xff000000;
            if (slot.AppliedPose.HasDarkColor)
            {
                Color32F? darkColor = slot.AppliedPose.GetDarkColor();
            }

            if (_clipping.IsClipping)
            {
                _clipping.ClipTriangles(worldVertices, indices, indicesCount, uvs);
                ExposedList<float> vertice = _clipping.ClippedVertices;
                uint vCount = (uint)_clipping.ClippedVertices.Count;
                ExposedList<float>uVs = _clipping.ClippedVertices;
                ExposedList<int>uUvs = _clipping.ClippedTriangles;
                uint iCount = (uint)_clipping.ClippedTriangles.Count;
            }
            _clipping.ClipEnd(slot);
        }
        _clipping.ClipEnd();
    }
}