using System.Runtime.InteropServices;
using SpiralNeo;

namespace Info.SpiralFramework.Neo.Interfaces
{
    // [StructLayout(LayoutKind.Sequential)]
    // public struct Dr1GameStateData
    // {
    //     public short TimeOfDay;
    //     public short NameColour;
    //     public short VibrationLower;
    //     public short VibrationUpper;
    //     public short ColourSet;
    //     public short Wait;
    //     public short WaitForce;
    //     public short Case;
    //     public short MapLoadPlayerPlacement;
    //     public short SCDATA_WAK_NONSTOP_MODE_CHK;
    //     public short LastUsedEvidence;
    //     public short SCDATA_WAK_MAXTIME;
    //     public short SkillPoints;
    //     public short SCDATA_WAK_SPEAK_ADD;
    //     public short SCDATA_WAK_RANDOM;
    //     public short Gamemode;
    //     public short SCDATA_WAK_MONOKUMA_MEDAL;
    //     public short UnlockedRulePages;
    //     public short ActionDifficulty;
    //     public short LogicDifficulty;
    // }

    public static class Dr1GameStateData
    {
        public static short TimeOfDay
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (0 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (0 << 1), value);
        }

        public static short NameColour
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (1 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (1 << 1), value);
        }

        public static short VibrationLower
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (2 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (2 << 1), value);
        }

        public static short VibrationUpper
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (3 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (3 << 1), value);
        }

        public static short ColourSet
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (4 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (4 << 1), value);
        }

        public static short Wait
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (5 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (5 << 1), value);
        }

        public static short WaitForce
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (6 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (6 << 1), value);
        }

        public static short Case
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (7 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (7 << 1), value);
        }

        public static short MapLoadPlayerPlacement
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (8 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (8 << 1), value);
        }

        public static short SCDATA_WAK_NONSTOP_MODE_CHK
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (9 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (9 << 1), value);
        }
        public static short LastUsedEvidence
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (10 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (10 << 1), value);
        }
        public static short SCDATA_WAK_MAXTIME
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (11 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (11 << 1), value);
        }
        public static short SkillPoints
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (12 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (12 << 1), value);
        }
        public static short SCDATA_WAK_SPEAK_ADD
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (13 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (13 << 1), value);
        }
        public static short SCDATA_WAK_RANDOM
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (14 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (14 << 1), value);
        }
        public static short Gamemode
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (15 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (15 << 1), value);
        }
        public static short SCDATA_WAK_MONOKUMA_MEDAL
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (16 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (16 << 1), value);
        }
        public static short UnlockedRulePages
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (17 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (17 << 1), value);
        }
        public static short ActionDifficulty
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (18 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (18 << 1), value);
        }
        public static short LogicDifficulty
        {
            get => Marshal.ReadInt16(Dr1Addresses.GameStateData + (19 << 1));
            set => Marshal.WriteInt16(Dr1Addresses.GameStateData + (19 << 1), value);
        }
    }
}