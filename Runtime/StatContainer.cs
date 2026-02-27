using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EGG.Stats
{
    [Serializable]
    public class StatContainer<TType> where TType : Enum
    {
        [SerializedDictionary("Stat Type", "Stat Value")]
        public SerializedDictionary<TType, Stat> Stats = new();
        private readonly Dictionary<TType, float> _defaultStats;


        public StatContainer(Dictionary<TType, float> statData) : this(statData, new Dictionary<TType, float>()) { }
        public StatContainer(Dictionary<TType, float> statData, Dictionary<TType, float> defaultStats)
        {
            Stats = new SerializedDictionary<TType, Stat>();
            foreach (var kvp in statData)
            {
                AddStat(kvp.Key, kvp.Value);
            }
            _defaultStats = defaultStats;
        }
        private void AddStat(TType type, float baseValue)
        {
            if (!Stats.ContainsKey(type))
            {
                var stat = new Stat(baseValue);
                Stats[type] = stat;
            }
        }

        public void RefreshAllCachedValues()
        {
            foreach (var stat in Stats.Values)
            {
                stat.MarkDirty();
            }
        }
        public void ListenToStatChange(TType type, Action callback)
        {
            if (Stats.ContainsKey(type))
            {
                Stats[type].OnValueChanged += callback;
                return;
            }
            Debug.LogWarning($"Stats does not contain {type}");
        }
        public void StopListeningToStatChange(TType type, Action callback)
        {
            if (Stats.ContainsKey(type))
            {
                Stats[type].OnValueChanged -= callback;
                return;
            }
            Debug.LogWarning($"Stats does not contain {type}");
        }

        public float GetStat(TType type)
        {
            if (Stats.TryGetValue(type, out var stat)) return stat.Value;
            else if (_defaultStats.TryGetValue(type, out var value)) return value;
            Debug.LogError($"StatContainer: Stat of type {type} not found after trying default stats.");
            return 0f;
        }

        #region Modifiers
        public Guid AddModifier(TType type, StatModifier mod)
        {
            if (!Stats.ContainsKey(type)) AddStat(type, _defaultStats.TryGetValue(type, out var value) ? value : 0);

            return Stats[type].AddModifier(mod);
        }

        public bool UpdateModifierValue(TType type, Guid id, float newValue)
        {
            if (!Stats.TryGetValue(type, out var stat))
            {
                Debug.LogError($"StatContainer: Stat {type} not found when trying to update modifier");
                return false;
            }

            bool result = stat.TryUpdateModifier(id, newValue);

            // Force re-assignment to ensure SerializedDictionary recognizes the change
            Stats[type] = stat;

            return result;
        }
        public void RemoveModifier(TType type, Guid id)
        {
            if (Stats.TryGetValue(type, out var stat))
            {
                stat.RemoveModifier(id);
                Stats[type] = stat; // Force re-assignment
            }
            else Debug.Log("E");
        }
        public void ClearStatModifiers(TType type)
        {
            if (Stats.TryGetValue(type, out var stat))
            {
                stat.ClearModifiers();
                Stats[type] = stat; // Force re-assignment
            }
        }
        public void ClearModifiers()
        {
            foreach (var kvp in Stats)
            {
                kvp.Value.ClearModifiers();
                Stats[kvp.Key] = kvp.Value; // Force re-assignment
            }
        }
        #endregion
    }
}