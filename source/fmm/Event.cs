namespace FMM
{
    internal abstract class Event
    {
        public byte ID { get; }

        public object Value { get; set; }

        public Event(byte id, object val)
        {
            ID = id;
            Value = val;
        }

        public abstract byte[] ToBytes();

        public override string ToString()
        {
            return $"[{ID}] {Value})";
        }

        public string GetName(byte i)
        {
            return i.ToString("X2") + "\tunknown";
        }
    }
}
