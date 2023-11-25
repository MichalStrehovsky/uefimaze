using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

unsafe partial class Program
{
    private static Guid EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID => new Guid(0x9042a9de, 0x23dc, 0x4a38, 0x96, 0xfb, 0x7a, 0xde, 0xd0, 0x80, 0x51, 0x6a);

    static void Main()
    {
        Console.WriteLine("****************************");
        Console.WriteLine("****** Hello from C#! ******");
        Console.WriteLine("****************************");

        var efiSys = (EFI_SYSTEM_TABLE*)GetEfiSystemTable(null);

        EFI_GRAPHICS_OUTPUT_PROTOCOL* gop;
        var status = efiSys->BootServices->LocateProtocol(EFI_GRAPHICS_OUTPUT_PROTOCOL_GUID, null, (void**)&gop);
        if (status != 0)
            Fail("LocateProtocol", status);

        EFI_GRAPHICS_OUTPUT_MODE_INFORMATION *info;
        nuint SizeOfInfo, nativeMode;
        status = gop->QueryMode(gop, gop->Mode == null ? 0 : gop->Mode->Mode, &SizeOfInfo, &info);
        if (status != 0)
            status = gop->SetMode(gop, 0);
        if (status != 0)
            Fail("Query(Set)Mode", status);

        uint bestMode = 0xFFFFFFFF;
        uint bestHRes = 0, bestVRes = 0;
        for (uint i = 0; i < gop->Mode->MaxMode; i++)
        {
            status = gop->QueryMode(gop, i, &SizeOfInfo, &info);
            if (status != 0)
                Fail("QueryMode", status);

            if (info->HorizontalResolution >= 640 && info->VerticalResolution >= 480)
            {
                if (bestMode == 0xFFFFFFFF
                    || (info->HorizontalResolution < bestHRes && info->VerticalResolution < bestVRes))
                {
                    bestMode = i;
                    bestHRes = info->HorizontalResolution;
                    bestVRes = info->VerticalResolution;
                }
            }
        }
        if (bestMode == 0xFFFFFFFF)
            Fail("No usable display mode found", 0);

        byte[] fb = new byte[640 * 480 * 4];

        System.Threading.Thread.Sleep(2000);

        fixed (byte* pBuffer = fb)
        {
            status = gop->SetMode(gop, bestMode);
            if (status != 0)
                Fail("Set", status);

            uint gameTime = 0;
            while (true)
            {
                keyState = default;
                if (Console.KeyAvailable)
                    keyState = Console.ReadKey(false).Key switch
                    {
                        ConsoleKey.UpArrow => KeyState.Up,
                        ConsoleKey.DownArrow => KeyState.Down,
                        ConsoleKey.LeftArrow => KeyState.Left,
                        ConsoleKey.RightArrow => KeyState.Right,
                        _ => default,
                    };

                RenderEffect(gameTime, pBuffer);

                status = gop->Blt(gop,
                    (EFI_GRAPHICS_OUTPUT_BLT_PIXEL*)pBuffer,
                    EFI_GRAPHICS_OUTPUT_BLT_OPERATION.EfiBltBufferToVideo,
                    0,
                    0,
                    (bestHRes - 640) / 2,
                    (bestVRes - 480) / 2,
                    640,
                    480,
                    0);

                System.Threading.Thread.Sleep(10);
                gameTime += 10;

                if (status != 0)
                    Fail("Blt", status);
            }
        }

        [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "s_efiSystemTable")]
        static extern ref void* GetEfiSystemTable(object e);

