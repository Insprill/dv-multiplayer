
using HarmonyLib;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Utils;

namespace Multiplayer.Patches.Train
{
    [HarmonyPatch(typeof(ObiRopeGrabArea))]
    public static class ObiRopeGrabAreaPatch
    {
        [HarmonyPatch(nameof(ObiRopeGrabArea.Start))]
        [HarmonyPrefix]
        public static void Start(ObiRopeGrabArea __instance)
        {
            if (__instance.transform.parent == null)
                __instance.GetOrAddComponent<ObiRopeGrabAreaHandler>();
            else
                __instance.transform.parent.GetOrAddComponent<ObiRopeGrabAreaHandler>();
        }

        [HarmonyPatch(nameof(ObiRopeGrabArea.StartGrab))]
        [HarmonyPrefix]
        public static void StartGrab(ObiRopeGrabArea __instance)
        {
            ObiRopeGrabAreaHandler handler;

            if (__instance.transform.parent == null)
                __instance.gameObject.TryGetComponent(out handler);
            else
                __instance.transform.parent.TryGetComponent(out handler);

            if (__instance.CanGrab())
                    handler?.OnGrabbed();
        }

        [HarmonyPatch(nameof(ObiRopeGrabArea.EndGrab))]
        [HarmonyPrefix]
        public static void EndGrab(ObiRopeGrabArea __instance)
        {
            ObiRopeGrabAreaHandler handler;

            if (__instance.transform.parent == null)
                __instance.gameObject.TryGetComponent(out handler);
            else
                __instance.transform.parent.TryGetComponent(out handler);

            handler?.OnUngrabbed();
        }
    }
}
