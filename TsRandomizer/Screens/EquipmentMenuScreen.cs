﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using TsRandomizer.Archipelago;
using TsRandomizer.Archipelago.Gifting;
using TsRandomizer.Extensions;
using TsRandomizer.IntermediateObjects;
using TsRandomizer.Randomisation;
using TsRandomizer.Screens.Menu;

namespace TsRandomizer.Screens
{
	[TimeSpinnerType("Timespinner.GameStateManagement.Screens.PauseMenu.EquipmentMenuScreen")]
	// ReSharper disable once UnusedMember.Global
	class EquipmentMenuScreen : Screen
	{
		const int DummyTeam = -999;
		const int NumberOfTraitsToDisplay = 7;

		static readonly Type EquipmentMenuScreenType =
			TimeSpinnerType.Get("Timespinner.GameStateManagement.Screens.PauseMenu.EquipmentMenuScreen");
		static readonly Type StatCollectionType =
			TimeSpinnerType.Get("Timespinner.GameStateManagement.Screens.BaseClasses.Menu.StatCollection");
		static readonly Type StatEntryType =
			TimeSpinnerType.Get("Timespinner.GameStateManagement.Screens.BaseClasses.Menu.StatEntry");
		static readonly Type StatEntryDisplayEnumType =
			TimeSpinnerType.Get("Timespinner.GameStateManagement.Screens.BaseClasses.Menu.StatEntry+EStatDisplayType");
		static readonly Type MenuUseItemInventoryType =
			TimeSpinnerType.Get("Timespinner.GameStateManagement.Screens.BaseClasses.Menu.MenuUseItemInventory");
		static readonly Type ConfirmationMenuEntryCollectionType =
			TimeSpinnerType.Get("Timespinner.GameStateManagement.ConfirmationMenuEntryCollection");

		static readonly Color StatEntryColor = new Color(240, 240, 208);

		readonly bool isUsedAsGiftingMenu;
		GCM gameContentManager;
		GameSave save;

		GiftingService giftingService;
		List<AcceptedTraits> acceptedTraitsPerSlot = new List<AcceptedTraits>();

		InventoryItem selectedItem;
		AcceptedTraits selectedPlayer;
		dynamic confirmMenuCollection;

		dynamic playerInfoCollection;
		
		public EquipmentMenuScreen(ScreenManager screenManager, GameScreen gameScreen) : base(screenManager, gameScreen)
		{
			var pauseMenuScreen = screenManager.FirstOrDefault<PauseMenuScreen>();
			
			isUsedAsGiftingMenu = pauseMenuScreen.IsOpeningGiftingSendMenu;
		}

		public static GameScreen Create(ScreenManager screenManager, GameSave save)
		{
			GCM gcm = screenManager.AsDynamic().GCM;

			void ResetPauseMenuOpenOverride()
			{
				var pauseMenuScreen = screenManager.FirstOrDefault<PauseMenuScreen>();
				pauseMenuScreen.IsOpeningGiftingSendMenu = false;
			}

			return (GameScreen)Activator.CreateInstance(EquipmentMenuScreenType, save, gcm, (Action)ResetPauseMenuOpenOverride, (Action)ResetPauseMenuOpenOverride);
		}

		public override void Initialize(ItemLocationMap itemLocationMap, GCM gcm)
		{
			if (!isUsedAsGiftingMenu)
				return;

			gameContentManager = gcm;
			save = Dynamic._saveFile;

			Dynamic._menuTitle = "Gifting - Sending";
			Dynamic.DoesDrawScrollbarWidget = true;

			giftingService = Client.GetGiftingService();
			acceptedTraitsPerSlot = giftingService.GetAcceptedTraits();

			var menuCollection = ((object)Dynamic._primaryMenuCollection).AsDynamic();

			menuCollection.DoesMenuAllowScrolling = true;
			menuCollection.ScrollRowHeight = 4;
			menuCollection.SetIsCenterAligned(false);
			
			confirmMenuCollection = ConfirmationMenuEntryCollectionType
				.CreateInstance(false, TimeSpinnerGame.Localizer.Get("use_item_yes"),
					TimeSpinnerGame.Localizer.Get("use_item_no"), "Gift item to player?",
					new Action<object, EventArgs>(OnGiftItemAccept), new Action<object, EventArgs>(OnGiftItemCancel)).AsDynamic();
			confirmMenuCollection.Font = gameContentManager.ActiveFont;

			playerInfoCollection = StatCollectionType.CreateInstance().AsDynamic();
			((IList)Dynamic.StatCollections).Add(~playerInfoCollection);

			PopulatePlayerMenus();
		}