        while (true);
    }

    static void Fail(string msg, ulong x)
    {
        Console.WriteLine(msg);
        if (x == 0) Console.WriteLine("0");
        else Console.WriteLine((int)x);
        while (true) ;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct EFI_HANDLE
    {
        private IntPtr _handle;
    }

    [StructLayout(LayoutKind.Sequential)]
    readonly struct EFI_TABLE_HEADER
    {
        public readonly ulong Signature;
        public readonly uint Revision;
        public readonly uint HeaderSize;
        public readonly uint Crc32;
        public readonly uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe readonly struct EFI_SYSTEM_TABLE
    {
        public readonly EFI_TABLE_HEADER Hdr;
        public readonly char* FirmwareVendor;
        public readonly uint FirmwareRevision;
        private readonly void* _pad0;
        private readonly void* _pad1;
        private readonly void* _pad2;
        private readonly void* _pad3;
        private readonly void* _pad4;
        private readonly void* _pad5;
        public readonly EFI_RUNTIME_SERVICES* RuntimeServices;
        public readonly EFI_BOOT_SERVICES* BootServices;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe readonly struct EFI_RUNTIME_SERVICES
    {
        public readonly EFI_TABLE_HEADER Hdr;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe readonly struct EFI_BOOT_SERVICES
    {
        readonly EFI_TABLE_HEADER Hdr;
        private readonly void* pad0;
        private readonly void* pad1;
        private readonly void* pad2;
        private readonly void* pad3;
        private readonly void* pad4;
        private readonly void* pad5;
        private readonly void* pad6;
        private readonly void* pad7;
        private readonly void* pad8;
        private readonly void* pad9;
        private readonly void* pad10;
        private readonly void* pad11;
        private readonly void* pad12;
        private readonly void* pad13;
        private readonly void* pad14;
        private readonly void* pad15;
        private readonly void* pad16;
        private readonly void* pad17;
        private readonly void* pad18;
        private readonly void* pad19;
        private readonly void* pad20;
        private readonly void* pad21;
        private readonly void* pad22;
        private readonly void* pad23;
        private readonly void* pad24;
        private readonly void* pad25;
        private readonly void* pad26;
        private readonly void* pad27;
        private readonly void* pad28;
        private readonly void* pad29;
        private readonly void* pad30;
        private readonly void* pad31;
        private readonly void* pad32;
        private readonly void* pad33;
        private readonly void* pad34;
        private readonly void* pad35;
        private readonly void* pad36;
        public readonly delegate* unmanaged<in Guid, void*, void**, ulong> LocateProtocol;
    }

    struct EFI_GRAPHICS_OUTPUT_PROTOCOL
    {
        public readonly delegate* unmanaged<EFI_GRAPHICS_OUTPUT_PROTOCOL*, uint, nuint*, EFI_GRAPHICS_OUTPUT_MODE_INFORMATION**, ulong> QueryMode;
        public readonly delegate* unmanaged<EFI_GRAPHICS_OUTPUT_PROTOCOL*, uint, ulong> SetMode;
        public readonly delegate* unmanaged<EFI_GRAPHICS_OUTPUT_PROTOCOL*, EFI_GRAPHICS_OUTPUT_BLT_PIXEL*, EFI_GRAPHICS_OUTPUT_BLT_OPERATION, nuint, nuint, nuint, nuint, nuint, nuint, nuint, ulong> Blt;
        public EFI_GRAPHICS_OUTPUT_PROTOCOL_MODE *Mode;
    }

    struct EFI_GRAPHICS_OUTPUT_BLT_PIXEL
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Reserved;
    }

    enum EFI_GRAPHICS_OUTPUT_BLT_OPERATION
    {
        EfiBltVideoFill,
        EfiBltVideoToBltBuffer,
        EfiBltBufferToVideo,
        EfiBltVideoToVideo,
        EfiGraphicsOutputBltOperationMax
    }

    struct EFI_GRAPHICS_OUTPUT_MODE_INFORMATION
    {
        public uint Version;
        public uint HorizontalResolution;
        public uint VerticalResolution;
        public EFI_GRAPHICS_PIXEL_FORMAT PixelFormat;
        public EFI_PIXEL_BITMASK PixelInformation;
        public uint PixelsPerScanLine;
    }

    enum EFI_GRAPHICS_PIXEL_FORMAT
    {
        PixelRedGreenBlueReserved8BitPerColor,
        PixelBlueGreenRedReserved8BitPerColor,
        PixelBitMask,
        PixelBltOnly,
        PixelFormatMax
    }

    struct EFI_PIXEL_BITMASK
    {
        public uint RedMask;
        public uint GreenMask;
        public uint BlueMask;
        public uint ReservedMask;
    }

    enum EFI_PHYSICAL_ADDRESS : ulong { }

    struct EFI_GRAPHICS_OUTPUT_PROTOCOL_MODE
    {
        public uint MaxMode;
        public uint Mode;
        public EFI_GRAPHICS_OUTPUT_MODE_INFORMATION *Info;
        public nuint SizeOfInfo;
        public EFI_PHYSICAL_ADDRESS FrameBufferBase;
        public nuint FrameBufferSize;
    }
}

namespace System.Runtime.CompilerServices
{
    public enum UnsafeAccessorKind { Constructor, Method, StaticMethod, Field, StaticField }

    public sealed class UnsafeAccessorAttribute : Attribute
    {
        public UnsafeAccessorAttribute(UnsafeAccessorKind kind) { }
        public string Name { get; set; }
    }
}
