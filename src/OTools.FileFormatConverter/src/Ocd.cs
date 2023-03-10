using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;

namespace OTools.FileFormatConverter;

#region Version 6/7/8

#region Types

file interface IOcdObj { }

/*
OCAD is written in 32-bit Delphi and this description uses the
names for the data types as they appear in Delphi. However the
same data types are available in other development systems
like C++.

longint       32-bit signed integer 
SmallInt      16-bit signed integer 
WordBool      16-bit boolean
String[x]     Pascal-style string. The first byte contains
              the number of characters followed by the characters.
              The string is not zero-terminated. The maximum
              number of characters is x. It occupies
              x + 1 bytes in the file.
Double        64-bit floating point number
TCord         A special data type used for all coordinates.
              It is defined as

              TCord = record x, y: longint; end;

              The lowest 8 Bits are used to mark special points:
              Marks for the x-coordinate:
                1: this point is the first curve point
                2: this point is the second curve point
                4: for double lines: there is no left line
                   between this point and the next point
              Marks for y-coordinate:
                1: this point is a corner point
                2: this point is the first point of a hole in
                   an area
                4: for double lines: there is no right line
                   between this point and the next point
                8: OCAD 7 only: this point is a dash point
              The upper 24 bits contain the coordinate value
              measured in units of 0.01 mm

Note: all file positions are in bytes starting from the
beginning of the file.
*/

//[StructLayout(LayoutKind.Explicit)]
//file struct FileHeader : IOcdObj
//{
//    /*
//      TFileHeader = record
//        OCADMark: SmallInt;        {3245 (hex 0cad)}
//        SectionMark: SmallInt;     {OCAD 6: 0
//                                    OCAD 7: 7
//                                    OCAD 8: 2 for normal files
//                                            3 for course setting files}
//        Version: SmallInt;         {6 for OCAD 6, 7 for OCAD 7, 8 for OCAD 8}
//        Subversion: SmallInt;      {number of subversion (0 for 6.00,
//                                   1 for 6.01 etc.)}
//        FirstSymBlk: longint;      {file position of the first symbol
//                                   block}
//        FirstIdxBlk: longint;      {file position of the first index
//                                   block}
//        SetupPos: longint;         {file position of the setup record }
//        SetupSize: longint;        {size (in bytes) of the setup record}
//        InfoPos: longint;          {file position of the file
//                                   information. The file information is
//                                   stored as a zero-terminated string with
//                                   up to 32767 characters + terminating
//                                   zero}
//        InfoSize: longint;         {size (in bytes) of the file
//                                   information}
//        FirstStIndexBlk: longint;  {OCAD 8 only. file position of the first
//                                   string index block}
//        Reserved2: longint;
//        Reserved3: longint;
//        Reserved4: longint;
//      end;
//     */

//    [FieldOffset(0)]
//    public short OCADMark;      // 3245 (hex 0cad)
//    [FieldOffset(2)]
//    public short SectionMark;   // OCAD 6: 0
//                                // OCAD 7: 7
//                                // OCAD 8: 2 for normal files
//                                //         3 for course setting files
//    [FieldOffset(4)]
//    public short Version;       // 
//    [FieldOffset(6)]
//    public short SubVersion;    // Number of subversion: 0 for 6.00, 1 for 6.01 etc
//    [FieldOffset(8)]
//    public int FirstSymBlk;     // File position of the first symbol block
//    [FieldOffset(12)]
//    public int FirstIdxBlk;     // File position of the first index block
//    [FieldOffset(16)]
//    public int SetupPos;        // File position of the setup record
//    [FieldOffset(20)]
//    public int SetupSize;       // Size (in bytes) of the setup record
//    [FieldOffset(24)]
//    public int InfoPos;         // File position of the file information.
//                                // The file information is stored as a zero-terminated string
//                                // with up to 32767 characters + terminating zero
//    [FieldOffset(28)]
//    public int InfoSize;        // Size (in bytes) of the file information
//    [FieldOffset(32)]
//    public int FirstStIndexBlk; // OCAD 8 only. File position of the first string index block
//    [FieldOffset(36)]
//    public int Reserved2;
//    [FieldOffset(40)]
//    public int Reserved3;
//    [FieldOffset(44)]
//    public int Reserved4;
//}

[StructLayout(LayoutKind.Sequential)]
file struct v6_FileHeader : IOcdObj
{
    /*
      TFileHeader = record
        OCADMark: SmallInt;        {3245 (hex 0cad)}
        SectionMark: SmallInt;     {OCAD 6: 0
                                    OCAD 7: 7
                                    OCAD 8: 2 for normal files
                                            3 for course setting files}
        Version: SmallInt;         {6 for OCAD 6, 7 for OCAD 7, 8 for OCAD 8}
        Subversion: SmallInt;      {number of subversion (0 for 6.00,
                                   1 for 6.01 etc.)}
        FirstSymBlk: longint;      {file position of the first symbol
                                   block}
        FirstIdxBlk: longint;      {file position of the first index
                                   block}
        SetupPos: longint;         {file position of the setup record }
        SetupSize: longint;        {size (in bytes) of the setup record}
        InfoPos: longint;          {file position of the file
                                   information. The file information is
                                   stored as a zero-terminated string with
                                   up to 32767 characters + terminating
                                   zero}
        InfoSize: longint;         {size (in bytes) of the file
                                   information}
        FirstStIndexBlk: longint;  {OCAD 8 only. file position of the first
                                   string index block}
        Reserved2: longint;
        Reserved3: longint;
        Reserved4: longint;
      end;
     */

    public short OCADMark;      // 3245 (hex 0cad)
    public short SectionMark;   // OCAD 6: 0
                                // OCAD 7: 7
                                // OCAD 8: 2 for normal files
                                //         3 for course setting files
    public short Version;       // As expected
    public short SubVersion;    // Number of subversion: 0 for 6.00, 1 for 6.01 etc
    public int FirstSymBlk;     // File position of the first symbol block
    public int FirstIdxBlk;     // File position of the first index block
    public int SetupPos;        // File position of the setup record
    public int SetupSize;       // Size (in bytes) of the setup record
    public int InfoPos;         // File position of the file information.
                                // The file information is stored as a zero-terminated string
                                // with up to 32767 characters + terminating zero
    public int InfoSize;        // Size (in bytes) of the file information
    public int FirstStIndexBlk; // OCAD 8 only. File position of the first string index block
    public int Reserved2;
    public int Reserved3;
    public int Reserved4;
}

