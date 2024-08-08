using System;
using System.Collections.Generic;
using System.Linq;

namespace FMM
{
    internal class ArrayEvent : Event
    {
        private static byte[] GatherBytes(byte[] b, ref uint i)
        {
            int num = b[i] & 0x7F;
            int num2 = 0;
            while ((b[i] & 0x80u) != 0)
            {
                i++;
                num |= (b[i] & 0x7F) << (num2 += 7);
            }
            i++;
            return b.Skip((int)i).Take(num).ToArray();
        }

        public ArrayEvent(byte id, byte[] b, ref uint i)
            : base(id, GatherBytes(b, ref i))
        {
        }

        public override byte[] ToBytes()
        {
            List<byte> list = new List<byte> { base.ID };
            List<byte> list2 = new List<byte>();
            int num = ((byte[])base.Value).Length;
            while (num > 0)
            {
                list2.Add((byte)((uint)num & 0x7Fu));
                num >>= 7;
                if (num > 0)
                {
                    list2[list2.Count - 1] += 128;
                }
            }
            list.AddRange(list2);
            list.AddRange((byte[])base.Value);
            return list.ToArray();
        }

        public override string ToString()
        {
            return "[" + GetName(base.ID) + "] " + BitConverter.ToString((byte[])base.Value).Replace("-", "") + ")";
        }
    }
}
