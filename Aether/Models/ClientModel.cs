using System;

namespace Aether.Models
{
    /// <summary>
    /// Her bir Client nesnesini temsil eden domain veri modeli.
    /// </summary>
    public class ClientModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; }

        // Bot ve modül durum değişkenleri (Default: false)
        public bool IsFishBotRunning { get; set; } = false;
        public bool IsUpgradeBotRunning { get; set; } = false;
        public bool IsFishPuzzleRunning { get; set; } = false;
        public bool IsAlchemyRunning { get; set; } = false;

        public ClientModel() { }

        public ClientModel(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
