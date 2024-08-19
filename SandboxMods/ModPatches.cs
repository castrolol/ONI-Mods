using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using PeterHan.PLib.Options;
using FreeResourceBuildings;
using System.IO;
using Jint;
using Jint.Runtime.Modules;

namespace FreeResourceBuildingsPatches
{

    // ----------------------------------------------------------------------------

    public class Mod : KMod.UserMod2
    {
        internal static ModConfig Options { get; private set; }

        public override void OnLoad(Harmony harmony)
        {
            harmony.PatchAll();

            // Registering for the config screen
            new POptions().RegisterOptions(this, typeof(ModConfig));

            // Read Settings
            Options = POptions.ReadSettings<ModConfig>() ?? new ModConfig();
        }
    }

    public class FreeEnergyPatch
    {

        private static void SetTechIfNeeded(string id, string early, string mid, string late)
        {
            var howToGetIt = Mod.Options.HowToGetTheseItems;

            switch (howToGetIt)
            {
                case CostOptions.JustSandbox:
                case CostOptions.EasyAndFree:
                case CostOptions.Easy:
                    return;
                case CostOptions.EarlyGame:
                    Db.Get().Techs.Get(early).unlockedItemIDs.Add(id);
                    break;
                case CostOptions.MidGAme:
                    Db.Get().Techs.Get(mid).unlockedItemIDs.Add(id);
                    break;
                case CostOptions.LateGame:
                    Db.Get().Techs.Get(late).unlockedItemIDs.Add(id);
                    break;
            }
        }

        public static void UnloackAllTechs()
        {



        }



        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix(ref List<System.Type> types)
            {
                types = types.Where(x => x != typeof(ProgrammableUnitConfig)).ToList();
            }

            public static void Postfix()
            {
                var engine = new Jint.Engine(options =>
                {
                    options.Modules.ModuleLoader = new VirtualModuleLoader();
                });


                engine.Modules.Add("main", File.ReadAllText(@"C:\Users\luan_\Documents\dev\oni\scripts\teste.js"));


                var main = engine.Modules.Import("main");


                var info = main.Get("info");

                var id = info.Get("id").AsString();
                var name = info.Get("name").AsString();
                var inputs = (int)info.Get("inputs").AsNumber();
                var outputs = (int)info.Get("outputs").AsNumber();





                var prog = new ProgrammableUnitConfig();
                prog._ID = $"ProgrammableCpu{id}";// "Teste1111";

                BuildingConfigManager.Instance.RegisterBuilding(prog);


                AddStrings(prog._ID, name, "", "");

                ModUtil.AddBuildingToPlanScreen(new HashedString("Base"), prog._ID, "uncategorized");


            }


            private static void AddStrings(string ID, string Name, string Description, string Effect)
            {
                Strings.Add(new string[]{
                        "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".NAME", Name
                });

                Strings.Add(new string[]{
                        "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".DESC",Description
                });

                Strings.Add(new string[]{
                        "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".EFFECT", Effect
                });

            }
        }


        //[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        //public static class Db_Initialize_Patch
        //{

        //    public static void Postfix()
        //    {
        //        var options = Mod.Options;




        //        Strings.Add(new string[]{
        //            "STRINGS.BUILDING.STATUSITEMS.COLLECTINGFREEHEP.NAME", "Generating Radbolts ({x}/cycle)"
        //        });
        //        Strings.Add(new string[]{
        //            "STRINGS.BUILDING.STATUSITEMS.COLLECTINGFREEHEP.TOOLTIP", "Generate Radbolts for free"
        //        });

        //        Strings.Add(new string[]{
        //            "STRINGS.UI.UISIDESCREENS.HEATERSLIDER.TEMP.TITLE", "DTUs produced"
        //    });

        //        Strings.Add(new string[]{
        //            "STRINGS.UI.UISIDESCREENS.HEATERSLIDER.TEMP.TOOLTIP", "Change the DTUs produced"
        //    });


        //    }

        //    private static void AddStrings(string ID, string Name, string Description, string Effect)
        //    {
        //        Strings.Add(new string[]{
        //            "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".NAME", Name
        //    });

        //        Strings.Add(new string[]{
        //            "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".DESC",Description
        //    });

        //        Strings.Add(new string[]{
        //            "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".EFFECT", Effect
        //    });

        //    }
        //}


