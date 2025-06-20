﻿using System;
using Snowship.NColony;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snowship.NJob
{
	[RegisterJob("Terraform", "Terrain", "Mine")]
	public class MineJobDefinition : JobDefinition<MineJob>
	{
		public override Func<Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.Stone,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public MineJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
			TimeToWork = 30;
		}
	}

	public class MineJob : Job<MineJobDefinition>
	{
		public MineJob(Tile tile) : base(tile) {
			TargetName = Tile.tileType.name;
			Description = $"Mining {Tile.tileType.name}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			foreach (ResourceRange resourceRange in Tile.tileType.resourceRanges) {
				Worker.Inventory.ChangeResourceAmount(resourceRange.resource, Random.Range(resourceRange.min, resourceRange.max + 1), false);
			}
			Tile.SetTileType(
				Tile.HasRoof()
					? TileType.GetTileTypeByEnum(TileType.TypeEnum.Dirt)
					: Tile.biome.tileTypes[TileTypeGroup.TypeEnum.Ground],
				false,
				true,
				true
			);

			GameManager.Get<ColonyManager>().colony.map.RemoveTileBrightnessEffect(Tile);

			foreach (LightSource lightSource in LightSource.lightSources) {
				if (Vector2.Distance(Tile.obj.transform.position, lightSource.obj.transform.position) <= lightSource.prefab.maxLightDistance) {
					lightSource.SetTileBrightnesses();
				}
			}
		}
	}
}