using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace Aether.Constants
{
    /// <summary>
    /// Assets/templates klasöründeki tüm şablon görsel (template) dosyalarını strongly-typed sabitler,
    /// önbellekleme (caching) ve OpenCvSharp tabanlı Template Matching yöntemleri ile sunan merkezi sınıf.
    /// </summary>
    public static class TemplateConstants
    {
        #region 1. Şablon Dosya Yolu Sabitleri (Constants)

        /// <summary>
        /// AutoPass (otomatik geçiş / doğrulama) şablonları.
        /// </summary>
        public static class AutoPass
        {
            public const string MinikBalik = "autopass/minik_balik.png";
            public const string TatliSuKaridesi = "autopass/tatli_su_karidesi.png";
            public const string BiseyTakildi = "autopass/bisey_takildi.png";

            public static readonly IReadOnlyList<string> All = new[]
            {
                MinikBalik,
                TatliSuKaridesi,
                BiseyTakildi
            };
        }

        /// <summary>
        /// Balık isimleri ve tutulan nesnelere ait şablonlar.
        /// </summary>
        public static class FishNames
        {
            public const string AltinAnahtar = "fishnames/altin_anahtar.png";
            public const string AltinParcasi = "fishnames/altin_parcasi.png";
            public const string AltinSudak = "fishnames/altin_sudak.png";
            public const string AltinYuzuk = "fishnames/altin_yuzuk.png";
            public const string AynaliSazan = "fishnames/aynali_sazan.png";
            public const string BeyazSacBoyasi = "fishnames/beyaz_sac_boyasi.png";
            public const string BilgeKralinEldiveni = "fishnames/bilge_kralin_eldiveni.png";
            public const string BilgeKralinSembolu = "fishnames/bilge_kralin_sembolu.png";
            public const string BuyukSudak = "fishnames/buyuk_sudak.png";
            public const string DenizKiziAnahtari = "fishnames/deniz_kizi_anahtari.png";
            public const string DereAlabaligi = "fishnames/dere_alabaligi.png";
            public const string GokkusagiAlabaligi = "fishnames/gokkusagi_alabaligi.png";
            public const string GorunmezlikPelerini = "fishnames/gorunmezlik_pelerini.png";
            public const string GumusAnahtar = "fishnames/gumus_anahtar.png";
            public const string Hamsi = "fishnames/hamsi.png";
            public const string Kadife = "fishnames/kadife.png";
            public const string KahverengiSacBoyasi = "fishnames/kahverengi_sac_boyasi.png";
            public const string KirmiziSacBoyasi = "fishnames/kirmizi_sac_boyasi.png";
            public const string KralYengeci = "fishnames/kral_yengeci.png";
            public const string KurbagaBaligi = "fishnames/kurbaga_baligi.png";
            public const string Levrek = "fishnames/levrek.png";
            public const string LucyYuzugu = "fishnames/lucy_yuzugu.png";
            public const string Lufer = "fishnames/lufer.png";
            public const string NehirAlabaligi = "fishnames/nehir_alabaligi.png";
            public const string OtSazani = "fishnames/ot_sazani.png";
            public const string Palamut = "fishnames/palamut.png";
            public const string Ringa = "fishnames/ringa.png";
            public const string SacBoyasiTemizleyici = "fishnames/sac_boyasi_temizleyici.png";
            public const string SariSacBoyasi = "fishnames/sari_sac_boyasi.png";
            public const string Sazan = "fishnames/sazan.png";
            public const string SevimliBalik = "fishnames/sevimli_balik.png";
            public const string SiyahSacBoyasi = "fishnames/siyah_sac_boyasi.png";
            public const string Som = "fishnames/som.png";
            public const string Sudak = "fishnames/sudak.png";
            public const string Tekir = "fishnames/tekir.png";
            public const string Yabbie = "fishnames/yabbie.png";
            public const string YayinBaligi = "fishnames/yayin_baligi.png";
            public const string YilanBasiBaligi = "fishnames/yilan_basi_baligi.png";
            public const string Zargana = "fishnames/zargana.png";

            public static readonly IReadOnlyList<string> All = new[]
            {
                AltinAnahtar,
                AltinParcasi,
                AltinSudak,
                AltinYuzuk,
                AynaliSazan,
                BeyazSacBoyasi,
                BilgeKralinEldiveni,
                BilgeKralinSembolu,
                BuyukSudak,
                DenizKiziAnahtari,
                DereAlabaligi,
                GokkusagiAlabaligi,
                GorunmezlikPelerini,
                GumusAnahtar,
                Hamsi,
                Kadife,
                KahverengiSacBoyasi,
                KirmiziSacBoyasi,
                KralYengeci,
                KurbagaBaligi,
                Levrek,
                LucyYuzugu,
                Lufer,
                NehirAlabaligi,
                OtSazani,
                Palamut,
                Ringa,
                SacBoyasiTemizleyici,
                SariSacBoyasi,
                Sazan,
                SevimliBalik,
                SiyahSacBoyasi,
                Som,
                Sudak,
                Tekir,
                Yabbie,
                YayinBaligi,
                YilanBasiBaligi,
                Zargana
            };
        }

        /// <summary>
        /// Durum ve bildirim şablonları (Waypoints).
        /// </summary>
        public static class Waypoints
        {
            public const string AltinTonBaligi1 = "waypoints/altin_ton_baligi1.png";
            public const string AltinTonBaligi2 = "waypoints/altin_ton_baligi2.png";
            public const string AltinTonBaligi3 = "waypoints/altin_ton_baligi3.png";
            public const string YakalananBalik = "waypoints/yakalanan_balik.png";
            public const string YemiKaybettin = "waypoints/yemi_kaybettin.png";
            public const string SolucanTaktin = "waypoints/solucan_taktin.png";
            public const string AnkiYemi = "waypoints/anki_yemi.png";
            public const string Tutamazsin = "waypoints/tutamazsin.png";
            public const string Yapboz = "waypoints/yapboz.png";

            public static readonly IReadOnlyList<string> All = new[]
            {
                SolucanTaktin,
                AnkiYemi,
                AltinTonBaligi1,
                AltinTonBaligi2,
                AltinTonBaligi3,
                YakalananBalik,
                YemiKaybettin,
                Tutamazsin,
                Yapboz
            };
        }

        /// <summary>
        /// Oyun pencere bileşenleri, menü başlıkları ve arayüz parçaları (Window Parts).
        /// </summary>
        public static class WindowParts
        {
            public const string EquipmentMenuTitle = "window_parts/EquipmentMenuTitle.png";
            public const string EquipmentMenuExitButton = "window_parts/EquipmentMenuExitButton.png";
            public const string FishingMenuTitle = "window_parts/FishingMenuTitle.png";
            public const string SaleExitButton = "window_parts/SaleExitButton.png";
            public const string DropItemQuestion = "window_parts/DropItemQuestion.png";
            public const string DropItemQuestionYesButton = "window_parts/DropItemQuestionYesButton.png";
            
            // Envanter Sayfaları (Kapalı & Açık/Aktif Şablonları)
            public const string Page1 = "window_parts/page1.png";
            public const string Page2 = "window_parts/page2.png";
            public const string Page3 = "window_parts/page3.png";
            public const string Page4 = "window_parts/page4.png";

            public const string Page1Acik = "window_parts/page1_acik.png";
            public const string Page2Acik = "window_parts/page2_acik.png";
            public const string Page3Acik = "window_parts/page3_acik.png";
            public const string Page4Acik = "window_parts/page4_acik.png";

            public static readonly IReadOnlyList<string> All = new[]
            {
                EquipmentMenuTitle,
                EquipmentMenuExitButton,
                Page1,
                Page2,
                Page3,
                Page4,
                Page1Acik,
                Page2Acik,
                Page3Acik,
                Page4Acik,
                FishingMenuTitle,
                SaleExitButton,
                DropItemQuestion,
                DropItemQuestionYesButton
            };
        }

        /// <summary>
        /// Envanter eşyaları ve slot şablonları (Inventory Items).
        /// </summary>
        public static class InventoryItems
        {
            public const string EmptySlot = "inventory_items/emptySlot.png";
            public const string Yem = "inventory_items/yem.png";
            public const string Yem200 = "inventory_items/yem200.png";
            public const string Ates = "inventory_items/ates.png";
            

            public static readonly IReadOnlyList<string> All = new[]
            {
                EmptySlot,
                Yem,
                Yem200,
                Ates
            };
        }

        /// <summary>
        /// Balıkçı NPC ve Market diyalog şablonları (Fisherman).
        /// </summary>
        public static class Fisherman
        {
            public const string Balikci = "fisherman/balikci.png";
            public const string Balikci2 = "fisherman/balikci2.png";
            public const string MarketiAc = "fisherman/marketiAc.png";
            public const string KampAtesiFloor = "fisherman/kampAtesiFloor.png";
            public const string KampAtesiFloor2 = "fisherman/kampAtesiFloor2.png";

            public const string MarketTitle = "fisherman/marketTitle.png";
            public static readonly IReadOnlyList<string> All = new[]
            {
                MarketTitle,
                Balikci,
                Balikci2,
                MarketiAc,
                KampAtesiFloor,
                KampAtesiFloor2
            };
        }

        /// <summary>
        /// Envanterdeki balık ve nesne simge şablonları (Fish Icon Templates).
        /// </summary>
        public static class FishIconTemplates
        {
            /// <summary>
            /// Yaygın balık simgeleri (Common).
            /// </summary>
            public static class Common
            {
                public const string BuyukSudakBaligi = "fishIconTemplates/common/Büyük_Sudak_Balığı.png";
                public const string DereAlabaligi = "fishIconTemplates/common/Dere_Alabalığı.png";
                public const string GokkusagiAlabaligi = "fishIconTemplates/common/Gökkuşağı_Alabalığı.png";
                public const string Hamsi = "fishIconTemplates/common/Hamsi.png";
                public const string Levrek = "fishIconTemplates/common/Levrek.png";
                public const string LuferBaligi = "fishIconTemplates/common/Lüfer_Balığı.png";
                public const string NehirAlabaligi = "fishIconTemplates/common/Nehir_Alabalığı.png";
                public const string OtSazani = "fishIconTemplates/common/Ot_Sazanı.png";
                public const string RingaBaligi = "fishIconTemplates/common/Ringa_Balığı.png";
                public const string Sazan = "fishIconTemplates/common/Sazan.png";
                public const string SomBaligi = "fishIconTemplates/common/Som_Balığı.png";
                public const string SudakBaligi = "fishIconTemplates/common/Sudak_Balığı.png";
                public const string TekirBaligi = "fishIconTemplates/common/Tekir_Balığı.png";
                public const string YayinBaligi = "fishIconTemplates/common/Yayın_Balığı.png";
                public const string Zargana = "fishIconTemplates/common/Zargana.png";

                public static readonly IReadOnlyList<string> All = new[]
                {
                    BuyukSudakBaligi,
                    DereAlabaligi,
                    GokkusagiAlabaligi,
                    Hamsi,
                    Levrek,
                    LuferBaligi,
                    NehirAlabaligi,
                    OtSazani,
                    RingaBaligi,
                    Sazan,
                    SomBaligi,
                    SudakBaligi,
                    TekirBaligi,
                    YayinBaligi,
                    Zargana
                };
            }

            /// <summary>
            /// Nadir balık simgeleri (Rare).
            /// </summary>
            public static class Rare
            {
                public const string AltinSudakBaligi = "fishIconTemplates/rare/Altın_Sudak_Balığı.png";
                public const string AynaliSazan = "fishIconTemplates/rare/Aynalı_Sazan.png";
                public const string KadifeBaligi = "fishIconTemplates/rare/Kadife_Balığı.png";
                public const string KralYengeci = "fishIconTemplates/rare/Kral_Yengeci.png";
                public const string KurbagaBaligi = "fishIconTemplates/rare/Kurbağa_Balığı.png";
                public const string PalamutBaligi = "fishIconTemplates/rare/Palamut_Balığı.png";
                public const string SevimliBalik = "fishIconTemplates/rare/Sevimli_Balık.png";
                public const string YabbieYengeci = "fishIconTemplates/rare/Yabbie_Yengeci.png";
                public const string YilanBasiBaligi = "fishIconTemplates/rare/Yılan_Başı_Balığı.png";

                public static readonly IReadOnlyList<string> All = new[]
                {
                    AltinSudakBaligi,
                    AynaliSazan,
                    KadifeBaligi,
                    KralYengeci,
                    KurbagaBaligi,
                    PalamutBaligi,
                    SevimliBalik,
                    YabbieYengeci,
                    YilanBasiBaligi
                };
            }

            /// <summary>
            /// Diğer nesne ve eşya simgeleri (Others).
            /// </summary>
            public static class Others
            {
                public const string AltinAnahtar = "fishIconTemplates/others/Altın_Anahtar.png";
                public const string AltinParcasi = "fishIconTemplates/others/Altın_Parçası.png";
                public const string AltinYuzuk = "fishIconTemplates/others/Altın_Yüzük.png";
                public const string BeyazSacBoyasi = "fishIconTemplates/others/Beyaz_Saç_Boyası.png";
                public const string BilgeKralinEldiveni = "fishIconTemplates/others/Bilge_Kralın_Eldiveni.png";
                public const string BilgeKralinSembolu = "fishIconTemplates/others/Bilge_Kralın_Sembolü.png";
                public const string DenizKiziAnahtari = "fishIconTemplates/others/Deniz_Kızı_Anahtarı.png";
                public const string GorunmezlikPelerini = "fishIconTemplates/others/Görünmezlik_Pelerini.png";
                public const string GumusAnahtar = "fishIconTemplates/others/Gümüş_Anahtar.png";
                public const string KahverengiSacBoyasi = "fishIconTemplates/others/Kahverengi_Saç_Boyası.png";
                public const string KirmiziSacBoyasi = "fishIconTemplates/others/Kırmızı_Saç_Boyası.png";
                public const string LucyninYuzugu = "fishIconTemplates/others/Lucy'nin_Yüzüğü.png";
                public const string SacBoyasiTemizleyici = "fishIconTemplates/others/Saç_Boyası_Temizleyici.png";
                public const string SariSacBoyasi = "fishIconTemplates/others/Sarı_Saç_Boyası.png";
                public const string SiyahSacBoyasi = "fishIconTemplates/others/Siyah_Saç_Boyası.png";

                /// <summary>
                /// Ölü balıktan çıkan ganimet simgeleri (DeadFishLoot).
                /// </summary>
                public static class DeadFishLoot
                {
                    public const string BeyazInci = "fishIconTemplates/others/deadFishLoot/Beyaz_İnci.png";
                    public const string Istiridye = "fishIconTemplates/others/deadFishLoot/İstiridye.png";
                    public const string KankirmiziInci = "fishIconTemplates/others/deadFishLoot/Kankırmızı_İnci.png";
                    public const string MaviInci = "fishIconTemplates/others/deadFishLoot/Mavi_İnci.png";
                    public const string TasParcasi = "fishIconTemplates/others/deadFishLoot/Taş_Parçası.png";

                    public static readonly IReadOnlyList<string> All = new[]
                    {
                        BeyazInci,
                        Istiridye,
                        KankirmiziInci,
                        MaviInci,
                        TasParcasi
                    };
                }

                public static readonly IReadOnlyList<string> All = new[]
                {
                    AltinAnahtar,
                    AltinParcasi,
                    AltinYuzuk,
                    BeyazSacBoyasi,
                    BilgeKralinEldiveni,
                    BilgeKralinSembolu,
                    DenizKiziAnahtari,
                    GorunmezlikPelerini,
                    GumusAnahtar,
                    KahverengiSacBoyasi,
                    KirmiziSacBoyasi,
                    LucyninYuzugu,
                    SacBoyasiTemizleyici,
                    SariSacBoyasi,
                    SiyahSacBoyasi,
                    DeadFishLoot.BeyazInci,
                    DeadFishLoot.Istiridye,
                    DeadFishLoot.KankirmiziInci,
                    DeadFishLoot.MaviInci,
                    DeadFishLoot.TasParcasi
                };
            }

            /// <summary>
            /// Ölü balık simgeleri (Dead Fishes).
            /// </summary>
            public static class DeadFishes
            {
                /// <summary>
                /// Yaygın ölü balık simgeleri (Common Dead Fishes).
                /// </summary>
                public static class Common
                {
                    public const string OluBuyukSudakBaligi = "fishIconTemplates/common/Ölü_Büyük_Sudak_Balığı.png";
                    public const string OluDereAlabaligi = "fishIconTemplates/common/Ölü_Dere_Alabalığı.png";
                    public const string OluGokkusagiAlabaligi = "fishIconTemplates/common/Ölü_Gökkuşağı_Alabalığı.png";
                    public const string OluHamsi = "fishIconTemplates/common/Ölü_Hamsi.png";
                    public const string OluLevrek = "fishIconTemplates/common/Ölü_Levrek.png";
                    public const string OluLuferBaligi = "fishIconTemplates/common/Ölü_Lüfer_Balığı.png";
                    public const string OluNehirAlabaligi = "fishIconTemplates/common/Ölü_Nehir_Alabalığı.png";
                    public const string OluOtSazani = "fishIconTemplates/common/Ölü_Ot_Sazanı.png";
                    public const string OluRingaBaligi = "fishIconTemplates/common/Ölü_Ringa_Balığı.png";
                    public const string OluSazan = "fishIconTemplates/common/Ölü_Sazan.png";
                    public const string OluSomBaligi = "fishIconTemplates/common/Ölü_Som_Balığı.png";
                    public const string OluSudakBaligi = "fishIconTemplates/common/Ölü_Sudak_Balığı.png";
                    public const string OluTekirBaligi = "fishIconTemplates/common/Ölü_Tekir_Balığı.png";
                    public const string OluYayinBaligi = "fishIconTemplates/common/Ölü_Yayın_Balığı.png";
                    public const string OluZargana = "fishIconTemplates/common/Ölü_Zargana.png";

                    public static readonly IReadOnlyList<string> All = new[]
                    {
                        OluBuyukSudakBaligi,
                        OluDereAlabaligi,
                        OluGokkusagiAlabaligi,
                        OluHamsi,
                        OluLevrek,
                        OluLuferBaligi,
                        OluNehirAlabaligi,
                        OluOtSazani,
                        OluRingaBaligi,
                        OluSazan,
                        OluSomBaligi,
                        OluSudakBaligi,
                        OluTekirBaligi,
                        OluYayinBaligi,
                        OluZargana
                    };
                }

                /// <summary>
                /// Nadir ölü balık simgeleri (Rare Dead Fishes).
                /// </summary>
                public static class Rare
                {
                    public const string OluAltinSudakBaligi = "fishIconTemplates/rare/Ölü_Altın_Sudak_Balığı.png";
                    public const string OluAynaliSazan = "fishIconTemplates/rare/Ölü_Aynalı_Sazan.png";
                    public const string OluKadifeBaligi = "fishIconTemplates/rare/Ölü_Kadife_Balığı.png";
                    public const string OluKralYengeci = "fishIconTemplates/rare/Ölü_Kral_Yengeci.png";
                    public const string OluKurbagaBaligi = "fishIconTemplates/rare/Ölü_Kurbağa_Balığı.png";
                    public const string OluPalamutBaligi = "fishIconTemplates/rare/Ölü_Palamut_Balığı.png";
                    public const string OluSevimliBalik = "fishIconTemplates/rare/Ölü_Sevimli_Balık.png";
                    public const string OluYabbieYengeci = "fishIconTemplates/rare/Ölü_Yabbie_Yengeci.png";
                    public const string OluYilanBasiBaligi = "fishIconTemplates/rare/Ölü_Yılan_Başı_Balığı.png";

                    public static readonly IReadOnlyList<string> All = new[]
                    {
                        OluAltinSudakBaligi,
                        OluAynaliSazan,
                        OluKadifeBaligi,
                        OluKralYengeci,
                        OluKurbagaBaligi,
                        OluPalamutBaligi,
                        OluSevimliBalik,
                        OluYabbieYengeci,
                        OluYilanBasiBaligi
                    };
                }

                public static readonly IReadOnlyList<string> All = new[]
                {
                    // Common
                    Common.OluBuyukSudakBaligi,
                    Common.OluDereAlabaligi,
                    Common.OluGokkusagiAlabaligi,
                    Common.OluHamsi,
                    Common.OluLevrek,
                    Common.OluLuferBaligi,
                    Common.OluNehirAlabaligi,
                    Common.OluOtSazani,
                    Common.OluRingaBaligi,
                    Common.OluSazan,
                    Common.OluSomBaligi,
                    Common.OluSudakBaligi,
                    Common.OluTekirBaligi,
                    Common.OluYayinBaligi,
                    Common.OluZargana,

                    // Rare
                    Rare.OluAltinSudakBaligi,
                    Rare.OluAynaliSazan,
                    Rare.OluKadifeBaligi,
                    Rare.OluKralYengeci,
                    Rare.OluKurbagaBaligi,
                    Rare.OluPalamutBaligi,
                    Rare.OluSevimliBalik,
                    Rare.OluYabbieYengeci,
                    Rare.OluYilanBasiBaligi
                };
            }

            /// <summary>
            /// Izgara balık simgeleri (Grilled Fishes).
            /// </summary>
            public static class GrilledFishes
            {
                /// <summary>
                /// Yaygın ızgara balık simgeleri (Common Grilled Fishes).
                /// </summary>
                public static class Common
                {
                    public const string IzgaraBuyukSudakBaligi = "fishIconTemplates/common/Izgara_Büyük_Sudak_Balığı.png";
                    public const string IzgaraDereAlabaligi = "fishIconTemplates/common/Izgara_Dere_Alabalığı.png";
                    public const string IzgaraGokkusagiAlabaligi = "fishIconTemplates/common/Izgara_Gökkuşağı_Alabalığı.png";
                    public const string IzgaraHamsi = "fishIconTemplates/common/Izgara_Hamsi.png";
                    public const string IzgaraLevrek = "fishIconTemplates/common/Izgara_Levrek.png";
                    public const string IzgaraLuferBaligi = "fishIconTemplates/common/Izgara_Lüfer_Balığı.png";
                    public const string IzgaraNehirAlabaligi = "fishIconTemplates/common/Izgara_Nehir_Alabalığı.png";
                    public const string IzgaraOtSazani = "fishIconTemplates/common/Izgara_Ot_Sazanı.png";
                    public const string IzgaraRingaBaligi = "fishIconTemplates/common/Izgara_Ringa_Balığı.png";
                    public const string IzgaraSazan = "fishIconTemplates/common/Izgara_Sazan.png";
                    public const string IzgaraSomBaligi = "fishIconTemplates/common/Izgara_Som_Balığı.png";
                    public const string IzgaraSudakBaligi = "fishIconTemplates/common/Izgara_Sudak_Balığı.png";
                    public const string IzgaraTekirBaligi = "fishIconTemplates/common/Izgara_Tekir_Balığı.png";
                    public const string IzgaraYayinBaligi = "fishIconTemplates/common/Izgara_Yayın_Balığı.png";
                    public const string IzgaraZargana = "fishIconTemplates/common/Izgara_Zargana.png";

                    public static readonly IReadOnlyList<string> All = new[]
                    {
                        IzgaraBuyukSudakBaligi,
                        IzgaraDereAlabaligi,
                        IzgaraGokkusagiAlabaligi,
                        IzgaraHamsi,
                        IzgaraLevrek,
                        IzgaraLuferBaligi,
                        IzgaraNehirAlabaligi,
                        IzgaraOtSazani,
                        IzgaraRingaBaligi,
                        IzgaraSazan,
                        IzgaraSomBaligi,
                        IzgaraSudakBaligi,
                        IzgaraTekirBaligi,
                        IzgaraYayinBaligi,
                        IzgaraZargana
                    };
                }

                /// <summary>
                /// Nadir ızgara balık simgeleri (Rare Grilled Fishes).
                /// </summary>
                public static class Rare
                {
                    public const string IzgaraAltinSudakBaligi = "fishIconTemplates/rare/Izgara_Altın_Sudak_Balığı.png";
                    public const string IzgaraAynaliSazan = "fishIconTemplates/rare/Izgara_Aynalı_Sazan.png";
                    public const string IzgaraKadifeBaligi = "fishIconTemplates/rare/Izgara_Kadife_Balığı.png";
                    public const string IzgaraKralYengeci = "fishIconTemplates/rare/Izgara_Kral_Yengeci.png";
                    public const string IzgaraKurbagaBaligi = "fishIconTemplates/rare/Izgara_Kurbağa_Balığı.png";
                    public const string IzgaraPalamutBaligi = "fishIconTemplates/rare/Izgara_Palamut_Balığı.png";
                    public const string IzgaraSevimliBalik = "fishIconTemplates/rare/Izgara_Sevimli_Balık.png";
                    public const string IzgaraYabbieYengeci = "fishIconTemplates/rare/Izgara_Yabbie_Yengeci.png";
                    public const string IzgaraYilanBasiBaligi = "fishIconTemplates/rare/Izgara_Yılan_Başı_Balığı.png";

                    public static readonly IReadOnlyList<string> All = new[]
                    {
                        IzgaraAltinSudakBaligi,
                        IzgaraAynaliSazan,
                        IzgaraKadifeBaligi,
                        IzgaraKralYengeci,
                        IzgaraKurbagaBaligi,
                        IzgaraPalamutBaligi,
                        IzgaraSevimliBalik,
                        IzgaraYabbieYengeci,
                        IzgaraYilanBasiBaligi
                    };
                }

                public static readonly IReadOnlyList<string> All = new[]
                {
                    // Common
                    Common.IzgaraBuyukSudakBaligi,
                    Common.IzgaraDereAlabaligi,
                    Common.IzgaraGokkusagiAlabaligi,
                    Common.IzgaraHamsi,
                    Common.IzgaraLevrek,
                    Common.IzgaraLuferBaligi,
                    Common.IzgaraNehirAlabaligi,
                    Common.IzgaraOtSazani,
                    Common.IzgaraRingaBaligi,
                    Common.IzgaraSazan,
                    Common.IzgaraSomBaligi,
                    Common.IzgaraSudakBaligi,
                    Common.IzgaraTekirBaligi,
                    Common.IzgaraYayinBaligi,
                    Common.IzgaraZargana,

                    // Rare
                    Rare.IzgaraAltinSudakBaligi,
                    Rare.IzgaraAynaliSazan,
                    Rare.IzgaraKadifeBaligi,
                    Rare.IzgaraKralYengeci,
                    Rare.IzgaraKurbagaBaligi,
                    Rare.IzgaraPalamutBaligi,
                    Rare.IzgaraSevimliBalik,
                    Rare.IzgaraYabbieYengeci,
                    Rare.IzgaraYilanBasiBaligi
                };
            }

            public static readonly IReadOnlyList<string> All = new[]
            {
                // Common
                Common.BuyukSudakBaligi,
                Common.DereAlabaligi,
                Common.GokkusagiAlabaligi,
                Common.Hamsi,
                Common.Levrek,
                Common.LuferBaligi,
                Common.NehirAlabaligi,
                Common.OtSazani,
                Common.RingaBaligi,
                Common.Sazan,
                Common.SomBaligi,
                Common.SudakBaligi,
                Common.TekirBaligi,
                Common.YayinBaligi,
                Common.Zargana,

                // Rare
                Rare.AltinSudakBaligi,
                Rare.AynaliSazan,
                Rare.KadifeBaligi,
                Rare.KralYengeci,
                Rare.KurbagaBaligi,
                Rare.PalamutBaligi,
                Rare.SevimliBalik,
                Rare.YabbieYengeci,
                Rare.YilanBasiBaligi,

                // DeadFishes (Common)
                DeadFishes.Common.OluBuyukSudakBaligi,
                DeadFishes.Common.OluDereAlabaligi,
                DeadFishes.Common.OluGokkusagiAlabaligi,
                DeadFishes.Common.OluHamsi,
                DeadFishes.Common.OluLevrek,
                DeadFishes.Common.OluLuferBaligi,
                DeadFishes.Common.OluNehirAlabaligi,
                DeadFishes.Common.OluOtSazani,
                DeadFishes.Common.OluRingaBaligi,
                DeadFishes.Common.OluSazan,
                DeadFishes.Common.OluSomBaligi,
                DeadFishes.Common.OluSudakBaligi,
                DeadFishes.Common.OluTekirBaligi,
                DeadFishes.Common.OluYayinBaligi,
                DeadFishes.Common.OluZargana,

                // DeadFishes (Rare)
                DeadFishes.Rare.OluAltinSudakBaligi,
                DeadFishes.Rare.OluAynaliSazan,
                DeadFishes.Rare.OluKadifeBaligi,
                DeadFishes.Rare.OluKralYengeci,
                DeadFishes.Rare.OluKurbagaBaligi,
                DeadFishes.Rare.OluPalamutBaligi,
                DeadFishes.Rare.OluSevimliBalik,
                DeadFishes.Rare.OluYabbieYengeci,
                DeadFishes.Rare.OluYilanBasiBaligi,

                // GrilledFishes (Common)
                GrilledFishes.Common.IzgaraBuyukSudakBaligi,
                GrilledFishes.Common.IzgaraDereAlabaligi,
                GrilledFishes.Common.IzgaraGokkusagiAlabaligi,
                GrilledFishes.Common.IzgaraHamsi,
                GrilledFishes.Common.IzgaraLevrek,
                GrilledFishes.Common.IzgaraLuferBaligi,
                GrilledFishes.Common.IzgaraNehirAlabaligi,
                GrilledFishes.Common.IzgaraOtSazani,
                GrilledFishes.Common.IzgaraRingaBaligi,
                GrilledFishes.Common.IzgaraSazan,
                GrilledFishes.Common.IzgaraSomBaligi,
                GrilledFishes.Common.IzgaraSudakBaligi,
                GrilledFishes.Common.IzgaraTekirBaligi,
                GrilledFishes.Common.IzgaraYayinBaligi,
                GrilledFishes.Common.IzgaraZargana,

                // GrilledFishes (Rare)
                GrilledFishes.Rare.IzgaraAltinSudakBaligi,
                GrilledFishes.Rare.IzgaraAynaliSazan,
                GrilledFishes.Rare.IzgaraKadifeBaligi,
                GrilledFishes.Rare.IzgaraKralYengeci,
                GrilledFishes.Rare.IzgaraKurbagaBaligi,
                GrilledFishes.Rare.IzgaraPalamutBaligi,
                GrilledFishes.Rare.IzgaraSevimliBalik,
                GrilledFishes.Rare.IzgaraYabbieYengeci,
                GrilledFishes.Rare.IzgaraYilanBasiBaligi,

                // Others
                Others.AltinAnahtar,
                Others.AltinParcasi,
                Others.AltinYuzuk,
                Others.BeyazSacBoyasi,
                Others.BilgeKralinEldiveni,
                Others.BilgeKralinSembolu,
                Others.DenizKiziAnahtari,
                Others.GorunmezlikPelerini,
                Others.GumusAnahtar,
                Others.KahverengiSacBoyasi,
                Others.KirmiziSacBoyasi,
                Others.LucyninYuzugu,
                Others.SacBoyasiTemizleyici,
                Others.SariSacBoyasi,
                Others.SiyahSacBoyasi,

                // DeadFishLoot
                Others.DeadFishLoot.BeyazInci,
                Others.DeadFishLoot.Istiridye,
                Others.DeadFishLoot.KankirmiziInci,
                Others.DeadFishLoot.MaviInci,
                Others.DeadFishLoot.TasParcasi
            };
        }

        /// <summary>
        /// Ölü balık şablonları üst düzey erişim (Dead Fishes).
        /// </summary>
        public static class DeadFishes
        {
            public static readonly IReadOnlyList<string> All = FishIconTemplates.DeadFishes.All;
        }

        /// <summary>
        /// Izgara balık şablonları üst düzey erişim (Grilled Fishes).
        /// </summary>
        public static class GrilledFishes
        {
            public static readonly IReadOnlyList<string> All = FishIconTemplates.GrilledFishes.All;
        }

        /// <summary>
        /// Projedeki tüm şablonların listesi.
        /// </summary>
        public static readonly IReadOnlyList<string> AllTemplates;

        static TemplateConstants()
        {
            var all = new List<string>();
            all.AddRange(AutoPass.All);
            all.AddRange(FishNames.All);
            all.AddRange(Waypoints.All);
            all.AddRange(WindowParts.All);
            all.AddRange(InventoryItems.All);
            all.AddRange(Fisherman.All);
            all.AddRange(FishIconTemplates.All);
            AllTemplates = all.AsReadOnly();
        }

        #endregion

        #region 2. Dosya Yolu Çözümleme (Path Resolution)

        /// <summary>
        /// Göreceli şablon yolunu (örn: "fishnames/sudak.png") disktteki mutlak (absolute) dosya yoluna dönüştürür.
        /// Hem derleme (bin/Debug/...) hem de geliştirme proje kök dizinini otomatik olarak kontrol eder.
        /// </summary>
        public static string? GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;

            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar)
                                            .Replace('\\', Path.DirectorySeparatorChar);

            // 1. Doğrudan veya BaseDirectory/Assets/templates/ kontrolü
            string p1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "templates", normalized);
            if (File.Exists(p1)) return p1;

            // 2. Geliştirme zamanı kaynak klasör kontrolü (../../../Assets/templates)
            string p2 = Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "templates", normalized);
            if (File.Exists(p2)) return p2;

            // 3. BaseDirectory içinde doğrudan göreceli yol kontrolü
            string p3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, normalized);
            if (File.Exists(p3)) return p3;

            // 4. Doğrudan var olan mutlak yol kontrolü
            if (File.Exists(relativePath)) return relativePath;

            return null;
        }

        #endregion

        #region 3. OpenCvSharp Mat Önbellekleme (Cache & Memory Management)

        private static readonly ConcurrentDictionary<string, Mat> _matCache = new();
        private static readonly ConcurrentDictionary<string, Mat> _grayMatCache = new();
        private static readonly object _lockObj = new();

        /// <summary>
        /// Belirtilen şablonun OpenCvSharp <see cref="Mat"/> nesnesini önbellekten hızlıca döndürür.
        /// Önbellekte yoksa diskten yükleyip hafızaya alır.
        /// </summary>
        public static Mat? GetMat(string templateRelativePath, ImreadModes mode = ImreadModes.Color)
        {
            string cacheKey = $"{templateRelativePath}_{(int)mode}";
            if (_matCache.TryGetValue(cacheKey, out Mat? cachedMat) && cachedMat != null && !cachedMat.IsDisposed)
            {
                return cachedMat;
            }

            lock (_lockObj)
            {
                if (_matCache.TryGetValue(cacheKey, out cachedMat) && cachedMat != null && !cachedMat.IsDisposed)
                {
                    return cachedMat;
                }

                string? fullPath = GetFullPath(templateRelativePath);
                if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                {
                    return null;
                }

                Mat loaded = Cv2.ImRead(fullPath, mode);
                if (loaded.Empty())
                {
                    loaded.Dispose();
                    return null;
                }

                _matCache[cacheKey] = loaded;
                return loaded;
            }
        }

        /// <summary>
        /// Şablonun Grayscale (Gri Tonlamalı) <see cref="Mat"/> kopyasını önbellekten döndürür.
        /// Gri tonlama karşılaştırmaları işlemci yükünü ciddi oranda azaltır.
        /// </summary>
        public static Mat? GetGrayMat(string templateRelativePath)
        {
            if (_grayMatCache.TryGetValue(templateRelativePath, out Mat? cachedGray) && cachedGray != null && !cachedGray.IsDisposed)
            {
                return cachedGray;
            }

            lock (_lockObj)
            {
                if (_grayMatCache.TryGetValue(templateRelativePath, out cachedGray) && cachedGray != null && !cachedGray.IsDisposed)
                {
                    return cachedGray;
                }

                Mat? colorMat = GetMat(templateRelativePath, ImreadModes.Color);
                if (colorMat == null || colorMat.Empty()) return null;

                Mat gray = new Mat();
                Cv2.CvtColor(colorMat, gray, ColorConversionCodes.BGR2GRAY);
                _grayMatCache[templateRelativePath] = gray;
                return gray;
            }
        }

        /// <summary>
        /// Tüm şablonları hafızaya önceden yükler (Preload).
        /// Bot döngüsünde ilk eşleşmedeki I/O gecikmelerini önlemek için başlatma aşamasında çağrılabilir.
        /// </summary>
        public static void PreloadAll()
        {
            foreach (var templatePath in AllTemplates)
            {
                GetMat(templatePath);
                GetGrayMat(templatePath);
            }
        }

        /// <summary>
        /// Önbellekteki tüm Mat nesnelerini dispose eder ve hafızayı serbest bırakır.
        /// </summary>
        public static void ClearCache()
        {
            lock (_lockObj)
            {
                foreach (var mat in _matCache.Values)
                {
                    if (mat != null && !mat.IsDisposed)
                    {
                        mat.Dispose();
                    }
                }
                _matCache.Clear();

                foreach (var mat in _grayMatCache.Values)
                {
                    if (mat != null && !mat.IsDisposed)
                    {
                        mat.Dispose();
                    }
                }
                _grayMatCache.Clear();
            }
        }

        #endregion

        #region 4. Template Matching (Şablon Eşleme) Yardımcı Metotları

        /// <summary>
        /// Kaynak görsel üzerinde (Mat) tek bir şablonu arar ve eşleşme sonucunu döndürür.
        /// </summary>
        /// <param name="sourceMat">Arama yapılacak ana görsel (Örn: Ekran görüntüsü)</param>
        /// <param name="templateRelativePath">TemplateConstants içerisindeki şablon yolu</param>
        /// <param name="threshold">Kabul edilebilir minimum benzerlik eşiği (Varsayılan: 0.85)</param>
        /// <param name="useGrayscale">Performans ve doğruluk için her iki görseli de gri tonda eşleştirsin mi? (Varsayılan: true)</param>
        /// <param name="mode">Eşleştirme algoritması (Varsayılan: CCoeffNormed)</param>
        public static TemplateMatchResult Match(
            Mat sourceMat,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true,
            TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)
        {
            if (sourceMat == null || sourceMat.Empty())
                return TemplateMatchResult.Failed(templateRelativePath);

            Mat? templateMat = useGrayscale ? GetGrayMat(templateRelativePath) : GetMat(templateRelativePath);
            if (templateMat == null || templateMat.Empty())
                return TemplateMatchResult.Failed(templateRelativePath);

            if (sourceMat.Width < templateMat.Width || sourceMat.Height < templateMat.Height)
                return TemplateMatchResult.Failed(templateRelativePath);

            Mat? srcToUse = null;
            bool disposeSrc = false;

            try
            {
                if (useGrayscale && sourceMat.Channels() > 1)
                {
                    srcToUse = new Mat();
                    Cv2.CvtColor(sourceMat, srcToUse, ColorConversionCodes.BGR2GRAY);
                    disposeSrc = true;
                }
                else
                {
                    srcToUse = sourceMat;
                }

                using (Mat matchResult = new Mat())
                {
                    Cv2.MatchTemplate(srcToUse, templateMat, matchResult, mode);
                    Cv2.MinMaxLoc(matchResult, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

                    bool isNormalizedSqDiff = (mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed);
                    double score = isNormalizedSqDiff ? (1.0 - minVal) : maxVal;
                    Point bestLoc = isNormalizedSqDiff ? minLoc : maxLoc;

                    bool isSuccess = isNormalizedSqDiff ? (minVal <= (1.0 - threshold)) : (maxVal >= threshold);

                    if (isSuccess)
                    {
                        return new TemplateMatchResult
                        {
                            IsSuccess = true,
                            TemplatePath = templateRelativePath,
                            TemplateName = Path.GetFileNameWithoutExtension(templateRelativePath),
                            Confidence = score,
                            Location = new System.Drawing.Point(bestLoc.X, bestLoc.Y),
                            Bounds = new Rectangle(bestLoc.X, bestLoc.Y, templateMat.Width, templateMat.Height)
                        };
                    }

                    return new TemplateMatchResult
                    {
                        IsSuccess = false,
                        TemplatePath = templateRelativePath,
                        TemplateName = Path.GetFileNameWithoutExtension(templateRelativePath),
                        Confidence = score,
                        Location = new System.Drawing.Point(bestLoc.X, bestLoc.Y),
                        Bounds = Rectangle.Empty
                    };
                }
            }
            finally
            {
                if (disposeSrc && srcToUse != null)
                {
                    srcToUse.Dispose();
                }
            }
        }

        /// <summary>
        /// GDI+ Bitmap formatındaki kaynak görsel üzerinde şablon eşleştirmesi yapar.
        /// </summary>
        public static TemplateMatchResult Match(
            Bitmap sourceBitmap,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true,
            TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)
        {
            if (sourceBitmap == null) return TemplateMatchResult.Failed(templateRelativePath);

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return Match(srcMat, templateRelativePath, threshold, useGrayscale, mode);
            }
        }

        /// <summary>
        /// Kaynak görselde belirtilen şablonun bulunup bulunmadığını hızlıca doğrular (bool döner).
        /// </summary>
        public static bool Contains(
            Mat sourceMat,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true)
        {
            var res = Match(sourceMat, templateRelativePath, threshold, useGrayscale);
            return res.IsSuccess;
        }

        /// <summary>
        /// Bitmap formatındaki kaynak görselde belirtilen şablonun bulunup bulunmadığını hızlıca doğrular.
        /// </summary>
        public static bool Contains(
            Bitmap sourceBitmap,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true)
        {
            var res = Match(sourceBitmap, templateRelativePath, threshold, useGrayscale);
            return res.IsSuccess;
        }

        /// <summary>
        /// Verilen şablon listesi arasında kaynak görselde en yüksek benzerlik puanına sahip olan en iyi eşleşmeyi bulur.
        /// Balık adı tespitinde veya durum kontrolünde çok kullanışlıdır.
        /// </summary>
        public static TemplateMatchResult? FindBestMatch(
            Mat sourceMat,
            IEnumerable<string> candidateTemplatePaths,
            double minThreshold = 0.80,
            bool useGrayscale = true)
        {
            if (sourceMat == null || sourceMat.Empty() || candidateTemplatePaths == null)
                return null;

            TemplateMatchResult? bestResult = null;
            double highestScore = -1.0;

            foreach (var templatePath in candidateTemplatePaths)
            {
                var result = Match(sourceMat, templatePath, minThreshold, useGrayscale);
                if (result.IsSuccess && result.Confidence > highestScore)
                {
                    highestScore = result.Confidence;
                    bestResult = result;
                }
            }

            return bestResult;
        }

        /// <summary>
        /// Verilen şablon listesi arasında kaynak Bitmap üzerinde en iyi eşleşmeyi bulur.
        /// </summary>
        public static TemplateMatchResult? FindBestMatch(
            Bitmap sourceBitmap,
            IEnumerable<string> candidateTemplatePaths,
            double minThreshold = 0.80,
            bool useGrayscale = true)
        {
            if (sourceBitmap == null) return null;

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return FindBestMatch(srcMat, candidateTemplatePaths, minThreshold, useGrayscale);
            }
        }

        /// <summary>
        /// Verilen şablon listesi arasında kaynak görselde eşleşen (threshold'u geçen) tüm sonuçlar arasından
        /// yatay eksende EN SOLDA (X koordinatı en küçük olan) eşleşmeyi döndürür.
        /// ChatBox üzerindeki metin okuma ve birden fazla şablonun aynı anda eşleştiği durumlarda birincil (en soldaki) eşleşmeyi seçmek için kullanılır.
        /// </summary>
        public static TemplateMatchResult? FindLeftmostMatch(
            Mat sourceMat,
            IEnumerable<string> candidateTemplatePaths,
            double minThreshold = 0.80,
            bool useGrayscale = true)
        {
            if (sourceMat == null || sourceMat.Empty() || candidateTemplatePaths == null)
                return null;

            var matches = new List<TemplateMatchResult>();

            foreach (var templatePath in candidateTemplatePaths)
            {
                var result = Match(sourceMat, templatePath, minThreshold, useGrayscale);
                if (result.IsSuccess)
                {
                    matches.Add(result);
                }
            }

            if (matches.Count == 0) return null;

            // En soldaki (X koordinatı en küçük) eşleşmeyi seç; X eşitse en yüksek benzerlik puanına sahip olanı seç
            return matches
                .OrderBy(m => m.Location.X)
                .ThenByDescending(m => m.Confidence)
                .First();
        }

        /// <summary>
        /// Verilen şablon listesi arasında kaynak Bitmap üzerinde yatay eksende en soldaki eşleşmeyi bulur.
        /// </summary>
        public static TemplateMatchResult? FindLeftmostMatch(
            Bitmap sourceBitmap,
            IEnumerable<string> candidateTemplatePaths,
            double minThreshold = 0.80,
            bool useGrayscale = true)
        {
            if (sourceBitmap == null) return null;

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return FindLeftmostMatch(srcMat, candidateTemplatePaths, minThreshold, useGrayscale);
            }
        }

        /// <summary>
        /// Kaynak görsel üzerinde aynı şablonun TÜM TEKRARLARINI (Multi-Instance) tespit eder (Örn: Envanterdeki 30 adet emptySlot veya yemler).
        /// Iterative peak suppression (masking) algoritması ile çakışmaları engelleyerek eşik değerini geçen her örneği yakalar.
        /// </summary>
        public static List<TemplateMatchResult> MatchAll(
            Mat sourceMat,
            string templateRelativePath,
            double threshold = 0.85,
            int maxMatches = 150,
            bool useGrayscale = true,
            TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)
        {
            var results = new List<TemplateMatchResult>();
            if (sourceMat == null || sourceMat.Empty()) return results;

            Mat? templateMat = useGrayscale ? GetGrayMat(templateRelativePath) : GetMat(templateRelativePath);
            if (templateMat == null || templateMat.Empty()) return results;

            if (sourceMat.Width < templateMat.Width || sourceMat.Height < templateMat.Height)
                return results;

            Mat? srcToUse = null;
            bool disposeSrc = false;

            try
            {
                if (useGrayscale)
                {
                    if (sourceMat.Channels() > 1)
                    {
                        srcToUse = new Mat();
                        Cv2.CvtColor(sourceMat, srcToUse, ColorConversionCodes.BGR2GRAY);
                        disposeSrc = true;
                    }
                    else
                    {
                        srcToUse = sourceMat;
                    }
                }
                else
                {
                    srcToUse = sourceMat;
                }

                using (Mat matchResult = new Mat())
                {
                    Cv2.MatchTemplate(srcToUse, templateMat, matchResult, mode);

                    int tW = templateMat.Width;
                    int tH = templateMat.Height;
                    int suppressW = Math.Max(4, (int)(tW * 0.75));
                    int suppressH = Math.Max(4, (int)(tH * 0.75));

                    bool isNormalizedSqDiff = (mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed);

                    while (results.Count < maxMatches)
                    {
                        Cv2.MinMaxLoc(matchResult, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

                        double score = isNormalizedSqDiff ? (1.0 - minVal) : maxVal;
                        Point bestLoc = isNormalizedSqDiff ? minLoc : maxLoc;
                        bool isSuccess = isNormalizedSqDiff ? (minVal <= (1.0 - threshold)) : (maxVal >= threshold);

                        if (!isSuccess) break;

                        results.Add(new TemplateMatchResult
                        {
                            IsSuccess = true,
                            TemplatePath = templateRelativePath,
                            TemplateName = Path.GetFileNameWithoutExtension(templateRelativePath),
                            Confidence = score,
                            Location = new System.Drawing.Point(bestLoc.X, bestLoc.Y),
                            Bounds = new Rectangle(bestLoc.X, bestLoc.Y, tW, tH)
                        });

                        // Bulunan noktanın çevresini maskele (Non-Maximum Suppression)
                        int maskX = Math.Max(0, bestLoc.X - (suppressW / 4));
                        int maskY = Math.Max(0, bestLoc.Y - (suppressH / 4));
                        int maskW = Math.Min(suppressW, matchResult.Cols - maskX);
                        int maskH = Math.Min(suppressH, matchResult.Rows - maskY);

                        if (maskW > 0 && maskH > 0)
                        {
                            using (Mat roi = new Mat(matchResult, new Rect(maskX, maskY, maskW, maskH)))
                            {
                                roi.SetTo(isNormalizedSqDiff ? new Scalar(1.0) : new Scalar(0.0));
                            }
                        }
                    }
                }
            }
            finally
            {
                if (disposeSrc && srcToUse != null)
                {
                    srcToUse.Dispose();
                }
            }

            return results;
        }

        /// <summary>
        /// Bitmap formatındaki kaynak görsel üzerinde şablonun tüm kopyalarını (Multi-Instance) bulur.
        /// </summary>
        public static List<TemplateMatchResult> MatchAll(
            Bitmap sourceBitmap,
            string templateRelativePath,
            double threshold = 0.85,
            int maxMatches = 150,
            bool useGrayscale = true,
            TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)
        {
            if (sourceBitmap == null) return new List<TemplateMatchResult>();

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return MatchAll(srcMat, templateRelativePath, threshold, maxMatches, useGrayscale, mode);
            }
        }

        /// <summary>
        /// Verilen şablon listesindeki TÜM şablonların ve her şablonun TÜM TEKRARLARININ eşleşmelerini liste olarak döndürür.
        /// </summary>
        public static List<TemplateMatchResult> FindAllMatches(
            Mat sourceMat,
            IEnumerable<string> candidateTemplatePaths,
            double threshold = 0.85,
            int maxMatchesPerTemplate = 100,
            bool useGrayscale = true)
        {
            var results = new List<TemplateMatchResult>();
            if (sourceMat == null || sourceMat.Empty() || candidateTemplatePaths == null)
                return results;

            foreach (var templatePath in candidateTemplatePaths)
            {
                var matches = MatchAll(sourceMat, templatePath, threshold, maxMatchesPerTemplate, useGrayscale);
                results.AddRange(matches);
            }

            return results;
        }

        /// <summary>
        /// Bitmap formatındaki kaynak görselde aday şablonların tüm kopyalarını bulup döndürür.
        /// </summary>
        public static List<TemplateMatchResult> FindAllMatches(
            Bitmap sourceBitmap,
            IEnumerable<string> candidateTemplatePaths,
            double threshold = 0.85,
            int maxMatchesPerTemplate = 100,
            bool useGrayscale = true)
        {
            if (sourceBitmap == null) return new List<TemplateMatchResult>();

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return FindAllMatches(srcMat, candidateTemplatePaths, threshold, maxMatchesPerTemplate, useGrayscale);
            }
        }

        #endregion

        #region 5. Bitmap <-> Mat Dönüştürücü (Converter)

        /// <summary>
        /// Standart System.Drawing.Bitmap nesnesini OpenCvSharp.Mat nesnesine bellek üzerinden dönüştürür.
        /// </summary>
        public static Mat BitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

            BitmapData? bmpData = null;
            try
            {
                PixelFormat format = bitmap.PixelFormat;
                int channels = 3;
                MatType matType = MatType.CV_8UC3;

                if (format == PixelFormat.Format24bppRgb)
                {
                    channels = 3;
                    matType = MatType.CV_8UC3;
                }
                else if (format == PixelFormat.Format32bppArgb || format == PixelFormat.Format32bppPArgb || format == PixelFormat.Format32bppRgb)
                {
                    channels = 4;
                    matType = MatType.CV_8UC4;
                }
                else if (format == PixelFormat.Format8bppIndexed)
                {
                    channels = 1;
                    matType = MatType.CV_8UC1;
                }
                else
                {
                    // Diğer formatlar için 24bppRGB'ye dönüştürerek oku
                    using (Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics g = Graphics.FromImage(clone))
                        {
                            g.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
                        }
                        return BitmapToMat(clone);
                    }
                }

                bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    format);

                Mat mat = Mat.FromPixelData(bitmap.Height, bitmap.Width, matType, bmpData.Scan0, bmpData.Stride);
                Mat result = mat.Clone(); // Veriyi bağımsız kopyala
                mat.Dispose();

                // Eğer 4 kanallıysa (BGRA), standart BGR'ye dönüştürebiliriz
                if (channels == 4)
                {
                    Mat bgr = new Mat();
                    Cv2.CvtColor(result, bgr, ColorConversionCodes.BGRA2BGR);
                    result.Dispose();
                    return bgr;
                }

                return result;
            }
            finally
            {
                if (bmpData != null)
                {
                    bitmap.UnlockBits(bmpData);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Şablon eşleme (Template Matching) sonucunu ifade eden model sınıfı.
    /// </summary>
    public class TemplateMatchResult
    {
        /// <summary> Eşleşme başarılı ve belirlenen eşik değerinin üzerinde mi? </summary>
        public bool IsSuccess { get; set; }

        /// <summary> Eşleşen şablon dosyasının göreceli yolu (Örn: "fishnames/sudak.png") </summary>
        public string TemplatePath { get; set; } = string.Empty;

        /// <summary> Şablonun dosya adı (Uzantısız, Örn: "sudak") </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary> Benzerlik katsayısı / Güven oranı (0.00 ile 1.00 arası, örn: 0.96) </summary>
        public double Confidence { get; set; }

        /// <summary> Eşleşmenin kaynak görsel üzerindeki sol-üst (X, Y) piksel koordinatı </summary>
        public System.Drawing.Point Location { get; set; }

        /// <summary> Eşleşen bölgenin dikdörtgen sınırları (X, Y, Width, Height) </summary>
        public Rectangle Bounds { get; set; }

        /// <summary> Eşleşen bölgenin merkez piksel koordinatı </summary>
        public System.Drawing.Point CenterPoint => new(
            Location.X + (Bounds.Width / 2),
            Location.Y + (Bounds.Height / 2));

        /// <summary> Görsel arayüzde ve log konsolunda kullanılacak özel vurgu rengi </summary>
        public Color HighlightColor { get; set; } = Color.Lime;

        public static TemplateMatchResult Failed(string templatePath) => new()
        {
            IsSuccess = false,
            TemplatePath = templatePath,
            TemplateName = Path.GetFileNameWithoutExtension(templatePath),
            Confidence = 0,
            Location = System.Drawing.Point.Empty,
            Bounds = Rectangle.Empty
        };

        public override string ToString() =>
            IsSuccess
                ? $"[{TemplateName}] Eşleşti! Benzerlik: %{Confidence * 100:F1}, Konum: ({Location.X}, {Location.Y}), Boyut: {Bounds.Width}x{Bounds.Height}"
                : $"[{TemplateName}] Eşleşmedi. En Yüksek Puan: %{Confidence * 100:F1}";
    }
}
