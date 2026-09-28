using System;
using System.Collections.Generic;
using Snowship.NEntity;
using Snowship.NMap.NTile;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("EntityBuild", "EntityBuild", "EntityBuild")]
	public class EntityBuildJobDefinition : JobDefinition<EntityBuildJob>
	{
		public override Func<Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			NSelection.SelectionConditions.WalkableIncludingFences,
			NSelection.SelectionConditions.Buildable,
			NSelection.SelectionConditions.NoPlant,
			NSelection.SelectionConditions.NoSameLayerObject,
			NSelection.SelectionConditions.NoSameLayerJobs
		};

		public override int Layer { get; protected set; } = 100;

		public EntityBuildJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class EntityBuildJobParams : IJobParams
	{
		public List<Func<Tile, int, bool>> SelectionConditions { get; } = new();
		public Sprite JobPreviewSprite { get; private set; }

		public readonly JsonEntity JsonEntity;
		public int Rotation;

		public EntityBuildJobParams(JsonEntity jsonEntity) {
			JsonEntity = jsonEntity;
			JobPreviewSprite = jsonEntity.Icon;
		}

		public int SetRotation(int rotation) {
			Rotation = rotation;
			return Rotation;
		}
	}

	public class EntityBuildJob : Job<EntityBuildJobDefinition>
	{
		public JsonEntity JsonEntity { get; }
		public int Rotation { get; }
		public Entity Entity { get; private set; }

		public EntityBuildJob(Tile tile, EntityBuildJobParams args) : base(tile) {
			JsonEntity = args.JsonEntity;
			Rotation = args.Rotation;
			
			Description = $"Building a {JsonEntity.Name}.";
		}

		public void InitializeEntity(Entity entity) {
			this.Entity = entity;
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();
			
			if (Entity != null) {
				Entity.Remove<BlueprintMarkerComponent>();
			}
		}
	}
}
