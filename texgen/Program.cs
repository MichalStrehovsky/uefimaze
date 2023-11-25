using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

// Builds textures.cs from the given PNG file and texture coordinates

using var image = Image.Load<Rgba32>(@"PC Computer - Wolfenstein 3D - Walls.png");

var pix = new Rgba32[image.Width * image.Height];
image.CopyPixelDataTo(pix);

int palIndex = 0;
var palette = new Dictionary<Rgba32, int>();

int[][] textures = [[2, 2], [2, 1], [4, 2], [0, 2], [2, 16]];

foreach (var t in textures)
    AddToPalette(t[0], t[1]);


void AddToPalette(int x0, int y0)
{
    for (int y = y0 * 64; y < y0 * 64 + 64; y++)
        for (int x = x0 * 64; x < x0 * 64 + 64; x++)
            if (!palette.ContainsKey(pix[x + y * image.Width]))
                palette.Add(pix[x + y * image.Width], palIndex++);
}

Rgba32[] pal = new Rgba32[palette.Count];
foreach (var p in palette)
    pal[p.Value] = p.Key;

Console.WriteLine("using System;");
Console.WriteLine();
Console.WriteLine("partial class Program");
Console.WriteLine("{");
Console.Write("    static ReadOnlySpan<byte> Palette => [");
foreach (var p in pal)
    Console.Write($"0x{p.B:X}, 0x{p.G:X}, 0x{p.R:X}, ");
Console.WriteLine("];");
Console.WriteLine();
Console.WriteLine("    static ReadOnlySpan<byte> GetTexture(int index)");
Console.WriteLine("    {");
for (int i = 0; i < textures.Length; i++)
{
    if (i != textures.Length - 1)
        Console.Write($"        if (index == {i}){Environment.NewLine}    ");
    Console.Write("        return [");
    for (int y = textures[i][1] * 64; y < textures[i][1] * 64 + 64; y++)
        for (int x = textures[i][0] * 64; x < textures[i][0] * 64 + 64; x++)
            Console.Write($"0x{palette[pix[x + y * image.Width]]:X}, ");
    Console.WriteLine("];");
}
Console.WriteLine("    }");
Console.WriteLine("}");
