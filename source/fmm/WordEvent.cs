using System;

namespace FMM
{
    internal class WordEvent : Event
    {
        public WordEvent(byte id, ushort val)
            : base(id, val)
        {
        }

        public override byte[] ToBytes()
        {
            byte[] bytes = BitConverter.GetBytes((ushort)base.Value);
            return new byte[3]
            {
                base.ID,
                bytes[0],
                bytes[1]
            };
        }
    }
}
