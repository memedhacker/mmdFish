using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Aether.Constants
{
    /// <summary>
    /// Balık Yapboz / Puzzle oyunundaki sabit parça türleri.
    /// </summary>
    public enum PuzzlePieceType
    {
        /// <summary> [T] Turuncu (1x1) </summary>
        Orange,

        /// <summary> [S] Sarı (Ters L) </summary>
        Yellow,

        /// <summary> [K] Kırmızı (Z) </summary>
        Red,

        /// <summary> [Y] Yeşil (L) </summary>
        Green,

        /// <summary> [M] Mavi (Dikey 3x1) </summary>
        Blue,

        /// <summary> [C] Camgöbeği (2x2) </summary>
        Cyan
    }

    /// <summary>
    /// Puzzle tahtası (4 satır x 6 sütun) üzerindeki satır/sütun koordinat ofseti.
    /// </summary>
    public readonly record struct PuzzleOffset(int Row, int Col);

    /// <summary>
    /// Her bir puzzle parçasının sabit geometrik ve görsel tanımı.
    /// </summary>
    public class PuzzlePieceDefinition
    {
        public PuzzlePieceType Type { get; }
        public char Code { get; }
        public string Name { get; }
        public string Description { get; }
        public Color Color { get; }
        public IReadOnlyList<PuzzleOffset> Offsets { get; }
        public int BlockCount => Offsets.Count;
        public int RowSpan { get; }
        public int ColSpan { get; }

        public PuzzlePieceDefinition(
            PuzzlePieceType type,
            char code,
            string name,
            string description,
            Color color,
            IEnumerable<PuzzleOffset> offsets)
        {
            Type = type;
            Code = code;
            Name = name;
            Description = description;
            Color = color;
            Offsets = offsets.ToList().AsReadOnly();

            int maxRow = Offsets.Max(o => o.Row);
            int maxCol = Offsets.Max(o => o.Col);
            RowSpan = maxRow + 1;
            ColSpan = maxCol + 1;
        }

        /// <summary>
        /// Belirtilen başlangıç (sol-üst) satır ve sütun için 24-bitlik bitboard maskesini üretir.
        /// Slot numaralandırması: slotIndex = (row * 6) + col (0..23).
        /// </summary>
        public ulong GetBitmask(int startRow, int startCol, int gridCols = PuzzleConstants.GridCols)
        {
            ulong mask = 0UL;
            foreach (var offset in Offsets)
            {
                int r = startRow + offset.Row;
                int c = startCol + offset.Col;
                int index = (r * gridCols) + c;
                mask |= (1UL << index);
            }
            return mask;
        }

        /// <summary>
        /// Parçanın belirtilen başlangıç koordinatına tahta sınırları aşılmadan ve dolu slotlarla çakışmadan yerleştirilip yerleştirilemeyeceğini kontrol eder.
        /// </summary>
        public bool CanPlaceAt(ulong currentBoardMask, int startRow, int startCol, int gridRows = PuzzleConstants.GridRows, int gridCols = PuzzleConstants.GridCols)
        {
            if (startRow < 0 || startCol < 0) return false;
            if (startRow + RowSpan > gridRows || startCol + ColSpan > gridCols) return false;

            ulong pieceMask = GetBitmask(startRow, startCol, gridCols);
            return (currentBoardMask & pieceMask) == 0UL;
        }
    }

    /// <summary>
    /// Balık Yapboz oyunu için tahta boyutları, renk eşleşmeleri ve sabit parça tanımları.
    /// Parçalar sabittir, döndürülemez (No rotation).
    /// </summary>
    public static class PuzzleConstants
    {
        public const int GridRows = 4;
        public const int GridCols = 6;
        public const int TotalSlots = GridRows * GridCols; // 24

        #region Sabit Parça Tanımları (Bitboard / Koordinat Ofsetleri)

        /// <summary>
        /// [T] Turuncu (1x1): (0,0)
        /// </summary>
        public static readonly PuzzlePieceDefinition Orange = new PuzzlePieceDefinition(
            PuzzlePieceType.Orange,
            'T',
            "Turuncu",
            "1x1",
            Colors.PuzzleOrange,
            new[]
            {
                new PuzzleOffset(0, 0)
            });

        /// <summary>
        /// [S] Sarı (Ters L): (0,0), (0,1), (1,1)
        /// </summary>
        public static readonly PuzzlePieceDefinition Yellow = new PuzzlePieceDefinition(
            PuzzlePieceType.Yellow,
            'S',
            "Sarı",
            "Ters L",
            Colors.PuzzleYellow,
            new[]
            {
                new PuzzleOffset(0, 0),
                new PuzzleOffset(0, 1),
                new PuzzleOffset(1, 1)
            });

        /// <summary>
        /// [K] Kırmızı (Z): (0,0), (0,1), (1,1), (1,2)
        /// </summary>
        public static readonly PuzzlePieceDefinition Red = new PuzzlePieceDefinition(
            PuzzlePieceType.Red,
            'K',
            "Kırmızı",
            "Z",
            Colors.PuzzleRed,
            new[]
            {
                new PuzzleOffset(0, 0),
                new PuzzleOffset(0, 1),
                new PuzzleOffset(1, 1),
                new PuzzleOffset(1, 2)
            });

        /// <summary>
        /// [Y] Yeşil (L): (0,0), (1,0), (1,1)
        /// </summary>
        public static readonly PuzzlePieceDefinition Green = new PuzzlePieceDefinition(
            PuzzlePieceType.Green,
            'Y',
            "Yeşil",
            "L",
            Colors.PuzzleGreen,
            new[]
            {
                new PuzzleOffset(0, 0),
                new PuzzleOffset(1, 0),
                new PuzzleOffset(1, 1)
            });

        /// <summary>
        /// [M] Mavi (Dikey 3x1): (0,0), (1,0), (2,0)
        /// </summary>
        public static readonly PuzzlePieceDefinition Blue = new PuzzlePieceDefinition(
            PuzzlePieceType.Blue,
            'M',
            "Mavi",
            "Dikey 3x1",
            Colors.PuzzleBlue,
            new[]
            {
                new PuzzleOffset(0, 0),
                new PuzzleOffset(1, 0),
                new PuzzleOffset(2, 0)
            });

        /// <summary>
        /// [C] Camgöbeği (2x2): (0,0), (0,1), (1,0), (1,1)
        /// </summary>
        public static readonly PuzzlePieceDefinition Cyan = new PuzzlePieceDefinition(
            PuzzlePieceType.Cyan,
            'C',
            "Camgöbeği",
            "2x2",
            Colors.PuzzleCyan,
            new[]
            {
                new PuzzleOffset(0, 0),
                new PuzzleOffset(0, 1),
                new PuzzleOffset(1, 0),
                new PuzzleOffset(1, 1)
            });

        #endregion

        /// <summary>
        /// Tüm parça tanımlarının listesi.
        /// </summary>
        public static readonly IReadOnlyList<PuzzlePieceDefinition> AllPieces = new[]
        {
            Orange,
            Yellow,
            Red,
            Green,
            Blue,
            Cyan
        };

        /// <summary>
        /// Parça türüne göre sözlük eşlemesi.
        /// </summary>
        public static readonly IReadOnlyDictionary<PuzzlePieceType, PuzzlePieceDefinition> PiecesByType =
            AllPieces.ToDictionary(p => p.Type);

        /// <summary>
        /// Renk değerine göre parça tanımını bulur. Eşleşme toleransı dahilinde en yakın parçayı döndürür.
        /// </summary>
        public static PuzzlePieceDefinition? GetPieceByColor(Color color, int maxTolerance = 35)
        {
            PuzzlePieceDefinition? bestPiece = null;
            int bestDiff = int.MaxValue;

            foreach (var piece in AllPieces)
            {
                int diff = Math.Abs(color.R - piece.Color.R) +
                           Math.Abs(color.G - piece.Color.G) +
                           Math.Abs(color.B - piece.Color.B);

                if (diff <= maxTolerance && diff < bestDiff)
                {
                    bestDiff = diff;
                    bestPiece = piece;
                }
            }

            return bestPiece;
        }

        /// <summary>
        /// Karakter koduna ('T', 'S', 'K', 'Y', 'M', 'C') göre parça tanımını döndürür.
        /// </summary>
        public static PuzzlePieceDefinition? GetPieceByCode(char code)
        {
            char upper = char.ToUpperInvariant(code);
            return AllPieces.FirstOrDefault(p => p.Code == upper);
        }
    }
}
