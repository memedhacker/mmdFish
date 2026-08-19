using Aether.BluePrints;
using Aether.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Aether.Engines
{
    /// <summary>
    /// Balık Puzzle (4x6 tahta) için PredefinedBlueprints ve Heuristik tabanlı hibrit çözüm motoru.
    /// 24-bit bitboard ve 3.382 adet önceden hesaplanmış çözüm şablonu üzerinde çalışır.
    /// </summary>
    public class FishPuzzleSolveEngine
    {
        private const int Rows = PuzzleConstants.GridRows;   // 4
        private const int Cols = PuzzleConstants.GridCols;    // 6
        private const int TotalSlots = PuzzleConstants.TotalSlots; // 24

        // ── Heuristik Puanlama Ağırlıkları (Eşitlik bozma ve fallback için) ──
        private const int WallContactScore = 3;   // Parça hücresi duvar/kenara temas ediyor
        private const int NeighborScore = 5;       // Parça hücresi dolu komşuya temas ediyor
        private const int CornerBonus = 2;         // Parça hücresi köşe pozisyonunda
        private const int BottomRowBonus = 1;      // Alt satırlar tercih edilir (satır indexi kadar bonus)
        private const int HolePenalty = -20;       // İzole 1x1 delik

        private ulong _boardMask;
        private List<PredefinedBlueprints.BlueprintData> _activeBlueprints = new();

        public FishPuzzleSolveEngine()
        {
            Reset();
        }

        /// <summary> Mevcut tahta durumu (24-bit bitboard). </summary>
        public ulong BoardMask => _boardMask;

        /// <summary> Tahta tamamen dolu mu? </summary>
        public bool IsBoardFull => _boardMask == ((1UL << TotalSlots) - 1);

        /// <summary> Dolu hücre sayısı. </summary>
        public int FilledCount => BitOperations.PopCount(_boardMask);

        /// <summary> Boş hücre sayısı. </summary>
        public int EmptyCount => TotalSlots - FilledCount;

        /// <summary> Mevcut tahta durumuyla uyumlu aktif aday şablon sayısı. </summary>
        public int ActiveBlueprintCount => _activeBlueprints.Count;

        /// <summary>
        /// Motoru ve aday şablonları sıfırlar.
        /// Eğer initialMask verilmişse, sadece o maskeyle uyumlu şablonları filtreler.
        /// </summary>
        public void Reset(ulong initialMask = 0UL)
        {
            _boardMask = initialMask & ((1UL << TotalSlots) - 1);

            if (_boardMask == 0UL)
            {
                _activeBlueprints = new List<PredefinedBlueprints.BlueprintData>(PredefinedBlueprints.AllBlueprints);
            }
            else
            {
                // Başlangıç tahtasındaki dolu hücrelerle tam uyuşan şablonları seç
                _activeBlueprints = PredefinedBlueprints.AllBlueprints.Where(b =>
                {
                    foreach (var p in b.Pieces)
                    {
                        ulong overlap = p.Mask & _boardMask;
                        // Parça ya tamamen boş olmalı ya da tahtadaki dolu bölgeyle tam örtüşmeli
                        if (overlap != 0 && overlap != p.Mask)
                            return false;
                    }
                    return true;
                }).ToList();

                // Eğer hiçbir şablon uyuşmazsa tüm şablonları yedek olarak tut
                if (_activeBlueprints.Count == 0)
                {
                    _activeBlueprints = new List<PredefinedBlueprints.BlueprintData>(PredefinedBlueprints.AllBlueprints);
                }
            }
        }

        /// <summary>
        /// Tahta durumunu Color dizisinden günceller.
        /// slotColors[0..23]: slot1..slot24 (satır-öncelikli, soldan sağa, yukarıdan aşağıya).
        /// </summary>
        public void UpdateBoard(Color[] slotColors)
        {
            ulong mask = 0UL;
            Color emptyColor = Color.FromArgb(64, 64, 64);

            for (int i = 0; i < Math.Min(slotColors.Length, TotalSlots); i++)
            {
                if (slotColors[i] != emptyColor)
                {
                    mask |= (1UL << i);
                }
            }

            if (_boardMask != mask)
            {
                _boardMask = mask;
                // Aktif şablonları yeni maskeye göre daralt
                _activeBlueprints = _activeBlueprints.Where(b =>
                {
                    foreach (var p in b.Pieces)
                    {
                        ulong overlap = p.Mask & _boardMask;
                        if (overlap != 0 && overlap != p.Mask)
                            return false;
                    }
                    return true;
                }).ToList();
            }
        }

        /// <summary>
        /// Tahtayı doğrudan bitboard olarak ayarlar.
        /// </summary>
        public void SetBoard(ulong mask)
        {
            Reset(mask);
        }

        /// <summary>
        /// Belirli bir hücrenin dolu olup olmadığını döner.
        /// </summary>
        public bool IsOccupied(int row, int col)
        {
            int idx = (row * Cols) + col;
            return (_boardMask & (1UL << idx)) != 0;
        }

        /// <summary>
        /// Belirtilen parça belirtilen konuma yerleştirildiğinde tahtanın yeni doluluk durumunu kontrol eder.
        /// Yerleştirme sonrası dolu hücre sayısı, doluluk oranı (0.0 - 1.0) ve tahtanın tamamen dolup dolmayacağını (IsLastPiece) döner.
        /// </summary>
        public (int AfterFilled, double OccupancyRate, bool IsLastPiece) CheckOccupancyAfterPlacement(PuzzlePieceDefinition piece, int row, int col)
        {
            ulong pieceMask = piece.GetBitmask(row, col, Cols);
            ulong boardAfter = _boardMask | pieceMask;
            int afterFilled = BitOperations.PopCount(boardAfter);
            double occupancyRate = (double)afterFilled / TotalSlots;
            bool isLastPiece = (afterFilled >= TotalSlots);

            return (afterFilled, occupancyRate, isLastPiece);
        }

        /// <summary>
        /// PredefinedBlueprints şablonlarını kullanarak verilen parça için en uygun yerleşim koordinatını bulur.
        /// Aday şablonlar arasında en yüksek sayıda şablon alternatifini koruyan ve en az parça ile bitirmeyi sağlayan konumu seçer.
        /// Eğer aktif şablonlara uygun bir yerleşim yoksa null döner (parçanın atılması gerektiğini belirtir).
        /// </summary>
        public (int Row, int Col)? FindBestBlueprintPlacement(PuzzlePieceDefinition piece)
        {
            string pieceCode = piece.Code.ToString();

            // Eğer aktif şablon kalmamışsa heuristik yönteme fallback yap
            if (_activeBlueprints.Count == 0)
            {
                return FindBestPlacement(piece);
            }

            // Aktif şablonlar içinde bu parçanın yerleşebileceği boş pozisyonları tespit et
            var candidatePlacements = new Dictionary<(int Row, int Col), (int MatchCount, int MinTotalPieces, int HeuristicScore)>();

            foreach (var bp in _activeBlueprints)
            {
                foreach (var p in bp.Pieces)
                {
                    if (p.Type != pieceCode)
                        continue;

                    // Bu parçanın kapladığı alan tahtada şu an boş mu?
                    if ((p.Mask & _boardMask) == 0UL)
                    {
                        var key = (p.Row, p.Col);
                        if (!candidatePlacements.TryGetValue(key, out var stats))
                        {
                            int hScore = ScorePlacement(piece, p.Row, p.Col);
                            candidatePlacements[key] = (1, bp.TotalPieces, hScore);
                        }
                        else
                        {
                            candidatePlacements[key] = (
                                stats.MatchCount + 1,
                                Math.Min(stats.MinTotalPieces, bp.TotalPieces),
                                stats.HeuristicScore
                            );
                        }
                    }
                }
            }

            if (candidatePlacements.Count == 0)
            {
                return null;
            }

            // En iyi pozisyonu seç:
            // 1. En çok aday şablonu koruyan (MatchCount DESC)
            // 2. En az toplam parça gerektiren şablonlara giden (MinTotalPieces ASC)
            // 3. Heuristik puanı yüksek olan (HeuristicScore DESC)
            var best = candidatePlacements
                .OrderByDescending(kv => kv.Value.MatchCount)
                .ThenBy(kv => kv.Value.MinTotalPieces)
                .ThenByDescending(kv => kv.Value.HeuristicScore)
                .First();

            return best.Key;
        }

        /// <summary>
        /// Parçayı tahtaya yerleştirir ve aktif şablon listesini bu yerleşimi içeren şablonlara daraltır.
        /// </summary>
        public void PlacePiece(PuzzlePieceDefinition piece, int row, int col)
        {
            ulong pieceMask = piece.GetBitmask(row, col, Cols);
            _boardMask |= pieceMask;

            string pieceCode = piece.Code.ToString();

            // Aktif şablonları bu parça ve konuma göre filtrele
            var filtered = _activeBlueprints
                .Where(b => b.Pieces.Any(p => p.Type == pieceCode && p.Row == row && p.Col == col))
                .ToList();

            if (filtered.Count > 0)
            {
                _activeBlueprints = filtered;
            }
        }

        /// <summary>
        /// Saf heuristik yerleşim bulucu (şablon dışı acil durumlar / fallback için).
        /// </summary>
        public (int Row, int Col)? FindBestPlacement(PuzzlePieceDefinition piece)
        {
            int bestScore = int.MinValue;
            (int Row, int Col)? bestPos = null;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (!piece.CanPlaceAt(_boardMask, r, c, Rows, Cols))
                        continue;

                    int score = ScorePlacement(piece, r, c);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPos = (r, c);
                    }
                }
            }

            return bestPos;
        }

        /// <summary>
        /// Belirtilen (targetRow, targetCol) hücresini kapsayacak şekilde
        /// tahtaya yerleştirilebilecek daha büyük bir parça olup olmadığını kontrol eder.
        /// </summary>
        public bool CanAnyLargerPieceCover(int targetRow, int targetCol)
        {
            var largerPieces = new[]
            {
                PuzzleConstants.Yellow, // Ters L (3 blok)
                PuzzleConstants.Green,  // L (3 blok)
                PuzzleConstants.Blue,   // Dikey 3x1 (3 blok)
                PuzzleConstants.Red,    // Z (4 blok)
                PuzzleConstants.Cyan    // 2x2 (4 blok)
            };

            foreach (var piece in largerPieces)
            {
                foreach (var offset in piece.Offsets)
                {
                    int startRow = targetRow - offset.Row;
                    int startCol = targetCol - offset.Col;

                    if (piece.CanPlaceAt(_boardMask, startRow, startCol, Rows, Cols))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tahtayı konsol-dostu string olarak döndürür (debug amaçlı).
        /// </summary>
        public string BoardToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    sb.Append(IsOccupied(r, c) ? "█ " : "· ");
                }
                if (r < Rows - 1) sb.AppendLine();
            }
            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════
        //  Heuristik Puanlama Yardımcıları
        // ══════════════════════════════════════════════════════════

        private int ScorePlacement(PuzzlePieceDefinition piece, int startRow, int startCol)
        {
            int score = 0;
            ulong pieceMask = piece.GetBitmask(startRow, startCol, Cols);
            ulong boardAfterPlace = _boardMask | pieceMask;

            int holesBefore = CountIsolatedHoles(_boardMask);

            foreach (var offset in piece.Offsets)
            {
                int r = startRow + offset.Row;
                int c = startCol + offset.Col;

                if (r == 0) score += WallContactScore;
                if (r == Rows - 1) score += WallContactScore;
                if (c == 0) score += WallContactScore;
                if (c == Cols - 1) score += WallContactScore;

                if ((r == 0 || r == Rows - 1) && (c == 0 || c == Cols - 1))
                    score += CornerBonus;

                score += CountNeighborContacts(r, c, pieceMask);
                score += r * BottomRowBonus;
            }

            int holesAfter = CountIsolatedHoles(boardAfterPlace);
            int newHoles = holesAfter - holesBefore;
            if (newHoles > 0)
            {
                score += newHoles * HolePenalty;
            }

            return score;
        }

        private int CountNeighborContacts(int row, int col, ulong pieceMask)
        {
            int contacts = 0;
            int[][] dirs = { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };

            foreach (var d in dirs)
            {
                int nr = row + d[0];
                int nc = col + d[1];

                if (nr < 0 || nr >= Rows || nc < 0 || nc >= Cols)
                    continue;

                int nIdx = (nr * Cols) + nc;

                bool isOccupiedOnBoard = (_boardMask & (1UL << nIdx)) != 0;
                bool isPartOfPiece = (pieceMask & (1UL << nIdx)) != 0;

                if (isOccupiedOnBoard && !isPartOfPiece)
                {
                    contacts += NeighborScore;
                }
            }

            return contacts;
        }

        private int CountIsolatedHoles(ulong boardMask)
        {
            int holes = 0;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    int idx = (r * Cols) + c;
                    if ((boardMask & (1UL << idx)) != 0)
                        continue;

                    bool topBlocked = (r == 0) || ((boardMask & (1UL << ((r - 1) * Cols + c))) != 0);
                    bool botBlocked = (r == Rows - 1) || ((boardMask & (1UL << ((r + 1) * Cols + c))) != 0);
                    bool leftBlocked = (c == 0) || ((boardMask & (1UL << (r * Cols + c - 1))) != 0);
                    bool rightBlocked = (c == Cols - 1) || ((boardMask & (1UL << (r * Cols + c + 1))) != 0);

                    if (topBlocked && botBlocked && leftBlocked && rightBlocked)
                    {
                        holes++;
                    }
                }
            }

            return holes;
        }
    }
}
