/// <summary>
/// Collection of fixes that required backwards compatibility in Qud game versions at some point. Usually this occurs when 
/// bugfixing Qud-beta changes, to ensure they work on Qud-stable for a short while afterwards.
/// </summary>
namespace CleverGirl {
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
        /// BodyPart used to have a field named "PreferedPrimary" that was corrected to become a property named "PreferredPrimary".
        /// They addressed the typo and changed it into a property instead of a field
        /// <returns>
        /// false if the processed part is NOT the preferred primary weapon, true otherwise
        /// </returns>
        /// </summary>
        public static bool CheckPreferredPrimary(BodyPart part) {
            PropertyInfo new_property = part.GetType().GetProperty("PreferredPrimary");
            if (new_property != null) {
                return (bool)new_property.GetValue(part);
            }
            FieldInfo old_field = part.GetType().GetField("PreferedPrimary");
            if (old_field != null) {
                return (bool)old_field.GetValue(part);
            }

            // This false return might expend the player's action turn regardless if primary limb changed or not, 
            // but the alternative NullReference error is a much worse outcome.
            Utility.MaybeLog("Could not find PreferredPrimary inside BodyPart. Will Assume this is not primary body part.");
            return false;
        }
    }
}