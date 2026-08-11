using System.Collections.Generic;

namespace Aether.Models
{
    /// <summary>
    /// Balık filtresi tablosundaki tek bir öğeye ait eylem seçimlerini tutar.
    /// Sütun ismi (columnKey), fish_filter_config.json'daki sütun headerText değeriyle eşleşir.
    /// </summary>
    public class FishFilterItemState
    {
        /// <summary> Öğenin dosya adından türetilen benzersiz kimliği (örn: "Altın_Sudak_Balığı"). </summary>
        public string ItemKey { get; set; }

        /// <summary> Sütun başlığı → checkbox durumu eşlemesi. Örn: {"Balığı Tut": true, "Pişir": false} </summary>
        public Dictionary<string, bool> ColumnChecks { get; set; }

        public FishFilterItemState(string itemKey)
        {
            ItemKey = itemKey;
            ColumnChecks = new Dictionary<string, bool>();
        }

        /// <summary> Belirtilen sütun adı için kayıtlı değeri döner; yoksa varsayılan değeri döner. </summary>
        public bool GetCheck(string columnKey, bool defaultValue = false)
        {
            return ColumnChecks.TryGetValue(columnKey, out var val) ? val : defaultValue;
        }

        /// <summary> Belirtilen sütun adına ait durumu günceller. </summary>
        public void SetCheck(string columnKey, bool value)
        {
            ColumnChecks[columnKey] = value;
        }
    }
}
