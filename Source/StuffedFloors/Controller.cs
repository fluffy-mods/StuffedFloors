using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using ArchitectSense;
using HugsLib;
using RimWorld;
using Verse;
using static StuffedFloors.ImpliedDefGenerator;

namespace StuffedFloors
{
    public class Controller : ModBase
    {
        public override string ModIdentifier => "StuffedFloors";

        public override void Initialize()
        {
            // get all the floortypedefs from the game's library
            var floorTypes = DefDatabase<FloorTypeDef>.AllDefsListForReading;

            // for each type, generate a set of terrainDefs
            floorTypes.ForEach( GenerateTerrainDefs );
        }

        public void GenerateTerrainDefs( FloorTypeDef floorType )
        {
            // for each allowed stuff, generate a TerrainDef
            var terrainDefs = GetStuffDefsFor( floorType.stuffCategories ).Select( floorType.GetStuffedTerrainDef ).ToList();
            
            // note that this happens _after_ defs are loaded and implied defs are generated, so we still need to do that manually.
            GenerateAndStoreImpliedThingDefs( terrainDefs );
            
            // add terrainDefs to the defDatabase
            DefDatabase<TerrainDef>.Add( terrainDefs );
            
            // from this list of defs, generate a SubCategoryDef
            CreateArchitectSubCategory( floorType, terrainDefs );
        }

        private static void CreateArchitectSubCategory( FloorTypeDef floorType, List<TerrainDef> terrainDefs )
        { 
            DesignationSubCategoryDef subCategoryDef = new DesignationSubCategoryDef();
            subCategoryDef.label = floorType.label;
            subCategoryDef.designationCategory = floorType.designationCategory;
            subCategoryDef.preview = false; // we want the stuff-like selector

            // poke ArchitectSense
            ArchitectSense.Controller.AddSubCategory( DefDatabase<DesignationCategoryDef>.GetNamed( "Floors" ), subCategoryDef, terrainDefs.ToList() );
        }

        private Dictionary<StuffCategoryDef, List<ThingDef>> _stuffCache = new Dictionary<StuffCategoryDef, List<ThingDef>>();

        public List<ThingDef> GetStuffDefsFor( List<StuffCategoryDef> stuffCategoryDefs )
        {
            return stuffCategoryDefs.SelectMany( GetStuffDefsFor ).ToList();
        }

        public List<ThingDef> GetStuffDefsFor( StuffCategoryDef stuffCategory )
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
