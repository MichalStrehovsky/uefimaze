#if BFLAT
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
class Math
{
    public static double Sin(double x)
        => x - ((x*x*x)/(6)) + ((x*x*x*x*x)/(120)) - ((x*x*x*x*x*x*x)/(5040));

    public static double Cos(double x)
        => 1 - ((x*x)/(2)) + ((x*x*x*x)/(24)) - ((x*x*x*x*x*x)/(720)) + ((x*x*x*x*x*x*x*x)/(40320));

    public static double Abs(double x)
        => x < 0 ? -x : x;

    public static double Floor(double x) => x >= 0 ? (double)(int)x : ((double)(int)(x - 1)) + 1;
}
#endif
