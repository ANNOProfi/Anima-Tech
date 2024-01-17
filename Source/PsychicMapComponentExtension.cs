using Verse;

namespace AnimaTech
{
    public static class PsychicMapComponentExtension
    {
        public static PsychicMapComponent PsychicComp(this Map map)
        {
            if (PsychicMapComponent.localCachedComponent != null && PsychicMapComponent.localCachedComponent.map.uniqueID == map.uniqueID)
            {
                return PsychicMapComponent.localCachedComponent;
            }
            if (PsychicMapComponent.components.TryGetValue(map.uniqueID, out var value))
            {
                PsychicMapComponent.localCachedComponent = value;
            }
            else
            {
                PsychicMapComponent.localCachedComponent = map.GetComponent<PsychicMapComponent>();
            }
            return PsychicMapComponent.localCachedComponent;
        }
    }
}