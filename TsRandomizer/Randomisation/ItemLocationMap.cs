﻿using System.Collections.Generic;
using System.Linq;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using TsRandomizer.IntermediateObjects;
using TsRandomizer.ReplacementObjects;
using R = TsRandomizer.Randomisation.Requirement;

namespace TsRandomizer.Randomisation
{
	class ItemLocationMap : LookupDictionairy<ItemKey, ItemLocation>
	{
		internal static readonly R MultipleSmallJumpsOfNpc = R.TimespinnerWheel | R.UpwardDash;
		internal static readonly Gate DoubleJumpOfNpc = (R.DoubleJump & R.TimespinnerWheel) | R.UpwardDash;
		internal static readonly Gate ForwardDashDoubleJump = (R.ForwardDash & R.DoubleJump) | R.UpwardDash;

		public static readonly R LowerLakeDesolationBridge = R.TimeStop | R.ForwardDash | R.GateKittyBoss | R.GateLeftLibrary;
		internal static readonly Gate AccessToPast = 
			(
			   R.TimespinnerWheel & R.TimespinnerSpindle //activateLibraryTimespinner
               & (LowerLakeDesolationBridge & R.CardD) //midLibrary
			) //libraryTimespinner
			| R.GateLakeSirineLeft
			| R.GateAccessToPast
			| R.GateLakeSirineRight
			| R.GateRoyalTowers
			| R.GateCastleRamparts
			| R.GateCastleKeep
			| (R.GateCavesOfBanishment & (R.DoubleJump | R.Swimming))
			| (R.GateMaw & R.DoubleJump);

		//past
		internal static readonly Gate LeftSideForestCaves = (AccessToPast & (R.TimespinnerWheel | R.ForwardDash | R.DoubleJump)) | R.GateLakeSirineRight | R.GateLakeSirineLeft;
		internal static readonly Gate UpperLakeSirine = (LeftSideForestCaves & (R.TimeStop | R.Swimming)) | R.GateLakeSirineLeft;
		internal static readonly Gate LowerlakeSirine = (LeftSideForestCaves | R.GateLakeSirineLeft) & R.Swimming;
		internal static readonly Gate LowerCavesOfBanishment = LowerlakeSirine | R.GateCavesOfBanishment | (R.GateMaw & R.DoubleJump);
		internal static readonly Gate UpperCavesOfBanishment = AccessToPast;
		internal static readonly Gate CastleRamparts = AccessToPast;
		internal static readonly Gate CastleKeep = CastleRamparts;
		internal static readonly Gate RoyalTower = (CastleKeep & R.DoubleJump) | R.GateRoyalTowers;
		internal static readonly Gate MidRoyalTower = RoyalTower & (MultipleSmallJumpsOfNpc | ForwardDashDoubleJump);
		internal static readonly Gate UpperRoyalTower = MidRoyalTower & R.DoubleJump;

		//future
		internal static readonly Gate UpperLakeDesolation = UpperLakeSirine & R.AntiWeed;
		internal static readonly Gate LeftLibrary = UpperLakeDesolation | LowerLakeDesolationBridge | (R.GateMilitairyGate & R.CardD & (R.CardB | (R.CardC & R.CardE)));
		internal static readonly Gate UpperLeftLibrary = LeftLibrary & (R.DoubleJump | R.ForwardDash);
		internal static readonly Gate MidLibrary = (LeftLibrary & R.CardD) | (R.GateMilitairyGate & (R.CardB | (R.CardC & R.CardE)));
		internal static readonly Gate UpperRightSideLibrary = (MidLibrary & (R.CardC | (R.CardB & R.CardE))) | (R.GateMilitairyGate & R.CardE);
		internal static readonly Gate RightSizeLibraryElevator = (MidLibrary & R.CardE & (R.CardC | R.CardB)) | (R.GateMilitairyGate & R.CardE);
		internal static readonly Gate LowerRightSideLibrary = (MidLibrary & R.CardB) | RightSizeLibraryElevator | R.GateMilitairyGate;
		internal static readonly R SealedCavesLeft = R.DoubleJump | R.GateSealedCaves;
		internal static readonly Gate SealedCavesLower = SealedCavesLeft & R.CardA;
		internal static readonly Gate SealedCavesSirens = (MidLibrary & R.CardB & R.CardE) | R.GateSealedSirensCave;
		internal static readonly Gate KillTwinsAndMaw = (LowerlakeSirine & R.DoubleJump) & (CastleKeep & R.TimeStop);
		internal static readonly Gate KillAll3MajorBosses = LowerRightSideLibrary & KillTwinsAndMaw & UpperRoyalTower;
		internal static readonly Gate MilitairyFortress = KillAll3MajorBosses;
		internal static readonly Gate MilitairyFortressHangar = MilitairyFortress;
		internal static readonly Gate RightSideMilitairyFortressHangar = MilitairyFortressHangar & R.DoubleJump;
		internal static readonly Gate TheLab = MilitairyFortressHangar & R.CardB;
		internal static readonly Gate TheLabPoweredOff = TheLab & DoubleJumpOfNpc;
		internal static readonly Gate UppereLab = TheLabPoweredOff & ForwardDashDoubleJump;
		internal static readonly Gate EmperorsTower = UppereLab;

