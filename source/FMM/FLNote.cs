namespace FMM
{
    public struct MidiNote
    {
        public uint Time;       // Tick or timestamp when the note event occurs
        public uint Duration;   // Duration of the note in ticks
        public byte Channel;    // MIDI channel (0-15)
        public byte NoteNumber; // MIDI note number (0-127)
        public byte Velocity;   // Note velocity (0-127)
        public byte Release;    // Release velocity (if applicable)
        public byte Flags;      // Additional flags or status
        public byte Panning;    // Panning position (if applicable)
        public byte ModX;       // Modulation X (if applicable)
        public byte ModY;       // Modulation Y (if applicable)
        public uint Pitch;      // Add this line to include the Pitch property

        public MidiNote(uint time, uint duration, byte channel, byte noteNumber, byte velocity)
        {
            Time = time;
            Duration = duration;
            Channel = channel;
            NoteNumber = noteNumber;
            Velocity = velocity;
            Release = 0;
            Flags = 0;
            Panning = 0;
            ModX = 0;
            ModY = 0;
            Pitch = 0;  // Initialize Pitch
        }
    }
}
