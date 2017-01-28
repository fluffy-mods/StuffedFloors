using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
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
        public int stuffCost;

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
            TerrainDef terrain = new TerrainDef
            {
                acceptFilth = acceptFilth,
                acceptTerrainSourceFilth = acceptTerrainSourceFilth,
                affordances = affordances.NullOrEmpty() 
                                ? new List<TerrainAffordance>() 
                                : new List<TerrainAffordance>( affordances ),
                avoidWander = avoidWander,
                altitudeLayer = altitudeLayer,
                changeable = changeable,
                driesTo = driesTo,
                designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( "Floors" ),
                edgeType = edgeType,
                fertility = fertility,
                holdSnow = holdSnow,
                layerable = layerable,
                menuHidden = menuHidden,
                passability = passability,
                pathCost = pathCost,
                pathCostIgnoreRepeat = pathCostIgnoreRepeat,
                placeWorkers = placeWorkers.NullOrEmpty() 
                                ? new List<Type>() 
                                : new List<Type>( placeWorkers ),
                placingDraggableDimensions = placingDraggableDimensions,
                renderPrecedence = renderPrecedence,
                repairEffect = repairEffect,
                researchPrerequisites = researchPrerequisites.NullOrEmpty() 
                                            ? new List<ResearchProjectDef>() 
                                            : new List<ResearchProjectDef>( researchPrerequisites ),
                scatterType = scatterType,
                smoothedTerrain = smoothedTerrain,
                specialDisplayRadius = specialDisplayRadius,
                statBases = statBases.NullOrEmpty() ? new List<StatModifier>() : new List<StatModifier>( statBases ),
                texturePath = texturePath,
                takeFootprints = takeFootprints,
                terrainFilthDef = terrainFilthDef,
                terrainAffordanceNeeded = terrainAffordanceNeeded
            };
            
            // apply stuff elements
            StuffProperties stuff = stuffThingDef.stuffProps;
            terrain.color = stuff.color;
            terrain.label = "ThingMadeOfStuffLabel".Translate( stuffThingDef.LabelAsStuff, label );
            terrain.description = description;
            terrain.defName = stuffThingDef.defName + "_" + defName;
            terrain.costList = new List<ThingCountClass>();
            if (!costList.NullOrEmpty())
                foreach ( ThingCountClass cost in costList )
                    terrain.costList.Add( new ThingCountClass( cost.thingDef, cost.count ) );
            if (stuffCost > 0)
                terrain.costList.Add( new ThingCountClass( stuffThingDef, (int)( stuffCost / stuffThingDef.VolumePerUnit ) ) );

#if DEBUG_IMPLIED_DEFS
            Log.Message($"Created {terrain.defName} from {stuffThingDef.defName}");
#endif

            // TODO: Implement stuff stat offsets

            // we need to assign hashes
            terrain.GiveShortHash();

            // done!
            terrain.PostLoad();
            return terrain;
        }
    }
}
