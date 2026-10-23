using Pencils.Platform.Vulkan.Utility;
using Pencils.Vulkan;
using Silk.NET.Windowing;
using Vortice.Vulkan;

namespace Pencils.Platform.Vulkan;

public class VkContext
{
    private Instance _instance;
    
    public VkContext(IWindow window)
    {
        VkVersion version = new  VkVersion(1, 3, 0);
        
        VkApplicationInfo applicationInfo = new()
        {
            sType =  VkStructureType.ApplicationInfo,
            apiVersion = version,
        };
        
        _instance = new Instance(applicationInfo, window.VkSurface);
    }
}