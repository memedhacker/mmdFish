namespace Aether.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            clientsControl1 = new Aether.Controls.ClientsControl();
            clientSettingsPanel = new Panel();
            showPagePanel = new Aether.Controls.DoubleBufferedFlowLayoutPanel();
            pageScrollBar = new Aether.Controls.CustomScrollBar();
            uıButton1 = new Sunny.UI.UIButton();
            uıButton2 = new Sunny.UI.UIButton();
            selectAllButton = new Sunny.UI.UICheckBox();
            pictureBox1 = new PictureBox();
            pFishBotButton = new Sunny.UI.UIButton();
            pPuzzleButton = new Sunny.UI.UIButton();
            pAlchemyButton = new Sunny.UI.UIButton();
            pAntiBanButton = new Sunny.UI.UIButton();
            pUpgradeButton = new Sunny.UI.UIButton();
            settingsButton = new Sunny.UI.UIImageButton();
            testButton = new Sunny.UI.UIButton();
            clientSettingsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)settingsButton).BeginInit();
            SuspendLayout();
            // 
            // clientsControl1
            // 
            clientsControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            clientsControl1.BackColor = Color.Transparent;
            clientsControl1.Location = new Point(9, 191);
            clientsControl1.Margin = new Padding(0);
            clientsControl1.Name = "clientsControl1";
            clientsControl1.Size = new Size(300, 416);
            clientsControl1.TabIndex = 0;
            // 
            // clientSettingsPanel
            // 
            clientSettingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            clientSettingsPanel.BackColor = Color.FromArgb(24, 24, 27);
            clientSettingsPanel.Controls.Add(showPagePanel);
            clientSettingsPanel.Controls.Add(pageScrollBar);
            clientSettingsPanel.Location = new Point(320, 67);
            clientSettingsPanel.Name = "clientSettingsPanel";
            clientSettingsPanel.Size = new Size(703, 739);
            clientSettingsPanel.TabIndex = 1;
            // 
            // showPagePanel
            // 
            showPagePanel.AutoScroll = true;
            showPagePanel.BackColor = Color.FromArgb(24, 24, 27);
            showPagePanel.Dock = DockStyle.Fill;
            showPagePanel.FlowDirection = FlowDirection.TopDown;
            showPagePanel.Location = new Point(0, 0);
            showPagePanel.Margin = new Padding(0);
            showPagePanel.Name = "showPagePanel";
            showPagePanel.Size = new Size(689, 739);
            showPagePanel.TabIndex = 0;
            showPagePanel.WrapContents = false;
            // 
            // pageScrollBar
            // 
            pageScrollBar.BackColor = Color.Transparent;
            pageScrollBar.Dock = DockStyle.Right;
            pageScrollBar.Location = new Point(689, 0);
            pageScrollBar.Name = "pageScrollBar";
            pageScrollBar.Size = new Size(14, 739);
            pageScrollBar.TabIndex = 1;
            // 
            // uıButton1
            // 
            uıButton1.FillColor = Color.FromArgb(0, 177, 255);
            uıButton1.FillColor2 = Color.FromArgb(0, 177, 255);
            uıButton1.FillHoverColor = Color.FromArgb(89, 189, 255);
            uıButton1.FillPressColor = Color.FromArgb(0, 177, 255);
            uıButton1.FillSelectedColor = Color.FromArgb(0, 177, 255);
            uıButton1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıButton1.Location = new Point(9, 610);
            uıButton1.MinimumSize = new Size(1, 1);
            uıButton1.Name = "uıButton1";
            uıButton1.Size = new Size(297, 35);
            uıButton1.TabIndex = 2;
            uıButton1.Text = "Seçilenleri Başlat";
            uıButton1.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // uıButton2
            // 
            uıButton2.FillColor = Color.FromArgb(244, 103, 136);
            uıButton2.FillColor2 = Color.FromArgb(244, 103, 136);
            uıButton2.FillHoverColor = Color.FromArgb(255, 139, 164);
            uıButton2.FillPressColor = Color.FromArgb(244, 103, 136);
            uıButton2.FillSelectedColor = Color.FromArgb(244, 103, 136);
            uıButton2.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıButton2.Location = new Point(9, 651);
            uıButton2.MinimumSize = new Size(1, 1);
            uıButton2.Name = "uıButton2";
            uıButton2.Size = new Size(297, 35);
            uıButton2.TabIndex = 3;
            uıButton2.Text = "Seçilenleri Durdur";
            uıButton2.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // selectAllButton
            // 
            selectAllButton.BackColor = Color.Transparent;
            selectAllButton.CheckBoxSize = 25;
            selectAllButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            selectAllButton.ForeColor = Color.White;
            selectAllButton.Location = new Point(9, 159);
            selectAllButton.MinimumSize = new Size(1, 1);
            selectAllButton.Name = "selectAllButton";
            selectAllButton.Size = new Size(294, 29);
            selectAllButton.TabIndex = 4;
            selectAllButton.Text = "Hepsini Seç";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.logo;
            pictureBox1.Location = new Point(9, 10);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(305, 178);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // pFishBotButton
            // 
            pFishBotButton.Enabled = false;
            pFishBotButton.FillColor = Color.FromArgb(89, 189, 255);
            pFishBotButton.FillColor2 = Color.FromArgb(89, 189, 255);
            pFishBotButton.FillDisableColor = Color.FromArgb(255, 139, 164);
            pFishBotButton.FillHoverColor = Color.FromArgb(89, 189, 255);
            pFishBotButton.FillPressColor = Color.FromArgb(0, 177, 255);
            pFishBotButton.FillSelectedColor = Color.FromArgb(0, 177, 255);
            pFishBotButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            pFishBotButton.ForeColor = Color.Black;
            pFishBotButton.Location = new Point(324, 26);
            pFishBotButton.MinimumSize = new Size(1, 1);
            pFishBotButton.Name = "pFishBotButton";
            pFishBotButton.Radius = 15;
            pFishBotButton.Size = new Size(112, 35);
            pFishBotButton.TabIndex = 0;
            pFishBotButton.Text = "Balık Botu";
            pFishBotButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // pPuzzleButton
            // 
            pPuzzleButton.FillColor = Color.FromArgb(89, 189, 255);
            pPuzzleButton.FillColor2 = Color.FromArgb(89, 189, 255);
            pPuzzleButton.FillDisableColor = Color.FromArgb(255, 139, 164);
            pPuzzleButton.FillHoverColor = Color.FromArgb(89, 189, 255);
            pPuzzleButton.FillPressColor = Color.FromArgb(0, 177, 255);
            pPuzzleButton.FillSelectedColor = Color.FromArgb(0, 177, 255);
            pPuzzleButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            pPuzzleButton.ForeColor = Color.Black;
            pPuzzleButton.Location = new Point(442, 26);
            pPuzzleButton.MinimumSize = new Size(1, 1);
            pPuzzleButton.Name = "pPuzzleButton";
            pPuzzleButton.Radius = 15;
            pPuzzleButton.Size = new Size(116, 35);
            pPuzzleButton.TabIndex = 0;
            pPuzzleButton.Text = "Puzzle Botu";
            pPuzzleButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // pAlchemyButton
            // 
            pAlchemyButton.FillColor = Color.FromArgb(89, 189, 255);
            pAlchemyButton.FillColor2 = Color.FromArgb(89, 189, 255);
            pAlchemyButton.FillDisableColor = Color.FromArgb(255, 139, 164);
            pAlchemyButton.FillHoverColor = Color.FromArgb(89, 189, 255);
            pAlchemyButton.FillPressColor = Color.FromArgb(0, 177, 255);
            pAlchemyButton.FillSelectedColor = Color.FromArgb(0, 177, 255);
            pAlchemyButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            pAlchemyButton.ForeColor = Color.Black;
            pAlchemyButton.Location = new Point(564, 26);
            pAlchemyButton.MinimumSize = new Size(1, 1);
            pAlchemyButton.Name = "pAlchemyButton";
            pAlchemyButton.Radius = 15;
            pAlchemyButton.Size = new Size(116, 35);
            pAlchemyButton.TabIndex = 0;
            pAlchemyButton.Text = "Simya Botu";
            pAlchemyButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // pAntiBanButton
            // 
            pAntiBanButton.FillColor = Color.FromArgb(89, 189, 255);
            pAntiBanButton.FillColor2 = Color.FromArgb(89, 189, 255);
            pAntiBanButton.FillDisableColor = Color.FromArgb(255, 139, 164);
            pAntiBanButton.FillHoverColor = Color.FromArgb(89, 189, 255);
            pAntiBanButton.FillPressColor = Color.FromArgb(0, 177, 255);
            pAntiBanButton.FillSelectedColor = Color.FromArgb(0, 177, 255);
            pAntiBanButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            pAntiBanButton.ForeColor = Color.Black;
            pAntiBanButton.Location = new Point(808, 26);
            pAntiBanButton.MinimumSize = new Size(1, 1);
            pAntiBanButton.Name = "pAntiBanButton";
            pAntiBanButton.Radius = 15;
            pAntiBanButton.Size = new Size(116, 35);
            pAntiBanButton.TabIndex = 0;
            pAntiBanButton.TagString = "";
            pAntiBanButton.Text = "Ban Koruması";
            pAntiBanButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // pUpgradeButton
            // 
            pUpgradeButton.FillColor = Color.FromArgb(89, 189, 255);
            pUpgradeButton.FillColor2 = Color.FromArgb(89, 189, 255);
            pUpgradeButton.FillDisableColor = Color.FromArgb(255, 139, 164);
            pUpgradeButton.FillHoverColor = Color.FromArgb(89, 189, 255);
            pUpgradeButton.FillPressColor = Color.FromArgb(0, 177, 255);
            pUpgradeButton.FillSelectedColor = Color.FromArgb(0, 177, 255);
            pUpgradeButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            pUpgradeButton.ForeColor = Color.Black;
            pUpgradeButton.Location = new Point(686, 26);
            pUpgradeButton.MinimumSize = new Size(1, 1);
            pUpgradeButton.Name = "pUpgradeButton";
            pUpgradeButton.Radius = 15;
            pUpgradeButton.Size = new Size(116, 35);
            pUpgradeButton.TabIndex = 0;
            pUpgradeButton.Text = "Oto Artı Basma";
            pUpgradeButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // settingsButton
            // 
            settingsButton.BackColor = Color.Transparent;
            settingsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            settingsButton.Image = Properties.Resources.settings;
            settingsButton.Location = new Point(988, 26);
            settingsButton.Name = "settingsButton";
            settingsButton.Size = new Size(33, 35);
            settingsButton.SizeMode = PictureBoxSizeMode.StretchImage;
            settingsButton.TabIndex = 6;
            settingsButton.TabStop = false;
            settingsButton.Text = null;
            // 
            // testButton
            // 
            testButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            testButton.Location = new Point(93, 711);
            testButton.MinimumSize = new Size(1, 1);
            testButton.Name = "testButton";
            testButton.Size = new Size(100, 35);
            testButton.TabIndex = 7;
            testButton.Text = "test et";
            testButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            testButton.Click += testButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(14, 14, 17);
            ClientSize = new Size(1033, 818);
            Controls.Add(testButton);
            Controls.Add(settingsButton);
            Controls.Add(pUpgradeButton);
            Controls.Add(pAntiBanButton);
            Controls.Add(pAlchemyButton);
            Controls.Add(pPuzzleButton);
            Controls.Add(pFishBotButton);
            Controls.Add(selectAllButton);
            Controls.Add(uıButton1);
            Controls.Add(uıButton2);
            Controls.Add(clientSettingsPanel);
            Controls.Add(clientsControl1);
            Controls.Add(pictureBox1);
            Name = "MainForm";
            Text = "654654623cc32";
            Load += MainForm_Load;
            clientSettingsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)settingsButton).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Aether.Controls.ClientsControl clientsControl1;
        private Panel clientSettingsPanel;
        private Aether.Controls.DoubleBufferedFlowLayoutPanel showPagePanel;
        private Aether.Controls.CustomScrollBar pageScrollBar;
        private Sunny.UI.UIButton uıButton1;
        private Sunny.UI.UIButton uıButton2;
        private Sunny.UI.UICheckBox selectAllButton;
        private PictureBox pictureBox1;
        private Sunny.UI.UIButton pFishBotButton;
        private Sunny.UI.UIButton pPuzzleButton;
        private Sunny.UI.UIButton pAlchemyButton;
        private Sunny.UI.UIButton pAntiBanButton;
        private Sunny.UI.UIButton pUpgradeButton;
        private Sunny.UI.UIImageButton settingsButton;
        private Sunny.UI.UIButton testButton;
    }
}