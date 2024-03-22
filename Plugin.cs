using BepInEx;
using HarmonyLib;
namespace GiantOverhaul;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        // Plugin load logic goes here!
        // This script acts like a unity object.
        harmony.PatchAll();
        Logger.LogInfo($"Giant Overhaul Active!");
    }
}
