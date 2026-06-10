using System;
using System.Collections.Generic;

namespace EGG.Stats
{
    [Serializable]
    public struct ScaledStat<TType> where TType : Enum
    {
        public TType Stat;
        public float Coefficient;
        public ScaleMode Mode;

        // Folds the transient modifiers onto the live stat, then scales by the coefficient.
        // Flat contributes stat*coeff; Multiplicative contributes a (1 + stat*coeff) factor.
        public readonly float Resolve(float baseStat, IEnumerable<StatModifier> modifiers)
        {
            float effective = StatMath.Fold(baseStat, modifiers);
            return Mode == ScaleMode.Flat ? effective * Coefficient : 1f + effective * Coefficient;
        }
    }
}
