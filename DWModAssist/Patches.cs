using HarmonyLib;
//using UnityEngine;

namespace DWModAssist
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix] //Need to be able to detach player when warping to prevent getting stuck
        [HarmonyPatch(typeof(PlayerAttachPoint), nameof(PlayerAttachPoint.AttachPlayer))]
        public static void PlayerAttachPoint_AttachPlayer_Postfix(PlayerAttachPoint __instance)
        {
            DWModAssist.CurrentAttachPoint = __instance;
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(OWTriggerVolume), nameof(OWTriggerVolume.AddObjectToVolume))]
        public static void ForDebugPostfix(OWTriggerVolume __instance, GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
                DWModAssist.ModInstance.ModHelper.Console.WriteLine($"Player has entered {__instance.gameObject.name}");
        }*/
    }
}
