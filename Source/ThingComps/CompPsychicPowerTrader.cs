using Verse;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;
using System;

namespace AnimaTech
{
    public class CompPsychicPowerTrader : CompPsychicGenerator
    {
        public CompPowerTrader powerTrader;

        public CompPsychicUser userComp;

        protected override float DesiredFocusGeneration => PowerTrade;

        public bool powerToFocus = false;

        public bool focusToPower = true;

        private float generationRate = 0f;

        private float consumptionRate = 0f;

        private float outputRate = 0f;

        private float energyGainRateCached = 0f;

        private float energyStoredCached = 0f;

        private float focusStoredCached = 0f;

        private float networkBalanceCached = 0f;

        private float energyMaximumCached = 0f;

        private float focusMaximumCached = 0f;

        public float conversionFactor = AT_Utilities.Settings.conversionFactor; //Conversion Power to Focus

        public bool automaticMode = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            powerTrader = parent.GetComp<CompPowerTrader>();
            userComp = parent.GetComp<CompPsychicUser>();

            base.PostSpawnSetup(respawningAfterLoad);
        }

        private float PowerTrade
        {
            get
            {
                energyStoredCached = powerTrader.PowerNet.CurrentStoredEnergy();

                energyMaximumCached = CalculateMaximumEnergy(powerTrader.PowerNet.batteryComps);

                focusStoredCached = pylonComp.Network.focusTotal;

                focusMaximumCached = pylonComp.Network.focusCapacity;

                energyGainRateCached = powerTrader.PowerNet.CurrentEnergyGainRate()/1.6666667E-05f;
                
                networkBalanceCached = pylonComp.Network.FocusBalance;

                if(automaticMode)
                {
                    if(powerTrader.PowerNet != null && pylonComp.Network != null)
                    {
                        CalculateFocusPowerTrade(energyGainRateCached, networkBalanceCached);
                    }
                }
                else
                {
                    CalculateFocusPowerTradeFixed();
                }

                powerTrader.PowerOutput = outputRate;

                userComp.FocusConsumption = consumptionRate;

                return generationRate;
            }
        }

        public float CalculateMaximumEnergy(List<CompPowerBattery> batteries)
        {
            float maximum = 0f;

            if(batteries.NullOrEmpty())
            {
                return maximum;
            }

            foreach(CompPowerBattery battery in batteries)
            {
                maximum += battery.Props.storedEnergyMax;
            }

            return maximum;
        }

        public void CalculateFocusPowerTradeFixed()
        {
            if(powerToFocus)
            {
                consumptionRate = 0f;

                if(energyStoredCached > 0f || energyGainRateCached > 0f)
                {
                    generationRate = Props.baseGenerationRate;
                }
                else
                {
                    generationRate = 0f;
                }
                outputRate = -generationRate / conversionFactor;
            }
            
            if(focusToPower)
            {
                generationRate = 0f;

                if(focusStoredCached > 0f || networkBalanceCached > 0f)
                {
                    consumptionRate = userComp.Props.baseFocusConsumption;
                }
                else
                {
                    consumptionRate = 0f;
                }

                outputRate = consumptionRate / conversionFactor;
            }
        }

        public void CalculateFocusPowerTrade(float gainRate, float focusExcess)
        {
            
            float neutralizedGainRate = gainRate - outputRate;

            float neutralizedFocusExcess = focusExcess - generationRate + consumptionRate;

            if(neutralizedGainRate < 0f && neutralizedFocusExcess > 0f)
            {
                generationRate = 0f;

                while((outputRate < ((-neutralizedGainRate)+1f)) && (consumptionRate < (neutralizedFocusExcess-5f)))
                {
                    outputRate++;

                    consumptionRate = outputRate * conversionFactor;
                }
            }
            else if(neutralizedGainRate > 0f && neutralizedFocusExcess < 0f)
            {
                consumptionRate = 0f;

                while((generationRate < ((-neutralizedFocusExcess)+1f)) && ((-outputRate) < (neutralizedGainRate-10f)))
                {
                    generationRate++;

                    outputRate = -(generationRate / conversionFactor);
                }
            }
            else if(energyMaximumCached > 0f && energyStoredCached < (energyMaximumCached * 0.05))
            {
                generationRate = 0f;

                if(neutralizedFocusExcess > 0f)
                {
                    if(neutralizedFocusExcess <= userComp.Props.baseFocusConsumption)
                    {
                        consumptionRate = neutralizedFocusExcess-1f;
                    }
                    else
                    {
                        consumptionRate = userComp.Props.baseFocusConsumption;
                    }
                }
                else
                {
                    consumptionRate = 0f;
                }

                outputRate = consumptionRate / conversionFactor;
            }
            else if(focusMaximumCached > 0f && focusStoredCached < (focusMaximumCached * 0.05))
            {
                consumptionRate = 0f;

                if(neutralizedGainRate > 0f)
                {
                    if(neutralizedGainRate <= Props.baseGenerationRate*conversionFactor)
                    {
                        generationRate = (neutralizedGainRate*conversionFactor)-1f;
                    }
                    else
                    {
                        generationRate = Props.baseGenerationRate;
                    }
                }
                else
                {
                    generationRate = 0f;
                }
                outputRate = -generationRate / conversionFactor;
            }
            else
            {
                consumptionRate = 0f;

                generationRate = 0f;

                outputRate = 0f;
            }
        }

        public override string CompInspectStringExtra()
        {
            string automatic = base.CompInspectStringExtra();
            if(automaticMode)
            {
                automatic = automatic + " automatic";
            }
            else
            {
                automatic = automatic + " not automatic";

                if(focusToPower)
                {
                    automatic = automatic + ", Current Mode: Focus to Power";
                }
                else
                {
                    automatic = automatic + ", Current Mode: Power to Focus";
                }
            }

            return automatic;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_Action
            {
                defaultLabel = "Toggle direction",
                defaultDesc = "AT_GeneratorToggleImbuementDesc".Translate(),
                action = delegate
                {
                    focusToPower = !focusToPower;
                    powerToFocus = !powerToFocus;
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Toggle automatic",
                defaultDesc = "AT_GeneratorToggleImbuementDesc".Translate(),
                action = delegate
                {
                    automaticMode = !automaticMode;
                }
            };
        }
    }
}