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

        public event EventHandler OnCardSelected;
        public event EventHandler OnCheckedChanged;

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
        }

        private void RegisterClickEvents(Control control)
        {
            control.Click += Card_Click;
            foreach (Control child in control.Controls)
            {
                if (child != pictureBox1 && child != uıCheckBox1)
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
