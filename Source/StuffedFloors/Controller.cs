#if DEBUG
//#define DEBUG_HIDE_DEFS
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
        [StaticConstructorOnStartup]
        public static class Init
        {
            static Init()
            {
                Initialize();
            }
        }

        public Controller( ModContentPack content ) : base( content )
        {
            var harmony = HarmonyInstance.Create( "Fluffy.StuffedFloors" );
            harmony.PatchAll( Assembly.GetExecutingAssembly() );
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
            var moreFloorsCategory = DefDatabase<DesignationCategoryDef>.GetNamedSilentFail("MoreFloors");
            if (moreFloorsCategory != null)
            {
                var floorsCategory = DefDatabase<DesignationCategoryDef>.GetNamed("Floors");
                DesignatorUtility.MergeDesignationCategories(floorsCategory, moreFloorsCategory);
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
