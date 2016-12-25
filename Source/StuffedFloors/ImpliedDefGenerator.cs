using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace StuffedFloors
{
    public static class ImpliedDefGenerator
    {

        private static readonly MethodInfo ImpliedBlueprintMethodInfo =
            typeof( ThingDefGenerator_Buildings ).GetMethod( "NewBlueprintDef_Terrain",
                                                             BindingFlags.Static | BindingFlags.NonPublic );
        private static readonly MethodInfo ImpliedFrameMethodInfo =
            typeof( ThingDefGenerator_Buildings ).GetMethod( "NewFrameDef_Terrain",
                                                             BindingFlags.Static | BindingFlags.NonPublic );
        private static readonly MethodInfo GiveShortHashMethodInfo =
            typeof( ShortHashGiver ).GetMethod( "GiveShortHash", BindingFlags.Static | BindingFlags.NonPublic );

        public static ThingDef GetFrameDef( TerrainDef terrainDef )
        {
            if ( ImpliedFrameMethodInfo == null )
                throw new Exception( "MethodInfo for implied frame ThingDef generator not found." );

            return ImpliedFrameMethodInfo.Invoke( null, new object[] { terrainDef } ) as ThingDef;
        }

        public static ThingDef GetBlueprintDef( TerrainDef terrainDef )
        {
            if ( ImpliedBlueprintMethodInfo == null )
                throw new Exception( "MethodInfo for implied blueprint ThingDef generator not found." );

            return ImpliedBlueprintMethodInfo.Invoke( null, new object[] { terrainDef } ) as ThingDef;
        }

        public static void GiveShortHash( this Def def )
        {
            if ( GiveShortHashMethodInfo == null )
                throw new Exception( "MethodInfo for short hash setting is null" );

            GiveShortHashMethodInfo.Invoke( null, new object[] {def} );
        }

        public static void GenerateAndStoreImpliedThingDefs( List<TerrainDef> terrainDefs )
        {
            foreach ( TerrainDef terrainDef in terrainDefs )
            {
                // frame
                ThingDef frameDef = GetFrameDef( terrainDef );
                frameDef.GiveShortHash();
                frameDef.PostLoad();
                DefDatabase<ThingDef>.Add( frameDef );

                // blueprint
                if ( terrainDef.designationCategory != null )
                {
                    ThingDef blueprintDef = GetBlueprintDef( terrainDef );
                    blueprintDef.GiveShortHash();
                    blueprintDef.PostLoad();
                    DefDatabase<ThingDef>.Add( blueprintDef );
                }
            }
        }
    }
}
