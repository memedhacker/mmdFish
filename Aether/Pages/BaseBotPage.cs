using Aether.Models;
using Aether.States;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Aether.Pages
{
    /// <summary>
    /// Tüm bot sayfaları için ortak temel sınıf.
    /// [DesignerCategory("Component")] veya [DesignerCategory("")] kullanımı sayesinde
    /// Visual Studio WinForms Out-Of-Process Designer arka planda 'BaseBotPage' için 
    /// kilitli izole tasarım iş parçacığı çalıştırmaz; alt sayfalar (FishBotPage vb.)
    /// tasarımcı ortamında doğrudan ve pürüzsüz açılır.
    /// </summary>
    [DesignerCategory("Component")]
    public class BaseBotPage : UserControl
    {
        /// <summary>
        /// Seçili client adının yazdırılacağı Label.
        /// Alt sınıflar kendi Designer.cs'indeki label'ı döndürmek üzere override etmelidir.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual Label? ClientNameLabel => null;

        public BaseBotPage()
        {
            // WinForms Designer varsayılan yapıcı metodu
        }

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
            if (ClientNameLabel != null)
            {
                ClientNameLabel.Text = clientInfo?.Name ?? "Seçim Yok";
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Sayfa kapatıldığında bellek sızıntısını önlemek için event aboneliğini kaldır
            ClientState.Instance.OnSelectedClientChanged -= OnSelectedClientChanged;
            base.OnHandleDestroyed(e);
        }
    }
}
