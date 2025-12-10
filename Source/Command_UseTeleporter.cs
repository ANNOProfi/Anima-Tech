using RimWorld;
using Verse;

namespace AnimaTech
{
    public class Command_UseTeleporter : Command_Action
    {
        protected CompTeleporter teleporter;

        public Command_UseTeleporter(CompTeleporter teleporter)
        {
            this.teleporter = teleporter;

            if(!teleporter.Active)
            {
                Disable("AT_NotEnoughFocus");
            }
        }
    }
}