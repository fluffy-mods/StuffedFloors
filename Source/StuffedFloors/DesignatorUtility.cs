// DesignatorUtility.cs
// Copyright Karel Kroeze, 2018-2018

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace StuffedFloors
{
    public static class DesignatorUtility
    {
        public static void RemoveDesignators( IEnumerable<TerrainDef> terrains )
        {
            HashSet<DesignationCategoryDef> affectedCategories = new HashSet<DesignationCategoryDef>();
            foreach ( var terrain in terrains )
            {
                affectedCategories.Add(terrain.designationCategory);

                terrain.designatorDropdown = null;
                terrain.designationCategory = null;
            };

            foreach ( var affectedCategory in affectedCategories )
                RecacheDesignationCategory( affectedCategory );
        }

        public static void MergeDesignationCategories( DesignationCategoryDef target, DesignationCategoryDef source )
        {
            // change designation category for all build designators in source
            foreach ( var designator in source.AllResolvedDesignators )
                if ( designator is Designator_Build build )
                    build.PlacingDef.designationCategory = target;

            // add specials that don't exist in target yet
            foreach ( var designator in source.specialDesignatorClasses )
                if ( !target.specialDesignatorClasses.Contains( designator ) )
                    target.specialDesignatorClasses.Add( designator );

            // recache target
            RecacheDesignationCategory( target );

            // remove source
            RemoveDesignationCategory( source );
        }

        private static void RemoveDesignationCategory( DesignationCategoryDef category )
        {
            ( DefDatabase<DesignationCategoryDef>.AllDefs as List<DesignationCategoryDef> )?.Remove( category );
            RecacheDesignationCategories();
        }

        private static void RecacheDesignationCategory( DesignationCategoryDef category )
        {
            Traverse.Create( category ).Method( "ResolveDesignators" ).GetValue();
        }

        private static void RecacheDesignationCategories()
        {
            Traverse.Create(MainButtonDefOf.Architect.TabWindow).Method("CacheDesPanels").GetValue();
        }
    }
}