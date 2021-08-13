// Extensions.cs
// Copyright Karel Kroeze, 2017-2017

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace StuffedFloors {
    public static class Extensions {
        public static StatModifier DeepCopy(this StatModifier statModifier) {
            return new StatModifier { stat = statModifier.stat, value = statModifier.value };
        }

        public static List<ThingDefCountClass> DeepCopy(this List<ThingDefCountClass> costList) {
            if (costList is null) {
                return null;
            }

            List<ThingDefCountClass> copy = new();
            foreach (ThingDefCountClass cost in costList) {
                copy.Add(new(cost.thingDef, cost.count));
            }
            return copy;
        }

        public static List<StatModifier> DeepCopy(this List<StatModifier> source) {
            if (source is null) {
                return null;
            }

            List<StatModifier> copy = new();
            foreach (StatModifier stat in source) {
                copy.Add(stat.DeepCopy());
            }
            return copy;
        }

        public static CostListForDifficulty DeepCopy(this CostListForDifficulty source) {
            if (source is null) {
                return null;
            }

            return new() {
                costList = source.costList.DeepCopy(),
                costStuffCount = source.costStuffCount,
                difficultyVar = source.difficultyVar,
                invert = source.invert
            };
        }

        public static List<T> DeepCopy<T>(this List<T> source) where T : Def {
            return source is null ? null : new(source);
        }

    }
}
