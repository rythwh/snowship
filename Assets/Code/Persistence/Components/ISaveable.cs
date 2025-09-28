namespace Snowship.Persistence
{
	public interface ISaveable
	{
		string Id { get; set; }

		// Return a plain DTO (serializable to JSON) that contains only primitives/structs/UID strings.
		object Save(SaveContext context);

		// Restore fields from the DTO. Do not resolve references here—stash referenced UIDs and resolve in OnAfterLoad.
		void Load(LoadContext context, object data, int typeVersion);

		// Called after all objects are created/loaded. Resolve cross-references here safely.
		void OnAfterLoad(LoadContext context);
	}
}
