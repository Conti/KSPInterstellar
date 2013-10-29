﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FNPlugin {
    class FNReactor : FNResourceSuppliableModule, FNThermalSource    {
        [KSPField(isPersistant = false)]
        public float ThermalTemp;
        [KSPField(isPersistant = false)]
        public float ThermalPower;
        [KSPField(isPersistant = false)]
        public float upgradedThermalTemp;
        [KSPField(isPersistant = false)]
        public float upgradedThermalPower;
        [KSPField(isPersistant = false)]
        public float upgradedUF6Rate;
        [KSPField(isPersistant = false)]
        public float AntimatterRate;
        [KSPField(isPersistant = false)]
        public float upgradedAntimatterRate;
        [KSPField(isPersistant = false)]
        public float UF6Rate;
		[KSPField(isPersistant = false)]
		public string animName;
        [KSPField(isPersistant = true)]
        public bool IsEnabled = true;
        [KSPField(isPersistant = true)]
        public bool isupgraded = false;
        [KSPField(isPersistant = false)]
        public string upgradedName;
        [KSPField(isPersistant = false)]
        public string originalName;
        [KSPField(isPersistant = false)]
		public float upgradeCost;
		[KSPField(isPersistant = true)]
		public bool breedtritium = false;
		[KSPField(isPersistant = false)]
		public float radius; 
		[KSPField(isPersistant = false)]
		public string upgradeTechReq = null;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Type")]
        public string reactorType;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Temp")]
        public string coretempStr;
		[KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
		public string statusStr;
        //[KSPField(isPersistant = false, guiActive = true, guiName = "Thermal Isp")]
        //public string thermalISPStr;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Upgrade")]
        public string upgradeCostStr;


		[KSPField(isPersistant = false, guiActive = true, guiName = "Tritium")]
		public string tritiumBreedRate;

        [KSPField(isPersistant = true)]
        public float last_active_time;
		[KSPField(isPersistant = true)]
		public float ongoing_consumption_rate;
		[KSPField(isPersistant = true)]
		public bool reactorInit = false;


		protected float antimatter_pcnt;
		protected float uf6_pcnt;

        protected bool hasScience = false;

        protected bool isNuclear = false;
		     

		protected float powerPcnt = 0;

		protected Animation anim;
		protected bool play_down = true;
		protected bool play_up = true;

		protected float tritium_rate = 0;
		protected float tritium_produced_f = 0;

		protected bool hasrequiredupgrade = false;
		protected int deactivate_timer = 0;


        //protected bool responsible_for_thermalmanager = false;
        //protected FNResourceManager thermalmanager;
		       

        [KSPEvent(guiActive = true, guiName = "Activate Reactor", active = false)]
        public void ActivateReactor() {
            if (isNuclear) { return; }
            IsEnabled = true;
        }

		[KSPEvent(guiName = "Repair Reactor", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 2.5f)]
		public void MaintainReactor() {
			if (!isNuclear) { return; }
			IsEnabled = true;
		}

        [KSPEvent(guiActive = true, guiName = "Deactivate Reactor", active = true)]
        public void DeactivateReactor() {
            if (isNuclear) { return; }
            IsEnabled = false;
        }

		[KSPEvent(guiActive = true, guiName = "Enable Tritium Breeding", active = false)]
		public void BreedTritium() {
			if (!isNuclear) { return; }
			breedtritium = true;
		}

		[KSPEvent(guiActive = true, guiName = "Disable Tritium Breeding", active = true)]
		public void StopBreedTritium() {
			if (!isNuclear) { return; }
			breedtritium = false;
		}

        [KSPEvent(guiActive = true, guiName = "Retrofit", active = true)]
        public void RetrofitReactor() {
			if (ResearchAndDevelopment.Instance == null) { return;} 
			if (isupgraded || ResearchAndDevelopment.Instance.Science < upgradeCost) { return; } 
			upgradePart ();
			ResearchAndDevelopment.Instance.Science = ResearchAndDevelopment.Instance.Science - upgradeCost;
          
            //IsEnabled = false;
        }

        [KSPAction("Activate Reactor")]
        public void ActivateReactorAction(KSPActionParam param) {
            if (isNuclear) { return; }
            ActivateReactor();
        }

        [KSPAction("Deactivate Reactor")]
        public void DeactivateReactorAction(KSPActionParam param) {
            if (isNuclear) { return; }
            DeactivateReactor();
        }

        [KSPAction("Toggle Reactor")]
        public void ToggleReactorAction(KSPActionParam param) {
            if (isNuclear) { return; }
            IsEnabled = !IsEnabled;
        }

		public override void OnLoad(ConfigNode node) {


			if (isupgraded) {
				ThermalPower = upgradedThermalPower;
				ThermalTemp = upgradedThermalTemp;
				UF6Rate = upgradedUF6Rate;
				reactorType = upgradedName;
				AntimatterRate = upgradedAntimatterRate;
			}else {
				reactorType = originalName;
			}

			if (UF6Rate > 0) {
				isNuclear = true;

				if (ThermalPower > 0) {
					tritium_rate = ThermalPower/1000.0f/28800.0f;
				}
			}


		}

		public void upgradePart() {
			isupgraded = true;
			ThermalPower = upgradedThermalPower;
			ThermalTemp = upgradedThermalTemp;
			UF6Rate = upgradedUF6Rate;
			AntimatterRate = upgradedAntimatterRate;

			//refreshDependantParts ();

			reactorType = upgradedName;
		}
		     
		public override void OnStart(PartModule.StartState state) {
			String[] resources_to_supply = {FNResourceManager.FNRESOURCE_THERMALPOWER,FNResourceManager.FNRESOURCE_WASTEHEAT};
			this.resources_to_supply = resources_to_supply;

			base.OnStart(state);

            Actions["ActivateReactorAction"].guiName = Events["ActivateReactor"].guiName = String.Format("Activate Reactor");
            Actions["DeactivateReactorAction"].guiName = Events["DeactivateReactor"].guiName = String.Format("Deactivate Reactor");
            Actions["ToggleReactorAction"].guiName = String.Format("Toggle Reactor");
            
            if (state == StartState.Editor) { return; }

			bool manual_upgrade = false;
			if(HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
				if(upgradeTechReq != null) {
					if(PluginHelper.hasTech(upgradeTechReq)) {
						hasrequiredupgrade = true;
					}else if(upgradeTechReq == "none") {
						manual_upgrade = true;
					}
				}else{
					manual_upgrade = true;
				}
			}else{
				hasrequiredupgrade = true;
			}

			if (reactorInit == false) {
				reactorInit = true;
				if(hasrequiredupgrade) {
					upgradePart();
				}
			}



			if(manual_upgrade) {
				hasrequiredupgrade = true;
			}

			anim = part.FindModelAnimators (animName).FirstOrDefault ();
			if (anim != null) {
				anim [animName].layer = 1;
				if (!IsEnabled) {
					anim [animName].normalizedTime = 1f;
					anim [animName].speed = -1f;

				} else {
					anim [animName].normalizedTime = 0f;
					anim [animName].speed = 1f;

				}
				anim.Play ();
			}

            
            this.part.force_activate();

            //print(last_active_time);
            if (IsEnabled && last_active_time != 0) {
                double now = Planetarium.GetUniversalTime();
                double time_diff = now - last_active_time;
                //print(time_diff);
                if (UF6Rate <= 0) {
                    List<PartResource> antimatter_resources = new List<PartResource>();
                    part.GetConnectedResources(PartResourceLibrary.Instance.GetDefinition("Antimatter").id, antimatter_resources);
                    float antimatter_current_amount = 0;
                    foreach (PartResource antimatter_resource in antimatter_resources) {
                        antimatter_current_amount += (float)antimatter_resource.amount;
                    }
                    float antimatter_to_take = (float) Math.Min(antimatter_current_amount, AntimatterRate * time_diff *ongoing_consumption_rate);
                    part.RequestResource("Antimatter", antimatter_to_take);
                    //print(antimatter_to_take);
                }else {
                    List<PartResource> uf6_resources = new List<PartResource>();
                    part.GetConnectedResources(PartResourceLibrary.Instance.GetDefinition("UF6").id, uf6_resources);
                    float uf6_current_amount = 0;
                    foreach (PartResource uf6_resource in uf6_resources) {
                        uf6_current_amount += (float)uf6_resource.amount;
                    }
					float uf6_to_take = (float)Math.Min(uf6_current_amount, UF6Rate * time_diff*ongoing_consumption_rate);
                    part.RequestResource("UF6", uf6_to_take);
                    part.RequestResource("DUF6", -uf6_to_take);

					if(breedtritium) {
						List<PartResource> lithium_resources = new List<PartResource>();
						part.GetConnectedResources(PartResourceLibrary.Instance.GetDefinition("Lithium").id, lithium_resources);
						float lithium_current_amount = 0;
						foreach (PartResource lithium_resource in lithium_resources) {
							lithium_current_amount += (float)lithium_resource.amount;
						}

						List<PartResource> tritium_resources = new List<PartResource>();
						part.GetConnectedResources(PartResourceLibrary.Instance.GetDefinition("Tritium").id, tritium_resources);
						float tritium_missing_amount = 0;
						foreach (PartResource tritium_resource in tritium_resources) {
							tritium_missing_amount += (float)(tritium_resource.maxAmount-tritium_resource.amount);
						}

						float lithium_to_take = (float) Math.Min(tritium_rate*time_diff*ongoing_consumption_rate,lithium_current_amount);
						float tritium_to_add = (float) -Math.Min(tritium_rate*time_diff*ongoing_consumption_rate,tritium_missing_amount);
						part.RequestResource("Lithium",lithium_to_take);
						part.RequestResource("Tritium",tritium_to_add);
					}
                }
            }

			//refreshDependantParts();
        }

        public override void OnUpdate() {
            Events["ActivateReactor"].active = !IsEnabled && !isNuclear;
            Events["DeactivateReactor"].active = IsEnabled && !isNuclear;
			if (ResearchAndDevelopment.Instance != null) {
				Events ["RetrofitReactor"].active = !isupgraded && ResearchAndDevelopment.Instance.Science >= upgradeCost && hasrequiredupgrade;
			} else {
				Events ["RetrofitReactor"].active = false;
			}
			Events["BreedTritium"].active = !breedtritium && isNuclear;
			Events["StopBreedTritium"].active = breedtritium && isNuclear;
			Events ["MaintainReactor"].guiActiveUnfocused = !IsEnabled && isNuclear;
			Fields["upgradeCostStr"].guiActive = !isupgraded && hasrequiredupgrade;
			Fields["tritiumBreedRate"].guiActive = breedtritium && isNuclear;



            coretempStr = ThermalTemp.ToString("0") + "K";
            //thermalISPStr = (Math.Sqrt(ThermalTemp) * 17).ToString("0.0") + "s";

			if (IsEnabled) {
				if (play_up && anim != null) {
					play_down = true;
					play_up = false;
					anim [animName].speed = 1f;
					anim [animName].normalizedTime = 0f;
					anim.Blend (animName, 2f);
				}
			} else {
				if (play_down && anim != null) {
					play_down = false;
					play_up = true;
					anim [animName].speed = -1f;
					anim [animName].normalizedTime = 1f;
					anim.Blend (animName, 2f);
				}
			}

            
			if (ResearchAndDevelopment.Instance != null) {
				upgradeCostStr = ResearchAndDevelopment.Instance.Science + "/" + upgradeCost.ToString ("0") + " Science";
			} 

			tritiumBreedRate = (tritium_produced_f * 86400).ToString ("0.00") + " Kg/day";
            
			if (IsEnabled) {
				if (antimatter_pcnt > 0 || uf6_pcnt > 0) {
					statusStr = "Active (" + powerPcnt.ToString ("0.00") + "%)";
				} else {
					if (isNuclear) {
						statusStr = "UF6 Deprived.";
					}else {
						statusStr = "Antimatter Deprived.";
					}
				}
			} else {
				statusStr = "Reactor Offline.";
			}
        }

        public float getThermalTemp() {
            return ThermalTemp;
        }

        public float getThermalPower() {
            return ThermalPower;
        }

		public bool getIsNuclear() {
			return isNuclear;
		}

		public bool getIsThermalHeatExchanger() {
			return false;
		}

		public float getRadius() {
			return radius;
		}

		public bool isActive() {
			return IsEnabled;
		}

		public void enableIfPossible() {
			if (!isNuclear && !IsEnabled) {
				IsEnabled = true;
			}
		}

		public override void OnFixedUpdate() {
			base.OnFixedUpdate ();

			//print ("Reactor Check-in 1 (" + vessel.GetName() + ")");

            if (UF6Rate > 0) {
                isNuclear = true;
            }

            if (IsEnabled && ThermalPower > 0 && (AntimatterRate >0 || UF6Rate >0)) {
				if (getResourceBarRatio (FNResourceManager.FNRESOURCE_WASTEHEAT) >= 0.95) {

					deactivate_timer++;
					if (deactivate_timer > 3) {
						IsEnabled = false;
						if (FlightGlobals.ActiveVessel == vessel) {
							ScreenMessages.PostScreenMessage ("Warning Dangerous Overheating Detected: Emergency reactor shutdown occuring NOW!", 5.0f, ScreenMessageStyle.UPPER_CENTER);
						}
					}
					return;
				}
				deactivate_timer = 0;

                if (!isNuclear) {
					List<PartResource> antimatter_resources = new List<PartResource>();
					part.GetConnectedResources(PartResourceLibrary.Instance.GetDefinition("Antimatter").id, antimatter_resources);
					double antimatter_current_amount = 0;
					foreach (PartResource antimatter_resource in antimatter_resources) {
						antimatter_current_amount += antimatter_resource.amount;
					}
                    double antimatter_provided = part.RequestResource("Antimatter", Math.Min(AntimatterRate * TimeWarp.fixedDeltaTime,antimatter_current_amount));

                    antimatter_pcnt = (float) (antimatter_provided / AntimatterRate / TimeWarp.fixedDeltaTime);

					double power_to_supply = Math.Max(ThermalPower * TimeWarp.fixedDeltaTime * antimatter_pcnt,0);
					double thermal_power_received = supplyManagedFNResource (power_to_supply, FNResourceManager.FNRESOURCE_THERMALPOWER);
					if (getResourceBarRatio (FNResourceManager.FNRESOURCE_WASTEHEAT) < 0.95) {
						supplyFNResource (thermal_power_received, FNResourceManager.FNRESOURCE_WASTEHEAT); // generate heat that must be dissipated
					}
					double thermal_power_pcnt = thermal_power_received / ThermalPower/TimeWarp.fixedDeltaTime;
					ongoing_consumption_rate = (float) thermal_power_pcnt;
					double return_pcnt = 1-thermal_power_pcnt;
					part.RequestResource("Antimatter", -antimatter_provided*return_pcnt); //return antimatter from <100% power
					powerPcnt = (float) (antimatter_pcnt*100.0f*thermal_power_pcnt);
                }else {
                    double uf6_provided = part.RequestResource("UF6", UF6Rate * TimeWarp.fixedDeltaTime);
                    part.RequestResource("DUF6", -uf6_provided);

                    uf6_pcnt = (float) (uf6_provided / UF6Rate / TimeWarp.fixedDeltaTime);
					double power_to_supply = Math.Max(ThermalPower * TimeWarp.fixedDeltaTime * uf6_pcnt,0);                    
					double thermal_power_received = supplyManagedFNResourceWithMinimum (power_to_supply,0.3f, FNResourceManager.FNRESOURCE_THERMALPOWER);
					if (getResourceBarRatio (FNResourceManager.FNRESOURCE_WASTEHEAT) < 0.95) {
						supplyFNResource (thermal_power_received, FNResourceManager.FNRESOURCE_WASTEHEAT); // generate heat that must be dissipated
					}
					double thermal_power_pcnt = thermal_power_received / ThermalPower/TimeWarp.fixedDeltaTime;
					//print ("TP: " + thermal_power_pcnt);
					ongoing_consumption_rate = (float) thermal_power_pcnt;
					double return_pcnt = 1-thermal_power_pcnt;
					double uf6_returned = part.RequestResource("UF6", -uf6_provided*return_pcnt); //return UF6 from <100% power
					part.RequestResource ("DUF6", -uf6_returned);
					powerPcnt = (float) (uf6_pcnt * 100.0f*thermal_power_pcnt);

					if (breedtritium) {
						float lith_used = part.RequestResource ("Lithium", tritium_rate * TimeWarp.fixedDeltaTime);
						tritium_produced_f = -part.RequestResource ("Tritium", -lith_used) / TimeWarp.fixedDeltaTime;
					}
                }
                if (Planetarium.GetUniversalTime() != 0) {
                    last_active_time = (float) Planetarium.GetUniversalTime();
                }
                
            }

			//print ("Reactor Check-in 2 (" + vessel.GetName() + ")");
            
        }

        public override string GetInfo() {
			if (UF6Rate > 0) {
				float uf6_rate_per_day = UF6Rate * 86400;
				float up_uf6_rate_per_day = upgradedUF6Rate * 86400;
				return String.Format ("Core Temperature: {0}K\n Thermal Power: {1}MW\n UF6 Max Consumption Rate: {2}L/day\n -Upgrade Information-\n Upgraded Core Temperate: {3}K\n Upgraded Power: {4}MW\n Upgraded UF6 Consumption: {5}L/day", ThermalTemp, ThermalPower, uf6_rate_per_day,upgradedThermalTemp,upgradedThermalPower,up_uf6_rate_per_day);
			} else {
				return String.Format ("Core Temperature: {0}K\n Thermal Power: {1}MW\n Antimatter Max Consumption Rate: {2}mg/sec\n -Upgrade Information-\n Upgraded Core Temperature: {3}K\n Upgraded Power: {4}MW\n Upgraded Antimatter Consumption: {5}mg/sec", ThermalTemp, ThermalPower, AntimatterRate,upgradedThermalTemp,upgradedThermalPower,upgradedAntimatterRate);
			}
        }

		public static double getTemperatureofHottestReactor(Vessel vess) {
			List<FNReactor> reactors = vess.FindPartModulesImplementing<FNReactor> ();
			double temp = 0;
			foreach (FNReactor reactor in reactors) {
				if (reactor != null) {
					if (reactor.getThermalTemp () > temp) {
						temp = reactor.getThermalTemp ();
					}
				}
			}
			return temp;
		}

		public static bool hasActiveReactors(Vessel vess) {
			List<FNReactor> reactors = vess.FindPartModulesImplementing<FNReactor> ();
			foreach (FNReactor reactor in reactors) {
				if (reactor != null) {
					if (reactor.IsEnabled) {
						return true;
					}
				}
			}
			return false;
		}


    }
}