//[StructLayout(LayoutKind.Explicit)]
//file struct SymHeader : IOcdObj
//{
//    /*
//      TSymHeader = record
//        nColors: SmallInt;         {Number of colors defined}
//        nColorSep: SmallInt;       {Number or color separations
//                                   defined}
//        CyanFreq: SmallInt;        {Halftone frequency of the
//                                   Cyan color separation. This
//                                   is 10 times the value entered
//                                   in the CMYK Separations dialog
//                                   box.}
//        CyanAng: SmallInt;         {Halftone angle of the cyan
//                                   color separation. This is 10 times
//                                   the value entered in the CMYK
//                                   separations dialog box.}
//        MagentaFreq: SmallInt;     {dito for magenta}
//        MagentaAng: SmallInt;      {dito for magenta}
//        YellowFreq: SmallInt;      {dito for yellow}
//        YellowAng: SmallInt;       {dito for yellow}
//        BlackFreq: SmallInt;       {dito for black}
//        BlackAng: SmallInt;        {dito for black}
//        Res1: SmallInt;
//        Res2: SmallInt;
//        aColorInfo: array [0..255] of TColorInfo;
//                                   {the TColorInfo classure is
//                                   explained below}
//        aColorSep: array [0..31] of TColorSep;
//                                   {the TColorSep classure is
//                                   explained below. Note that only
//                                   24 color separations are allowed.
//                                   The rest is reserved for future
//                                   use.}
//      end;
//    */

//    [FieldOffset(0)]
//    public short nColors;           // Number of colors defined
//    [FieldOffset(2)]
//    public short nColorSep;         // Number of color seperations defined
//    [FieldOffset(4)]
//    public short CyanFreq;          // Halftone frequency of the Cyan color seperation.
//                                    // This is 10 times the value entered in the CMYK Separations dialog box
//    [FieldOffset(6)]
//    public short CyanAng;           // Halftone angle of the Cyan color seperation.
//                                    // This is 10 times the value entered in the CMYK Separations dialog box
//    [FieldOffset(6)]
//    public short MagentaFreq;
//    [FieldOffset(8)]
//    public short MagentaAng;
//    [FieldOffset(10)]
//    public short YellowFreq;
//    [FieldOffset(14)]
//    public short YllowAng;
//    [FieldOffset(16)]
//    public short BlackFreq;
//    [FieldOffset(18)]
//    public short BlackAng;
//    [FieldOffset(20)]
//    public short Res1;
//    [FieldOffset(22)]
//    public short Res2;
//    [FieldOffset(24)]
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
//    public ColorInfo[] aColorInfo;  // (256) explained below
//    [FieldOffset(96)] // TBD
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
//    public ColorSep[] aColorSep;    // (32)  explained below. Only 24 allowed
//}

