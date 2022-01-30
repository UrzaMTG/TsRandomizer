﻿using System;
using System.Collections.Generic;
using TsRandomizer.Settings.GameSettingObjects;

namespace TsRandomizer.Settings
{
	public class SettingCollection
	{
		public static readonly GameSettingCategoryInfo[] Categories = {
			new GameSettingCategoryInfo { Name = "Stats", Description = "Settings related to player stat scaling.", 
				SettingsPerCategory = new List<Func<SettingCollection, GameSetting>> {
					s => s.DamageRando
				}},
			new GameSettingCategoryInfo { Name = "Loot", Description = "Settings related to shop inventory and loot.", 
				SettingsPerCategory = new List<Func<SettingCollection, GameSetting>> {
					s => s.ShopFill, s => s.ShopMultiplier, s => s.ShopWarpShards
				}},
			new GameSettingCategoryInfo { Name = "Archipelago", Description = "Settings related to games with the Archipelago multiworld.",
				SettingsPerCategory = new List<Func<SettingCollection, GameSetting>> {
					s => s.NumberOfOnScreenLogLines, s => s.OnScreenLogLineScreenTime, s => s.ShowSendItemsFromMe, s => s.ShowReceivedItemsFromMe,
					s => s.ShowSendGenericItems, s => s.ShowSendTrapItems, s => s.ShowSendImportantItems, s => s.ShowSendProgressionItems,
					s => s.ShowSystemMessages
				}}
		};

		public OnOffGameSetting DamageRando = new OnOffGameSetting("Damage Randomizer",
			"Adds a high chance to make orb damage very low, and a low chance to make orb damage very, very high", false);

		public SpecificValuesGameSetting ShopFill = new SpecificValuesGameSetting("Shop Inventory",
			"Sets the items for sale in Merchant Crow's shops. Options: [Default,Random,Vanilla,Empty]",
			new List<string> { "Default", "Random", "Vanilla", "Empty" });

		public NumberGameSetting ShopMultiplier = new NumberGameSetting("Shop Price Multiplier",
			"Multiplier for the cost of items in the shop. Set to 0 for free shops", 0, 10, 1, 1);

		public OnOffGameSetting ShopWarpShards = new OnOffGameSetting("Always Sell Warp Shards",
			"Shops always sell warp shards (when keys possessed), ignoring inventory setting.", false);

		public NumberGameSetting NumberOfOnScreenLogLines = new NumberGameSetting("Log Number of Lines",
			"Max number of messages to show at the bottom left of the screen, 0 to turn onscreen log off", 0, 10, 1, 3);

		public NumberGameSetting OnScreenLogLineScreenTime = new NumberGameSetting("Log Line ScreenTime",
			"How long does a single line shown at the bottom left of the screen stay visible", 1, 10, 0.5, 5);

		public OnOffGameSetting ShowSendItemsFromMe = new OnOffGameSetting("Log Items send by you",
			"Logs Generic items send between other players", false);

		public OnOffGameSetting ShowReceivedItemsFromMe = new OnOffGameSetting("Log Items received by you",
			"Logs Generic items send between other players", false);

		public OnOffGameSetting ShowSendGenericItems = new OnOffGameSetting("Log Generic Items",
			"Logs Generic items send between other players", false);

		public OnOffGameSetting ShowSendTrapItems = new OnOffGameSetting("Log Trap Items",
			"Logs Traps send between other players", true);

		public OnOffGameSetting ShowSendImportantItems = new OnOffGameSetting("Log Importent Items",
			"Logs Importent items send between other players", false);

		public OnOffGameSetting ShowSendProgressionItems = new OnOffGameSetting("Log Progression Items",
			"Logs Progression items send between other players", true);

		public OnOffGameSetting ShowSystemMessages = new OnOffGameSetting("Log System Message",
			"Logs System messages, like who connected/left and who changed tags", true);
	}
}