		void PopulatePlayerMenus()
		{
			var menuEntries = (IList)Dynamic.MenuEntries;
			menuEntries.Clear();

			var subMenuCollections = (IList)Dynamic._subMenuCollections;
			subMenuCollections.Clear();

			if (!acceptedTraitsPerSlot.Any())
			{
				var mainMenuEntry = MenuEntry.Create("No Available Players", () => { });
				mainMenuEntry.IsCenterAligned = false;
				mainMenuEntry.DoesDrawLargeShadow = false;
				mainMenuEntry.ColumnWidth = 144;
				menuEntries.Add(mainMenuEntry.AsTimeSpinnerMenuEntry());
			}
			else
			{
				foreach (var acceptedTraits in acceptedTraitsPerSlot)
				{
					var inventoryMenu = CreateMenuUseItemInventory(acceptedTraits);
					if (inventoryMenu == null)
						continue;

					subMenuCollections.Add(inventoryMenu);

					var mainMenuEntry = MenuEntry.Create(acceptedTraits.Name, () => { Dynamic.ChangeMenuCollection(inventoryMenu, true); });
					mainMenuEntry.IsCenterAligned = false;
					mainMenuEntry.DoesDrawLargeShadow = false;
					mainMenuEntry.ColumnWidth = 144;
					menuEntries.Add(mainMenuEntry.AsTimeSpinnerMenuEntry());
				}
			}

			subMenuCollections.Add(~confirmMenuCollection);
		}

		/*
		if (itemEntry.ItemType == EInventoryCategoryType.Equipment && this.GetAvailableQuantityByItem(itemEntry.Item) - this.SaveFile.Inventory.GetEquipmentEquippedCount(itemEntry.Item.Key) < itemEntry.QuanityToBuy)
        {
          flag = false;
          this.PlayErrorSound();
          this.ChangeDescription(Loc.Get("shop_buy_already_wearing"), EInventoryItemIcon.None);
        }
		*/

		object CreateMenuUseItemInventory(AcceptedTraits acceptedTraits)
		{
			bool OnUseItemSelected(InventoryItem item)
			{
				selectedItem = item;
				selectedPlayer = acceptedTraits;

				confirmMenuCollection.SetDescription($"Yeet a '{item.Name}' to {acceptedTraits.Name}?");

				Dynamic.ChangeMenuCollection(~confirmMenuCollection, true);

				return true;
			}

			var collection = new GiftingInventoryCollection(OnUseItemSelected);
			foreach (var item in save.Inventory.UseItemInventory.Inventory.Values)
			{
				if (!TraitMapping.ValuesPerItem.TryGetValue(item.UseItemType, out var traits))
					continue;

				if (acceptedTraits.AcceptsAnyTrait || acceptedTraits.DesiredTraits.Any(t => traits.ContainsKey(t)))
					collection.AddItem(item.UseItemType, item.Count);
			}
			foreach (var item in save.Inventory.EquipmentInventory.Inventory.Values)
			{
				if (!TraitMapping.ValuesPerItem.TryGetValue(item.EquipmentType, out var traits))
					continue;

				if (acceptedTraits.AcceptsAnyTrait || acceptedTraits.DesiredTraits.Any(t => traits.ContainsKey(t)))
				{
					var count = item.Count - save.Inventory.AsDynamic().GetEquipmentEquippedCount(item.Key);
					if (count > 0)
						collection.AddItem(item.EquipmentType, count);
				}
			}

			collection.RefreshItemNameAndDescriptions();

			var inventoryMenu = MenuUseItemInventoryType.CreateInstance(false, collection, (Func<InventoryUseItem, bool>)collection.OnUseItemSelected).AsDynamic();
			inventoryMenu.Font = gameContentManager.ActiveFont;