		//pyramid
		internal static readonly Gate LeftPyramid = UppereLab & (
			R.TimespinnerWheel & R.TimespinnerSpindle &
			R.TimespinnerPiece1 & R.TimespinnerPiece2 & R.TimespinnerPiece3);
		internal static readonly Gate Nightmare = LeftPyramid & R.UpwardDash;

		public new ItemLocation this[ItemKey key] => GetItemLocationBasedOnKeyOrRoomKey(key);

		readonly ItemInfoProvider itemProvider;

		public ItemLocationMap(ItemInfoProvider itemInfoProvider) : base(160, l => l.Key)
		{
			itemProvider = itemInfoProvider;

			AddPresentItemLocations();
			AddPastItemLocations();
			AddPyramidItemLocations();
		}

		void AddPresentItemLocations()
		{
			//tutorial
			Add(ItemKey.TutorialMeleeOrb, "Yo Momma", itemProvider.Get(EInventoryOrbType.Blue, EOrbSlot.Melee));
			Add(ItemKey.TutorialSpellOrb, "Yo Momma", itemProvider.Get(EInventoryOrbType.Blue, EOrbSlot.Spell));
			//starter lake desolation
			Add(new ItemKey(1, 1, 1528, 144), "Starter chest 2", itemProvider.Get(EInventoryUseItemType.FuturePotion));
			Add(new ItemKey(1, 15, 264, 144), "Starter chest 3", itemProvider.Get(EInventoryEquipmentType.OldCoat));
			Add(new ItemKey(1, 25, 296, 176), "Starter chest 1", itemProvider.Get(EInventoryUseItemType.FutureHiPotion));
			Add(new ItemKey(1, 9, 600, 144 + TimespinnerWheel.YOffset), "Timespinner Wheel room", itemProvider.Get(EInventoryRelicType.TimespinnerWheel));
			Add(new ItemKey(1, 14, 40, 176), "Forget me not chest", itemProvider.Get(EInventoryUseItemType.EssenceCrystal), UpperLakeDesolation);
			//lower lake desolation
			Add(new ItemKey(1, 2, 1016, 384), null, itemProvider.Get(EItemType.MaxSand), R.TimeStop);
			Add(new ItemKey(1, 11, 72, 240), null, itemProvider.Get(EItemType.MaxHP), LowerLakeDesolationBridge);
			Add(new ItemKey(1, 3, 56, 176), null, itemProvider.Get(EItemType.MaxAura), R.TimeStop);
			//upper lake desolation
			Add(new ItemKey(1, 17, 152, 96), null, itemProvider.Get(EInventoryUseItemType.GoldRing), UpperLakeDesolation);
			Add(new ItemKey(1, 21, 200, 144), null, itemProvider.Get(EInventoryUseItemType.EssenceCrystal), UpperLakeDesolation);
			Add(new ItemKey(1, 20, 232, 96), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), UpperLakeDesolation & R.DoubleJump);
			Add(new ItemKey(1, 20, 168, 240), null, itemProvider.Get(EInventoryUseItemType.FuturePotion), UpperLakeDesolation);
			Add(new ItemKey(1, 22, 344, 160), null, itemProvider.Get(EInventoryUseItemType.FutureHiPotion), UpperLakeDesolation);
			Add(new ItemKey(1, 18, 1320, 189), "Crash site pedestal", itemProvider.Get(EInventoryOrbType.Moon, EOrbSlot.Melee), UpperLakeDesolation);
			Add(new ItemKey(1, 18, 1272, 192), "Crash site chest 1", itemProvider.Get(EInventoryEquipmentType.CaptainsCap), UpperLakeDesolation & R.GassMask & KillTwinsAndMaw);
			Add(new ItemKey(1, 18, 1368, 192), "Crash site chest 2", itemProvider.Get(EInventoryEquipmentType.CaptainsJacket), UpperLakeDesolation & R.GassMask & KillTwinsAndMaw);
			Add(new RoomItemKey(1, 5), "Kitty Boss", itemProvider.Get(EInventoryOrbType.Blade, EOrbSlot.Melee), UpperLakeDesolation | LowerLakeDesolationBridge);
			//libary left
			Add(new ItemKey(2, 60, 328, 160), null, itemProvider.Get(EItemType.MaxHP), LeftLibrary);
			Add(new ItemKey(2, 54, 296, 176), null, itemProvider.Get(EInventoryRelicType.ScienceKeycardD), LeftLibrary); 
			Add(new ItemKey(2, 44, 680, 368), null, itemProvider.Get(EInventoryRelicType.FoeScanner), LeftLibrary);
			Add(new ItemKey(2, 47, 216, 208), "Library storage room chest 1", itemProvider.Get(EInventoryUseItemType.Ether), LeftLibrary & R.CardD);
			Add(new ItemKey(2, 47, 152, 208), "Library storage room chest 2", itemProvider.Get(EInventoryOrbType.Blade, EOrbSlot.Passive), LeftLibrary & R.CardD);
			Add(new ItemKey(2, 47, 88, 208), "Library storage room chest 3", itemProvider.Get(EInventoryOrbType.Blade, EOrbSlot.Spell), LeftLibrary & R.CardD);
			//libary top
			Add(new ItemKey(2, 56, 168, 192), "Backer room chest 5", itemProvider.Get(EInventoryUseItemType.GoldNecklace), UpperLeftLibrary);
			Add(new ItemKey(2, 56, 392, 192), "Backer room chest 4", itemProvider.Get(EInventoryUseItemType.GoldRing), UpperLeftLibrary);
			Add(new ItemKey(2, 56, 616, 192), "Backer room chest 3", itemProvider.Get(EInventoryUseItemType.EssenceCrystal), UpperLeftLibrary);
			Add(new ItemKey(2, 56, 840, 192), "Backer room chest 2", itemProvider.Get(EInventoryUseItemType.EssenceCrystal), UpperLeftLibrary);
			Add(new ItemKey(2, 56, 1064, 192), "Backer room chest 1", itemProvider.Get(EInventoryUseItemType.MagicMarbles), UpperLeftLibrary);
			//libary mid
			Add(new ItemKey(2, 34, 232, 1200), null, itemProvider.Get(EInventoryUseItemType.Jerky), MidLibrary);
			Add(new ItemKey(2, 40, 344, 176), "Ye olde Timespinner", itemProvider.Get(EInventoryRelicType.ScienceKeycardC), MidLibrary);
			Add(new ItemKey(2, 32, 328, 160), null, itemProvider.Get(EInventoryUseItemType.GoldRing), MidLibrary & R.CardC);
			Add(new ItemKey(2, 7, 232, 144), null, itemProvider.Get(EItemType.MaxAura), MidLibrary);
			Add(new ItemKey(2, 25, 328, 192), null, itemProvider.Get(EItemType.MaxSand), MidLibrary & R.CardE);
			//libary right, 
			Add(new ItemKey(2, 15, 760, 192), null, itemProvider.Get(EInventoryUseItemType.FuturePotion), UpperRightSideLibrary);
			Add(new ItemKey(2, 20, 72, 1200), null, itemProvider.Get(EInventoryUseItemType.Jerky), RightSizeLibraryElevator);
			Add(new ItemKey(2, 23, 72, 560), null, itemProvider.Get(EInventoryUseItemType.FutureHiPotion), UpperRightSideLibrary & (R.CardE | R.DoubleJump)); //needs only UpperRightSideLibrary but requires Elevator Card | Double Jump to get out
			Add(new ItemKey(2, 23, 1112, 112), null, itemProvider.Get(EInventoryUseItemType.FutureHiPotion), UpperRightSideLibrary & (R.CardE | R.DoubleJump)); //needs only UpperRightSideLibrary but requires Elevator Card | Double Jump to get out
			Add(new ItemKey(2, 23, 136, 304), null, itemProvider.Get(EInventoryRelicType.ElevatorKeycard), UpperRightSideLibrary & (R.CardE | R.DoubleJump)); //needs only UpperRightSideLibrary but requires Elevator Card | Double Jump to get out
			Add(new ItemKey(2, 11, 104, 192), null, itemProvider.Get(EInventoryUseItemType.EssenceCrystal), LowerRightSideLibrary);
			Add(new ItemKey(2, 29, 280, 222 + TimespinnerSpindle.YOffset), "Varndagroth", itemProvider.Get(EInventoryRelicType.TimespinnerSpindle), RightSizeLibraryElevator);
			Add(new RoomItemKey(2, 52), "Library spider hell", itemProvider.Get(EInventoryRelicType.TimespinnerGear2), RightSizeLibraryElevator & R.CardA);
			//Sealed Caves left
			Add(new ItemKey(9, 10, 248, 848), null, itemProvider.Get(EInventoryRelicType.ScienceKeycardB), SealedCavesLeft);
			Add(new ItemKey(9, 19, 664, 704), null, itemProvider.Get(EInventoryUseItemType.Antidote), SealedCavesLower & R.TimeStop);
			Add(new ItemKey(9, 39, 88, 192), null, itemProvider.Get(EInventoryUseItemType.Antidote), SealedCavesLower);
			Add(new ItemKey(9, 41, 312, 192), null, itemProvider.Get(EInventoryUseItemType.GalaxyStone), SealedCavesLower & ForwardDashDoubleJump);
			Add(new ItemKey(9, 42, 328, 192), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), SealedCavesLower);
			Add(new ItemKey(9, 12, 280, 160), null, itemProvider.Get(EItemType.MaxHP), SealedCavesLower);
			Add(new ItemKey(9, 48, 104, 160), null, itemProvider.Get(EInventoryUseItemType.FutureEther), SealedCavesLower);
			Add(new ItemKey(9, 15, 248, 192), null, itemProvider.Get(EInventoryUseItemType.FutureEther), SealedCavesLower & R.DoubleJump);
			Add(new RoomItemKey(9, 13), "Xarion", itemProvider.Get(EInventoryRelicType.TimespinnerGear3), SealedCavesLower);
			//Sealed Caves (sirens)
			Add(new ItemKey(9, 5, 88, 496), null, itemProvider.Get(EItemType.MaxSand), SealedCavesSirens & R.Swimming);
			Add(new ItemKey(9, 3, 1848, 576), null, itemProvider.Get(EInventoryEquipmentType.BirdStatue), SealedCavesSirens & R.Swimming);
			Add(new ItemKey(9, 3, 744, 560), null, itemProvider.Get(EItemType.MaxAura), SealedCavesSirens & R.Swimming);
			Add(new ItemKey(9, 2, 184, 176), null, itemProvider.Get(EInventoryUseItemType.WarpCard), SealedCavesSirens);
			Add(new ItemKey(9, 2, 104, 160), null, itemProvider.Get(EInventoryRelicType.WaterMask), SealedCavesSirens);
			//Militairy Fortress
			Add(new ItemKey(10, 3, 264, 128), null, itemProvider.Get(EItemType.MaxSand), MilitairyFortress & DoubleJumpOfNpc);
			Add(new ItemKey(10, 11, 296, 192), null, itemProvider.Get(EItemType.MaxAura), MilitairyFortress);
			Add(new ItemKey(10, 4, 1064, 176), null, itemProvider.Get(EInventoryUseItemType.FutureHiPotion), MilitairyFortressHangar);
			Add(new ItemKey(10, 10, 104, 192), null, itemProvider.Get(EInventoryRelicType.AirMask), MilitairyFortressHangar);
			Add(new ItemKey(10, 8, 1080, 176), null, itemProvider.Get(EInventoryEquipmentType.LabGlasses), MilitairyFortressHangar);
			Add(new ItemKey(10, 7, 104, 192), null, itemProvider.Get(EInventoryUseItemType.PlasmaIV), RightSideMilitairyFortressHangar & R.CardB);
			Add(new ItemKey(10, 7, 152, 192), null, itemProvider.Get(EItemType.MaxSand), RightSideMilitairyFortressHangar & R.CardB);
			Add(new ItemKey(10, 18, 280, 189), null, itemProvider.Get(EInventoryOrbType.Gun, EOrbSlot.Melee), RightSideMilitairyFortressHangar & (DoubleJumpOfNpc | ForwardDashDoubleJump));
			// The lab
			Add(new ItemKey(11, 36, 312, 192), null, itemProvider.Get(EInventoryUseItemType.FoodSynth), TheLab);
			Add(new ItemKey(11, 3, 1528, 192), null, itemProvider.Get(EItemType.MaxHP), TheLab & R.DoubleJump);
			Add(new ItemKey(11, 3, 72, 192), null, itemProvider.Get(EInventoryUseItemType.FuturePotion), TheLab & R.UpwardDash); //when lab power is only, it only requires DoubleJumpOfNpc, but we cant code for the power state
			Add(new ItemKey(11, 25, 104, 192), null, itemProvider.Get(EItemType.MaxAura), TheLab & R.DoubleJump);
			Add(new ItemKey(11, 18, 824, 128), null, itemProvider.Get(EInventoryUseItemType.ChaosHeal), TheLabPoweredOff);
			Add(new RoomItemKey(11, 39), "Dynamo Works", itemProvider.Get(EInventoryOrbType.Eye, EOrbSlot.Melee), TheLabPoweredOff);
			Add(new RoomItemKey(11, 21), "Blob mom", itemProvider.Get(EInventoryRelicType.ScienceKeycardA), UppereLab);
			Add(new RoomItemKey(11, 1), "Lab Experiment #13", itemProvider.Get(EInventoryRelicType.Dash), TheLabPoweredOff);
			Add(new ItemKey(11, 6, 328, 192), null, itemProvider.Get(EInventoryEquipmentType.LabCoat), UppereLab);
			Add(new ItemKey(11, 27, 296, 160), null, itemProvider.Get(EItemType.MaxSand), UppereLab);
			Add(new RoomItemKey(11, 26), "Lab spider hell", itemProvider.Get(EInventoryRelicType.TimespinnerGear1), TheLabPoweredOff & R.CardA);
			//Emperors tower
			Add(new ItemKey(12, 5, 344, 192), null, itemProvider.Get(EItemType.MaxAura), EmperorsTower);
			Add(new ItemKey(12, 3, 200, 160), null, itemProvider.Get(EInventoryEquipmentType.LachiemCrown), EmperorsTower & R.UpwardDash);
			Add(new ItemKey(12, 25, 360, 176), null, itemProvider.Get(EInventoryEquipmentType.EmpressCoat), EmperorsTower & R.UpwardDash);
			Add(new ItemKey(12, 22, 56, 192), null, itemProvider.Get(EItemType.MaxSand), EmperorsTower);
			Add(new ItemKey(12, 9, 344, 928), null, itemProvider.Get(EInventoryUseItemType.FutureHiEther), EmperorsTower);
			Add(new ItemKey(12, 19, 72, 192), null, itemProvider.Get(EInventoryEquipmentType.FiligreeClasp), EmperorsTower & DoubleJumpOfNpc);
			Add(new ItemKey(12, 13, 120, 176), null, itemProvider.Get(EItemType.MaxHP), EmperorsTower);
			Add(new ItemKey(12, 11, 264, 208), "Dad's Chambers chest", itemProvider.Get(EInventoryRelicType.EmpireBrooch), EmperorsTower); 
			Add(new ItemKey(12, 11, 136, 205), "Dad's Chambers pedistal", itemProvider.Get(EInventoryOrbType.Empire, EOrbSlot.Melee), EmperorsTower);
		}

		void AddPastItemLocations()
		{
			//Refugee Camp
			Add(new RoomItemKey(3, 0), "Neliste's Bra", itemProvider.Get(EInventoryOrbType.Flame, EOrbSlot.Melee), AccessToPast); //neliste
			Add(new ItemKey(3, 30, 296, 176), "Refugee camp storage chest 3", itemProvider.Get(EInventoryUseItemType.EssenceCrystal), AccessToPast);
			Add(new ItemKey(3, 30, 232, 176), "Refugee camp storage chest 2", itemProvider.Get(EInventoryUseItemType.GoldNecklace), AccessToPast);
			Add(new ItemKey(3, 30, 168, 176), "Refugee camp storage chest 1", itemProvider.Get(EInventoryRelicType.JewelryBox), AccessToPast);
			//Forest
			Add(new ItemKey(3, 3, 648, 272), null, itemProvider.Get(EInventoryUseItemType.Herb), AccessToPast);
			Add(new ItemKey(3, 15, 248, 112), null, itemProvider.Get(EItemType.MaxAura), AccessToPast & (DoubleJumpOfNpc | ForwardDashDoubleJump));
			Add(new ItemKey(3, 21, 120, 192), null, itemProvider.Get(EItemType.MaxSand), AccessToPast);
			Add(new ItemKey(3, 12, 776, 560), null, itemProvider.Get(EInventoryEquipmentType.PointyHat), AccessToPast);
			Add(new ItemKey(3, 11, 392, 608), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), AccessToPast & R.Swimming);
			Add(new ItemKey(3, 5, 184, 192), null, itemProvider.Get(EInventoryEquipmentType.Pendulum), AccessToPast & R.Swimming);
			Add(new ItemKey(3, 2, 584, 368), null, itemProvider.Get(EInventoryUseItemType.Potion), AccessToPast);
			Add(new ItemKey(4, 20, 264, 160), null, itemProvider.Get(EItemType.MaxAura), AccessToPast);
			Add(new ItemKey(3, 29, 248, 192), null, itemProvider.Get(EItemType.MaxHP), LeftSideForestCaves);
			//Upper Lake Sirine
			Add(new ItemKey(7, 16, 152, 96), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), UpperLakeSirine);
			Add(new ItemKey(7, 19, 248, 96), null, itemProvider.Get(EItemType.MaxAura), UpperLakeSirine & R.DoubleJump);
			Add(new ItemKey(7, 19, 168, 240), null, itemProvider.Get(EInventoryEquipmentType.TravelersCloak), UpperLakeSirine);
			Add(new ItemKey(7, 27, 184, 144), null, itemProvider.Get(EInventoryFamiliarType.Griffin), UpperLakeSirine);
			Add(new ItemKey(7, 13, 56, 176), null, itemProvider.Get(EInventoryUseItemType.WarpCard), UpperLakeSirine);
			Add(new ItemKey(7, 30, 296, 176), "Pyramid keys room", itemProvider.Get(EInventoryRelicType.PyramidsKey), UpperLakeSirine);
			//Lower Lake Sirine
			Add(new ItemKey(7, 3, 440, 1232), null, itemProvider.Get(EInventoryUseItemType.Potion), LowerlakeSirine);
			Add(new ItemKey(7, 7, 1432, 576), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), LowerlakeSirine);
			Add(new ItemKey(7, 6, 520, 496), null, itemProvider.Get(EInventoryUseItemType.Potion), LowerlakeSirine);
			Add(new ItemKey(7, 11, 88, 240), null, itemProvider.Get(EItemType.MaxHP), LowerlakeSirine);
			Add(new ItemKey(7, 2, 1016, 384), null, itemProvider.Get(EInventoryUseItemType.Ether), LowerlakeSirine);
			Add(new ItemKey(7, 20, 248, 96), null, itemProvider.Get(EItemType.MaxSand), LowerlakeSirine);
			Add(new ItemKey(7, 9, 584, 189), null, itemProvider.Get(EInventoryOrbType.Ice, EOrbSlot.Melee), LowerlakeSirine);
			//Caves of Banishment
			Add(new ItemKey(8, 19, 664, 704), null, itemProvider.Get(EInventoryUseItemType.SilverOre), LowerCavesOfBanishment & R.DoubleJump);
			Add(new ItemKey(8, 12, 280, 160), null, itemProvider.Get(EItemType.MaxHP), LowerCavesOfBanishment);
			Add(new ItemKey(8, 48, 104, 160), null, itemProvider.Get(EInventoryUseItemType.Herb), LowerCavesOfBanishment);
			Add(new ItemKey(8, 39, 88, 192), null, itemProvider.Get(EInventoryUseItemType.SilverOre), LowerCavesOfBanishment);
			Add(new ItemKey(8, 41, 168, 192), "Jackpot room chest 1", itemProvider.Get(EInventoryUseItemType.GoldNecklace), LowerCavesOfBanishment & ForwardDashDoubleJump);
			Add(new ItemKey(8, 41, 216, 192), "Jackpot room chest 2", itemProvider.Get(EInventoryUseItemType.GoldRing), LowerCavesOfBanishment & ForwardDashDoubleJump);
			Add(new ItemKey(8, 41, 264, 192), "Jackpot room chest 3", itemProvider.Get(EInventoryUseItemType.EssenceCrystal), LowerCavesOfBanishment & ForwardDashDoubleJump);
			Add(new ItemKey(8, 41, 312, 192), "Jackpot room chest 4", itemProvider.Get(EInventoryUseItemType.MagicMarbles), LowerCavesOfBanishment & ForwardDashDoubleJump);
			Add(new ItemKey(8, 42, 216, 189), null, itemProvider.Get(EInventoryOrbType.Wind, EOrbSlot.Melee), LowerCavesOfBanishment);
			Add(new ItemKey(8, 15, 248, 192), null, itemProvider.Get(EInventoryUseItemType.SilverOre), LowerCavesOfBanishment & R.DoubleJump);
			Add(new ItemKey(8, 31, 88, 400), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), LowerCavesOfBanishment & R.DoubleJump);
			//Caves of banishment (sirens)
			Add(new ItemKey(8, 4, 664, 144), null, itemProvider.Get(EInventoryUseItemType.SilverOre), UpperCavesOfBanishment);
			Add(new ItemKey(8, 3, 808, 144), null, itemProvider.Get(EInventoryUseItemType.SilverOre), UpperCavesOfBanishment);
			Add(new ItemKey(8, 3, 744, 560), null, itemProvider.Get(EInventoryUseItemType.SilverOre), UpperCavesOfBanishment & R.Swimming);
			Add(new ItemKey(8, 3, 1848, 576), null, itemProvider.Get(EItemType.MaxAura), UpperCavesOfBanishment & R.Swimming);
			Add(new ItemKey(8, 5, 88, 496), null, itemProvider.Get(EItemType.MaxSand), UpperCavesOfBanishment & R.Swimming);
			//Caste Ramparts
			Add(new ItemKey(4, 1, 456, 160), null, itemProvider.Get(EItemType.MaxSand), CastleRamparts & MultipleSmallJumpsOfNpc);
			Add(new ItemKey(4, 3, 136, 144), null, itemProvider.Get(EItemType.MaxHP), CastleRamparts & R.TimeStop);
			Add(new ItemKey(4, 10, 56, 192), null, itemProvider.Get(EInventoryUseItemType.HiPotion), CastleRamparts);
			Add(new ItemKey(4, 11, 344, 192), null, itemProvider.Get(EInventoryUseItemType.HiPotion), CastleRamparts);
			Add(new ItemKey(4, 22, 104, 189), null, itemProvider.Get(EInventoryOrbType.Iron, EOrbSlot.Melee), CastleRamparts);
			//Caste Keep
			Add(new ItemKey(5, 9, 104, 189), null, itemProvider.Get(EInventoryOrbType.Blood, EOrbSlot.Melee), CastleKeep);
			Add(new ItemKey(5, 10, 104, 192), null, itemProvider.Get(EInventoryFamiliarType.Sprite), CastleKeep);
			Add(new ItemKey(5, 14, 88, 208), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), CastleKeep & R.PinkOrb & R.DoubleJump);
			Add(new ItemKey(5, 44, 216, 192), null, itemProvider.Get(EInventoryUseItemType.Potion), CastleKeep);
			Add(new ItemKey(5, 45, 104, 192), null, itemProvider.Get(EItemType.MaxHP), CastleKeep);
			Add(new ItemKey(5, 15, 296, 192), null, itemProvider.Get(EItemType.MaxAura), CastleKeep);
			Add(new ItemKey(5, 41, 72, 160), null, itemProvider.Get(EInventoryEquipmentType.BuckleHat), CastleKeep);
			Add(new RoomItemKey(5, 5), "Twins", itemProvider.Get(EInventoryRelicType.DoubleJump), CastleKeep & R.TimeStop); //sucabus
			Add(new ItemKey(5, 22, 312, 176), null, itemProvider.Get(EItemType.MaxSand), CastleKeep & ForwardDashDoubleJump);
			//Royal towers
			Add(new ItemKey(6, 19, 200, 176), null, itemProvider.Get(EItemType.MaxAura), RoyalTower & R.DoubleJump);
			Add(new ItemKey(6, 27, 472, 384), null, itemProvider.Get(EInventoryUseItemType.MagicMarbles), MidRoyalTower);
			Add(new ItemKey(6, 1, 1512, 288), null, itemProvider.Get(EInventoryUseItemType.Potion), UpperRoyalTower);
			Add(new ItemKey(6, 25, 360, 176), null, itemProvider.Get(EInventoryUseItemType.HiEther), UpperRoyalTower & DoubleJumpOfNpc);
			Add(new ItemKey(6, 3, 120, 208), null, itemProvider.Get(EInventoryFamiliarType.Demon), UpperRoyalTower & DoubleJumpOfNpc);
			Add(new ItemKey(6, 17, 200, 112), null, itemProvider.Get(EItemType.MaxHP), UpperRoyalTower & DoubleJumpOfNpc);
			Add(new ItemKey(6, 17, 56, 448), null, itemProvider.Get(EInventoryEquipmentType.VileteCrown), UpperRoyalTower);
			Add(new ItemKey(6, 17, 360, 1840), null, itemProvider.Get(EInventoryEquipmentType.MidnightCloak), MidRoyalTower);
			Add(new ItemKey(6, 13, 120, 176), null, itemProvider.Get(EItemType.MaxSand), UpperRoyalTower);
			Add(new ItemKey(6, 22, 88, 208), null, itemProvider.Get(EInventoryUseItemType.Ether), UpperRoyalTower);
			Add(new ItemKey(6, 11, 360, 544), null, itemProvider.Get(EInventoryUseItemType.HiPotion), UpperRoyalTower);
			Add(new ItemKey(6, 23, 856, 208), "Statue room", itemProvider.Get(EInventoryEquipmentType.VileteDress), UpperRoyalTower & R.UpwardDash);
			Add(new ItemKey(6, 14, 136, 208), null, itemProvider.Get(EInventoryOrbType.Pink, EOrbSlot.Melee), UpperRoyalTower);
			Add(new ItemKey(6, 14, 184, 205), null, itemProvider.Get(EInventoryUseItemType.WarpCard), UpperRoyalTower);
		}

		void AddPyramidItemLocations()
		{
			//ancient pyramid
			Add(new ItemKey(16, 14, 312, 192), null, itemProvider.Get(EItemType.MaxSand), LeftPyramid);
			Add(new ItemKey(16, 3, 88, 192), null, itemProvider.Get(EItemType.MaxHP), LeftPyramid);
			Add(new ItemKey(16, 22, 200, 192), null, itemProvider.Get(EItemType.MaxAura), Nightmare); //only requires LeftPyramid to rach but Nightmate to escape
			Add(new ItemKey(16, 16, 1512, 144), null, itemProvider.Get(EInventoryRelicType.EssenceOfSpace), Nightmare); //only requires LeftPyramid to rach but Nightmate to escape
			//Add(new ItemKey(16, 5, 136, 192), null, itemProvider.Get(EInventoryRelicType.EternalBrooch), LeftPyramid); //Post nightmare

			//temporal gyre
			/*var challengeDungion = Nightmare;
			Add(new ItemKey(14, 14, 200, 832), ItemInfo.Dummy, challengeDungion); //transition chest 1
			Add(new ItemKey(14, 17, 200, 832), ItemInfo.Dummy, challengeDungion); //transition chest 2
			Add(new ItemKey(14, 20, 200, 832), ItemInfo.Dummy, challengeDungion); //transition chest 3
			Add(new ItemKey(14, 8, 120, 176), ItemInfo.Dummy, challengeDungion); //Ravenlord pre fight
			Add(new ItemKey(14, 9, 280, 176), ItemInfo.Dummy, challengeDungion); //Ravenlord post fight
			Add(new ItemKey(14, 6, 40, 208), ItemInfo.Dummy, challengeDungion); //ifrid pre fight
			Add(new ItemKey(14, 7, 280, 208), ItemInfo.Dummy, challengeDungion); //ifrid post fight*/
		}

		ItemLocation GetItemLocationBasedOnKeyOrRoomKey(ItemKey key)
		{
			return TryGetValue(key, out var itemLocation)
				? itemLocation
				: TryGetValue(key.ToRoomItemKey(), out var roomItemLocation)
					? roomItemLocation
					: null;
		}

		public bool IsBeatable()
		{
			//gassmask may never be placed in a gass effected place
			//the verry basics to reach maw shouldd also allow you to get gassmask
			var gassmarkLocation = this.First(l => l.ItemInfo?.Identifier == new ItemIdentifier(EInventoryRelicType.AirMask));
			if (gassmarkLocation.Key.LevelId == 1 || !gassmarkLocation.Gate.CanBeOpenedWith(
				    R.DoubleJump | R.GateAccessToPast | R.Swimming))
				return false;

			var obtainedRequirements = R.None;
			var itteration = 0;

			do
			{
				itteration++;
				var previusObtainedRequirements = obtainedRequirements;

				obtainedRequirements = GetObtainedRequirements(obtainedRequirements);

				if (obtainedRequirements == previusObtainedRequirements)
					return false;

			} while (!CanCompleteGame(obtainedRequirements) && itteration <= ItemUnlockingMap.ProgressionItemCount);

			return true;
		}

		R GetObtainedRequirements(R obtainedRequirements)
		{
			var reachableLocations = GetReachableLocations(obtainedRequirements)
				.Where(l => l.ItemInfo != null)
				.ToArray();

			var unlockedRequirements = reachableLocations
				.Where(l => !(l.ItemInfo is PogRessiveItemInfo))
				.Select(l => l.ItemInfo.Unlocks)
				.Aggregate(R.None, (current, unlock) => current | unlock);

			var progressiveItemsPerLocations = reachableLocations
				.Where(l => l.ItemInfo is PogRessiveItemInfo)
				.GroupBy(l => l.ItemInfo as PogRessiveItemInfo);

			foreach (var progressiveItemsPerLocation in progressiveItemsPerLocations)
			{
				var progressiveItem = progressiveItemsPerLocation.Key;

				progressiveItem.Reset();

				for (int i = 0; i < progressiveItemsPerLocation.Count(); i++)
				{
					unlockedRequirements |= progressiveItem.Unlocks;

					progressiveItem.Next();
				}
			}

			return unlockedRequirements;
		}

		public IEnumerable<ItemLocation> GetReachableLocations(R obtainedRequirements)
		{
			return this.Where(l => l.Gate.CanBeOpenedWith(obtainedRequirements));
		}

		static bool CanCompleteGame(R obtainedRequirements)
		{
			return Nightmare.CanBeOpenedWith(obtainedRequirements);
		}

		public void BaseOnSave(GameSave gameSave)
		{
			var progressiveItemInfos = this
				.Where(l => l.ItemInfo is PogRessiveItemInfo)
				.Select(l => (PogRessiveItemInfo)l.ItemInfo);

			foreach (var progressiveItem in progressiveItemInfos)
				progressiveItem.Reset();

			foreach (var itemLocation in this)
				itemLocation.BsseOnGameSave(gameSave);
		}

		void Add(ItemKey itemKey, string name, ItemInfo defaultItem)
		{
			Add(new ItemLocation(itemKey, name, defaultItem));
		}

		void Add(ItemKey itemKey, string name, ItemInfo defaultItem, R requirement)
		{
			Add(new ItemLocation(itemKey, name, defaultItem, requirement));
		}

		void Add(ItemKey itemKey, string name, ItemInfo defaultItem, Gate gate)
		{
			Add(new ItemLocation(itemKey, name, defaultItem, gate));
		}
	}
}
