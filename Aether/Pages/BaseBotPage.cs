using Aether.Models;
using Aether.States;
using System;
using System.Windows.Forms;

namespace Aether.Pages
{
    /// <summary>
    /// Tüm bot sayfaları için ortak temel sınıf.
    ///
    /// SORUMLULUKLAR:
    /// - ClientState.OnSelectedClientChanged event aboneliğini yönetir.
    /// - Seçili client adını thread-safe şekilde ClientNameLabel'a yansıtır.
    /// - Handle yok edildiğinde event aboneliğini otomatik temizler.
    ///
    /// ALT SINIF KURALLARI:
    /// - protected override Label ClientNameLabel => clientNameLabel; — mutlaka implemente edilmeli.
    /// - InitializeComponent() constructor'da çağrılmalı.
    /// - FishBotPage gibi ek davranış gereken sayfalar OnLoad'u override edebilir (base.OnLoad(e) çağrılmalı).
    /// </summary>
    public abstract class BaseBotPage : UserControl
    {
        /// <summary>
        /// Seçili client adının yazdırılacağı Label.
        /// Alt sınıf kendi Designer.cs'teki label'ı döndürmelidir.
        /// </summary>
        protected abstract Label ClientNameLabel { get; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                // Sayfa yüklendiğinde mevcut seçili client'ı göster
                UpdateClientDisplay(ClientState.Instance.SelectedClient);

                // Canlı değişim dinleyicisini bağla
                ClientState.Instance.OnSelectedClientChanged += OnSelectedClientChanged;
            }
        }

        private void OnSelectedClientChanged(object? sender, ClientInfo? clientInfo)
        {
            if (InvokeRequired)
                Invoke(new Action(() => UpdateClientDisplay(clientInfo)));
            else
                UpdateClientDisplay(clientInfo);
        }

        private void UpdateClientDisplay(ClientInfo? clientInfo)
        {
            ClientNameLabel.Text = clientInfo?.Name ?? "Seçim Yok";
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Sayfa kapatıldığında bellek sızıntısını önlemek için event aboneliğini kaldır
            ClientState.Instance.OnSelectedClientChanged -= OnSelectedClientChanged;
            base.OnHandleDestroyed(e);
        }
    }
}
