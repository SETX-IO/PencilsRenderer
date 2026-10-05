using System.Numerics;
using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;
using Pencils.Platform.DirectX11;
using Pencils.RendererApi;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Pencils.Extension;

public class ImGuiRenderer
{
    public readonly Renderer renderer;

    private IShader shader;
    private IMesh mesh;
    private IVertexBuffer? vertexBuffer;
    private IIndexBuffer? indexBuffer;
    
    private Dictionary<nint, ITexture> _textures = new();

    private int _vertexBufferSize;
    private int _indexBufferSize;

    public event Action? ImGuiGenGuiEvent;
    
    public ImGuiRenderer(Renderer renderer)
    {
        this.renderer = renderer;
        
        ImGui.CreateContext();
        ImGuiIOPtr io = ImGui.GetIO();

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasTextures;

        io.DisplaySize = new Vector2(800, 600);
        // Vector2 fbScale = new Vector2(800 > 0 ? (float)800 / 800 : 1f, 600 > 0 ? (float)600 / 600 : 1f);
        // io.DisplayFramebufferScale = fbScale;
        
        Init();
    }

    public void Init()
    {
        VertexAttrib[] attribTypes = [ new(VertexAttribType.Position2), new(VertexAttribType.TexCoord), new(VertexAttribType.Color4U) ];
        
        mesh = DxMesh.Create();
        shader = DxShader.Create("Shader/ImGuiShader.hlsl", attribTypes);

        BlendDescription blendInfo = new();
        ref RenderTargetBlendDescription blendRt = ref blendInfo.RenderTarget[0];
        blendRt.BlendEnable = true;
        blendRt.SourceBlend = Blend.SourceAlpha;
        blendRt.DestinationBlend = Blend.InverseSourceAlpha;
        blendRt.BlendOperation = BlendOperation.Add;
        blendRt.SourceBlendAlpha = Blend.One;
        blendRt.DestinationBlendAlpha = Blend.InverseDestinationAlpha;
        blendRt.BlendOperationAlpha = BlendOperation.Add;
        blendRt.RenderTargetWriteMask = ColorWriteEnable.All;
    }
    
    public unsafe void Render(float deltaTime)
    {
        var io = ImGui.GetIO();
        io.DeltaTime = deltaTime;

        ImGui.NewFrame();
        ImGuiGenGuiEvent?.Invoke();
        ImGui.Render();
        
        if (ImGuiGenGuiEvent == null) return;
        
        ImDrawDataPtr drawData = ImGui.GetDrawData();
        
        if (drawData.DisplaySize.X == 0 || drawData.DisplaySize.Y == 0) return;
        
        for (int i = 0; i < drawData.Textures.Size; i++)
        {
            ImTextureDataPtr textureData = drawData.Textures[i];

            if (textureData.Status != ImTextureStatus.Ok)
                UpdateTexture(textureData);
        }
        
        if (vertexBuffer == null || _vertexBufferSize < drawData.TotalVtxCount)
        {
            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }
        
            _vertexBufferSize = drawData.TotalVtxCount + 5000;
            vertexBuffer = DxVertexBuffer.Create<ImDrawVert>((uint)_vertexBufferSize);
            mesh.SetVertexBuffer(0, vertexBuffer);
        }
        
        if (indexBuffer == null || _indexBufferSize < drawData.TotalIdxCount)
        {
            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
                indexBuffer = null;
            }
        
