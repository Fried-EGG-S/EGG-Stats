using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using EGG.Serialization;

namespace EGG.Stats
{
    [Serializable]
    public class Stat
    {
        [field: SerializeField] public float BaseValue { get; private set; }
        [field: SerializeField] public float CachedValue { get; private set; }
        [SerializeField] private bool _isDirty = true;

        [SerializeField] private SerializedDictionary<SerializedGUID, StatModifier> _modifiers = new();

        public event Action OnValueChanged;

        public Stat(float baseValue)
        {
            BaseValue = baseValue;
            CachedValue = BaseValue;
            _isDirty = true;
        }

        public float Value
        {
            get
            {
                if (!_isDirty) return CachedValue;

                float value = BaseValue;

                float flatIncrease = 0f;
                float additivePercent = 1f;
                float multiplicativeTotal = 1f;
                float? overrideValue = null;

                foreach (var mod in _modifiers.Values)
                {
                    switch (mod.Type)
                    {
                        case StatModType.Flat:
                            flatIncrease += mod.Value;
                            break;

                        case StatModType.Additive:
                            additivePercent += mod.Value;
                            break;

                        case StatModType.Multiplicative:
                            multiplicativeTotal *= mod.Value;
                            break;

                        case StatModType.Override:
                            overrideValue = mod.Value;
                            break;
                    }
                }

                value += flatIncrease;
                value *= additivePercent;
                value *= multiplicativeTotal;
                CachedValue = Mathf.Max(overrideValue ?? value, 0);
                _isDirty = false;

                return CachedValue;
            }
        }
        public Guid AddModifier(StatModifier modifier)
        {
            _modifiers[modifier.Id] = modifier;
            MarkDirty();
            return modifier.Id;
        }
        public bool RemoveModifier(Guid id)
        {
            if (!_modifiers.Remove(id)) return false;

            MarkDirty();
            return true;
        }
        public bool TryGetModifier(Guid id, out StatModifier modifier)
        {
            return _modifiers.TryGetValue(id, out modifier);
        }
        public bool TryUpdateModifier(Guid id, float newValue)
        {
            if (!_modifiers.TryGetValue(id, out var mod)) return false;

            mod.Value = newValue;
            _modifiers[id] = mod;
            MarkDirty();
            return true;
        }
        public void ClearModifiers()
        {
            if (_modifiers.Count == 0) return;
            _modifiers.Clear();
            MarkDirty();
        }
        public void MarkDirty()
        {
            _isDirty = true;
            OnValueChanged?.Invoke();
        }
    }
}