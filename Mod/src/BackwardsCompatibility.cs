/// <summary>
/// Collection of fixes that required backwards compatibility in Qud game versions at some point. Usually this occurs when 
/// bugfixing Qud-beta changes, to ensure they work on Qud-stable for a short while afterwards.
/// </summary>
namespace XRL.World.CleverGirl {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using XRL.World.Anatomy;
    using XRL.World.Parts.Mutation;
    using Qud.API;

    public static class BackwardsCompatibility {

        /// <summary>
        /// Qud Version [2.0.206.19]:
        /// Some enums from JournalAccomplishment were moved into Qud.API
        /// Ended up cutting a couple enums from the arguement list for now, as I couldn't figure out how to grab 2 sets of enums cleanly
        /// Please restore the MuralCategory/MuralWeight default parameters once these stable Qud merges these enums into Qud.API
        /// </summary>
        public static void AddInspiredDishAccomplishment(string text, string muralText) {
            // JournalAPI.AddAccomplishment(description, muralCategory: MuralCategory.CreatesSomething, muralWeight: MuralWeight.Low);
            JournalAPI.AddAccomplishment(text, muralText);
        }

        /// <summary>
        /// Qud Version [2.0.206.19]:
        /// CyberneticsTerminal was refactored with newer more consistent field names.
        /// PLEASE remove this backwards compatibility class ASAP, I dislike this hack of an implementation very much!
        /// </summary>
        public class CyberneticsTerminal {
            // Any known renames of fields in order from newest -> oldest
            private static readonly List<string> Known_Terminal = new List<string>{ "Terminal", "terminal" };
            private static readonly List<string> Known_Subjects = new List<string>{ "Subject", "obj" };
            private static readonly List<string> Known_Credits = new List<string>{ "Credits", "nCredits" };
            private static readonly List<string> Known_Selected = new List<string>{ "Selected", "nSelected" };

            private XRL.UI.CyberneticsTerminal terminal;

            public CyberneticsTerminal(XRL.UI.CyberneticsTerminal terminal) {
                this.terminal = terminal;
            }

            public CyberneticsTerminal(XRL.UI.CyberneticsScreenRemove screen) {
                this.terminal = GetTerminal(screen);
            }

            public static XRL.UI.CyberneticsTerminal GetTerminal(XRL.UI.CyberneticsScreenRemove screen) {

                // Technically field is in a base class, so flatten the hierarchy
                if (XRL.UI.CyberneticsTerminal.instance != null) {
                    return XRL.UI.CyberneticsTerminal.instance;
                }

                Utility.MaybeLog("Could not find CyberneticsTerminal. This could be critical?");
                return null;
            }


            public GameObject GetSubject() {
                foreach (var field in Known_Subjects) {
                    FieldInfo prop = terminal.GetType().GetField(field);
                    if (prop != null) {
                        return (GameObject)prop.GetValue(terminal);
                    }
                }
                Utility.MaybeLog("Could not find Subject field in CyberneticsTerminal. This could be critical?");
                return null;
            }

            public void AddCredits(int value) {
                foreach (var field in Known_Credits) {
                    FieldInfo prop = terminal.GetType().GetField(field);
                    if (prop != null) {
                        prop.SetValue(terminal, (int)prop.GetValue(terminal) + value);
                        return;
                    }
                }
                Utility.MaybeLog("Could not find Credits field in CyberneticsTerminal. This could be critical?");
            }

            public int GetSelected() {
                foreach (var field in Known_Selected) {
                    FieldInfo prop = terminal.GetType().GetField(field);
                    if (prop != null) {
                        return (int)prop.GetValue(terminal);
                    }
                }
                Utility.MaybeLog("Could not find Selected field in CyberneticsTerminal. This could be critical?");
                return 0;
            }
        }

        /// <summary>
        /// Qud Version [2.0.206.8]:
        /// BaseMutation was changed such that SetVariant accepted string typed parameters instead of integer
        /// </summary>
        public static void RandomizeMutationVariant(BaseMutation mutation, Random seededRandom) {
            List<string> variants = mutation.GetVariants();
            if (variants != null && variants.Count > 0) {
                int variantIndex = seededRandom.Next(variants.Count);
                Utility.MaybeLog("BaseMutation.SetVariant(...): '" + variants[variantIndex] + "' (index: " + variantIndex + ")");

                MethodInfo method;

                // Look for new SetVariant() method
                method = typeof(BaseMutation).GetMethod("SetVariant", new Type[] { typeof(string) });
                if (method != null) {
                    method.Invoke(mutation, new object[] { variants[variantIndex] });
                    return;
                }

                // Look for old SetVariant() method
                method = typeof(BaseMutation).GetMethod("SetVariant", new Type[] { typeof(int) });
                if (method != null) {
                    method.Invoke(mutation, new object[] { variantIndex });
                    return;
                }

                Utility.MaybeLog("Could not find a valid method to SetVariant of selected mutation.");
            }
        }

        /// <summary>
        /// Qud Version [2.0.204.65]:
        /// BodyPart had a typo in one of its field names that was fixed by devs.
        /// <returns>
        /// false if the processed part is NOT the preferred primary weapon, true otherwise
        /// </returns>
        /// </summary>
        public static bool CheckPreferredPrimary(BodyPart part) {
            FieldInfo prop = part.GetType().GetField("PreferredPrimary") ??
                             part.GetType().GetField("PreferedPrimary");
            if (prop == null) {
                Utility.MaybeLog("Could not find PreferredPrimary field in BodyPart. This could be critical?");

                // This return will expend the player's action turn when it might not need to, but the potential NullReference error
                // codepath below is arguably worse.
                return false;
            }
            return (bool)prop.GetValue(part);
        }
    }
}