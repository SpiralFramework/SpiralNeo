using System.Runtime.InteropServices;

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

    public static unsafe class Dr1GameStateData
    {
        public static short TimeOfDay
        {
            get => *(Dr1Addresses.GameStateData);
            set => *(Dr1Addresses.GameStateData) = value;
        }

        public static short NameColour
        {
            get => *(Dr1Addresses.GameStateData + 1);
            set => *(Dr1Addresses.GameStateData + 1) = value;
        }

        public static short VibrationLower
        {
            get => *(Dr1Addresses.GameStateData + 2);
            set => *(Dr1Addresses.GameStateData + 2) = value;
        }

        public static short VibrationUpper
        {
            get => *(Dr1Addresses.GameStateData + 3);
            set => *(Dr1Addresses.GameStateData + 3) = value;
        }

        public static short ColourSet
        {
            get => *(Dr1Addresses.GameStateData + 4);
            set => *(Dr1Addresses.GameStateData + 4) = value;
        }

        public static short Wait
        {
            get => *(Dr1Addresses.GameStateData + 5);
            set => *(Dr1Addresses.GameStateData + 5) = value;
        }

        public static short WaitForce
        {
            get => *(Dr1Addresses.GameStateData + 6);
            set => *(Dr1Addresses.GameStateData + 6) = value;
        }

        public static short Case
        {
            get => *(Dr1Addresses.GameStateData + 7);
            set => *(Dr1Addresses.GameStateData + 7) = value;
        }

        public static short MapLoadPlayerPlacement
        {
            get => *(Dr1Addresses.GameStateData + 8);
            set => *(Dr1Addresses.GameStateData + 8) = value;
        }

        public static short SCDATA_WAK_NONSTOP_MODE_CHK
        {
            get => *(Dr1Addresses.GameStateData + 9);
            set => *(Dr1Addresses.GameStateData + 9) = value;
        }

        public static short LastUsedEvidence
        {
            get => *(Dr1Addresses.GameStateData + 10);
            set => *(Dr1Addresses.GameStateData + 10) = value;
        }

        public static short SCDATA_WAK_MAXTIME
        {
            get => *(Dr1Addresses.GameStateData + 11);
            set => *(Dr1Addresses.GameStateData + 11) = value;
        }

        public static short SkillPoints
        {
            get => *(Dr1Addresses.GameStateData + 12);
            set => *(Dr1Addresses.GameStateData + 12) = value;
        }

        public static short SCDATA_WAK_SPEAK_ADD
        {
            get => *(Dr1Addresses.GameStateData + 13);
            set => *(Dr1Addresses.GameStateData + 13) = value;
        }

        public static short SCDATA_WAK_RANDOM
        {
            get => *(Dr1Addresses.GameStateData + 14);
            set => *(Dr1Addresses.GameStateData + 14) = value;
        }

        public static short Gamemode
        {
            get => *(Dr1Addresses.GameStateData + 15);
            set => *(Dr1Addresses.GameStateData + 15) = value;
        }

        public static short SCDATA_WAK_MONOKUMA_MEDAL
        {
            get => *(Dr1Addresses.GameStateData + 16);
            set => *(Dr1Addresses.GameStateData + 16) = value;
        }

        public static short UnlockedRulePages
        {
            get => *(Dr1Addresses.GameStateData + 17);
            set => *(Dr1Addresses.GameStateData + 17) = value;
        }

        public static short ActionDifficulty
        {
            get => *(Dr1Addresses.GameStateData + 18);
            set => *(Dr1Addresses.GameStateData + 18) = value;
        }

        public static short LogicDifficulty
        {
            get => *(Dr1Addresses.GameStateData + 19);
            set => *(Dr1Addresses.GameStateData + 19) = value;
        }
    }
}