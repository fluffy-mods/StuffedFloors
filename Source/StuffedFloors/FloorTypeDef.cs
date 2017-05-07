#if DEBUG
//#define DEBUG_STUFFING
//#define DEBUG_IMPLIED_DEFS
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace StuffedFloors
{
    public class FloorTypeDef : TerrainDef
    {
        // TerrainDef basically contains all the fields we need, so just derive from that.
        // note that we're hardwiring stuffed terrains to the Floors category, that way we
        // can leave designationCategory on null for the floorType, and we don't have to deal
        // with the game adding the category placeholder itself as a buildable floor. This is 
        // not exactly a perfect solution, and should be refactored at some point in the (near)
        // future.

        // the stuffcost that will be used for all the generated terrains. Note that this is in addition 
        // to the floorTypeDef's base cost.
        public int stuffCost;

        // a list of vanilla or modded terrains that are made obsolete by this category and should be removed.
        // Note that we're using strings instead of defs so that we can manually resolve references, and allow 
        // for obsoleting of defs regardless of wether the mods they belong to are actually active.
        public List<string> obsoletes = new List<string>();
        
        // a list of terrains that have been generated for this floorTypeDef
        public List<TerrainDef> terrains = new List<TerrainDef>();

        // list of relevant stats that should be affected by stuff
        public List<StatDef> statsAffectedByStuff = new List<StatDef>();

        public override IEnumerable<string> ConfigErrors()
        {
            List<string> errors = base.ConfigErrors()?.ToList() ?? new List<string>();
            if (designationCategory != null)
                errors.Add( "FloorTypeDef should not have designationCategory set (hardcoded to Floors)" );

            return errors;
        }

        public TerrainDef GetStuffedTerrainDef( ThingDef stuffThingDef )
        {
            if ( !stuffThingDef.IsStuff )
                throw new ArgumentException( stuffThingDef.defName + " is not a stuff!" );
            
            // create new terrain
            TerrainDef terrain = new TerrainDef();
            terrain.acceptFilth = acceptFilth;
            terrain.acceptTerrainSourceFilth = acceptTerrainSourceFilth;
            terrain.affordances = affordances.NullOrEmpty()
                                      ? new List<TerrainAffordance>()
                                      : new List<TerrainAffordance>( affordances );
            terrain.avoidWander = avoidWander;
            terrain.altitudeLayer = altitudeLayer;
            terrain.buildingPrerequisites = buildingPrerequisites.NullOrEmpty()
                                                ? new List<ThingDef>()
                                                : new List<ThingDef>( buildingPrerequisites );
            terrain.burnedDef = burnedDef;
            terrain.changeable = changeable;
            terrain.driesTo = driesTo;
            terrain.designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( "Floors" );
            terrain.edgeType = edgeType;
            terrain.fertility = fertility;
            terrain.holdSnow = holdSnow;
            terrain.layerable = layerable;
            terrain.menuHidden = menuHidden;
            terrain.passability = passability;
            terrain.pathCost = pathCost;
            terrain.pathCostIgnoreRepeat = pathCostIgnoreRepeat;
            terrain.placeWorkers = placeWorkers.NullOrEmpty()
                                       ? new List<Type>()
                                       : new List<Type>( placeWorkers );
            terrain.placingDraggableDimensions = placingDraggableDimensions;
            terrain.renderPrecedence = renderPrecedence;
            terrain.researchPrerequisites = researchPrerequisites.NullOrEmpty()
                                                ? new List<ResearchProjectDef>()
                                                : new List<ResearchProjectDef>( researchPrerequisites );
            terrain.resourcesFractionWhenDeconstructed = resourcesFractionWhenDeconstructed;
            terrain.scatterType = scatterType;
            terrain.smoothedTerrain = smoothedTerrain;
            terrain.specialDisplayRadius = specialDisplayRadius;
            terrain.statBases = statBases.NullOrEmpty() ? new List<StatModifier>() : new List<StatModifier>( statBases );
            terrain.tags = tags.NullOrEmpty() ? new List<string>() : new List<string>( tags );
            terrain.takeFootprints = takeFootprints;
            terrain.takeSplashes = takeSplashes;
            terrain.terrainFilthDef = terrainFilthDef;
            terrain.terrainAffordanceNeeded = terrainAffordanceNeeded;
            terrain.texturePath = texturePath;


            // apply stuff elements
            StuffProperties stuff = stuffThingDef.stuffProps;
            terrain.color = stuff.color;
            terrain.constructEffect = stuff.constructEffect;
            terrain.repairEffect = stuff.constructEffect;
            terrain.label = "ThingMadeOfStuffLabel".Translate( stuffThingDef.LabelAsStuff, label );
            terrain.description = description;
            terrain.defName = stuffThingDef.defName + "_" + defName;
            terrain.costList = new List<ThingCountClass>();
            if (!costList.NullOrEmpty())
                foreach ( ThingCountClass cost in costList )
                    terrain.costList.Add( new ThingCountClass( cost.thingDef, cost.count ) );
            if (stuffCost > 0)
                terrain.costList.Add( new ThingCountClass( stuffThingDef, Mathf.CeilToInt( stuffCost / stuffThingDef.VolumePerUnit ) ) );

#if DEBUG_IMPLIED_DEFS
            Log.Message($"Created {terrain.defName} from {stuffThingDef.defName}");
#endif
            
            // apply stuff offsets and factors, but apply them to a new list of statmodifiers, re-using the same list 
            // keeps the actual statmodifier entries around as references, and leads to exponentially increasing stats 
            // for terrains of the same base def and different stuffs
            if ( !statsAffectedByStuff.NullOrEmpty() )
            {
                // prepare variables
                var stats = new List<StatModifier>();
                StringBuilder text = new StringBuilder();

                foreach ( StatDef stat in statsAffectedByStuff )
                {
                    // get base/default value
                    float value = terrain.statBases.GetStatValueFromList( stat, stat.defaultBaseValue );
                    text.AppendLine( $"Base {stat.label} for {terrain.label}: {value}" );

                    // apply offset
                    float offset = stuff.statOffsets.GetStatOffsetFromList( stat );

                    // apply factor
                    float factor = ( value >= 0 || stat.applyFactorsIfNegative )
                                       ? stuff.statFactors.GetStatFactorFromList( stat )
                                       : 1f;

                    // lower impact of stuff beauty on floors
                    if ( stat == StatDefOf.Beauty )
                    {
                        offset *= 1 / 3f;
                        factor = Mathf.Sqrt( factor );
                    }
                    
                    // calculate new value
                    float final = ( value + offset ) * factor;
                    text.AppendLine( $"\tstuffed: ({value} + {offset}) x {factor} = {final}" );

                    StatUtility.SetStatValueInList( ref stats, stat, final );
                }

#if DEBUG_STUFFING
                Log.Message( text.ToString() );
#endif

                // asign the stats, overwriting the statBases list
                terrain.statBases = stats;
            }

            //// we need to assign hashes
            //terrain.GiveShortHash();

            //// done!
            //terrain.PostLoad();
            return terrain;
        }
    }
}
