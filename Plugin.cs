using System;
using System.Collections.Generic;
using System.Linq;
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

    [HarmonyPatch(typeof(EnemyAI), "GetAllPlayersInLineOfSight")]
    class GiantSightPatch 
    {
        static void Postfix(ref EnemyAI __instance, ref PlayerControllerB[] __result) 
        {
            
            if (__instance.enemyType.enemyName == "ForestGiant" && __result != null) 
            {
                List<PlayerControllerB> updatedSight = new();
                if (StartOfRound.Instance.timeSinceRoundStarted <= secondsUntilMad) 
                {
                    for (int i = 0; i < __result.Length; i++) 
                    {
                        if (__result[i].twoHanded) 
                        {
                            updatedSight.Add(__result[i]);
                        }
                    }
                }
                else 
                {
                    updatedSight = __result.ToList<PlayerControllerB>();
                    bool playerHasCandy = false;
                    for (int i = 0; i < __result.Length; i++) 
                    {
                        if (__result[i].twoHanded) 
                        {
                            playerHasCandy = true;
                            break;
                        }
                    }
                    if (playerHasCandy)
                    {
                        for (int i = 0; i < __result.Length;) 
                        {
                            if (!__result[i].twoHanded) 
                            {
                                updatedSight.RemoveAt(i);
                            }
                            else i++;
                        }
                    }
                }
                if (__result.Length > updatedSight.Count && __instance.targetPlayer != null && (updatedSight.Count == 0 || !updatedSight.Contains(__instance.targetPlayer))) 
                {
                    __instance.SwitchToBehaviourState(0);
                }
                if (updatedSight.Count == 0) __result = null;
                else __result = updatedSight.ToArray();
            }
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

    [HarmonyPatch(typeof(ForestGiantAI), "Update")]
    class GiantEatCandyPatch 
    {

        public static MethodInfo eatAnim = typeof(ForestGiantAI).GetMethod("EatPlayerAnimation", BindingFlags.Instance | BindingFlags.NonPublic);

        static bool Prefix(ref ForestGiantAI __instance) 
        {
            if(!__instance.inSpecialAnimation) __instance.StartCoroutine((System.Collections.IEnumerator) eatAnim.Invoke(__instance, new object[]{null, UnityEngine.Vector3.zero, 0}));
            return true;

        }
    }
}
