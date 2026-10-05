using System;
using Pencils.Platform.Vulkan.Utility;
using Silk.NET.Windowing;
using Vortice.Vulkan;
using Vk = Vortice.Vulkan.Vulkan;

namespace Pencils.Platform.Vulkan;

public class VkContext
{
    private VkInstance _instance;
    public unsafe VkContext(IWindow window)
    {
        
        // window.VkSurface
        VkApplicationInfo applicationInfo = new VkApplicationInfo
        {
            sType =  VkStructureType.ApplicationInfo,
            apiVersion = VkInfoTools.GetLatestApiVersion(),
        };
        VkInstanceCreateInfo createInfo = new VkInstanceCreateInfo
        {
            pApplicationInfo = &applicationInfo,
        };
        
        if (Vk.vkCreateInstance(in createInfo, out _instance) != VkResult.Success)
            throw new NullReferenceException("Failed to create instance");
    }
}