[StructLayout(LayoutKind.Sequential)]
file struct v6_SymHeader : IOcdObj
{
    /*
      TSymHeader = record
        nColors: SmallInt;         {Number of colors defined}
        nColorSep: SmallInt;       {Number or color separations
                                   defined}
        CyanFreq: SmallInt;        {Halftone frequency of the
                                   Cyan color separation. This
                                   is 10 times the value entered
                                   in the CMYK Separations dialog
                                   box.}
        CyanAng: SmallInt;         {Halftone angle of the cyan
                                   color separation. This is 10 times
                                   the value entered in the CMYK
                                   separations dialog box.}
        MagentaFreq: SmallInt;     {dito for magenta}
        MagentaAng: SmallInt;      {dito for magenta}
        YellowFreq: SmallInt;      {dito for yellow}
        YellowAng: SmallInt;       {dito for yellow}
        BlackFreq: SmallInt;       {dito for black}
        BlackAng: SmallInt;        {dito for black}
        Res1: SmallInt;
        Res2: SmallInt;
        aColorInfo: array [0..255] of TColorInfo;
                                   {the TColorInfo classure is
                                   explained below}
        aColorSep: array [0..31] of TColorSep;
                                   {the TColorSep classure is
                                   explained below. Note that only
                                   24 color separations are allowed.
                                   The rest is reserved for future
                                   use.}
      end;
    */

    public short nColors;           // Number of colors defined
    public short nColorSep;         // Number of color seperations defined
    public short CyanFreq;          // Halftone frequency of the Cyan color seperation.
                                    // This is 10 times the value entered in the CMYK Separations dialog box
    public short CyanAng;           // Halftone angle of the Cyan color seperation.
                                    // This is 10 times the value entered in the CMYK Separations dialog box
    public short MagentaFreq;
    public short MagentaAng;
    public short YellowFreq;
    public short YllowAng;
    public short BlackFreq;
    public short BlackAng;
    public short Res1;
    public short Res2;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public v6_ColorInfo[] aColorInfo;  // (256) explained below
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public v6_ColorSep[] aColorSep;    // (32)  explained below. Only 24 allowed
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_ColorInfo : IOcdObj
{
    /*
     TColorInfo = record
        ColorNum: SmallInt;        {Color number. This number is
                                   used in the symbols when
                                   referring a color.}
        Reserved: SmallInt;
        Color: TCmyk;              {Color value. The classure
                                   is explained below.}
        ColorName: string[31];     {Description of the color}
        SepPercentage: array [0..31] of byte;
                                   {Definition how the color
                                   appears in the different spot
                                   color separations.
                                     0..200: 2 times the separation
                                       percentage as it appears
                                       in the Color dialog box (to
                                       allow half percents)
                                     255: the color does not
                                       appear in the corresponding
                                       color separation (empty field
                                       in the color dialog box)}
      end;
    */

    public short ColorNum;
    public short Reserved;
    public v6_Cmyk Color;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string ColorName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] SepPercentage; // 32
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_Cmyk : IOcdObj
{
    /*
     TCmyk = record
        cyan: byte;                {2 times the cyan value as it
                                   appears in the Define Color dialog
                                   box (to allow half percents)}
        magenta: byte;             {dito for magenta}
        yellow: byte;              {dito for yellow}
        black: byte;               {dito for black}
      end;
    */

    public byte Cyan;           // 2 times the Cyan value as it appears in the Define Color dialog box (to alllow half %s)
    public byte Magenta;        // ditto
    public byte Yellow;         // ditto
    public byte Black;          // ditto
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_ColorSep : IOcdObj
{
    /*
     TColorSep = record
        SepName: string[15];       {Name of the color separation}
        Color: TCmyk;              {0 in OCAD 6, CMYK value of
                                   the separation in OCAD 7.
                                   This value is only used in
                                   the AI (Adobe Illustrator) export}
        RasterFreq: SmallInt;      {10 times the halfton frequency
                                   as it appears in the Color
                                   Separation dialog box.}
        RasterAngle: SmallInt;     {10 times the halftone angle
                                   as it appears in the Color
                                   Separation dialog box.}
      end;
    */

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string SepName;      // Name of the color seperation
    public v6_Cmyk Color;          // 0 in OCAD 6
                                // CMYK value of the seperation in OCAD 7
                                // Only used in the Adobe Illustrator export
    public short RasterFreq;    // 10 times the halftone frequency as it appears in the Color Seperation dialog box
    public short RasterAngle;   // 10 times the halftone angle as it appears in the Color Seperation dialog box
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_SymbolBlock : IOcdObj
{
    /*
      TSymbolBlock = record
        NextBlock: longint;        {file position of the next symbol
                                   block. 0 if this is the last
                                   symbol block.}
        FilePos: array[0..255] of longint;
                                   {file position of up to 256
                                   symbols. 0 if there is no symbol
                                   for this index.}
      end;
     */

    public int NextBlock;       // File position of the next symbol block.
                                // 0 of this is the last symbol block
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public int[] FilePos;       // File position of up to 256 symbols.
                                // 0 if there is no symbol for this index
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_BaseSym : IOcdObj
{
    /*
      TBaseSym = record
        Size: SmallInt;            {Size of the symbol in bytes. This
                                   depends on the type and the
                                   number of subsymbols.}
        Sym: SmallInt;             {Symbol number. This is 10 times
                                   the value which appears on the
                                   screen (1010 for 101.0)}
        Otp: SmallInt;             {Object type
                                     1: Point symbol
                                     2: Line symbol or Line text
                                        symbol
                                     3: Area symbol
                                     4: Text symbol
                                     5: Rectangle symbol}
        SymTp: byte;               {Symbol type
                                     1: for Line text and text
                                        symbols
                                     0: for all other symbols}
        Flags: byte;               {OCAD 6/7: must be 0
                                    OCAD 8: bit flags
                                      1: not oriented to north (inverted for
                                         better compatibility)
                                      2: Icon is compressed}
        Extent: SmallInt;          {Extent how much the rendered
                                   symbols can reach outside the
                                   coordinates of an object with
                                   this symbol.
                                   For a point object it tells
                                   how far away from the coordinates
                                   of the object anything of the
                                   point symbol can appear.}
        Selected: boolean;         {Symbol is selected in the symbol
                                   box}
        Status: byte;              {Status of the symbol
                                     0: Normal
                                     1: Protected
                                     2: Hidden}
        Res2: SmallInt;
        Res3: SmallInt;
        FilePos: longint;          {File position, not used in the
                                   file, only when loaded in
                                   memory. Value in the file is
                                   not defined.}
        Cols: TColors;             {Set of the colors used in this
                                   symbol. TColors is an array of
                                   32 bytes, where each bit
                                   represents 1 of the 256 colors.
                                     TColors = set of 0..255;
                                   The color with the number 0 in
                                   the color table appears as the
                                   lowest bit in the first byte of
                                   the structure.}
        Description: string [31];  {The description of the symbol}
        IconBits: array[0..263] of byte;
                                   {the icon can be uncompressed (16-bit colors)
                                   or compressed (256 color palette) depending
                                   on the Flags field.
                                   In OCAD 6/7 it is always uncompressed}
      end;
     */

    public short Size;          // Size of the symbol in bytes.
                                // This depends on the type and the number of subsymbols
    public short Sym;           // Symbol number.
                                // This is 10 times the value that appears on screen (1010 for 101.0)
    public short Otp;           // Object type:
                                //  1: Point symbol
                                //  2: Line symbol or Line text symbol
                                //  3: Area symbol
                                //  4: Text symbol
                                //  5: Rectangle symbol
    public byte SymTp;          // Symbol type:
                                //  1: Line text and text symbols
                                //  0: All other symbols
    public byte Flags;          // OCAD 6/7: must be 0
                                // OCAD 8:
                                //  1: not oriented to north (inverted for better compatibility)
                                //  2: Icon is compressed
    public short Extent;        // Extent how much the rendered symbols can reach outside the coordinates of an object with this symbol
    public bool Selected;       // Symbol is selected in the symbol box
    public byte Status;         // Status of the symbol:
                                //  0: Normal
                                //  1: Protected
                                //  2: Hidden
    public short Res2, Res3;
    public int FilePos;         // File position, not used in the file, only when loaded in memory. Value in the file is not defined
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Description;  // The description of the symbol
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 264)]
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_PointSym : IOcdObj
{
    /*
      TPointSym = record
        Size: SmallInt;            {size of this structure including
                                   the objects following this
                                   structure}
        Sym: SmallInt;             {see TBaseSym}
        Otp: SmallInt;             {see TBaseSym}
        SymTp: byte;               {see TBaseSym}
        Flags: byte;               {see TBaseSym}
        Extent: SmallInt;          {see TBaseSym}
        Selected: boolean;         {Symbol is selected in the symbol
                                   box}
        Status: byte;              {Status of the symbol
                                     0: Normal
                                     1: Protected
                                     2: Hidden}
        Res2: SmallInt;
        Res3: SmallInt;
        FilePos: longint;          {see TBaseSym}
        Cols: TColors;             {see TBaseSym}
        Description: string [31];  {see TBaseSym}
        IconBits: array[0..263] of byte;
                                   {see TBaseSym}
        DataSize: SmallInt;        {number of coordinates (each 8 bytes)
                                   which follow this structure,
                                   each object header counts as
                                   2 Coordinates (16 bytes).
                                   The maximum value is 512}
        Reserved: SmallInt;
      end;
     */

    public short Size;          // Size of the symbol in bytes.
                                // This depends on the type and the number of subsymbols
    public short Sym;           // Symbol number.
                                // This is 10 times the value that appears on screen (1010 for 101.0)
    public short Otp;           // Object type:
                                //  1: Point symbol
                                //  2: Line symbol or Line text symbol
                                //  3: Area symbol
                                //  4: Text symbol
                                //  5: Rectangle symbol
    public byte SymTp;          // Symbol type:
                                //  1: Line text and text symbols
                                //  0: All other symbols
    public byte Flags;          // OCAD 6/7: must be 0
                                // OCAD 8:
                                //  1: not oriented to north (inverted for better compatibility)
                                //  2: Icon is compressed
    public short Extent;        // Extent how much the rendered symbols can reach outside the coordinates of an object with this symbol
    public bool Selected;       // Symbol is selected in the symbol box
    public byte Status;         // Status of the symbol:
                                //  0: Normal
                                //  1: Protected
                                //  2: Hidden
    public short Res2, Res3;
    public int FilePos;         // File position, not used in the file, only when loaded in memory. Value in the file is not defined
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Description;  // The description of the symbol
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 264)]
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
    public short DataSize;      // Number of coordinates (each 8 bytes) which follow this structure, each object
                                // hheader counts as 2 coords (16 bytes). The max value is 512
    public short Reserved;
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_SymElt : IOcdObj
{
    /*
      TSymElt = record
        stType: SmallInt;          {type of the object
                                     1: line
                                     2: area
                                     3: circle
                                     4: dot (filled circle)}
        stFlags: word;             {Flags
                                     1: line with round ends}
        stColor: SmallInt;         {color of the object. This is the
                                   number which appears in the Colors
                                   dialog box}
        stLineWidth: SmallInt;     {line width for lines and circles
                                   unit 0.01 mm}
        stDiameter: SmallInt;      {diameter for circles and dots. The
                                   line width is included in this
                                   dimension for circles. Unit 0.01 mm}
        stnPoly: SmallInt;         {number of coordinates}
        stRes1: SmallInt;
        stRes2: SmallInt;
        stPoly: array[0..511] of TCord;
                                   {coordinates of the object}
      end;
     */

    public short stType;        // Type of the object:
                                //  1: Line
                                //  2: Area
                                //  3: Circle
                                //  4: Dot (filled circle)
    public ushort stFlags;      // Flags:
                                //  1: Line with round ends
    public short stColor;       // Color of the object. This is the number that appears in the Colors dialog box
    public short stLineWidth;   // Line width for lines and circles (unit 0.01mm)
    public short stDiameter;    // Diameter for circles and dots. The line width is included in this dimension for circles (Unit 0.01mm)
    public short stnPoly;       // Number of coordinates
    public short stRes1, stRes2;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
    public v6_Cord[] stPoly;       // Coordinates of the object ([0..511])
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_LineSym : IOcdObj
{
    /*
      TLineSym = record
        Size: SmallInt;            {size of this structure including
                                   the objects following this
                                   structure}
        Sym: SmallInt;             {see TBaseSym}
        Otp: SmallInt;             {2}
        SymTp: byte;               {0}
        Flags: byte;               {see TBaseSym}
        Extent: SmallInt;          {see TBaseSym}
        Selected: boolean;         {Symbol is selected in the symbol
                                   box}
        Status: byte;              {Status of the symbol
                                     0: Normal
                                     1: Protected
                                     2: Hidden}
        Tool: SmallInt;            {OCAD 6: reserved
                                   OCAD 7/8: Preferred drawing tool
                                     0: off
                                     1: Curve mode
                                     2: Ellipse mode
                                     3: Circle mode
                                     4: Rectangular mode
                                     5: Straight line mode
                                     6: Freehand mode}
        Res3: SmallInt;
        FilePos: longint;          {see TBaseSym}
        Cols: TColors;             {see TBaseSym}
        Description: string [31];  {see TBaseSym}
        IconBits: array[0..263] of byte;
                                   {see TBaseSym}
        LineColor: SmallInt;       {Line color}
        LineWidth: SmallInt;       {Line width}
        LineEnds: word;            {true if Round line ends is checked}
        DistFromStart: SmallInt;   {Distance from start}
        DistToEnd: SmallInt;       {Distance to the end}
        MainLength: SmallInt;      {Main length a}
        EndLength: SmallInt;       {End length b}
        MainGap: SmallInt;         {Main gap C}
        SecGap: SmallInt;          {Gap D}
        EndGap: SmallInt;          {Gap E}
        MinSym: SmallInt;          {-1: at least 0 gaps/symbols
                                    0: at least 1 gap/symbol
                                    1: at least 2 gaps/symbols
                                    etc.
                                    for OCAD 6 only the values 0 and 1 are
                                    allowed}
        nPrimSym: SmallInt;        {No. of symbols}
        PrimSymDist: SmallInt;     {Distance}
        DblMode: word;             {Mode (Double line page)}
        DblFlags: word;            {low order bit is set if
                                   Fill is checked}
        DblFillColor: SmallInt;    {Fill color}
        DblLeftColor: SmallInt;    {Left line/Color}
        DblRightColor: SmallInt;   {Right line/Color}
        DblWidth: SmallInt;        {Width}
        DblLeftWidth: SmallInt;    {Left line/Line width}
        DblRightWidth: SmallInt;   {Right line/Line width}
        DblLength: SmallInt;       {Dashed/Distance a}
        DblGap: SmallInt;          {Dashed/Gap}
        DblRes: array[0..2] of SmallInt;
        DecMode: word;             {Decrease mode
                                     0: off
                                     1: decreasing towards the end
                                     2: decreasing towards both ends}
        DecLast: SmallInt;         {Last symbol}
        DecRes: SmallInt;          {Reserved}
        FrColor: SmallInt;         {OCAD 6: reserved
                                    OCAD 7/8: color of the framing line}
        FrWidth: SmallInt;         {OCAD 6: reserved
                                    OCAD 7/8: Line width of the framing line}
        FrStyle: SmallInt;         {OCAD 6: reserved
                                    OCAD 7/8: Line style of the framing line
                                      0: flat cap/bevel join
                                      1: round cap/round join
                                      4: flat cap/miter join}
        PrimDSize: SmallInt;       {number or coordinates (8 bytes)
                                   for the Main symbol A which
                                   follow this structure
                                   Each symbol header counts as
                                   2 coordinates (16 bytes).
                                   The maximum value is 512.}
        SecDSize: SmallInt;        {number or coordinates (8 bytes)
                                   for the Secondary symbol which
                                   follow the Main symbol A
                                   Each symbol header counts as
                                   2 coordinates (16 bytes).
                                   The maximum value is 512.}
        CornerDSize: SmallInt;     {number or coordinates (8 bytes)
                                   for the Corner symbol which
                                   follow the Secondary symbol
                                   Each symbol header counts as
                                   2 coordinates (16 bytes).
                                   The maximum value is 512.}
        StartDSize: SmallInt;      {number or coordinates (8 bytes)
                                   for the Start symbol C which
                                   follow the Corner symbol
                                   Each symbol header counts as
                                   2 coordinates (16 bytes).
                                   The maximum value is 512.}
        EndDSize: SmallInt;        {number or coordinates (8 bytes)
                                   for the End symbol D which
                                   follow the Start symbol C
                                   Each symbol header counts as
                                   2 coordinates (16 bytes).
                                   The maximum value is 512.}
        Reserved: SmallInt;
      end;
     */

    public short Size;          // Size of the symbol in bytes.
                                // This depends on the type and the number of subsymbols
    public short Sym;           // Symbol number.
                                // This is 10 times the value that appears on screen (1010 for 101.0)
    public short Otp;           // Object type:
                                //  1: Point symbol
                                //  2: Line symbol or Line text symbol
                                //  3: Area symbol
                                //  4: Text symbol
                                //  5: Rectangle symbol
    public byte SymTp;          // Symbol type:
                                //  1: Line text and text symbols
                                //  0: All other symbols
    public byte Flags;          // OCAD 6/7: must be 0
                                // OCAD 8:
                                //  1: not oriented to north (inverted for better compatibility)
                                //  2: Icon is compressed
    public short Extent;        // Extent how much the rendered symbols can reach outside the coordinates of an object with this symbol
    public bool Selected;       // Symbol is selected in the symbol box
    public byte Status;         // Status of the symbol:
                                //  0: Normal
                                //  1: Protected
                                //  2: Hidden
    public short Tool;          // OCAD 6: Reserved
                                // OCAD 7/8: Preferred drawing tool
                                //  0: Off
                                //  1: Curve mode
                                //  2: Ellipse mode
                                //  3: Circle mode
                                //  4: Rectangular mode
                                //  5: Straight line mode
                                //  6: Freehand mode
    public short Res3;
    public int FilePos;         // File position, not used in the file, only when loaded in memory. Value in the file is not defined
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Description;  // The description of the symbol
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 264)]
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
    public short LineColor;     // Line color
    public short LineWidth;     // Line width
    public bool LineEnds;       // True if round line ends is checked
    public short DistFromStart; // Distance from start
    public short DistToEnd;     // Distance to the end
    public short MainLength;    // Main length a
    public short EndLength;     // End length b
    public short MainGap;       // Main gap c
    public short SecGap;        // Gap d
    public short EndGap;        // Gap e
    public short MinSym;        // -1: at least 0 gaps/symbols
                                //  0: at least 1 gap /symbol
                                //  1: at least 2 gaps/symbols
    public short nPrimSym;      // Number of symbols
    public short PrimSymDist;   // Distance
    public ushort DblMode;      // Mode (Double line page)
    public ushort DblFlags;     // Low order bit is set if fill is checked
    public short DbFillColor;   // Fill color
    public short DblLeftColor;  // Left line / Color
    public short DblRightColor; // Right line / Color
    public short DblWidth;      // Width
    public short DblLeftWidth;  // Left line / Line width
    public short DblRightWidth; // Right line / Line width
    public short DblLength;     // Dashed / Distance a
    public short DblGap;        // Dashed / Gap
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public short[] DblRes;
    public ushort DecMode;      // Decrease mode: (ie. gullys?)
                                //  0: off
                                //  1: decreasing towards the end
                                //  2: decreasing towards both ends
    public short DecLast;       // Last Symbol
    public short DecRes;
    public short FrColor;       // OCAD 6: Reserved
                                // OCAD 7/8: Color of the framing line
    public short FrWidth;       // OCAD 6: Reserved
                                // OCAD 7/8: Line width of the framing line
    public short FrStyle;       // OCAD 6: Reserved
                                // OCAD 7/8: Line style of the framing line
                                //  0: flat cap  / bevel join
                                //  1: round cap / round join
                                //  2: flat cap  / miter join
    public short PrimDSize;     // Number of coordinates (8 bytes) for the main symbol A which
                                // follow this this struct. Each symbol header counts as 2 coords (16 bytes). Max 512
    public short SecDSize;      // Number of coordinates (8 bytes) for the Secondary symbol which follows the
                                // main symbol A. Each symbol header counts as 2 coords (16 bytes). Max 512
    public short CornerDSize;   // Number of coordinates (8 bytes) for the Corner symbol which follows the
                                // secondary symbol. Each symbol header counts as 2 coords (16 bytes). Max 512 
    public short StartDSize;    // Number of coordinates (8 bytes) for the Start symbol C which follows the
                                // corner symbol. Each symbol header counts as 2 coords (16 bytes). Max 512
    public short EndDSize;      // Number of coordinates (8 bytes) for the End Symbol D which follows the
                                // start symbol c. Each symbol header counts as 2 coords (16 bytes). Max 512
    public short Reserved;
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_LTextSym : IOcdObj
{
    /*
      TLTextSym = record
        Size: SmallInt;            {size of this structure}
        Sym: SmallInt;             {see TBaseSym}
        Otp: SmallInt;             {2}
        SymTp: byte;               {1}
        Flags: byte;               {see TBaseSym}
        Extent: SmallInt;          {see TBaseSym}
        Selected: boolean;         {Symbol is selected in the symbol
                                   box}
        Status: byte;              {Status of the symbol
                                     0: Normal
                                     1: Protected
                                     2: Hidden}
        Tool: SmallInt;            {OCAD 6: reserved
                                   OCAD 7/8: Preferred drawing tool
                                     0: off
                                     1: Curve mode
                                     2: Ellipse mode
                                     3: Circle mode
                                     4: Rectangular mode
                                     5: Straight line mode
                                     6: Freehand mode}
        FrWidth: SmallInt;         {OCAD 6: reserved
                                    OCAD 7: Framing width
                                      sorry, this had to be squeezed
                                      in here
                                    OCAD 8: not used}
        FilePos: longint;          {see TBaseSym}
        Cols: TColors;             {see TBaseSym}
        Description: string [31];  {see TBaseSym}
        IconBits: array[0..263] of byte;
                                   {see TBaseSym}
        FontName: string[31];      {TrueType font}
        FontColor: SmallInt;       {Color}
        FontSize: SmallInt;        {10 times the value entered in Size}
        Weight: SmallInt;          {Bold as used in the Windows GDI
                                     400: normal
                                     700: bold}
        Italic: boolean;           {true if Italic is checked}
        CharSet: byte;             {OCAD 6/7: must be 0
                                   {OCAD 8: CharSet of the text, if the text is
                                      not Unicode}
        CharSpace: SmallInt;       {Char. spacing}
        WordSpace: SmallInt;       {Word spacing}
        Alignment: SmallInt;       {Alignment
                                     0: Left
                                     1: Center
                                     2: Right
                                     3: All line}
        FrMode: SmallInt;          {Framing mode
                                     0: no framing
                                     1: framing with a framing font
                                     2: OCAD 7/8 only: framing with a line}
                                   {Note this feature is called
                                   "Second font" in OCAD 6 but
                                   "Framing" in OCAD 7}
        FrName: string[31];        {OCAD 6/7: TrueType font (Second/Framing font)
                                    OCAD 8: not used}
        FrColor: SmallInt;         {Color (Second/Framing font)}
        FrSize: SmallInt;          {OCAD 6/7: Size (Second/Framing font)
                                    OCAD 8: Framing width}
        FrWeight: SmallInt;        {OCAD 6/7: Bold (Second/Framing font)
                                     as used in the Windows GDI
                                       400: normal
                                       700: bold
                                    OCAD 8: not used}
        FrItalic: wordbool;        {OCAD 6/7: true if Italic is checked
                                     (Second/Framing font)
                                    OCAD 8: not used}
        FrOfX: SmallInt;           {OCAD 6/7: Horizontal offset
                                    OCAD 8: not used}
        FrOfY: SmallInt;           {OCAD 6/7: Vertical offset
                                    OCAD 8: not used}
      end;
     */

    public short Size;          // Size of the symbol in bytes.
                                // This depends on the type and the number of subsymbols
    public short Sym;           // Symbol number.
                                // This is 10 times the value that appears on screen (1010 for 101.0)
    public short Otp;           // Object type:
                                //  1: Point symbol
                                //  2: Line symbol or Line text symbol
                                //  3: Area symbol
                                //  4: Text symbol
                                //  5: Rectangle symbol
    public byte SymTp;          // Symbol type:
                                //  1: Line text and text symbols
                                //  0: All other symbols
    public byte Flags;          // OCAD 6/7: must be 0
                                // OCAD 8:
                                //  1: not oriented to north (inverted for better compatibility)
                                //  2: Icon is compressed
    public short Extent;        // Extent how much the rendered symbols can reach outside the coordinates of an object with this symbol
    public bool Selected;       // Symbol is selected in the symbol box
    public byte Status;         // Status of the symbol:
                                //  0: Normal
                                //  1: Protected
                                //  2: Hidden
    public short Tool;          // OCAD 6: Reserved
                                // OCAD 7/8: Preferred drawing tool
                                //  0: Off
                                //  1: Curve mode
                                //  2: Ellipse mode
                                //  3: Circle mode
                                //  4: Rectangular mode
                                //  5: Straight line mode
                                //  6: Freehand mode
    public short FrWidth;       // OCAD 6: Reserved
                                // OCAD 7: Framing width (sorry, had to be squeezed in here)
                                // OCAD 8: Not used
    public int FilePos;         // File position, not used in the file, only when loaded in memory. Value in the file is not defined
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Description;  // The description of the symbol
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 264)]
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string FontName;     // TrueType font
    public short FontColor;     // Color
    public short FontSize;      // 10 times the value in pt
    public short Weight;        // Bold as used in the Windows GDI
                                //  400: normal
                                //  700: bold
    public bool Italic;         // True if Italic is checked
    public byte CharSet;        // OCAD 6/7: Must be 0
                                // OCAD 8: CharSet of the text, if the text is not unicode
    public short CharSpace;     // Character Spacing
    public short WordSpace;     // Word Spacing
    public short Alignment;     // Alignment:
                                //  0: Left
                                //  1: Center
                                //  2: Right
                                //  3: All line
    public short FrMode;        // Framing mode:
                                //  0: No framing
                                //  1: Framing with a framing font
                                //  2: OCAD 7/8 only: framing with a line
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string FrName;       // OCAD 6/7: TrueType font (Second/Framing font)
                                // OCAD 8: Not used
    public short FrColor;       // Color (Second/Framing font)
    public short FrSize;        // OCAD 6/7: Size (Second/Framing font)
                                // OCAD 8: Framing width
    public short FrWeight;      // OCAD 6/7: Bold as used in the Windows GDI
                                //  400: normal
                                //  700: bold
                                // OCAD 7: Not used
    public ushort FrItalic;     // OCAD 6/7: True if Italic is checked (Second/Framing font)
    public short FrOfX;         // OCAD 6/7: Horizontal offset
                                // OCAD 8: Not used
    public short FrOfY;         // OCAD 6/7: Vertical offset
                                // OCAD 8: Not used
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_AreaSym : IOcdObj
{
    /*
      TAreaSym = record
        Size: SmallInt;            {size of this structure including
                                   the objects following this
                                   structure}
        Sym: SmallInt;             {see TBaseSym}
        Otp: SmallInt;             {3}
        SymTp: byte;               {0}
        Flags: byte;               {see TBaseSym}
        Extent: SmallInt;          {see TBaseSym}
        Selected: boolean;         {Symbol is selected in the symbol
                                   box}
        Status: byte;              {Status of the symbol
                                     0: Normal
                                     1: Protected
                                     2: Hidden}
        Tool: SmallInt;            {OCAD 6: reserved
                                   OCAD 7/8: Preferred drawing tool
                                     0: off
                                     1: Curve mode
                                     2: Ellipse mode
                                     3: Circle mode
                                     4: Rectangular mode
                                     5: Straight line mode
                                     6: Freehand mode}
        Res3: SmallInt;
        FilePos: longint;          {see TBaseSym}
        Cols: TColors;             {see TBaseSym}
        Description: string[31];   {see TBaseSym}
        IconBits: array[0..263] of byte;
                                   {see TBaseSym}
        AreaFlags: word;           {reserved}
        FillOn: wordbool;          {true if Fill background is
                                   checked}
        FillColor: SmallInt;       {Fill color}
        HatchMode: SmallInt;       {Hatch mode
                                     0: None
                                     1: Single hatch
                                     2: Cross hatch}
        HatchColor: SmallInt;      {Color (Hatch page)}
        HatchLineWidth: SmallInt;  {Line width}
        HatchDist: SmallInt;       {Distance}
        HatchAngle1: SmallInt;     {Angle 1}
        HatchAngle2: SmallInt;     {Angle 2}
        HatchRes: SmallInt;
        StructMode: SmallInt;      {Structure
                                     0: None
                                     1: aligned rows
                                     2: shifted rows}
        StructWidth: SmallInt;     {Width}
        StructHeight: SmallInt;    {Height}
        StructAngle: SmallInt;     {Angle}
        StructRes: SmallInt;
        DataSize: SmallInt;        {number of coordinates (each 8 bytes)
                                   which follow this structure,
                                   each object header counts as
                                   2 Coordinates (16 bytes).
                                   The maximum value is 512.}
      end;
     */

    public short Size;          // Size of the symbol in bytes.
                                // This depends on the type and the number of subsymbols
    public short Sym;           // Symbol number.
                                // This is 10 times the value that appears on screen (1010 for 101.0)
    public short Otp;           // Object type:
                                //  1: Point symbol
                                //  2: Line symbol or Line text symbol
                                //  3: Area symbol
                                //  4: Text symbol
                                //  5: Rectangle symbol
    public byte SymTp;          // Symbol type:
                                //  1: Line text and text symbols
                                //  0: All other symbols
    public byte Flags;          // OCAD 6/7: must be 0
                                // OCAD 8:
                                //  1: not oriented to north (inverted for better compatibility)
                                //  2: Icon is compressed
    public short Extent;        // Extent how much the rendered symbols can reach outside the coordinates of an object with this symbol
    public bool Selected;       // Symbol is selected in the symbol box
    public byte Status;         // Status of the symbol:
                                //  0: Normal
                                //  1: Protected
                                //  2: Hidden
    public short Tool;          // OCAD 6: Reserved
                                // OCAD 7/8: Preferred drawing tool
                                //  0: Off
                                //  1: Curve mode
                                //  2: Ellipse mode
                                //  3: Circle mode
                                //  4: Rectangular mode
                                //  5: Straight line mode
                                //  6: Freehand mode
    public short Res3;
    public int FilePos;         // File position, not used in the file, only when loaded in memory. Value in the file is not defined
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Description;  // The description of the symbol
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 264)]
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
    public ushort AreaFlags;    // Reserved
    public ushort FillOn;       // True if fill background is checked
    public short FillColor;     // Fill Color
    public short HatchMode;     // Hatch Mode:
                                //  0: None
                                //  1: Single hatch
                                //  2: Cross hatch
    public short HatchColor;    // Color (Hatch page)
    public short HatchLineWidth;// Line width
    public short HatchDist;     // Distance
    public short HatchAngle1;   // Angle 1
    public short HatchAngle2;   // Angle 2
    public short HatchRes; 
    public short StructMode;    // Structure:
                                //  0: None
                                //  1: Aligned rows
                                //  2: Shifted rows
    public short StructWidth;   // Width
    public short StructHeight;  // Height
    public short StructAngle;   // Angle
    public short StructRes;
    public short DataSize;      // Number of coordinates (each 8 bytes) which follow this structure, each object
                                // header counts as 2 coords (16 bytes). Max 512
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_TextSym : IOcdObj
{
    /*
      TTextSym = record
        Size: SmallInt;            {size of this structure}
        Sym: SmallInt;             {see TBaseSym}
        Otp: SmallInt;             {4}
        SymTp: byte;               {1}
        Flags: byte;               {see TBaseSym}
        Extent: SmallInt;          {0}
        Selected: boolean;         {Symbol is selected in the symbol
                                   box}
        Status: byte;              {Status of the symbol
                                     0: Normal
                                     1: Protected
                                     2: Hidden}
        Res2: SmallInt;
        FrWidth: SmallInt;         {OCAD 6: reserved
                                    OCAD 7: Framing width
                                      sorry, this had to be squeezed
                                      in here
                                    OCAD 8: not used}
        FilePos: longint;          {see TBaseSym}
        Cols: TColors;             {see TBaseSym}
        Description: string [31];  {see TBaseSym}
        IconBits: array[0..263] of byte;
                                   {see TBaseSym}
        FontName: string[31];      {TrueType font}
        FontColor: SmallInt;       {Color}
        FontSize: SmallInt;        {10 times the size in pt}
        Weight: SmallInt;          {Bold as used in the Windows GDI
                                     400: normal
                                     700: bold}
        Italic: boolean;           {true if Italic is checked}
        CharSet: byte;             {OCAD 6/7: must be 0
                                   {OCAD 8: CharSet of the text, if the text is
                                      not Unicode}
        CharSpace: SmallInt;       {Char. spacing}
        WordSpace: SmallInt;       {Word spacing}
        Alignment: SmallInt;       {Alignment
                                     0: Left
                                     1: Center
                                     2: Right
                                     3: Justified}
        LineSpace: SmallInt;       {Line spacing}
        ParaSpace: SmallInt;       {Space after Paragraph}
        IndentFirst: SmallInt;     {Indent first line}
        IndentOther: SmallInt;     {Indent other lines}
        nTabs: SmallInt;           {number of Tabulators}
        Tabs: array[0..31] of longint;
                                   {Tabulators}
        LBOn: wordbool;            {true if Line below On is checked}
        LBColor: SmallInt;         {Line color (Line below)}
        LBWidth: SmallInt;         {Line width (Line below)}
        LBDist: SmallInt;          {Distance from text}
        Res4: SmallInt;
        FrMode: SmallInt;          {Framing mode
                                     0: no framing
                                     1: framing with a framing font
                                     2: OCAD 7/8 only: framing with a line}
                                   {Note this feature is called
                                   "Second font" in OCAD 6 but
                                   "Framing" in OCAD 7/8}
        FrName: string[31];        {OCAD 6/7: TrueType font (Second/Framing font)
                                    OCAD 8: not used}
        FrColor: SmallInt;         {Color (Second/Framing font)}
        FrSize: SmallInt;          {OCAD 6/7: Size (Second/Framing font)
                                    OCAD 8: framing width}
        FrWeight: SmallInt;        {OCAD 6/7: Bold (Second/Framing font)
                                     400: normal
                                     700: bold
                                    OCAD 8: not used}
        FrItalic: wordbool;        {true if Second/Framing font Italic
                                   is checked}
        FrOfX: SmallInt;           {OCAD 6/7: Horizontal offset
                                    OCAD 8: not used}
        FrOfY: SmallInt;           {OCAD 6/7: Vertical offset
                                    OCAD 8: not used}
      end;
     */

    public short Size;          // Size of the symbol in bytes.
                                // This depends on the type and the number of subsymbols
    public short Sym;           // Symbol number.
                                // This is 10 times the value that appears on screen (1010 for 101.0)
    public short Otp;           // Object type:
                                //  1: Point symbol
                                //  2: Line symbol or Line text symbol
                                //  3: Area symbol
                                //  4: Text symbol
                                //  5: Rectangle symbol
    public byte SymTp;          // Symbol type:
                                //  1: Line text and text symbols
                                //  0: All other symbols
    public byte Flags;          // OCAD 6/7: must be 0
                                // OCAD 8:
                                //  1: not oriented to north (inverted for better compatibility)
                                //  2: Icon is compressed
    public short Extent;        // Extent how much the rendered symbols can reach outside the coordinates of an object with this symbol
    public bool Selected;       // Symbol is selected in the symbol box
    public byte Status;         // Status of the symbol:
                                //  0: Normal
                                //  1: Protected
                                //  2: Hidden
    public short Res2;
    public short FrWidth;       // OCAD 6: Reserved
                                // OCAD 7: Framing width. Sorry, this had to be squeezed in here
                                // OCAD 8: Not used
    public int FilePos;         // File position, not used in the file, only when loaded in memory. Value in the file is not defined
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Description;  // The description of the symbol
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 264)]
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string FontName;     // TrueType font
    public short FontColor;     // Color
    public short FontSize;      // 10 times the value in pt
    public short Weight;        // Bold as used in the Windows GDI
                                //  400: normal
                                //  700: bold
    public bool Italic;         // True if Italic is checked
    public byte CharSet;        // OCAD 6/7: Must be 0
                                // OCAD 8: CharSet of the text, if the text is not unicode
    public short CharSpace;     // Character Spacing
    public short WordSpace;     // Word Spacing
    public short Alignment;     // Alignment:
                                //  0: Left
                                //  1: Center
                                //  2: Right
                                //  3: All line
    public short LineSpace;     // Line spacing
    public short ParaSpace;     // Paragraph Spacing
    public short IndentFirst;   // Indent first line
    public short IndentOther;   // Indent other lines
    public short nTabs;         // Number of tabulators
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public int[] Tabs;          // Tabulators
    public ushort LBOn;         // True if Line below On is checked
    public short LBColor;       // Line color (Line below)
    public short LBWidth;       // Line width (Line below)
    public short LBDist;        // Distance from text
    public short Res4;
    public short FrMode;        // Framing mode:
                                //  0: No framing
                                //  1: Framing with a framing font
                                //  2: OCAD 7/8 only: framing with a line
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string FrName;       // OCAD 6/7: TrueType font (Second/Framing font)
                                // OCAD 8: Not used
    public short FrColor;       // Color (Second/Framing font)
    public short FrSize;        // OCAD 6/7: Size (Second/Framing font)
                                // OCAD 8: Framing width
    public short FrWeight;      // OCAD 6/7: Bold as used in the Windows GDI
                                //  400: normal
                                //  700: bold
                                // OCAD 7: Not used
    public ushort FrItalic;     // OCAD 6/7: True if Italic is checked (Second/Framing font)
    public short FrOfX;         // OCAD 6/7: Horizontal offset
                                // OCAD 8: Not used
    public short FrOfY;         // OCAD 6/7: Vertical offset
                                // OCAD 8: Not used
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_RectSym : IOcdObj
{
    /*
      TRectSym = record
        Size: SmallInt;            {size of this structure}
        Sym: SmallInt;             {see TBaseSym}
        Otp: SmallInt;             {5}
        SymTp: byte;               {0}
        Flags: byte;               {see TBaseSym}
        Extent: SmallInt;          {see TBaseSym}
        Selected: boolean;         {Symbol is selected in the symbol
                                   box}
        Status: byte;              {Status of the symbol
                                     0: Normal
                                     1: Protected
                                     2: Hidden}
        Res2: SmallInt;
        Res3: SmallInt;
        FilePos: longint;          {see TBaseSym}
        Cols: TColors;             {see TBaseSym}
        Description: string [31];  {see TBaseSym}
        IconBits: array[0..263] of byte;
                                   {see TBaseSym}
        LineColor: SmallInt;       {Line color}
        LineWidth: SmallInt;       {Line width}
        Radius: SmallInt;          {Corner radius}
        GridFlags: word;           {Flags
                                     1: Grid On
                                     2: Numbered from the bottom}
        CellWidth: SmallInt;       {Cell width}
        CellHeight: SmallInt;      {Cell height}
        ResGridLineColor: SmallInt;
        ResGridLineWidth: SmallInt;
        UnnumCells: SmallInt;      {Unnumbered Cells}
        UnnumText: string[3];      {Text in unnumbered Cells}
        GridRes2: SmallInt;
        ResFontName: string[31];
        ResFontColor: SmallInt;
        ResFontSize: SmallInt;
        ResWeight: SmallInt;
        ResItalic: wordbool;
        ResOfsX: SmallInt;
        ResOfsY: SmallInt;
      end;
     */

    public short Size;          // Size of the symbol in bytes.
                                // This depends on the type and the number of subsymbols
    public short Sym;           // Symbol number.
                                // This is 10 times the value that appears on screen (1010 for 101.0)
    public short Otp;           // Object type:
                                //  1: Point symbol
                                //  2: Line symbol or Line text symbol
                                //  3: Area symbol
                                //  4: Text symbol
                                //  5: Rectangle symbol
    public byte SymTp;          // Symbol type:
                                //  1: Line text and text symbols
                                //  0: All other symbols
    public byte Flags;          // OCAD 6/7: must be 0
                                // OCAD 8:
                                //  1: not oriented to north (inverted for better compatibility)
                                //  2: Icon is compressed
    public short Extent;        // Extent how much the rendered symbols can reach outside the coordinates of an object with this symbol
    public bool Selected;       // Symbol is selected in the symbol box
    public byte Status;         // Status of the symbol:
                                //  0: Normal
                                //  1: Protected
                                //  2: Hidden
    public short Res2;
    public short Res3;
    public int FilePos;         // File position, not used in the file, only when loaded in memory. Value in the file is not defined
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    public string Description;  // The description of the symbol
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
    public short LineColor;     // Line color
    public short LineWidth;     // Line width
    public short Radius;        // Corner radius
    public ushort GridFlags;    // Flags:
                                //  1: Grid on
                                //  2: Numbered from the bottom
    public short CellWidth;     // Cell width
    public short CellHeight;    // Cell height
    public short ResGridLineColor;
    public short ResGridLineWidth;
    public short UnnumCells;    // Unnumbered cells
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
    public string UnnumText;    // Text in unnumbered Cells
    public short GridRes2;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string ResFontName;
    public short ResFontColor;
    public short ResFontSize;
    public short ResWeight;
    public ushort ResItalic;
    public short ResOfsX;
    public short ResOfsY;
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_Cord
{
    public int X, Y;
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_IndexBlock : IOcdObj
{
    /*
    Each Index block contains the file position of 256 objects. In
    addition it contains the symbol number and the extent (where on
    the map the object is located) of each object.

      TIndexBlock = record
        NextBlock: longint;        {file position of the next block
                                   0 if this is the last block}
        IndexArr: array[0..255] of TIndex;
                                   {TIndex as defined below}
      end;
     */

    public int NextBlock;   // File position of the next block. 0 if this is the last block
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public Index IndexArr;  // (256)
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_Index : IOcdObj
{
    public v6_Cord LowerLeft;   // Lower left corner of a rectangle covering the entire object. All flag bits are set to 0
    public v6_Cord UpperRight;  // Upper right corner of a rectangle covering the entire object. All flag bits are set to 0
    public int Pos;             // File position of the object
    public ushort Len;          // OCAD 6/7: Size of the object in the file in bytes
                                // OCAD 8: Number of coordinate pairs, the size of the object in the file is then calculated by:
                                //         32 + 8 * Len
                                // Note this is reserved space in the file, the actual length may be shorter
    public short Sym;           // 10 times the symbol number. Deleted objects are marked with sym = 0
}

[StructLayout(LayoutKind.Sequential)]
file struct v6_Element : IOcdObj
{
    public short Sym;       // 10 times the symbol number
    public byte Otp;        // Object Type:
                            //  1: Point Object
                            //  2: Line or Line Text Object
                            //  3: Area Object
                            //  4: Unformatted Text Object
                            //  5: Formatted Text Object or Rectangle Object
    public byte Unicode;    // OCAD 6/7: Must be 0
                            // OCAD 8: 1 if the text is Unicode
    public short nItem;     // Number of coordinates in the Poly array
    public short nText;     // Number of coordinates in the Poly array used for storing text
                            // nText is >0 for:
                            //  - Line text objects
                            //  - Unformatted text objects
                            //  - Formatted text objects
                            // For all other objects it is 0
    public short Ang;       // Angle, unit is .1 degrees
                            // Used for:
                            //  - Point object
                            //  - Area objects with structure
                            //  - Unformatted & formatted text objects
                            //  - Rectangle objects
    public short Res1;
    public int ResHeight;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string ResId;    // (16)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32768)]
    public v6_Cord[] Poly; // 6/7: 2000, 8: 32768 ? Not all used
}

#endregion

internal static class v6_Read
{
    public static void H()
    {
        FileStream fs = new(@"D:\Orienteering\Maps\UK\North\West\WCOC\Marron Leys - Oct 13.ocd", FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(fs);

        v6_FileHeader fz   H = reader.ReadBytes<v6_FileHeader>();
        v6_SymHeader sH = reader.ReadBytes<v6_SymHeader>();

        Console.WriteLine(Marshal.SizeOf<v6_BaseSym>());
        Console.WriteLine(Marshal.SizeOf<v6_PointSym>());
        Console.WriteLine(Marshal.SizeOf<v6_LineSym>());
        Console.WriteLine(Marshal.SizeOf<v6_LTextSym>());
        Console.WriteLine(Marshal.SizeOf<v6_AreaSym>());
        Console.WriteLine(Marshal.SizeOf<v6_TextSym>());
        Console.WriteLine(Marshal.SizeOf<v6_RectSym>());

        Console.ReadLine();
    }

    public static T? ByteToType<T>(byte[] bytes)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T? rObj = (T?)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();

        return rObj;
    }
    public static T? ByteToType<T>(BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T? rObj = (T?)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();

        return rObj;
    }

    public static T? ReadBytes<T>(this BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(Marshal.SizeOf<T>());

        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T? ret = (T?)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();

        return ret;
    }

    private static class ByteUtils
    {
        public static byte[] Seek(byte[] arr, int startPoint, int length) 
            => arr.Skip(startPoint).Take(length).ToArray();
    }
}

#endregion