using System.IO;
using System.Text;

namespace OTools.FileFormatConverter.src;

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

file struct FileHeader : IOcdObj
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
    public short Version;       // 
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

    public int Reserved2, Reserved3, Reserved4;
}

file struct SymHeader : IOcdObj
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
    public short YellowAng;
    public short BlackFreq;
    public short BlackAng;
    public short Res1;
    public short Res2;
    public ColorInfo[] aColorInfo;  // (256) explained below
    public ColorSep[] aColorSep;    // (32)  explained below. Only 24 allowed
}

file struct ColorInfo : IOcdObj
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
    public Cmyk Color;
    public string ColorName;
    public byte[] SepPercentage;
}

file struct Cmyk : IOcdObj
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

    public byte Cyan;       // 2 times the Cyan value as it appears in the Define Color dialog box (to alllow half %s)
    public byte Magenta;    // ditto
    public byte Yellow;     // ditto
    public byte Black;      // ditto
}

file struct ColorSep : IOcdObj
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

    public string SepName;      // Name of the color seperation
    public Cmyk Color;          // 0 in OCAD 6
                                // CMYK value of the seperation in OCAD 7
                                // Only used in the Adobe Illustrator export
    public short RasterFreq;    // 10 times the halftone frequency as it appears in the Color Seperation dialog box
    public short RasterAngle;   // 10 times the halftone angle as it appears in the Color Seperation dialog box
}

file struct SymbolBlock : IOcdObj
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

    public int NextBlock;   // File position of the next symbol block.
                            // 0 of this is the last symbol block
    public int[] FilePos;   // File position of up to 256 symbols.
                            // 0 if there is no symbol for this index
}

file struct BaseSym : IOcdObj
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
    public byte[] Cols;         // (256) Set of the colors used in this symbol.
    public string Description;  // The description of the symbol
    public byte[] IconBits;     // (264) The icon can be uncompressed (16-bit colors) or compresed (256 color palette)
                                // depending on the Flags field. In OCAD 6/7 it is always uncompressed
}

file struct PointSym : IOcdObj
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

    public short DataSize; // Number of coordinates (each 8 bytes) which follow this structure
                           // each object header counts as 2 coordinates (16 bytes). Max 512
    public short Reserved;
}

file struct SymElt : IOcdObj
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
    public ushort stFlags; // unsure of data type (word)
    public short stColor;
    public short stLineWidth;
    public short stDiameter;
    public short stnPoly;
    public short stRes1, stRes2;
    public Cord[] stPoly;
}

file struct LineSym : IOcdObj
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

    public short Tool;
    public short LineColor;
    public short LineWidth;
    public bool LineEnds; // unsure of type (word) but states a bool value
    public short DistFromStart;
    public short DistToEnd;
    public short MainLength;
    public short EndLength;
    public short MainGap;
    public short SecGap;
    public short EndGap;
    public short MinSym;
    public short nPrimSym;
    public short PrimSymDist;
    public ushort DblMode;
    public ushort DblFlags;
    public short DbFillColor;
    public short DblLeftColor;
    public short DblRightColor;
    public short DblWidth;
    public short DblLeftWidth;
    public short DblRightWidth;
    public short DblLength;
    public short DblGap;
    public short[] DblRes;
    public ushort DecMode;
    public short DecLast;
    public short DecRes;
    public short FrColor;
    public short FrWidth;
    public short FrStyle;
    public short PrimDSize;
    public short SecDSize;
    public short CornerDSize;
    public short StartDSize;
    public short EndDSize;
    public short Reserved;
}

file struct LTextSym : IOcdObj
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

    public short Tool;
    public short FrWidth;
    public string FontName;
    public short FontColor;
    public short FontSize;
    public short Weight;
    public bool Italic;
    public byte CharSet;
    public short CharSpace;
    public short WordSpace;
    public short Alignment;
    public short FrMode;
    public string FrName;
    public short FrColor;
    public short FrSize;
    public short FrWeight;
    public ushort FrItalic; // wordbool?
    public short FrOfX;
    public short FrOfY;
}

file struct AreaSym : IOcdObj
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

    public short Tool;
    public ushort AreaFlags;
    public short FillColor;
    public short HatchMode;
    public short HatchColor;
    public short HatchLineWidth;
    public short HatchDist;
    public short HatchAngle1;
    public short HatchAngle2;
    public short HatchRes;
    public short StructMode;
    public short StructWidth;
    public short StructHeight;
    public short StructAngle;
    public short StructRes;
    public short DataSize;
}

file struct TextSym : IOcdObj
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

    public short FrWidth;
    public string FontName;
    public short FontColor;
    public short FontSize;
    public short Weight;
    public bool Italic;
    public byte CharSet;
    public short CharSpace;
    public short WordSpace;
    public short Alignment;
    public short LineSpace;
    public short ParaSpace;
    public short IndentFirst;
    public short IndentOther;
    public short nTabs;
    public int[] Tabs;
    public ushort LBOn;
    public short LBColor;
    public short LBWidth;
    public short LBDist;
    public short Res4;
    public short FrMode;
    public string FrName;
    public short FrColor;
    public short FrSize;
    public short FrWeight;
    public ushort FrItalic;
    public short FrOfX;
    public short FrOfY;
}

file struct RectSym : IOcdObj
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

    public short LineColor;
    public short LineWidth;
    public short Radius;
    public ushort GridFlags;
    public short CellWidth;
    public short CellHeight;
    public short ResGridLineColor;
    public short ResGridLineWidth;
    public short UnnumCells;
    public string UnnumText;
    public short GridRes2;
    public string ResFontName;
    public short ResFontColor;
    public short ResFontSize;
    public short ResWeight;
    public ushort ResItalic;
    public short ResOfsX;
    public short ResOfsY;
}

file struct Cord
{
    public int X, Y;
}

#endregion

internal static class _Read
{
    public static void H()
    {
        FileStream fs = new("D:\\Orienteering\\Maps\\Badlands.ocd", FileMode.Open, FileAccess.Read);
        BinaryReader br = new(fs);
        byte[] outpu = br.ReadBytes((int)fs.Length);
        Console.ReadLine();
    }
}

#endregion