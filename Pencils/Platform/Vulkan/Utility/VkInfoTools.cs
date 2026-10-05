using System;
using Vortice.Vulkan;
using Vk = Vortice.Vulkan.Vulkan;

namespace Pencils.Platform.Vulkan.Utility;

public class VkInfoTools
{
    public static unsafe VkVersion GetLatestApiVersion()
    {
        return Vk.vkGetInstanceProcAddr(0, "vkEnumerateInstanceVersion").Value != null ? throw new MethodAccessException("No find vkEnumerateInstanceVersion Method.") : Vk.vkEnumerateInstanceVersion();
    }
}