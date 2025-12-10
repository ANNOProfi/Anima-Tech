using UnityEngine;
using Verse;
using HarmonyLib;

namespace AnimaTech
{
    public class AnimaTechMod : Mod
    {
        public static AT_Settings settings;

        private string intervalBuffer = 60.ToString();

        private string conversionBuffer = 0.5f.ToString();

        public AnimaTechMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AT_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();

            listing_Standard.Begin(inRect);

            listing_Standard.Gap(10f);
            listing_Standard.CheckboxLabeled("AnimaTech.Settings.InteractiveRunes".Translate(), ref settings.useInteractiveRunes, "AnimaTech.Settings.InteractiveRunesDesc".Translate());

            if(DebugSettings.godMode)
            {
                listing_Standard.Gap(10f);
                listing_Standard.Label("Tick interval".Translate());
                listing_Standard.TextFieldNumeric(ref settings.tickInterval, ref intervalBuffer, 1f, 6000);

                listing_Standard.Gap(10f);
                listing_Standard.Label("Conversion factor (Power to focus)".Translate());
                listing_Standard.TextFieldNumeric(ref settings.conversionFactor, ref conversionBuffer, 0.1f, 100);

                listing_Standard.End();
            }

            listing_Standard.End();
        }

        public override string SettingsCategory()
        {
            return "AnimaTech";
        }
    }
}