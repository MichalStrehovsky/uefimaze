using System.Runtime.InteropServices;

namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Guid
    {
        private readonly int _a;
        private readonly short _b;
        private readonly short _c;
        private readonly byte _d;
        private readonly byte _e;
        private readonly byte _f;
        private readonly byte _g;
        private readonly byte _h;
        private readonly byte _i;
        private readonly byte _j;
        private readonly byte _k;

        public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _a = (int)a;
            _b = (short)b;
            _c = (short)c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }
    }
}
