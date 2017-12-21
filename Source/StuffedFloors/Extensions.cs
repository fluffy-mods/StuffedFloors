// Extensions.cs
// Copyright Karel Kroeze, 2017-2017

using RimWorld;

namespace StuffedFloors
{
    public static class Extensions
    {
        public static StatModifier DeepCopy( this StatModifier statModifier )
        {
            return new StatModifier {stat = statModifier.stat, value = statModifier.value};
        }
    }
}