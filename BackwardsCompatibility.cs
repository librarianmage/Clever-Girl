/// <summary>
/// Collection of fixes that required backwards compatibility in Qud game versions at some point. Usually this occurs when 
/// bugfixing Qud-beta changes, to ensure they work on Qud-stable for a short while afterwards.
/// </summary>
namespace XRL.World.CleverGirl {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using XRL.Rules;
    using XRL.UI;
    using XRL.World.Anatomy;
    using XRL.World.Parts;
    using XRL.World.Parts.Mutation;
    using ConsoleLib.Console;
    
    public static class BackwardsCompatibility {

        /// <summary>
        /// Qud Version [2.0.206.8]:
        /// BaseMutation was changed such that SetVariant accepted string typed parameters instead of integer
        /// </summary>
        public static void RandomizeMutationVariant(BaseMutation mutation, Random seededRandom) {
            List<string> variants = mutation.GetVariants();
            if (variants != null && variants.Count > 0) {
                int variantIndex = seededRandom.Next(variants.Count);
                Utility.MaybeLog("BaseMutation.SetVariant(...): '" + variants[variantIndex] + "' (index: " + variantIndex + ")");

                MethodInfo method = null;

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