using System;

namespace EGG.Stats
{
    [Serializable]
    public class StatModifier
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public float Value;
        public StatModType Type;

        public StatModifier(float value, StatModType type)
        {
            Id = Guid.NewGuid();
            Value = value;
            Type = type;
        }
    }
}