			return ~inventoryMenu;
		}

#if DEBUG
		void LoadAcceptedTraitsDummyData()
		{
			acceptedTraitsPerSlot.Clear();
			
			acceptedTraitsPerSlot.Add(new AcceptedTraits
			{
				Team = DummyTeam,
				Slot = 1,
				Game = "Timespinner",
				Name = "HealMe",
				AcceptsAnyTrait = false,
				DesiredTraits = new[] { Trait.Heal }
			});

			acceptedTraitsPerSlot.Add(new AcceptedTraits
			{
				Team = DummyTeam,
				Slot = 2,
				Game = "Satisfactory",
				Name = "JarnoSF",
				AcceptsAnyTrait = true,
				DesiredTraits = new Trait[0]
			});

			acceptedTraitsPerSlot.Add(new AcceptedTraits
			{
				Team = DummyTeam,
				Slot = 3,
				Game = "SomeGame",
				Name = "I Need Mana",
				AcceptsAnyTrait = false,
				DesiredTraits = new[] { Trait.Mana }
			});

			acceptedTraitsPerSlot.Add(new AcceptedTraits
			{
				Team = DummyTeam,
				Slot = 4,
				Game = "Yolo",
				Name = "Fishy",
				AcceptsAnyTrait = false,
				DesiredTraits = new[] { Trait.Fish }
			});

			acceptedTraitsPerSlot.Add(new AcceptedTraits
			{
				Team = DummyTeam,
				Slot = 5,
				Game = "Yolo2",
				Name = "Some really rather long name",
				AcceptsAnyTrait = false,
				DesiredTraits = new[] { Trait.Consumable, Trait.Flower, Trait.Heal, Trait.Food, Trait.Cure, Trait.Drink, Trait.Vegetable, Trait.Fruit }
			});

			PopulatePlayerMenus();

			giftingService.NumberOfGifts += 1;
		}
#endif

		void OnGiftItemAccept(object obj, EventArgs args)
		{
			if (selectedPlayer.Team == DummyTeam || giftingService.Send(selectedItem, selectedPlayer))
			{
				confirmMenuCollection.IsVisible = false;
				Dynamic.GoToPreviousMenuCollection();

				switch (selectedItem)
				{
					case InventoryUseItem useItem:
						save.Inventory.UseItemInventory.RemoveItem((int)useItem.UseItemType);
						((object)Dynamic._selectedMenuCollection).AsDynamic().RemoveItem(useItem);
						break;
					case InventoryEquipment equipment:
						save.Inventory.EquipmentInventory.RemoveItem((int)equipment.EquipmentType);
						((object)Dynamic._selectedMenuCollection).AsDynamic().RemoveItem(equipment.ToInventoryUseItem());
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(selectedItem), "paramter should be either UseItem or Equipment");
				}
				
				ScreenManager.Jukebox.PlayCue(ESFX.MenuSell);
			}
			else
			{
				ScreenManager.Jukebox.PlayCue(ESFX.MenuError);
			}
		}

		void OnGiftItemCancel(object obj, EventArgs args) => Dynamic.OnCancel(obj, args);
		
		public override void Update(GameTime gameTime, InputState input)
		{
			if (!isUsedAsGiftingMenu)
				return;

#if DEBUG
			if (input.IsNewPressTertiary(null))
				LoadAcceptedTraitsDummyData();
#endif

			var subMenuCollection = (IList)Dynamic._subMenuCollections;
			foreach (var subMenu in subMenuCollection)
			{
				var dynamicSubMenu = subMenu.AsDynamic();
				dynamicSubMenu.DrawPosition = Dynamic.ListTextDrawPosition;
				dynamicSubMenu.SetColumnWidth(Dynamic.ListColumnWidth, Dynamic.Zoom);
			}

			var menuCollection = ((object)Dynamic._primaryMenuCollection).AsDynamic();
			menuCollection.DrawPosition = new Vector2(0.05f * (float)Dynamic._screenWidth + (float)Dynamic._screenLeft, menuCollection.DrawPosition.Y);

			playerInfoCollection.Location = new Vector2(0.55f * (float)Dynamic._screenWidth + (float)Dynamic._screenLeft, (float)Dynamic._screenTop + 5f / 32f * (float)Dynamic._topSectionHeight + (float)Dynamic.Zoom);
			playerInfoCollection.Width = (int)(0.3f * (float)Dynamic._screenWidth);

			((object)Dynamic._selectedItemStats).AsDynamic().Location = new Vector2(-10000, 10000); //yeet lunais stats display
			Dynamic._iconDisplayFramePosition = new Vector2(-10000, 10000); //yeet enquipment icons

			confirmMenuCollection.DrawPosition = new Vector2(
				(float)Dynamic.DescriptionDrawPosition.X + (float)Dynamic._screenWidth * 0.125f, 
				(float)Dynamic.DescriptionDrawPosition.Y + (float)Dynamic._bottomSectionHeight * 0.5f);
			confirmMenuCollection.SetColumnWidth(Dynamic.ListColumnWidth, Dynamic.Zoom);

			RefreshPlayerGiftboxInfo(gameContentManager.ActiveFont);
		}

