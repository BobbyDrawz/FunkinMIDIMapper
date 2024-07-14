using System;

namespace FMM
{
    internal class DwordEvent : Event
    {
        public DwordEvent(byte id, uint val)
            : base(id, val)
        {
        }

        public override byte[] ToBytes()
        {
            byte[] bytes = BitConverter.GetBytes((uint)base.Value);
            return new byte[5]
            {
                base.ID,
                bytes[0],
                bytes[1],
                bytes[2],
                bytes[3]
            };
        }
    }
}
