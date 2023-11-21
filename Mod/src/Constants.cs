namespace XRL.World.CleverGirl {
    using XRL;
    using static XRL.UI.Options;

    [HasOptionFlagUpdate]
    public static partial class Static {
        public static bool OptionDebug { get; private set; }

        [OptionFlagUpdate]
        public static void UpdateFlags() {
            bool OptionDebugBefore = OptionDebug;
            OptionDebug = GetOption("OptionCleverGirlDebug", "Yes").EqualsNoCase("Yes");
            if (OptionDebug != OptionDebugBefore) {
                MetricsManager.LogInfo("Changed Clever Girl debug logging to : " + OptionDebug);
            }
        }
    }
}