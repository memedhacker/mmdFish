using System;
using System.Drawing;

namespace Aether.Constants
{
    /// <summary>
    /// Oyun penceresi içerisinde kırpılacak (Crop) veya şablon aranacak bir ekran bölgesinin
    /// koordinatlarını (startX, startY, endX, endY) temsil eden değişmez (immutable) veri modeli.
    /// </summary>
    public readonly record struct WindowRegion(int StartX, int StartY, int EndX, int EndY)
    {
        /// <summary> Bölgenin genişliği (px) </summary>
        public int Width => Math.Max(0, EndX - StartX);

        /// <summary> Bölgenin yüksekliği (px) </summary>
        public int Height => Math.Max(0, EndY - StartY);

        /// <summary> System.Drawing.Rectangle formatına dönüştürür. </summary>
        public Rectangle ToRectangle() => new Rectangle(StartX, StartY, Width, Height);

        public override string ToString() => $"({StartX}, {StartY}) -> ({EndX}, {EndY}) [{Width}x{Height} px]";
    }

    /// <summary>
    /// HWND oyun penceresi içerisinde Template Matching / Görsel Tanıma yapılacak
    /// sabit ekran bölgelerinin koordinatlarını barındıran merkezi sabitler sınıfı.
    /// 
    /// =========================================================================================
    /// 📘 KULLANIM REHBERİ (HOW TO USE):
    /// =========================================================================================
    /// 
    /// 1. Tanımlı Pozisyon Üzerinden Bölgesel Ekran Görüntüsü Alma:
    ///    --------------------------------------------------------------------------------------
    ///    // Örnek 1: HWND belirterek doğrudan ChatBox bölgesini çekme:
    ///    using Bitmap? chatBmp = Helpers.WindowRegionCaptureHelper.CaptureRegion(
    ///        client.Handle, 
    ///        RegionConstants.ChatBoxPosition
    ///    );
    /// 
    ///    // Örnek 2: Aktif seçili istemciden ChatBox bölgesini çekme (Kısayol):
    ///    using Bitmap? chatBmp = Helpers.WindowRegionCaptureHelper.CaptureRegion(
    ///        RegionConstants.ChatBoxPosition
    ///    );
    /// 
    /// 2. Alınan Görsel ile Şablon Eşleme (Template Matching):
    ///    --------------------------------------------------------------------------------------
    ///    if (chatBmp != null)
    ///    {
    ///        // Chat kutusunda "Bişey Takıldı" waypoint şablonunu ara:
    ///        var result = TemplateConstants.Match(
    ///            chatBmp, 
    ///            TemplateConstants.Waypoints.BiseyTakildi, 
    ///            threshold: 0.85
    ///        );
    /// 
    ///        if (result.IsSuccess)
    ///        {
    ///            Debug.WriteLine($"Chat kutusunda şablon bulundu! Konum: {result.Location}, Güven: %{result.Confidence * 100:F1}");
    ///        }
    ///    }
    /// 
    /// 3. Gelecekte Yeni Pozisyonlar Eklemek İçin:
    ///    --------------------------------------------------------------------------------------
    ///    // Bu sınıf içerisine yeni bir public static readonly WindowRegion alanı eklemeniz yeterlidir:
    ///    // public static readonly WindowRegion EnvanterAlani = new WindowRegion(startX, startY, endX, endY);
    ///    // public static readonly WindowRegion BalikcilikBari = new WindowRegion(startX, startY, endX, endY);
    /// =========================================================================================
    /// </summary>
    public static class RegionConstants
    {
        /// <summary>
        /// Chat (Sohbet / Bilgi) Kutusu Bölgesi:
        /// Başlangıç: (X: 97, Y: 547) | Bitiş: (X: 532, Y: 564) | Boyut: 435x17 px
        /// Balık tutulduğunda, misina koptuğunda veya sistem bildirimleri geldiğinde taranacak alan.
        /// </summary>
        /// 
        public static readonly WindowRegion EquipmentMenuTitlePosition = new WindowRegion(625, 0, 799, 49);
        public static readonly WindowRegion EquipmentMenuExitButtonPosition = new WindowRegion(764, 0, 799, 38);
        public static readonly WindowRegion ChatBoxPosition = new WindowRegion(97, 547, 532, 564);
        public static readonly WindowRegion InventoryPagesPosition = new WindowRegion(627, 213, 799, 243);
        public static readonly WindowRegion InventoryPosition = new WindowRegion(623, 235, 798, 542);
        public static readonly WindowRegion InventoryFishArea = new WindowRegion(627, 234, 799, 469);
        public static readonly WindowRegion InventoryBaitArea = new WindowRegion(627, 464, 799, 539);
        public static readonly WindowRegion OpenMarketPosition = new WindowRegion(371, 213, 428, 233);
        public static readonly WindowRegion MarketBaitPosition = new WindowRegion(483, 83, 505, 102);
        public static readonly WindowRegion MarketFirePosition = new WindowRegion(453, 49, 471, 70);
        public static readonly WindowRegion MarketExitButtonPosition = new WindowRegion(558, 20, 577, 36);
        public static readonly WindowRegion FisherManSearchArea = new WindowRegion(0, 4, 644, 555);
        public static readonly WindowRegion NewDMPosition = new WindowRegion(706, 159, 799, 440);
        public static readonly WindowRegion MapPosition = new WindowRegion(644, 8, 799, 145);
        public static readonly WindowRegion FishingMenuExitButtonPosition = new WindowRegion(360, 57, 376, 75);
        public static readonly WindowRegion GoldenTonFishReleaseButtonPosition = new WindowRegion(362, 230, 436, 249);
        /// <summary>
        /// Test pencerelerinde ve arayüz ComboBox'larında hızlı seçim yapabilmek için tüm tanımlı sabit bölgelerin listesi.
        /// </summary>
        public static readonly (string Name, WindowRegion Region)[] AllRegions = new[]
        {
            ("💬 ChatBoxPosition (97, 547 -> 532, 564)", ChatBoxPosition),
            ("💬 EquipmentMenuTitlePosition (625, 0 -> 799, 49)", EquipmentMenuTitlePosition),
            ("💬 EquipmentMenuExitButtonPosition (762, 8 -> 793, 30)", EquipmentMenuExitButtonPosition),
            ("💬 InventoryPagesPosition (634, 221 -> 792, 242)", InventoryPagesPosition),
            ("💬 InventoryPosition (623, 235 -> 798, 542)", InventoryPosition),
            ("💬 InventoryFishArea (627, 234 -> 799, 469)", InventoryFishArea),
            ("💬 InventoryBaitArea (627, 464 -> 799, 539)", InventoryBaitArea),
            ("💬 OpenMarketPosition (354, 205 -> 448, 237)", OpenMarketPosition),
            ("💬 MarketBaitPosition (443, 182 -> 485, 216)", MarketBaitPosition),
            ("💬 MarketFirePosition (418, 149 -> 446, 184)", MarketFirePosition),
            ("💬 MarketExitButtonPosition (558, 20 -> 577, 36)", MarketExitButtonPosition),
            ("💬 FisherManSearchArea (0, 4 -> 644, 555)", FisherManSearchArea),
            ("💬 NewDMPosition (706, 159 -> 799, 440)", NewDMPosition),
            ("💬 MapPosition (644, 8 -> 799, 145)", MapPosition),
            ("💬 FishingMenuExitButtonPosition (360, 57 -> 376, 75)", FishingMenuExitButtonPosition),
            ("💬 GoldenTonFishReleaseButtonPosition (362, 230 -> 436, 249)", GoldenTonFishReleaseButtonPosition)
        };

        // 📍 Gelecekte eklenecek yeni oyun bölgeleri buraya tanımlanabilir:
        // public static readonly WindowRegion BalikDaireAlani = new WindowRegion(..., ..., ..., ...);
        // public static readonly WindowRegion EnvanterAlani = new WindowRegion(..., ..., ..., ...);
    }
}
