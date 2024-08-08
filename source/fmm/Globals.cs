using System.Collections.Generic;

namespace FMM
{
    internal static class Globals
    {
        public const int VersionNumber = 2;
        public const int NoteSize = 24;

        // Default pulses per quarter note for MIDI
        public static ushort ppqn = 96;

        public static string name = "";

        // BPM information
        public static float bpm = 120f;  // Default BPM if not set
        public static List<float> bpmList = new List<float>();

        // Time signature information
        public static int timeSignatureNumerator = 4;   // Default time signature numerator
        public static int timeSignatureDenominator = 4; // Default time signature denominator

        public static int needsVoices = 0;

        public static string player1 = "";
        public static string player2 = "";

        // Speed parameter
        public static float speed = 1.0f;

        // Add any other relevant global parameters for MIDI handling here
    }
}
