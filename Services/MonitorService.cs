using System.Collections.Generic;

namespace screenshareav.Services
{
    public class MonitorInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Index { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPrimary { get; set; }
    }

    public static class MonitorService
    {
        // TODO: Implement cross-platform monitor detection
        public static List<MonitorInfo> GetMonitors()
        {
            // Stub: Return two fake monitors for now
            return new List<MonitorInfo>
            {
                new MonitorInfo { Name = "Monitor 1", Index = 0, Width = 1920, Height = 1080, IsPrimary = true },
                new MonitorInfo { Name = "Monitor 2", Index = 1, Width = 2560, Height = 1440, IsPrimary = false }
            };
        }
    }
}
