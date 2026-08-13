using Aether.Constants;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Aether.Controls
{
    public partial class ClientCard : UserControl
    {
        private int clientNumber = 0;
        private bool isSelected = false;
        private bool isBotRunning = false;

        public event EventHandler OnCardSelected;
        public event EventHandler OnCheckedChanged;
        public event EventHandler? OnStartClientClicked;

        public ClientCard()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            InitializeComponent();
            RegisterClickEvents(this);

            uıCheckBox1.Click += (s, e) => OnCheckedChanged?.Invoke(this, EventArgs.Empty);
            uıCheckBox1.ValueChanged += (s, value) => OnCheckedChanged?.Invoke(this, EventArgs.Empty);
            startClient.Click += (s, e) => OnStartClientClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RegisterClickEvents(Control control)
        {
            control.Click += Card_Click;
            foreach (Control child in control.Controls)
            {
                if (child != startClient && child != uıCheckBox1)
                {
                    RegisterClickEvents(child);
                }
            }
        }

        private void Card_Click(object sender, EventArgs e)
        {
            OnCardSelected?.Invoke(this, EventArgs.Empty);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ClientName
        {
            get => lblClientName.Text;
            set => lblClientName.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ClientNumber
        {
            get => clientNumber;
            set => clientNumber = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                UpdateCardAppearance();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsChecked
        {
            get => uıCheckBox1.Checked;
            set => uıCheckBox1.Checked = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsBotRunning
        {
            get => isBotRunning;
            set
            {
                isBotRunning = value;
                startClient.Image = isBotRunning ? Properties.Resources.stop_button : Properties.Resources.play_button;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string GameWindowText
        {
            get => selectedGameWindow.Text;
            set
            {
                selectedGameWindow.Text = value;
                if (!string.IsNullOrWhiteSpace(value) && value != "Client Seçilmedi")
                {
                    selectedGameWindow.ForeColor = Colors.YesilAcik;
                }
                else
                {
                    selectedGameWindow.ForeColor = Color.Red;
                }
            }
        }


        private void UpdateCardAppearance()
        {
            if (isSelected)
            {
                cardPanel.RectColor = Colors.YesilKoyu;
            }
            else
            {
                cardPanel.RectColor = Colors.MaviAcik;
            }
        }

        private void cardPanel_MouseLeave(object sender, EventArgs e)
        {
            if (!isSelected)
            {
                cardPanel.RectColor = Colors.MaviAcik;
            }
            else
            {
                cardPanel.RectColor = Colors.YesilKoyu;
            }
        }

        private void cardPanel_MouseHover(object sender, EventArgs e)
        {
            cardPanel.RectColor = Colors.YesilKoyu;
        }
    }
}
