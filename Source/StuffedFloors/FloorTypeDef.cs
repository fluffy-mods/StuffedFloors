#if DEBUG
#define DEBUG_IMPLIED_DEFS
//#define DEBUG_COSTLIST
#define DEBUG_STUFFING
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

        private DesignatorDropdownGroupDef _designatorGroup;
        public DesignatorDropdownGroupDef designatorGroup
        {
            get
            {
                if ( _designatorGroup == null )
                {
                    // create and register group.
                    _designatorGroup = new DesignatorDropdownGroupDef();
                    _designatorGroup.defName = defName;
                    DefDatabase<DesignatorDropdownGroupDef>.Add( _designatorGroup );
                }
                return _designatorGroup;
            }
        }

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
            
            // label it as ours
            terrain.modContentPack = Controller.Instance.Content;

            // assign to designator group
            terrain.designatorDropdown = designatorGroup;
            
            // copy properties. Everything that could potentially be relevant.
            terrain.affordances = affordances.NullOrEmpty()
                                      ? new List<TerrainAffordanceDef>()
                                      : new List<TerrainAffordanceDef>( affordances );
            terrain.avoidWander = avoidWander;
            terrain.altitudeLayer = altitudeLayer;
            terrain.artisticSkillPrerequisite = artisticSkillPrerequisite;
            terrain.blueprintDef = blueprintDef;
            terrain.buildingPrerequisites = buildingPrerequisites.NullOrEmpty()
                                                ? new List<ThingDef>()
                                                : new List<ThingDef>( buildingPrerequisites );
            terrain.burnedDef = burnedDef;
            terrain.changeable = changeable;
            terrain.clearBuildingArea = clearBuildingArea;
            terrain.constructionSkillPrerequisite = constructionSkillPrerequisite;
            terrain.driesTo = driesTo;
            terrain.destroyBuildingsOnDestroyed = destroyBuildingsOnDestroyed;
            terrain.destroyEffect = destroyEffect;
            terrain.destroyEffectWater = destroyEffectWater;
            terrain.destroyOnBombDamageThreshold = destroyOnBombDamageThreshold;
            terrain.defaultPlacingRot = defaultPlacingRot;
            terrain.designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( "Floors" );
            terrain.edgeType = edgeType;
            terrain.extinguishesFire = extinguishesFire;
            terrain.extraDeteriorationFactor = extraDeteriorationFactor;
            terrain.extraDraftedPerceivedPathCost = extraDraftedPerceivedPathCost;
            terrain.extraNonDraftedPerceivedPathCost = extraNonDraftedPerceivedPathCost;
            terrain.fertility = fertility;
            terrain.filthAcceptanceMask = filthAcceptanceMask;
            terrain.frameDef = frameDef;
            terrain.generatedFilth = generatedFilth;
            terrain.holdSnow = holdSnow;
            terrain.layerable = layerable;
            terrain.maxTechLevelToBuild = maxTechLevelToBuild;
            terrain.minTechLevelToBuild = minTechLevelToBuild;
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
            terrain.terrainAffordanceNeeded = terrainAffordanceNeeded;
            terrain.texturePath = texturePath;
            terrain.tools = tools.NullOrEmpty() ? new List<Tool>() : new List<Tool>( tools );
            terrain.traversedThought = traversedThought;
            terrain.waterDepthMaterial = waterDepthMaterial;
            terrain.waterDepthShader = waterDepthShader;
            terrain.waterDepthShaderParameters = waterDepthShaderParameters.NullOrEmpty()
                ? new List<ShaderParameter>()
                : new List<ShaderParameter>( waterDepthShaderParameters );
            
            // apply stuff elements
            StuffProperties stuff = stuffThingDef.stuffProps;
            terrain.color = stuff.color;
            terrain.constructEffect = stuff.constructEffect;
            terrain.repairEffect = stuff.constructEffect;
            terrain.label = "ThingMadeOfStuffLabel".Translate( stuffThingDef.LabelAsStuff, label );
            terrain.description = description;
            terrain.defName = stuffThingDef.defName + "_" + defName;
            terrain.costList = new List<ThingDefCountClass>();
            if (!costList.NullOrEmpty())
                foreach ( var cost in costList )
                    terrain.costList.Add( new ThingDefCountClass( cost.thingDef, cost.count ) );
            if (stuffCost > 0)
                terrain.costList.Add( new ThingDefCountClass( stuffThingDef, Mathf.CeilToInt( stuffCost / stuffThingDef.VolumePerUnit ) ) );


            // apply stuff offsets and factors, but apply them to a new list of statmodifiers, re-using the same list 
            // keeps the actual statmodifier entries around as references, and leads to exponentially increasing stats 
            // for terrains of the same base def and different stuffs
            if ( !statsAffectedByStuff.NullOrEmpty() )
            {
                // prepare variables
                var stats = new List<StatModifier>( statBases.Select( sb => sb.DeepCopy() ) );
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
                
#if DEBUG_IMPLIED_DEFS
                Log.Message($"Created {terrain.defName} from {stuffThingDef.defName}");
#if DEBUG_COSTLIST
                foreach (ThingCountClass count in terrain.costList)
                {
                    Log.Message($"\t{count.thingDef.defName}: {count.count}");
                }
#endif
#if DEBUG_STUFFING
                Log.Message( text.ToString() );
#endif
#endif

                // asign the stats, overwriting the statBases list
                terrain.statBases = stats;
            }
            
            return terrain;
        }
    }
}
