using BepInEx;
using HarmonyLib;
namespace GiantOverhaul;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{

    //TODO: Get actual amount of seconds
    private const int secondsUntilMad = 330;
    Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        // Plugin load logic goes here!
        // This script acts like a unity object.
        harmony.PatchAll();
        Logger.LogInfo($"Giant Overhaul Active!");
    }

    [HarmonyPatch(typeof(ForestGiantAI), "Update")]
    class GiantPassiveDayPatch 
    {
        static bool Prefix(ref ForestGiantAI __instance) 
        {
            if (StartOfRound.Instance.timeSinceRoundStarted <= secondsUntilMad) 
            {
                __instance.currentBehaviourStateIndex = 0;
            }
            return true;
        }
    }
}
