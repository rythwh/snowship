namespace Snowship.NMaterial
{
	public readonly struct StatModifier
	{
		public EStat Id { get; }
		public EStatOperation Operation { get; }
		public float Value { get; }

		public StatModifier(EStat id, EStatOperation operation, float value)
		{
			Id = id;
			Operation = operation;
			Value = value;
		}
	}
}
