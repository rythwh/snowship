namespace Snowship.NLife
{
	public class LifeData
	{
		public string Name;
		public Gender Gender { get; }

		public LifeData(string name, Gender gender) {
			Name = name;
			Gender = gender;
		}
	}
}
