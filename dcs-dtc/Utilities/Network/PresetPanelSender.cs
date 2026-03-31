using DTC.New.Presets.V2.Base;
using DTC.Utilities;
using System.Text;

namespace DTC.Utilities.Network;

/// <summary>
/// Sends the current aircraft's preset list to the DCS in-game preset browser panel.
/// Serializes with a fixed JSON key order so the Lua pattern matcher can parse without a JSON library.
/// </summary>
internal static class PresetPanelSender
{
    private static readonly UDPSocket _socket = new UDPSocket();

    public static void SendPresetList(IEnumerable<IPreset> presets)
    {
        var sb = new StringBuilder("[");
        bool first = true;
        foreach (var p in presets)
        {
            if (!first) sb.Append(',');
            first = false;

            int waypointCount = GetWaypointCount(p);
            string radios = GetRadioSummary(p);
            string escapedName = p.Name.Replace("\\", "\\\\").Replace("\"", "\\\"");

            // Key order MUST match the Lua string.gmatch pattern in dtcPanel.lua
            sb.Append($"{{\"name\":\"{escapedName}\",\"waypoints\":{waypointCount},\"radios\":\"{radios}\"}}");
        }
        sb.Append(']');

        _socket.Send("preset_list:" + sb.ToString(), "127.0.0.1", Settings.TCPSendPort);
    }

    private static int GetWaypointCount(IPreset preset)
    {
        try
        {
            if (preset.Configuration is not Configuration cfg) return 0;
            foreach (var sys in cfg.GetSystems())
            {
                if (!sys.PropertyName.Contains("Waypoint") && !sys.PropertyName.Contains("Route")) continue;
                var val = cfg.GetType().GetProperty(sys.PropertyName)?.GetValue(cfg);
                if (val == null) continue;
                var wptsProp = val.GetType().GetProperty("Waypoints");
                if (wptsProp?.GetValue(val) is System.Collections.ICollection col)
                    return col.Count;
            }
        }
        catch { }
        return 0;
    }

    private static string GetRadioSummary(IPreset preset)
    {
        try
        {
            if (preset.Configuration is not Configuration cfg) return "";
            var radiosProp = cfg.GetType().GetProperty("Radios");
            if (radiosProp == null) return "";
            var radiosObj = radiosProp.GetValue(cfg);
            if (radiosObj == null) return "";
            var radiosListProp = radiosObj.GetType().GetProperty("Radios");
            if (radiosListProp?.GetValue(radiosObj) is not System.Collections.IList list || list.Count < 2) return "";
            string r1 = GetFreq(list[0]);
            string r2 = GetFreq(list[1]);
            return $"{r1}/{r2}";
        }
        catch { }
        return "";
    }

    private static string GetFreq(object? radio)
    {
        if (radio == null) return "---";
        return radio.GetType().GetProperty("Frequency")?.GetValue(radio)?.ToString() ?? "---";
    }
}
