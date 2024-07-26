using UnityEngine;
using Verse;

namespace AnimaTech
{
    public class AnimaTechMod : Mod
    {
        private AT_Settings settings;

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

            listing_Standard.End();
        }

        public override string SettingsCategory()
        {
            return "AnimaTech";
        }
    }
}