using System;
using System.Collections.Generic;
using Snowship.NMap.NTile;
using JetBrains.Annotations;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Build", "Build", "Build")]
	public class BuildJobDefinition : JobDefinition<BuildJob>
	{
		public override Func<Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.WalkableIncludingFences,
			Selectable.SelectionConditions.Buildable,
			Selectable.SelectionConditions.NoPlant,
			Selectable.SelectionConditions.NoSameLayerObject,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public override int Layer { get; protected set; } = 100;

		public BuildJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class BuildJobParams : IJobParams
	{
		public List<Func<Tile, int, bool>> SelectionConditions { get; }
		public Sprite JobPreviewSprite { get; private set; }

		[NotNull] public readonly ObjectPrefab ObjectPrefab;
		public readonly int Layer;
		public int Rotation;

		public BuildJobParams(ObjectPrefab objectPrefab) {
			ObjectPrefab = objectPrefab;
			Layer += ObjectPrefab.layer;
			SetRotation(0);

			// SelectionConditions.AddRange(ObjectPrefab.SelectionConditions); // TODO
		}

		public int SetRotation(int rotation) {
			if (!ObjectPrefab.canRotate) {
				UpdateJobPreviewSprite(ObjectPrefab.GetBaseSpriteForVariation(ObjectPrefab.selectedVariation));
				return 0;
			}

			List<Sprite> bitmaskSprites = ObjectPrefab.GetBitmaskSpritesForVariation(ObjectPrefab.selectedVariation);

			Rotation = rotation > bitmaskSprites.Count - 1 ? 0 : rotation;

			UpdateJobPreviewSprite(bitmaskSprites[Rotation]);

			return Rotation;
		}

		public void UpdateJobPreviewSprite() {
			UpdateJobPreviewSprite(ObjectPrefab.GetSpriteFromVariationAndRotation(ObjectPrefab.selectedVariation, Rotation));
		}

		private void UpdateJobPreviewSprite(Sprite sprite) {
			JobPreviewSprite = sprite;
		}
	}

	public class BuildJob : Job<BuildJobDefinition>
	{
		[NotNull] public ObjectPrefab ObjectPrefab { get; }
		public Variation Variation { get; }
		public int Rotation { get; }

		public BuildJob(Tile tile, BuildJobParams args) : base(tile) {
			ObjectPrefab = args.ObjectPrefab;
			Variation = ObjectPrefab.selectedVariation;
			Rotation = args.Rotation;
			Layer = args.Layer;
			Experience = Variation?.timeToBuild ?? ObjectPrefab?.timeToBuild ?? Experience;

			Description = $"Building a {ObjectPrefab.Name}.";
			RequiredResources.AddRange(ObjectPrefab.commonResources);
			if (Variation != null) {
				RequiredResources.AddRange(Variation.uniqueResources);
			}
			SetTimeToWork(ObjectPrefab.timeToBuild);

			SetJobPreviewObject();
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (ObjectPrefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Roofs) {
				Tile.SetRoof(true);
				return;
			}

			ObjectInstance objectInstance = ObjectInstance.CreateObjectInstance(ObjectPrefab, Variation, Tile, Rotation, true);
			Tile.SetObject(objectInstance);
			objectInstance.sr.color = Color.white; // TODO This should be in FinishCreation or SetObject probably?
			if (ObjectPrefab.canRotate) {
				objectInstance.sr.sprite = ObjectPrefab.GetBitmaskSpritesForVariation(Variation)[Rotation];
			}
			objectInstance.FinishCreation();
		}

		private void SetJobPreviewObject() {
			JobPreviewObject.GetComponent<SpriteRenderer>().sprite = ObjectPrefab.GetSpriteFromVariationAndRotation(Variation, Rotation);
			JobPreviewObject.GetComponent<SpriteRenderer>().sortingOrder = (int)SortingOrder.Job + Layer;
		}
	}
}