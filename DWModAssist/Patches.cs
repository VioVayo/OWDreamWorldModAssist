using HarmonyLib;
using UnityEngine;

namespace DWModAssist
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.StartRoasting))]
        public static void Campfire_StartRoasting_Postfix(Campfire __instance)
        {
            DWModAssist.LastUsedCampfire = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.StartSleeping))]
        public static void Campfire_StartSleeping_Postfix(Campfire __instance)
        {
            DWModAssist.LastUsedCampfire = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.OnSocketableDonePlacing))]
        public static void NomaiRemoteCameraPlatform_SwitchToRemoteCamera_Postfix(NomaiRemoteCameraPlatform __instance)
        {
            DWModAssist.LastUsedProjectionPool = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.OnPressInteract))]
        public static void SlideProjector_OnPressInteract_Postfix(SlideProjector __instance)
        {
            DWModAssist.LastUsedSlideProjector = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Peephole), nameof(Peephole.Peep))]
        public static void Peephole_Peep_Postfix(Peephole __instance)
        {
            DWModAssist.LastUsedPeephole = __instance;
        }

        [HarmonyPostfix] 
        [HarmonyPatch(typeof(PlayerAttachPoint), nameof(PlayerAttachPoint.AttachPlayer))]
        public static void PlayerAttachPoint_AttachPlayer_Postfix(PlayerAttachPoint __instance)
        {
            DWModAssist.LastAttachedPoint = __instance;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(RingWorldController), nameof(RingWorldController.OnExitDreamWorld))]
        public static void RingWorldController_OnExitDreamWorld_Postfix()
        {
            Locator.GetCloakFieldController().OnPlayerEnter.Invoke();
        }

        [HarmonyPrefix] //Don't add player to AudioVolumes they don't spawn inside of
        [HarmonyPatch(typeof(DreamCampfire), nameof(DreamCampfire.OnExitDreamWorld))]
        public static bool DreamCampfire_OnExitDreamWorld_Prefix(DreamCampfire __instance)
        {
            var receiver = __instance._interactVolume as InteractReceiver;
            var distance = Vector3.Distance(Locator.GetPlayerTransform().position, receiver.gameObject.transform.position);
            return (distance < receiver._interactRange * 2.5f);
        }


        /*[HarmonyPostfix] //For Debug
        [HarmonyPatch(typeof(OWTriggerVolume), nameof(OWTriggerVolume.AddObjectToVolume))]
        public static void AAAAAAAAA_Postfix(OWTriggerVolume __instance, GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
                DWModAssist.ModInstance.ModHelper.Console.WriteLine($"Player has entered {__instance.gameObject.name}");
        }*/
    }
}
