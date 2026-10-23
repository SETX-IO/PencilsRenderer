using System;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Vortice.Vulkan;
using Vk = Vortice.Vulkan.Vulkan;

namespace Pencils.Vulkan;

public unsafe class Instance
{
    private VkInstance _instance;
    
    public Instance(VkApplicationInfo applicationInfo, IVkSurface surface)
    {

        byte** extensionName = surface.GetRequiredExtensions(out var extensionCount);
        
        VkInstanceCreateInfo info = new()
        {
            pApplicationInfo = &applicationInfo,
            enabledExtensionCount = extensionCount,
            ppEnabledExtensionNames = extensionName,
        };
        
        if (Vk.vkCreateInstance(in info, out _instance) != VkResult.Success)
            throw new NullReferenceException("Failed to create instance");
        
        VkSurfaceKHR vkSurface = surface.Create<VkSurfaceKHR>(new VkHandle(_instance.Handle), null).Handle;
    }
}