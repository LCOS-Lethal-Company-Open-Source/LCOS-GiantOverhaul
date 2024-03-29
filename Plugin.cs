using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
namespace GiantOverhaul;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{

    //TODO: Get actual amount of seconds
    private const int secondsUntilMad = 330;
    Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static Plugin Instance;

    private void Awake()
    {
        // Plugin load logic goes here!
        // This script acts like a unity object.
        Instance = this;
        harmony.PatchAll();
        Logger.LogInfo($"Giant Overhaul Active!");
    }

    [HarmonyPatch(typeof(ForestGiantAI), "Update")]
    class GiantPassiveDayPatch 
    {

        static FieldInfo lostPlayerInChase = typeof(ForestGiantAI).GetField("lostPlayerInChase", 
                                                                             BindingFlags.Instance | 
                                                                             BindingFlags.NonPublic);
        static bool Prefix(ref ForestGiantAI __instance) 
        {
            bool playerHasCandy = false;
            PlayerControllerB[] playersInLOS = __instance.GetAllPlayersInLineOfSight(50f, 70, __instance.eye, 3f, StartOfRound.Instance.collidersRoomDefaultAndFoliage);
            for (int i = 0; i < playersInLOS.Length; i++) 
            {
                if (playersInLOS[i].twoHanded) playerHasCandy = true;
            }
            if (StartOfRound.Instance.timeSinceRoundStarted <= secondsUntilMad && !playerHasCandy) 
            {
                __instance.currentBehaviourStateIndex = 0;
            }
            if (playerHasCandy) 
            {
                __instance.currentBehaviourStateIndex = 1;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ForestGiantAI), "BeginEatPlayer")]
    class GiantNoEatInDayPatch 
    {
        static bool Prefix(ref ForestGiantAI __instance) 
        {
            if (StartOfRound.Instance.timeSinceRoundStarted <= secondsUntilMad) 
            {
                bool playerHasCandy = false;
                PlayerControllerB[] playersInLOS = __instance.GetAllPlayersInLineOfSight(50f, 70, __instance.eye, 3f, StartOfRound.Instance.collidersRoomDefaultAndFoliage);
                for (int i = 0; i < playersInLOS.Length; i++) 
                {
                    if (playersInLOS[i].twoHanded) playerHasCandy = true;
                }
                return playerHasCandy;
            }
            else return true;
        }
    }
}
