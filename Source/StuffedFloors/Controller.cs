#if DEBUG
#define DEBUG_HIDE_DEFS
#endif

using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void DefsLoaded()
        {
            // get all the floortypedefs from the game's library
            var floorTypes = DefDatabase<FloorTypeDef>.AllDefsListForReading;

            // for each type, generate a set of terrainDefs
            floorTypes.ForEach( GenerateTerrainDefs );

            // hide some vanilla and modded floors
            HideObsoleteFloors();
        }
        
        public string[] obsoleteFloorDefs = new[]
                                            {
                                                // vanilla stone tiles
                                                "TileSandstone",
                                                "TileGranite",
                                                "TileLimestone",
                                                "TileSlate",
                                                "TileMarble",

                                                // vanilla wood plank
                                                "WoodPlankFloor",

                                                // extended woodworking
                                                "WoodPlankFloor_Oak",
                                                "WoodPlankFloor_Poplar",
                                                "WoodPlankFloor_Pine",
                                                "WoodPlankFloor_Birch",
                                                "WoodPlankFloor_Cecropia",
                                                "WoodPlankFloor_Teak",
                                                "WoodPlankFloor_Red",
                                                "WoodPlankFloor_Green",
                                                "WoodPlankFloor_Blue",
                                                "WoodPlankFloor_Yellow",
                                                "WoodPlankFloor_White",
                                                "WoodPlankFloor_Black",

                                                // [T]More Floors - stone
                                                "FloorStoneSlabsSandstone",
                                                "FloorStoneSlabsGranite",
                                                "FloorStoneSlabsLimestone",
                                                "FloorStoneSlabsSlate",
                                                "FloorStoneSlabsMarble",
                                                "FloorStoneRoughSandstone",
                                                "FloorStoneRoughGranite",
                                                "FloorStoneRoughLimestone",
                                                "FloorStoneRoughSlate",
                                                "FloorStoneRoughMarble",
                                                "FloorStoneHexSandstone",
                                                "FloorStoneHexGranite",
                                                "FloorStoneHexLimestone",
                                                "FloorStoneHexSlate",
                                                "FloorStoneHexMarble",
                                                "FloorStoneMosaicSandstone",
                                                "FloorStoneMosaicGranite",
                                                "FloorStoneMosaicLimestone",
                                                "FloorStoneMosaicSlate",
                                                "FloorStoneMosaicMarble",
                                                "FloorStoneRandomSandstone",
                                                "FloorStoneRandomGranite",
                                                "FloorStoneRandomLimestone",
                                                "FloorStoneRandomSlate",
                                                "FloorStoneRandomMarble",
                                                "FloorStoneChequerMarbleSlate",
                                                "FloorStoneChequerMarbleSandstone",
                                                "FloorStoneChequerMarbleGranite",

                                                // [T]More Floors - wood
                                                "FloorWood1",
                                                "FloorWood2",
                                                "FloorWood3",
                                                "FloorWood4",
                                                "FloorWood5",
                                                "FloorWood6",

                                                // Minerals and Materials
                                                "TileYellow",
                                                "TileGreen",
                                                "TilePurple",
                                                "WoodPlankDark",
                                                "WoodPlankLight",
                                                "WoodPlankRose",
                                                "WoodPlankWalnut"
                                            };

        public void HideObsoleteFloors()
        {
            List<DesignationCategoryDef> affectedCategories = new List<DesignationCategoryDef>();

            foreach ( string defName in obsoleteFloorDefs )
            {
                var terrain = DefDatabase<TerrainDef>.GetNamedSilentFail( defName );
                if ( terrain != null )
                {
                    affectedCategories.Add( terrain.designationCategory );
                    terrain.menuHidden = true;
                    terrain.designationCategory = null;
                    terrain.designationHotKey = null;
                }
            }

            foreach ( DesignationCategoryDef cat in affectedCategories.Where( cat => cat != null ) )
            {
                // try re-resolving designators.
                cat.ResolveReferences();
            }
        }

        public void GenerateTerrainDefs( FloorTypeDef floorType )
        {
            // for each allowed stuff, generate a TerrainDef
            var terrainDefs = GetStuffDefsFor( floorType.stuffCategories ).Select( floorType.GetStuffedTerrainDef ).ToList();
            
            // we need to manually create implied defs (blueprint / frame).
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
