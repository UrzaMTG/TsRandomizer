﻿using System;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using TsRandomizer.Extensions;
using TsRandomizer.IntermediateObjects;
using TsRandomizer.ItemTracker;
using TsRandomizer.Randomisation;

namespace TsRandomizer.Archipelago
{
	class ArchipelagoItemLocationMap : ItemLocationMap
	{
		readonly ItemInfoProvider itemInfoProvider;
		public GameSave GameSave { get; private set; }

		public ItemKey[] CheckedLocations { get; set; }

		public ArchipelagoItemLocationMap(ItemInfoProvider itemInfoProvider, ItemUnlockingMap itemUnlockingMap, SeedOptions options) : base(itemInfoProvider, itemUnlockingMap, options)
		{
			this.itemInfoProvider = itemInfoProvider;
		}

		public override bool IsBeatable() => true;

		public override void Initialize(GameSave gameSave)
		{
			GameSave = gameSave;

			base.Initialize(gameSave);

			foreach (var locationKey in CheckedLocations)
				this[locationKey].SetPickedUp();

			foreach (var itemIdentifier in Client.GetReceivedItems())
				RecieveItem(itemIdentifier, null);
		}

		public override void Update(Level level)
		{
			foreach (var itemIdentifier in Client.GetReceivedItems())
				RecieveItem(itemIdentifier, level);
		}

		public override ProgressionChain GetProgressionChain()
		{
			throw new InvalidOperationException("Progression chains arent supported for Archipelago seeds");
		}

		void RecieveItem(ItemIdentifier itemIdentifier, Level level)
		{
			var item = itemInfoProvider.Get(itemIdentifier);
			item.OnPickup(level);

			if (item.IsProgression)
			{
				Add(new ExteralItemLocation(item));
				ItemTrackerUplink.UpdateState(ItemTrackerState.FromItemLocationMap(this));
			}

			level?.ShowItemAwardPopup(itemIdentifier);
		}
	}
}
