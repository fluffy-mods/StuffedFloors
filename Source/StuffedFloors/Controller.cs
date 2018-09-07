#if DEBUG
#define DEBUG_IMPLIED_DEFS
//#define DEBUG_COSTLIST
//#define DEBUG_STUFFING
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArchitectSense;
using Harmony;
using RimWorld;
using Verse;
using DesignatorUtility = ArchitectSense.DesignatorUtility;

namespace StuffedFloors
{
    public class Controller : Mod
    {
        // use static constructoronstartup for a late init point (after all defs of all mods are loaded).
        [StaticConstructorOnStartup]
        public static class Init
        {
            static Init()
            {
                Initialize();
            }
        }

        private static Controller _instance;
        public static Controller Instance => _instance;
        
        // constructor as an early init point (before any defs are loaded).
        public Controller( ModContentPack content ) : base( content )
        {
            var harmony = HarmonyInstance.Create( "Fluffy.StuffedFloors" );
            harmony.PatchAll( Assembly.GetExecutingAssembly() );
            _instance = this;

            // Patches;
            // HarmonyPatch_GenerateImpliedDefs_PreResolve Prefix to add implied terrains
        }

        public static void Initialize()
        {
            // get all the floortypedefs from the game's library
            var floorTypes = DefDatabase<FloorTypeDef>.AllDefsListForReading;

            // from this list of defs, generate a SubCategoryDef
            floorTypes.ForEach( CreateArchitectSubCategory );

            // hide obsoleted vanilla and modded floors
            foreach (string defName in floorTypes.SelectMany(type => type.obsoletes))
            {
                BuildableDef def = DefDatabase<TerrainDef>.GetNamedSilentFail(defName);
                if ( def != null )
                {
#if DEBUG_HIDE_DEFS
                    Log.Message( $"hiding {def.defName}" );
#endif
                    DesignatorUtility.HideDesignator(def);
                }
            }

            // remove category added by More Floors
            LongEventHandler.QueueLongEvent(() => RemoveObsoleteCategory("MoreFloors"), "StuffedFloors.Initialize", false, null);
        }

        private static void RemoveObsoleteCategory( string categoryName )
        {

            var category = DefDatabase<DesignationCategoryDef>.GetNamedSilentFail(categoryName);
            if (category != null)
            {
                var floorsCategory = DefDatabase<DesignationCategoryDef>.GetNamed("Floors");
                DesignatorUtility.MergeDesignationCategories(floorsCategory, category);
            }
        }

        private static void CreateArchitectSubCategory( FloorTypeDef floorType )
        { 
            DesignationSubCategoryDef subCategoryDef = new DesignationSubCategoryDef();
            subCategoryDef.label = floorType.label;
            subCategoryDef.description = floorType.description;
            subCategoryDef.designationCategory = floorType.designationCategory;
            subCategoryDef.preview = false; // we want the stuff-like selector
            subCategoryDef.emulateStuff = true;

#if DEBUG_IMPLIED_DEFS
            var defs =
                floorType.terrains.Select( td => "\t" + td.defName + "\t" + ( td.blueprintDef?.defName ?? "NULL" ) + "\t" +
                                              ( td.frameDef?.defName ?? "NULL" ) ).ToArray();
            Log.Message( $"Generated defs for {floorType.defName}:\n {String.Join("\n", defs )}" );
#endif

            // poke ArchitectSense
            DesignatorUtility.AddSubCategory( DefDatabase<DesignationCategoryDef>.GetNamed( "Floors" ), subCategoryDef, floorType.terrains );
        }

        private static Dictionary<StuffCategoryDef, List<ThingDef>> _stuffCache = new Dictionary<StuffCategoryDef, List<ThingDef>>();

        public static List<ThingDef> GetStuffDefsFor( List<StuffCategoryDef> stuffCategoryDefs )
        {
            return stuffCategoryDefs.SelectMany( GetStuffDefsFor ).ToList();
        }

        public static List<ThingDef> GetStuffDefsFor( StuffCategoryDef stuffCategory )
        {
            // there doesn't seem to be a good way to get a list of things by stuffType, so we'll have to match the list manually.
            // I really don't want to loop this list for every single floorTypeDef, so let's cache this
            List<ThingDef> stuffs;

            // try get from cache
            if ( _stuffCache.TryGetValue( stuffCategory, out stuffs ) )
                return stuffs;

            // manually fetch list.
            stuffs = DefDatabase<ThingDef>.AllDefsListForReading
                                          .Where( td => td.IsStuff && td.stuffProps.categories.Contains( stuffCategory ) )
                                          .ToList();

            // cache
            _stuffCache.Add( stuffCategory, stuffs );
            return stuffs;
        }
    }
}
