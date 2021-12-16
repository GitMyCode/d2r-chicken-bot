using System;
using System.Runtime.InteropServices;

namespace D.Models {

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct UiGameMenus {
        [FieldOffset(0x00)] public byte IsGameMenuOpen;
        [FieldOffset(0x08)] public byte IsInGame;
        [FieldOffset(0x09)] public byte IsTrading; // not sure
        [FieldOffset(0x0A)] public byte StatsWindowOpen;
        [FieldOffset(0x0B)] public byte MouseAttackMenuOpen;
        [FieldOffset(0x0C)] public byte SkillTreeMenuOpen;
        [FieldOffset(0x10)] public byte IsTalkingToNpc;
        [FieldOffset(0x13)] public byte IsTrading2; // not sure
        [FieldOffset(0x1B)] public byte WaypointMenuOpen;
        [FieldOffset(0x20)] public byte ChestOpen;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct StatList {
        [FieldOffset(0x30)] public readonly StatArray Stats;
        [FieldOffset(0x80)] public readonly StatArray Stats2;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct StatArray {
        [FieldOffset(0x0)] public readonly IntPtr Array;
        [FieldOffset(0x8)] public readonly ulong Size;
    }

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct StatValue {
        [FieldOffset(0x2)] public readonly Stat Stat;
        [FieldOffset(0x4)] public readonly int Value;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct UnitHashTable {
        [FieldOffset(0x00)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public IntPtr[] UnitTable;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct UnitAny {
        //https://github.com/noah-/d2bs#public-int-type-2
        [FieldOffset(0x00)] public uint UnityType;
        [FieldOffset(0x04)] public uint TxtFileNo;
        [FieldOffset(0x08)] public uint UnitId;
        [FieldOffset(0x10)] public IntPtr UnitData;
        [FieldOffset(0x20)] public IntPtr pAct;
        [FieldOffset(0x38)] public IntPtr pPath;
        [FieldOffset(0x88)] public IntPtr StatsList;
        [FieldOffset(0x90)] public IntPtr Inventory;
        [FieldOffset(0xB8)] public uint OwnerType; // ?
        [FieldOffset(0xC4)] public ushort X;
        [FieldOffset(0xC6)] public ushort Y;
        [FieldOffset(0x150)] public IntPtr pListNext;
        [FieldOffset(0x158)] public IntPtr pRoomNext;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct Path {
        [FieldOffset(0x02)] public ushort DynamicX;
        [FieldOffset(0x06)] public ushort DynamicY;
        [FieldOffset(0x10)] public ushort StaticX;
        [FieldOffset(0x14)] public ushort StaticY;
        [FieldOffset(0x20)] public IntPtr pRoom;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct Room {
        [FieldOffset(0x00)] public IntPtr pRoomsNear;
        [FieldOffset(0x18)] public IntPtr pRoomEx;
        [FieldOffset(0x40)] public uint numRoomsNear;
        [FieldOffset(0x48)] public IntPtr pAct;
        [FieldOffset(0xA8)] public IntPtr pUnitFirst;
        [FieldOffset(0xB0)] public IntPtr pRoomNext;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct RoomEx {
        [FieldOffset(0x90)] public IntPtr pLevel;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct Act {
        [FieldOffset(0x14)] public uint MapSeed;
        [FieldOffset(0x20)] public uint ActId;
        [FieldOffset(0x70)] public IntPtr ActMisc;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ActMisc {
        [FieldOffset(0x830)] public Difficulty GameDifficulty;
        [FieldOffset(0x858)] public IntPtr pAct;
        [FieldOffset(0x868)] public IntPtr pLevelFirst;
    }

    public enum Difficulty : ushort {
        Normal = 0,
        Nightmare = 1,
        Hell = 2
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct Level {
        [FieldOffset(0x1F8)] public Area LevelId;
    }

    public enum Area : uint {
        Abaddon = 125,
        AncientTunnels = 65,
        ArcaneSanctuary = 74,
        ArreatPlateau = 112,
        ArreatSummit = 120,
        Barracks = 28,
        BlackMarsh = 6,
        BloodMoor = 2,
        BloodyFoothills = 110,
        BurialGrounds = 17,
        CanyonOfTheMagi = 46,
        CatacombsLevel1 = 34,
        CatacombsLevel2 = 35,
        CatacombsLevel3 = 36,
        CatacombsLevel4 = 37,
        Cathedral = 33,
        CaveLevel1 = 9,
        CaveLevel2 = 13,
        ChaosSanctuary = 108,
        CityOfTheDamned = 106,
        ClawViperTempleLevel1 = 58,
        ClawViperTempleLevel2 = 61,
        ColdPlains = 3,
        Crypt = 18,
        CrystallinePassage = 113,
        DarkWood = 5,
        DenOfEvil = 8,
        DisusedFane = 95,
        DisusedReliquary = 99,
        DrifterCavern = 116,
        DryHills = 42,
        DuranceOfHateLevel1 = 100,
        DuranceOfHateLevel2 = 101,
        DuranceOfHateLevel3 = 102,
        DurielsLair = 73,
        FarOasis = 43,
        FlayerDungeonLevel1 = 88,
        FlayerDungeonLevel2 = 89,
        FlayerDungeonLevel3 = 91,
        FlayerJungle = 78,
        ForgottenReliquary = 96,
        ForgottenSands = 134,
        ForgottenTemple = 97,
        ForgottenTower = 20,
        FrigidHighlands = 111,
        FrozenRiver = 114,
        FrozenTundra = 117,
        FurnaceOfPain = 135,
        GlacialTrail = 115,
        GreatMarsh = 77,
        HallsOfAnguish = 122,
        HallsOfPain = 123,
        HallsOfTheDeadLevel1 = 56,
        HallsOfTheDeadLevel2 = 57,
        HallsOfTheDeadLevel3 = 60,
        HallsOfVaught = 124,
        HaremLevel1 = 50,
        HaremLevel2 = 51,
        Harrogath = 109,
        HoleLevel1 = 11,
        HoleLevel2 = 15,
        IcyCellar = 119,
        InfernalPit = 127,
        InnerCloister = 32,
        JailLevel1 = 29,
        JailLevel2 = 30,
        JailLevel3 = 31,
        KurastBazaar = 80,
        KurastCauseway = 82,
        KurastDocks = 75,
        LostCity = 44,
        LowerKurast = 79,
        LutGholein = 40,
        MaggotLairLevel1 = 62,
        MaggotLairLevel2 = 63,
        MaggotLairLevel3 = 64,
        MatronsDen = 133,
        Mausoleum = 19,
        MonasteryGate = 26,
        MooMooFarm = 39,
        NihlathaksTemple = 121,
        None = 0,
        OuterCloister = 27,
        OuterSteppes = 104,
        PalaceCellarLevel1 = 52,
        PalaceCellarLevel2 = 53,
        PalaceCellarLevel3 = 54,
        PitLevel1 = 12,
        PitLevel2 = 16,
        PitOfAcheron = 126,
        PlainsOfDespair = 105,
        RiverOfFlame = 107,
        RockyWaste = 41,
        RogueEncampment = 1,
        RuinedFane = 98,
        RuinedTemple = 94,
        SewersLevel1Act2 = 47,
        SewersLevel1Act3 = 92,
        SewersLevel2Act2 = 48,
        SewersLevel2Act3 = 93,
        SewersLevel3Act2 = 49,
        SpiderCave = 84,
        SpiderCavern = 85,
        SpiderForest = 76,
        StonyField = 4,
        StonyTombLevel1 = 55,
        StonyTombLevel2 = 59,
        SwampyPitLevel1 = 86,
        SwampyPitLevel2 = 87,
        SwampyPitLevel3 = 90,
        TalRashasTomb1 = 66,
        TalRashasTomb2 = 67,
        TalRashasTomb3 = 68,
        TalRashasTomb4 = 69,
        TalRashasTomb5 = 70,
        TalRashasTomb6 = 71,
        TalRashasTomb7 = 72,
        TamoeHighland = 7,
        TheAncientsWay = 118,
        ThePandemoniumFortress = 103,
        TheWorldstoneChamber = 132,
        TheWorldStoneKeepLevel1 = 128,
        TheWorldStoneKeepLevel2 = 129,
        TheWorldStoneKeepLevel3 = 130,
        ThroneOfDestruction = 131,
        TowerCellarLevel1 = 21,
        TowerCellarLevel2 = 22,
        TowerCellarLevel3 = 23,
        TowerCellarLevel4 = 24,
        TowerCellarLevel5 = 25,
        Travincal = 83,
        Tristram = 38,
        UberTristram = 136,
        UndergroundPassageLevel1 = 10,
        UndergroundPassageLevel2 = 14,
        UpperKurast = 81,
        ValleyOfSnakes = 45,
        MapsAncientTemple = 137,
        MapsDesecratedTemple = 138,
        MapsFrigidPlateau = 139,
        MapsInfernalTrial = 140,
        MapsRuinedCitadel = 141,
    }
}