		void RefreshPlayerGiftboxInfo(SpriteFont menuFont)
		{
			var selectedIndex = ((object)Dynamic._primaryMenuCollection).AsDynamic().SelectedIndex;
			if (selectedIndex >= acceptedTraitsPerSlot.Count)
				return;
			
			AcceptedTraits selectedPlayerTraits = acceptedTraitsPerSlot[selectedIndex];

			var entries = (IList)playerInfoCollection.Entries;
			entries.Clear();

			var gameEntry = StatEntryType.CreateInstance().AsDynamic();
			gameEntry.Type = StatEntryDisplayEnumType.GetEnumValue("ColoredText");
			gameEntry.Title = "Game:";
			gameEntry.Text = selectedPlayerTraits.Game;
			gameEntry.TextColor = StatEntryColor;
			gameEntry.Initialize(menuFont);
			gameEntry._drawStringWidth = (int)(menuFont.MeasureString(gameEntry._drawString).X - 24);
			gameEntry._titleTextWidthReduction = gameEntry._drawStringWidth + 2;

			entries.Add(~gameEntry);

			if (selectedPlayerTraits.AcceptsAnyTrait)
			{
				var allTraitsEntry = StatEntryType.CreateInstance().AsDynamic();
				allTraitsEntry.Type = StatEntryDisplayEnumType.GetEnumValue("ColoredText");
				allTraitsEntry.Title = "Wants:";
				allTraitsEntry.Text = "All";
				allTraitsEntry.TextColor = StatEntryColor;
				allTraitsEntry.Initialize(menuFont);
				allTraitsEntry._drawStringWidth = (int)(menuFont.MeasureString(allTraitsEntry._drawString).X - 24);
				allTraitsEntry._titleTextWidthReduction = allTraitsEntry._drawStringWidth + 2;

				entries.Add(~allTraitsEntry);
			}
			else
			{
				var traits = new List<string>(NumberOfTraitsToDisplay + 1) {
					"Wants:"
				};

				for (var i = 0; i < NumberOfTraitsToDisplay; i++)
				{
					if (i == NumberOfTraitsToDisplay - 1)
					{
						if (selectedPlayerTraits.DesiredTraits.Length == NumberOfTraitsToDisplay)
							traits.Add(selectedPlayerTraits.DesiredTraits[i-1].ToString());
						else if (selectedPlayerTraits.DesiredTraits.Length < NumberOfTraitsToDisplay)
							traits.Add("");
						else
							traits.Add("More...");

					}
					else
					{
						if (i < selectedPlayerTraits.DesiredTraits.Length)
							traits.Add(selectedPlayerTraits.DesiredTraits[i].ToString());
						else
							traits.Add("");
					}
				}

				for (var i = 0; i < NumberOfTraitsToDisplay; i+=2)
				{
					var statEntry = StatEntryType.CreateInstance().AsDynamic();
					statEntry.Type = StatEntryDisplayEnumType.GetEnumValue("ColoredText");
					statEntry.Title = traits[i];
					statEntry.Text = traits[i+1];
					statEntry.TextColor = StatEntryColor;
					statEntry.Initialize(menuFont);
					statEntry._drawStringWidth = (int)(menuFont.MeasureString(statEntry._drawString).X - 24);
					statEntry._titleTextWidthReduction = statEntry._drawStringWidth + 2;

					entries.Add(~statEntry);
				}
			}
		}
	}

	class GiftingInventoryCollection : InventoryUseItemCollection
	{
		readonly Func<InventoryItem, bool> onItemSelected;

		public GiftingInventoryCollection(Func<InventoryItem, bool> onItemSelected)
		{
			this.onItemSelected = onItemSelected;
		}

		public void AddItem(EInventoryUseItemType item) => AddItem(item, 1);
		public void AddItem(EInventoryUseItemType item, int count) => AddItem((int)item, count);
		public void AddItem(EInventoryEquipmentType item) => AddItem(item, 1);
		public void AddItem(EInventoryEquipmentType item, int count) => AddItem((int)item.ToEInventoryUseItemType(), count);
		
		public override void RefreshItemNameAndDescriptions()
		{
			// ReSharper disable once SuggestVarOrType_SimpleTypes
			foreach (InventoryUseItem useItem in Inventory.Values)
			{
				if (!useItem.IsEquipment()) 
					continue;

				var equipment = useItem.ToInventoryEquipment();
				var dynamicInventoryItem = useItem.AsDynamic();
				dynamicInventoryItem.NameKey = equipment.NameKey;
				dynamicInventoryItem.DescriptionKey = equipment.DescriptionKey;
			}

			base.RefreshItemNameAndDescriptions();
		}

		public bool OnUseItemSelected(InventoryUseItem useItem) =>
			!useItem.IsEquipment() 
				? onItemSelected(useItem) 
				: onItemSelected(useItem.ToInventoryEquipment());
	}
}
