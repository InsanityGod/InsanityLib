using HarmonyLib;
using Vintagestory.API.Client;

namespace InsanityLib.HarmonyPatches.Temporary
{
    /// <summary>
    /// Temporary harmony patches in regards to memory leaks in UI code :P
    /// </summary>
    [HarmonyPatch]
    public static class MemoryLeakFixes
    {
        [HarmonyPatch(typeof(GuiElementDropDown), nameof(GuiElementDropDown.Dispose))]
        [HarmonyPostfix]
        public static void FixGuiElementDropDown(GuiElementDropDown __instance)
        {
            __instance.richTextElem?.Dispose();
        }


        [HarmonyPatch(typeof(GuiElementListMenu), nameof(GuiElementListMenu.Dispose))]
        [HarmonyPostfix]
        public static void FixGuiElementList(GuiElementListMenu __instance)
        {
            var texts = Traverse.Create(__instance).Field<GuiElementRichtext[]>("richtTextElem").Value;
            if(texts is not null)
            {
                foreach (var item in texts)
                {
                    item.Dispose();
                }
            }

            var switches = Traverse.Create(__instance).Field<GuiElementSwitch[]>("switches").Value;

            if(switches is not null)
            {
                foreach(var item in switches)
                {
                    item.Dispose();
                }
            }
        }
    }
}
