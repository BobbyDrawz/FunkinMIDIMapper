namespace FMM
{
    internal class ByteEvent : Event
    {
        public ByteEvent(byte id, byte val)
            : base(id, val)
        {
        }

        public override byte[] ToBytes()
        {
            return new byte[2]
            {
                base.ID,
                (byte)base.Value
            };
        }

        public override string ToString()
        {
            return "[" + GetName(base.ID) + "] " + ((byte)base.Value).ToString("X2") + ")";
        }
    }
}
