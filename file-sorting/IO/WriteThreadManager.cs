#if NET6_0_OR_GREATER
#if WINDOWS
using System.Management;
#endif
#endif

namespace FileSorting.IO;

public static class WriteThreadManager
{
    private static readonly bool IsSSDCached = DetectSSD();
    
    public static int DetermineMaxParallelism(bool enableSmartMode)
    {
        if (!enableSmartMode)
        {
            return 1;
        }

        int cpuCores = Environment.ProcessorCount;

        if (IsSSDCached)
        {
            return Math.Max(4, cpuCores / 4);
        }
        else
        {
            return 1;
        }
    }

    private static bool DetectSSD()
    {
        try
        {
#if WINDOWS
            return IsSSD_Windows();
#elif LINUX
            return IsSSD_Linux();
#elif OSX
            return IsSSD_MacOS();
#else
            return false;
#endif
        }
        catch { }
        return false;
    }

#if WINDOWS
    private static bool IsSSD_Windows()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT MediaType FROM Win32_DiskDrive"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["MediaType"]?.ToString()?.Contains("Solid State Drive") == true)
                    {
                        return true;
                    }
                }
            }
        }
        catch { }
        return false;
    }
#endif

#if LINUX
    private static bool IsSSD_Linux()
    {
        try
        {
            string sysPath = "/sys/block/sda/queue/rotational";
            if (File.Exists(sysPath))
            {
                return File.ReadAllText(sysPath).Trim() == "0";
            }
        }
        catch { }
        return false;
    }
#endif

#if OSX
    private static bool IsSSD_MacOS()
    {
        return false;
    }
#endif
}