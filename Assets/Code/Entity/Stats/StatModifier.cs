namespace Snowship.NEntity
{
	public readonly struct StatModifier
	{
		public EStat Id { get; }
		public EStatOp Op { get; }
		public float Value { get; }

		public StatModifier(EStat id, EStatOp op, float value)
		{
			Id = id;
			Op = op;
			Value = value;
		}
	}
}
