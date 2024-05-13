/*
namespace CleverGirl {
    using HarmonyLib;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using XRL.World;

    public class CleverGirl_INoSavePart : IPart { }

    // hide any INoSaveParts so GameObject doesn't try to save them; restore after save
    [HarmonyPatch(typeof(GameObject), "Save", new Type[] { typeof(SerializationWriter) })]
    public static class GameObject_Save_Patch {
        private static List<CleverGirl_INoSavePart> cachedParts;
        public static void Prefix(GameObject __instance) {
            cachedParts = __instance.GetPartsDescendedFrom<CleverGirl_INoSavePart>();
            __instance.RemovePartsDescendedFrom<CleverGirl_INoSavePart>();
        }
        public static void Postfix(GameObject __instance) {
            if (cachedParts != null) {
                foreach (var part in cachedParts) {
                    __instance.PartsList.Add(part);
                }
                cachedParts = null;
            }
        }
    }

    // attach all the parts we didn't save based on whether they have their corresponding property
    [HarmonyPatch(typeof(GameObject), "Load", new Type[] { typeof(SerializationReader) })]
    public static class GameObject_Load_Patch {
        private static List<Type> classes;
        public static void Postfix(GameObject __instance) {
            if (classes == null) {
                classes = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(x => x.GetTypes())
                            .Where(x => typeof(CleverGirl_INoSavePart).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                            .ToList();
                Utility.MaybeLog("INoSavePart Load Patch_1: " + classes.Count);
            }
            foreach (var clazz in classes) {
                string prop = clazz.GetProperty("PROPERTY")?.GetValue(null) as string ?? "";
                Utility.MaybeLog("INoSavePart Load Patch_2: " + prop);
                if (prop != "" && __instance.HasProperty(prop)) {
                    Utility.MaybeLog("INoSavePart Load Patch_3: true");
                    _ = __instance.AddPart(Activator.CreateInstance(clazz) as IPart);
                }
            }
        }
    }
}
*/