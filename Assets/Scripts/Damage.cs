namespace StrategyEngine.Modifiers
{
    public enum ModifierType
    {
        Flat,
        Percent 
    }

    [System.Serializable]
    public struct DamageModifier
    {
        public string SourceId;      
        public ModifierType Type;    
        public float Value;

        public DamageModifier(string sourceId, ModifierType type, float value)
        {
            SourceId = sourceId;
            Type = type;
            Value = value;
        }
    }
}