using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace PEAKGaming
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PeakGamingPlugin : BaseUnityPlugin
    {
        private readonly Harmony patcher = new(MyPluginInfo.PLUGIN_GUID);
        internal static new ManualLogSource Log;

        public void Awake()
        {
            // Plugin startup logic
            Log = base.Logger;
            patcher.PatchAll();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        public void OnDestroy()
        {
            Harmony.UnpatchAll();
        }

        [HarmonyPatch(typeof(CharacterMovement), "Update")]
        public static class MovementAndJumpPatch
        {
            public static void Prefix(CharacterMovement __instance)
            {
                //Sprint Patch
                Traverse.Create(__instance).Field("sprintMultiplier").SetValue(25f);

                //Jump patch
                if (Input.GetKey(KeyCode.Space))
                {
                    Character character = (Character)Traverse.Create(__instance).Field("character").GetValue();
                    if (character.IsLocal)
                    {
                        character.refs.view.RPC("JumpRpc", 0, [false]);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CharacterClimbing), "GetRequestedPostition")]
        public static class ClimbingPatch
        {
            public static void Prefix(CharacterClimbing __instance)
            {
                __instance.climbSpeedMod = 30f;
            }
        }

        [HarmonyPatch(typeof(Character), "GetTotalStamina")]
        public static class StaminaPatch
        {
            public static void Prefix(Character __instance)
            {
                __instance.data.currentStamina = 100f;                    
            }
        }

        [HarmonyPatch(typeof(CharacterMovement), "TryToJump")]
        public static class JumpPatch
        {
            /**
             * Since we re-implement jumping entirely we skip the original method.
             */
            public static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterAfflictions), "Update")]
        public static class HealthPatch
        {
            public static void Prefix(CharacterAfflictions __instance)
            {
                Traverse.Create(__instance.character.data).Field("isInvincible").SetValue(true);
            }
        }
    }
}