            _indexBufferSize = drawData.TotalVtxCount + 10000;
            indexBuffer = DxIndexBuffer.Create((uint)_indexBufferSize);
            mesh.SetIndexBuffer(indexBuffer);
        }
                
        Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
            drawData.DisplayPos.X,
            drawData.DisplayPos.X + drawData.DisplaySize.X,
            drawData.DisplayPos.Y + drawData.DisplaySize.Y, // 注意：ImGui 的 Y 轴向下，这里翻转一下
            drawData.DisplayPos.Y,
            0.0f,
            1.0f
        );

        nint vBufferMap = vertexBuffer.Map();
        nint iBufferMap = indexBuffer.Map();
        
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];

            Unsafe.CopyBlock((void*)vBufferMap, cmdList.VtxBuffer.Data, (uint)(cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>()));
            Unsafe.CopyBlock((void*)iBufferMap, cmdList.IdxBuffer.Data, (uint)(cmdList.IdxBuffer.Size * Unsafe.SizeOf<ushort>()));

            vBufferMap += cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            iBufferMap += cmdList.IdxBuffer.Size * Unsafe.SizeOf<ushort>();
        }
        
        vertexBuffer.CloseMap();
        indexBuffer.CloseMap();

        BlendStateInfo blendInfo = new BlendStateInfo()
        {
            srcFactor = BlendFactor.SourceAlpha,
            dstFactor = BlendFactor.InverseSourceAlpha,
            dstAlphaFactor = BlendFactor.InverseDestinationAlpha
        };

        DepthStencilInfo depthStencilInfo = new DepthStencilInfo()
        {
            DepthWriteMask = WriteMask.Off
        };
        
        renderer.RCommand.SetFillAndCull(Fill.Solid, Cull.Off);
        renderer.RCommand.SetDepthStStencilState(depthStencilInfo);
        renderer.RCommand.SetBlendState(blendInfo);
        
        mesh.Bind();
        
        shader.Use();
        shader.UploadConstantMat44("vertexBuffer", mvp, false);
        renderer.SetViewport(drawData.DisplaySize.X * drawData.FramebufferScale.X,
            drawData.DisplaySize.Y * drawData.FramebufferScale.Y, 1.0f);
        
        Vector2 clipOff = drawData.DisplayPos;
        Vector2 clipScale = drawData.FramebufferScale;

        int vtxOffset = 0;
        int idxOffset = 0;
        
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];

            for (int cmd_i = 0; cmd_i < cmdList.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmd cmd = cmdList.CmdBuffer[cmd_i];

                Vector2 clipMin = new((cmd.ClipRect.X - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y);
                Vector2 clipMax = new((cmd.ClipRect.Z - clipOff.X) * clipScale.X,
                    (cmd.ClipRect.W - clipOff.Y) * clipScale.Y);
                if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                    continue;
                
                renderer.SetScissor((int)clipMin.X, (int)clipMin.Y, (int)clipMax.X, (int)clipMax.Y);
                
                _textures.TryGetValue(cmd.GetTexID(), out ITexture? texture);
                texture?.Bind();
                
                renderer.RCommand.DrawIndexed(cmd.ElemCount, (uint)(cmd.IdxOffset + idxOffset), (uint)(cmd.VtxOffset + vtxOffset));
                
                texture?.Unbind();
            }

            idxOffset += cmdList.IdxBuffer.Size;
            vtxOffset += cmdList.VtxBuffer.Size;
        }
        
        mesh.Unbind();
    }

    public void Update(float deltaTime)
    {
        
    }

    private unsafe void UpdateTexture(ImTextureDataPtr textureData)
    {
        switch (textureData.Status)
        {
            case ImTextureStatus.WantCreate:
            {
                nint pixels = (nint)textureData.GetPixels();
                ITexture2D texture2D = DxTexture2D.Create((uint)textureData.Width, (uint)textureData.Height);
                texture2D.SetData(pixels, (uint)textureData.GetPitch());

                textureData.SetStatus(ImTextureStatus.Ok);
                textureData.SetTexID((nint)texture2D.Id);

                _textures.Add((nint)texture2D.Id, texture2D);
                break;
            }
            case ImTextureStatus.WantUpdates:
            {
                for (int i = 0; i < textureData.Updates.Size; i++)
                {
                    var r = textureData.Updates[i];
                        
                    Viewport rect = new Viewport(r.X, r.Y, r.X + r.W, r.Y + r.H);
                    _textures[textureData.TexID].SetData((nint)textureData.GetPixelsAt(r.X, r.Y), (uint)textureData.GetPitch(), rect);
                }

                textureData.SetStatus(ImTextureStatus.Ok);
                break;
            }
        }
    }
}