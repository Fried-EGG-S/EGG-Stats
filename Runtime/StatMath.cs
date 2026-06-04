using System.Collections.Generic;
using UnityEngine;

namespace EGG.Stats
{
    public static class StatMath
    {
        public static float Fold(float baseValue, IEnumerable<StatModifier> modifiers)
        {
            var fold = new StatFold();
            if (modifiers != null)
            {
                foreach (var modifier in modifiers) fold.Add(modifier.Value, modifier.Type);
            }
            return fold.Resolve(baseValue);
        }
    }

    public struct StatFold
    {
        private float _flat;
        private float _additivePercent;
        private float _multiplicative;
        private float? _override;
        private bool _initialized;

        public void Add(float value, StatModType type)
        {
            EnsureInitialized();
            switch (type)
            {
                case StatModType.Flat: _flat += value; break;
                case StatModType.Additive: _additivePercent += value; break;
                case StatModType.Multiplicative: _multiplicative *= value; break;
                case StatModType.Override: _override = value; break;
            }
        }

        public readonly float Resolve(float baseValue)
        {
            if (!_initialized) return Mathf.Max(baseValue, 0f);

            float value = (baseValue + _flat) * _additivePercent * _multiplicative;
            return Mathf.Max(_override ?? value, 0f);
        }

        // default(StatFold) zeroes the percent/product fields; seed them on first Add so an unused fold resolves to base.
        private void EnsureInitialized()
        {
            if (_initialized) return;

            _additivePercent = 1f;
            _multiplicative = 1f;
            _initialized = true;
        }
    }
}