        //[HarmonyPatch(typeof(FilterSideScreen))]
        //[HarmonyPatch(nameof(FilterSideScreen.IsValidForTarget))]
        //public class FilterSideScreen_IsValidForTarget
        //{
        //    private static bool Prefix(GameObject target, FilterSideScreen __instance, ref bool __result)
        //    {
        //        if (target.GetComponent<FlowSlider>() != null)
        //        {
        //            __result = !__instance.isLogicFilter;
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(HighEnergyParticleDirectionSideScreen), nameof(HighEnergyParticleDirectionSideScreen.IsValidForTarget))]
        //class HighEnergyParticleDirectionSideScreen_IsValidForTarget
        //{
        //    static void Postfix(GameObject target, ref bool __result)
        //    {
        //        if (!__result)
        //        {
        //            if (target.GetComponent<FreeRadbolt>() != null)
        //            {
        //                __result = true;
        //            }
        //        }

        //    }
        //}


        //    [HarmonyPatch(typeof(SideScreenContent), nameof(SideScreenContent.GetSideScreenSortOrder))]
        //    public class SideScreenContent_GetSideScreenSortOrder_Patch
        //    {
        //        public static void Postfix(SideScreenContent __instance, ref int __result)
        //        {
        //            if (__instance is FilterSideScreen)
        //            {
        //                __result = 2;
        //            }
        //            else if (__instance is IntSliderSideScreen)
        //            {
        //                __result = 1;
        //            }
        //        }
        //    }

        //}

        //[HarmonyPatch(typeof(AgeMonitor), "TimeToDie")]
        //public class Game_OnSpawn_Patch
        //{


        //    public static void Postfix(ref AgeMonitor.Instance smi, AgeMonitor __instance, ref bool __result)
        //    {

        //        if (__result == true)
        //        {

        //            //var ageMonitor = Traverse.Create(__instance);


        //            // var agingAM = ageMonitor.Field<Klei.AI.AttributeModifier>("aging").Value;
        //            smi.RandomizeAge();


        //            // private AttributeModifier aging;

        //            __result = false;
        //        }

        //    }

        //}


        //[HarmonyPatch(typeof(ResearchScreen), "Update")]
        //public class Game_OnSpawn_Patch
        //{
        //    static bool inited = false;

        //    public static void Postfix()
        //    {
        //        //if (CustomGameSettings.Instance.GetCurrentQualitySetting(StartWithAllResearchPatches.StartWithAllResearch).id != "Enabled")
        //        //    return;

        //        if (Game_OnSpawn_Patch.inited) return;

        //        foreach (TechItem tech in Db.Get().TechItems.resources)
        //        {
        //            if (!tech.IsComplete())
        //            {
        //                TechInstance ti = Research.Instance.Get(tech.ParentTech);
        //                ti.Purchased();
        //                Game.Instance.Trigger((int)GameHashes.ResearchComplete, (object)tech.ParentTech);
        //            }
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(MinionResume), "AddExperience")]
        //public static class EntityTemplates_ExtendEntityToWildCreature
        //{
        //    public static void Prefix(ref float amount)
        //    {

        //        amount *= 1000f;

        //    }
        //}

        //[HarmonyPatch(typeof(ResearchScreen), "Update")]
        //public class Game_OnSpawn_Patch
        //{
        //    static bool inited = false;

        //    public static void Postfix()
        //    {
        //        if (Game_OnSpawn_Patch.inited) return;

        //        foreach (TechItem tech in Db.Get().TechItems.resources)
        //        {

        //            if (!tech.IsComplete())
        //            {

        //                TechInstance ti = Research.Instance.Get(tech.ParentTech);

        //                ti.Purchased();

        //                Game.Instance.Trigger((int)GameHashes.ResearchComplete, (object)tech.ParentTech);
        //            }
        //        }

        //        Game_OnSpawn_Patch.inited = true;
        //    }
        //}

        //[HarmonyPatch(typeof(ResearchEntry), "ResearchCompleted")]
        //public class ResearchEntry_ResearchCompleted_Patch
        //{
        //    public static void Prefix(ref bool notify)
        //    {
        //        Debug.Log("@@@@  ResearchCompleted_Prefix TESTEEE");
        //        //if (CustomGameSettings.Instance.GetCurrentQualitySetting(StartWithAllResearchPatches.StartWithAllResearch).id == "Enabled")
        //        notify = false;
        //    }
        //}


    }

}