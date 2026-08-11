namespace Aether.Pages
{
    partial class FishBotPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            panelHeader = new Panel();
            fishBotTime = new Label();
            clientNameLabel = new Label();
            fishBotStatus = new Label();
            label1 = new Label();
            gameWindowSelectComboBox = new Sunny.UI.UIComboBox();
            refresGameWindowList = new Sunny.UI.UIButton();
            highlightGameWindowButton = new Sunny.UI.UIButton();
            clientPanel = new Sunny.UI.UIPanel();
            selectGameWindow = new Sunny.UI.UIButton();
            uıPanel1 = new Sunny.UI.UIPanel();
            closeGameMinuteSelectUpDown = new Sunny.UI.UIUpDownTextBox();
            closeGameCheckBox = new Sunny.UI.UICheckBox();
            label2 = new Label();
            uıPanel2 = new Sunny.UI.UIPanel();
            uıLine1 = new Sunny.UI.UILine();
            changeChannelMinuteUpDown = new Sunny.UI.UIUpDownTextBox();
            ch6CheckBox = new Sunny.UI.UICheckBox();
            ch3CheckBox = new Sunny.UI.UICheckBox();
            ch5CheckBox = new Sunny.UI.UICheckBox();
            ch2CheckBox = new Sunny.UI.UICheckBox();
            ch4CheckBox = new Sunny.UI.UICheckBox();
            ch1CheckBox = new Sunny.UI.UICheckBox();
            selectAllChannelsCheckBox = new Sunny.UI.UICheckBox();
            changeChannelCheckBox = new Sunny.UI.UICheckBox();
            label3 = new Label();
            channelsLine = new Sunny.UI.UILine();
            uıPanel3 = new Sunny.UI.UIPanel();
            characterScreenUpDown = new Sunny.UI.UIUpDownTextBox();
            characterScreenCheckBox = new Sunny.UI.UICheckBox();
            label4 = new Label();
            uıPanel4 = new Sunny.UI.UIPanel();
            campFireCountUpDown = new Sunny.UI.UIUpDownTextBox();
            buyCampfireCheckBox = new Sunny.UI.UICheckBox();
            label5 = new Label();
            uıPanel5 = new Sunny.UI.UIPanel();
            wormCountUpDown = new Sunny.UI.UIUpDownTextBox();
            buyWormCheckbox = new Sunny.UI.UICheckBox();
            label6 = new Label();
            uıPanel6 = new Sunny.UI.UIPanel();
            animationModeSwitch = new Sunny.UI.UISwitch();
            label7 = new Label();
            uıPanel7 = new Sunny.UI.UIPanel();
            inventoryPageSelectUpDown = new Sunny.UI.UIUpDownTextBox();
            label8 = new Label();
            uıPanel8 = new Sunny.UI.UIPanel();
            maxFishSpeedTextBox = new Sunny.UI.UITextBox();
            minFishSpeedTextBox = new Sunny.UI.UITextBox();
            label12 = new Label();
            label13 = new Label();
            label11 = new Label();
            label10 = new Label();
            fishFilterPanel = new Sunny.UI.UIPanel();
            uıPanel9 = new Sunny.UI.UIPanel();
            botSettingsNameTextBox = new Sunny.UI.UITextBox();
            loadBotSettingsButton = new Sunny.UI.UIButton();
            botSettingsListComboBox = new Sunny.UI.UIComboBox();
            deleteBotSettingsButton = new Sunny.UI.UIButton();
            label9 = new Label();
            addBotSettingsButton = new Sunny.UI.UIButton();
            panelHeader.SuspendLayout();
            clientPanel.SuspendLayout();
            uıPanel1.SuspendLayout();
            uıPanel2.SuspendLayout();
            uıPanel3.SuspendLayout();
            uıPanel4.SuspendLayout();
            uıPanel5.SuspendLayout();
            uıPanel6.SuspendLayout();
            uıPanel7.SuspendLayout();
            uıPanel8.SuspendLayout();
            uıPanel9.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Calibri", 18F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(15, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(230, 29);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🎣 Balık Botu Ayarları";
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 30, 35);
            panelHeader.Controls.Add(fishBotTime);
            panelHeader.Controls.Add(clientNameLabel);
            panelHeader.Controls.Add(fishBotStatus);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(680, 55);
            panelHeader.TabIndex = 0;
            // 
            // fishBotTime
            // 
            fishBotTime.AutoSize = true;
            fishBotTime.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            fishBotTime.ForeColor = SystemColors.ControlLightLight;
            fishBotTime.Location = new Point(467, 15);
            fishBotTime.Name = "fishBotTime";
            fishBotTime.Size = new Size(94, 28);
            fishBotTime.TabIndex = 1;
            fishBotTime.Text = "00:00:00";
            // 
            // clientNameLabel
            // 
            clientNameLabel.AutoSize = true;
            clientNameLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            clientNameLabel.ForeColor = Color.FromArgb(135, 193, 109);
            clientNameLabel.Location = new Point(563, 12);
            clientNameLabel.Name = "clientNameLabel";
            clientNameLabel.Size = new Size(101, 32);
            clientNameLabel.TabIndex = 1;
            clientNameLabel.Text = "Client 0";
            // 
            // fishBotStatus
            // 
            fishBotStatus.AutoSize = true;
            fishBotStatus.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            fishBotStatus.ForeColor = SystemColors.ControlLightLight;
            fishBotStatus.Location = new Point(312, 15);
            fishBotStatus.Name = "fishBotStatus";
            fishBotStatus.Size = new Size(161, 28);
            fishBotStatus.TabIndex = 1;
            fishBotStatus.Text = "Çalışıyor/Durdu";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15F);
            label1.ForeColor = SystemColors.ControlLightLight;
            label1.Location = new Point(13, 25);
            label1.Name = "label1";
            label1.Size = new Size(98, 28);
            label1.TabIndex = 1;
            label1.Text = "Client seç:";
            // 
            // gameWindowSelectComboBox
            // 
            gameWindowSelectComboBox.DataSource = null;
            gameWindowSelectComboBox.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            gameWindowSelectComboBox.FillColor = Color.White;
            gameWindowSelectComboBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            gameWindowSelectComboBox.ItemHoverColor = Color.FromArgb(155, 200, 255);
            gameWindowSelectComboBox.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            gameWindowSelectComboBox.Location = new Point(13, 66);
            gameWindowSelectComboBox.Margin = new Padding(4, 5, 4, 5);
            gameWindowSelectComboBox.MinimumSize = new Size(63, 0);
            gameWindowSelectComboBox.Name = "gameWindowSelectComboBox";
            gameWindowSelectComboBox.Padding = new Padding(0, 0, 30, 2);
            gameWindowSelectComboBox.Radius = 15;
            gameWindowSelectComboBox.Size = new Size(265, 35);
            gameWindowSelectComboBox.SymbolSize = 24;
            gameWindowSelectComboBox.TabIndex = 2;
            gameWindowSelectComboBox.Text = "Seç....";
            gameWindowSelectComboBox.TextAlignment = ContentAlignment.MiddleLeft;
            gameWindowSelectComboBox.Watermark = "";
            // 
            // refresGameWindowList
            // 
            refresGameWindowList.FillColor = Color.FromArgb(135, 193, 109);
            refresGameWindowList.FillColor2 = Color.FromArgb(135, 193, 109);
            refresGameWindowList.FillHoverColor = Color.FromArgb(135, 193, 109);
            refresGameWindowList.FillPressColor = Color.FromArgb(99, 168, 71);
            refresGameWindowList.FillSelectedColor = Color.FromArgb(99, 168, 71);
            refresGameWindowList.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            refresGameWindowList.ForeColor = Color.Black;
            refresGameWindowList.Location = new Point(131, 25);
            refresGameWindowList.MinimumSize = new Size(1, 1);
            refresGameWindowList.Name = "refresGameWindowList";
            refresGameWindowList.RectColor = Color.FromArgb(135, 193, 109);
            refresGameWindowList.RectHoverColor = Color.FromArgb(135, 193, 109);
            refresGameWindowList.RectPressColor = Color.FromArgb(99, 168, 71);
            refresGameWindowList.RectSelectedColor = Color.FromArgb(99, 168, 71);
            refresGameWindowList.Size = new Size(98, 35);
            refresGameWindowList.TabIndex = 3;
            refresGameWindowList.Text = "Yenile";
            refresGameWindowList.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // highlightGameWindowButton
            // 
            highlightGameWindowButton.FillColor = Color.FromArgb(255, 139, 164);
            highlightGameWindowButton.FillColor2 = Color.FromArgb(255, 139, 164);
            highlightGameWindowButton.FillHoverColor = Color.FromArgb(255, 139, 164);
            highlightGameWindowButton.FillPressColor = Color.FromArgb(244, 103, 136);
            highlightGameWindowButton.FillSelectedColor = Color.FromArgb(244, 103, 136);
            highlightGameWindowButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            highlightGameWindowButton.ForeColor = Color.Black;
            highlightGameWindowButton.Location = new Point(235, 25);
            highlightGameWindowButton.MinimumSize = new Size(1, 1);
            highlightGameWindowButton.Name = "highlightGameWindowButton";
            highlightGameWindowButton.RectColor = Color.FromArgb(255, 139, 164);
            highlightGameWindowButton.RectHoverColor = Color.FromArgb(255, 139, 164);
            highlightGameWindowButton.RectPressColor = Color.FromArgb(244, 103, 136);
            highlightGameWindowButton.RectSelectedColor = Color.FromArgb(244, 103, 136);
            highlightGameWindowButton.Size = new Size(101, 35);
            highlightGameWindowButton.TabIndex = 3;
            highlightGameWindowButton.Text = "Öne Çıkart";
            highlightGameWindowButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // clientPanel
            // 
            clientPanel.BackColor = Color.FromArgb(30, 30, 35);
            clientPanel.Controls.Add(selectGameWindow);
            clientPanel.Controls.Add(gameWindowSelectComboBox);
            clientPanel.Controls.Add(highlightGameWindowButton);
            clientPanel.Controls.Add(label1);
            clientPanel.Controls.Add(refresGameWindowList);
            clientPanel.FillColor = Color.FromArgb(30, 30, 35);
            clientPanel.FillColor2 = Color.FromArgb(30, 30, 35);
            clientPanel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            clientPanel.Location = new Point(15, 208);
            clientPanel.Margin = new Padding(4, 5, 4, 5);
            clientPanel.MinimumSize = new Size(1, 1);
            clientPanel.Name = "clientPanel";
            clientPanel.Radius = 15;
            clientPanel.RectColor = Color.White;
            clientPanel.Size = new Size(353, 131);
            clientPanel.TabIndex = 4;
            clientPanel.Text = null;
            clientPanel.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // selectGameWindow
            // 
            selectGameWindow.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            selectGameWindow.Location = new Point(285, 66);
            selectGameWindow.MinimumSize = new Size(1, 1);
            selectGameWindow.Name = "selectGameWindow";
            selectGameWindow.Size = new Size(53, 35);
            selectGameWindow.TabIndex = 4;
            selectGameWindow.Text = "Seç";
            selectGameWindow.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // uıPanel1
            // 
            uıPanel1.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel1.Controls.Add(closeGameMinuteSelectUpDown);
            uıPanel1.Controls.Add(closeGameCheckBox);
            uıPanel1.Controls.Add(label2);
            uıPanel1.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel1.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel1.Location = new Point(15, 351);
            uıPanel1.Margin = new Padding(4, 5, 4, 5);
            uıPanel1.MinimumSize = new Size(1, 1);
            uıPanel1.Name = "uıPanel1";
            uıPanel1.Radius = 15;
            uıPanel1.RectColor = Color.White;
            uıPanel1.Size = new Size(208, 98);
            uıPanel1.TabIndex = 4;
            uıPanel1.Text = null;
            uıPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // closeGameMinuteSelectUpDown
            // 
            closeGameMinuteSelectUpDown.DoubleStep = 1D;
            closeGameMinuteSelectUpDown.DoubleValue = 25D;
            closeGameMinuteSelectUpDown.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            closeGameMinuteSelectUpDown.IntValue = 25;
            closeGameMinuteSelectUpDown.Location = new Point(13, 52);
            closeGameMinuteSelectUpDown.Margin = new Padding(4, 5, 4, 5);
            closeGameMinuteSelectUpDown.Minimum = 0D;
            closeGameMinuteSelectUpDown.MinimumSize = new Size(1, 16);
            closeGameMinuteSelectUpDown.Name = "closeGameMinuteSelectUpDown";
            closeGameMinuteSelectUpDown.Padding = new Padding(5);
            closeGameMinuteSelectUpDown.ShowText = false;
            closeGameMinuteSelectUpDown.Size = new Size(71, 29);
            closeGameMinuteSelectUpDown.TabIndex = 2;
            closeGameMinuteSelectUpDown.Text = "25";
            closeGameMinuteSelectUpDown.TextAlignment = ContentAlignment.MiddleRight;
            closeGameMinuteSelectUpDown.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            closeGameMinuteSelectUpDown.Watermark = "";
            // 
            // closeGameCheckBox
            // 
            closeGameCheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            closeGameCheckBox.CheckBoxSize = 25;
            closeGameCheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            closeGameCheckBox.ForeColor = SystemColors.ControlLightLight;
            closeGameCheckBox.Location = new Point(13, 15);
            closeGameCheckBox.MinimumSize = new Size(1, 1);
            closeGameCheckBox.Name = "closeGameCheckBox";
            closeGameCheckBox.Size = new Size(178, 29);
            closeGameCheckBox.TabIndex = 0;
            closeGameCheckBox.Text = "Oyundan çık";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.ForeColor = SystemColors.ControlLightLight;
            label2.Location = new Point(91, 56);
            label2.Name = "label2";
            label2.Size = new Size(100, 21);
            label2.TabIndex = 1;
            label2.Text = "Dakika sonra";
            // 
            // uıPanel2
            // 
            uıPanel2.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel2.Controls.Add(uıLine1);
            uıPanel2.Controls.Add(changeChannelMinuteUpDown);
            uıPanel2.Controls.Add(ch6CheckBox);
            uıPanel2.Controls.Add(ch3CheckBox);
            uıPanel2.Controls.Add(ch5CheckBox);
            uıPanel2.Controls.Add(ch2CheckBox);
            uıPanel2.Controls.Add(ch4CheckBox);
            uıPanel2.Controls.Add(ch1CheckBox);
            uıPanel2.Controls.Add(selectAllChannelsCheckBox);
            uıPanel2.Controls.Add(changeChannelCheckBox);
            uıPanel2.Controls.Add(label3);
            uıPanel2.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel2.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel2.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel2.Location = new Point(250, 351);
            uıPanel2.Margin = new Padding(4, 5, 4, 5);
            uıPanel2.MinimumSize = new Size(1, 1);
            uıPanel2.Name = "uıPanel2";
            uıPanel2.Radius = 15;
            uıPanel2.RectColor = Color.White;
            uıPanel2.Size = new Size(414, 213);
            uıPanel2.TabIndex = 4;
            uıPanel2.Text = null;
            uıPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uıLine1
            // 
            uıLine1.BackColor = Color.Transparent;
            uıLine1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıLine1.ForeColor = Color.FromArgb(99, 168, 71);
            uıLine1.LineColor = Color.FromArgb(99, 168, 71);
            uıLine1.Location = new Point(14, 76);
            uıLine1.MinimumSize = new Size(1, 1);
            uıLine1.Name = "uıLine1";
            uıLine1.Size = new Size(378, 29);
            uıLine1.TabIndex = 3;
            uıLine1.Text = "Kanal Seç";
            // 
            // changeChannelMinuteUpDown
            // 
            changeChannelMinuteUpDown.DoubleStep = 1D;
            changeChannelMinuteUpDown.DoubleValue = 25D;
            changeChannelMinuteUpDown.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            changeChannelMinuteUpDown.IntValue = 25;
            changeChannelMinuteUpDown.Location = new Point(169, 35);
            changeChannelMinuteUpDown.Margin = new Padding(4, 5, 4, 5);
            changeChannelMinuteUpDown.Minimum = 0D;
            changeChannelMinuteUpDown.MinimumSize = new Size(1, 16);
            changeChannelMinuteUpDown.Name = "changeChannelMinuteUpDown";
            changeChannelMinuteUpDown.Padding = new Padding(5);
            changeChannelMinuteUpDown.ShowText = false;
            changeChannelMinuteUpDown.Size = new Size(71, 29);
            changeChannelMinuteUpDown.TabIndex = 2;
            changeChannelMinuteUpDown.Text = "25";
            changeChannelMinuteUpDown.TextAlignment = ContentAlignment.MiddleRight;
            changeChannelMinuteUpDown.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            changeChannelMinuteUpDown.Watermark = "";
            // 
            // ch6CheckBox
            // 
            ch6CheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            ch6CheckBox.CheckBoxSize = 25;
            ch6CheckBox.Checked = true;
            ch6CheckBox.Enabled = false;
            ch6CheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            ch6CheckBox.ForeColor = SystemColors.ControlLightLight;
            ch6CheckBox.Location = new Point(321, 150);
            ch6CheckBox.MinimumSize = new Size(1, 1);
            ch6CheckBox.Name = "ch6CheckBox";
            ch6CheckBox.Size = new Size(71, 29);
            ch6CheckBox.TabIndex = 0;
            ch6CheckBox.Text = "Ch6";
            // 
            // ch3CheckBox
            // 
            ch3CheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            ch3CheckBox.CheckBoxSize = 25;
            ch3CheckBox.Checked = true;
            ch3CheckBox.Enabled = false;
            ch3CheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            ch3CheckBox.ForeColor = SystemColors.ControlLightLight;
            ch3CheckBox.Location = new Point(321, 115);
            ch3CheckBox.MinimumSize = new Size(1, 1);
            ch3CheckBox.Name = "ch3CheckBox";
            ch3CheckBox.Size = new Size(71, 29);
            ch3CheckBox.TabIndex = 0;
            ch3CheckBox.Text = "Ch3";
            // 
            // ch5CheckBox
            // 
            ch5CheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            ch5CheckBox.CheckBoxSize = 25;
            ch5CheckBox.Checked = true;
            ch5CheckBox.Enabled = false;
            ch5CheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            ch5CheckBox.ForeColor = SystemColors.ControlLightLight;
            ch5CheckBox.Location = new Point(244, 150);
            ch5CheckBox.MinimumSize = new Size(1, 1);
            ch5CheckBox.Name = "ch5CheckBox";
            ch5CheckBox.Size = new Size(71, 29);
            ch5CheckBox.TabIndex = 0;
            ch5CheckBox.Text = "Ch5";
            // 
            // ch2CheckBox
            // 
            ch2CheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            ch2CheckBox.CheckBoxSize = 25;
            ch2CheckBox.Checked = true;
            ch2CheckBox.Enabled = false;
            ch2CheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            ch2CheckBox.ForeColor = SystemColors.ControlLightLight;
            ch2CheckBox.Location = new Point(244, 115);
            ch2CheckBox.MinimumSize = new Size(1, 1);
            ch2CheckBox.Name = "ch2CheckBox";
            ch2CheckBox.Size = new Size(71, 29);
            ch2CheckBox.TabIndex = 0;
            ch2CheckBox.Text = "Ch2";
            // 
            // ch4CheckBox
            // 
            ch4CheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            ch4CheckBox.CheckBoxSize = 25;
            ch4CheckBox.Checked = true;
            ch4CheckBox.Enabled = false;
            ch4CheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            ch4CheckBox.ForeColor = SystemColors.ControlLightLight;
            ch4CheckBox.Location = new Point(167, 150);
            ch4CheckBox.MinimumSize = new Size(1, 1);
            ch4CheckBox.Name = "ch4CheckBox";
            ch4CheckBox.Size = new Size(71, 29);
            ch4CheckBox.TabIndex = 0;
            ch4CheckBox.Text = "Ch4";
            // 
            // ch1CheckBox
            // 
            ch1CheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            ch1CheckBox.CheckBoxSize = 25;
            ch1CheckBox.Checked = true;
            ch1CheckBox.Enabled = false;
            ch1CheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            ch1CheckBox.ForeColor = SystemColors.ControlLightLight;
            ch1CheckBox.Location = new Point(167, 115);
            ch1CheckBox.MinimumSize = new Size(1, 1);
            ch1CheckBox.Name = "ch1CheckBox";
            ch1CheckBox.Size = new Size(71, 29);
            ch1CheckBox.TabIndex = 0;
            ch1CheckBox.Text = "Ch1";
            // 
            // selectAllChannelsCheckBox
            // 
            selectAllChannelsCheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            selectAllChannelsCheckBox.CheckBoxSize = 25;
            selectAllChannelsCheckBox.Checked = true;
            selectAllChannelsCheckBox.Enabled = false;
            selectAllChannelsCheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            selectAllChannelsCheckBox.ForeColor = SystemColors.ControlLightLight;
            selectAllChannelsCheckBox.Location = new Point(14, 133);
            selectAllChannelsCheckBox.MinimumSize = new Size(1, 1);
            selectAllChannelsCheckBox.Name = "selectAllChannelsCheckBox";
            selectAllChannelsCheckBox.Size = new Size(178, 29);
            selectAllChannelsCheckBox.TabIndex = 0;
            selectAllChannelsCheckBox.Text = "Tüm Kanallar";
            // 
            // changeChannelCheckBox
            // 
            changeChannelCheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            changeChannelCheckBox.CheckBoxSize = 25;
            changeChannelCheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            changeChannelCheckBox.ForeColor = SystemColors.ControlLightLight;
            changeChannelCheckBox.Location = new Point(13, 35);
            changeChannelCheckBox.MinimumSize = new Size(1, 1);
            changeChannelCheckBox.Name = "changeChannelCheckBox";
            changeChannelCheckBox.Size = new Size(178, 29);
            changeChannelCheckBox.TabIndex = 0;
            changeChannelCheckBox.Text = "Kanal Değiştir";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.ForeColor = SystemColors.ControlLightLight;
            label3.Location = new Point(247, 39);
            label3.Name = "label3";
            label3.Size = new Size(100, 21);
            label3.TabIndex = 1;
            label3.Text = "Dakika sonra";
            // 
            // channelsLine
            // 
            channelsLine.BackColor = Color.Transparent;
            channelsLine.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            channelsLine.ForeColor = Color.FromArgb(99, 168, 71);
            channelsLine.LineColor = Color.FromArgb(99, 168, 71);
            channelsLine.Location = new Point(15, 691);
            channelsLine.MinimumSize = new Size(1, 1);
            channelsLine.Name = "channelsLine";
            channelsLine.Size = new Size(649, 29);
            channelsLine.TabIndex = 3;
            channelsLine.Text = "Balık Filtresi";
            // 
            // uıPanel3
            // 
            uıPanel3.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel3.Controls.Add(characterScreenUpDown);
            uıPanel3.Controls.Add(characterScreenCheckBox);
            uıPanel3.Controls.Add(label4);
            uıPanel3.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel3.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel3.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel3.Location = new Point(15, 466);
            uıPanel3.Margin = new Padding(4, 5, 4, 5);
            uıPanel3.MinimumSize = new Size(1, 1);
            uıPanel3.Name = "uıPanel3";
            uıPanel3.Radius = 15;
            uıPanel3.RectColor = Color.White;
            uıPanel3.Size = new Size(208, 98);
            uıPanel3.TabIndex = 4;
            uıPanel3.Text = null;
            uıPanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // characterScreenUpDown
            // 
            characterScreenUpDown.DoubleStep = 1D;
            characterScreenUpDown.DoubleValue = 25D;
            characterScreenUpDown.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            characterScreenUpDown.IntValue = 25;
            characterScreenUpDown.Location = new Point(13, 52);
            characterScreenUpDown.Margin = new Padding(4, 5, 4, 5);
            characterScreenUpDown.Minimum = 0D;
            characterScreenUpDown.MinimumSize = new Size(1, 16);
            characterScreenUpDown.Name = "characterScreenUpDown";
            characterScreenUpDown.Padding = new Padding(5);
            characterScreenUpDown.ShowText = false;
            characterScreenUpDown.Size = new Size(71, 29);
            characterScreenUpDown.TabIndex = 2;
            characterScreenUpDown.Text = "25";
            characterScreenUpDown.TextAlignment = ContentAlignment.MiddleRight;
            characterScreenUpDown.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            characterScreenUpDown.Watermark = "";
            // 
            // characterScreenCheckBox
            // 
            characterScreenCheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            characterScreenCheckBox.CheckBoxSize = 25;
            characterScreenCheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            characterScreenCheckBox.ForeColor = SystemColors.ControlLightLight;
            characterScreenCheckBox.Location = new Point(13, 15);
            characterScreenCheckBox.MinimumSize = new Size(1, 1);
            characterScreenCheckBox.Name = "characterScreenCheckBox";
            characterScreenCheckBox.Size = new Size(178, 29);
            characterScreenCheckBox.TabIndex = 0;
            characterScreenCheckBox.Text = "Karakter At";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F);
            label4.ForeColor = SystemColors.ControlLightLight;
            label4.Location = new Point(91, 56);
            label4.Name = "label4";
            label4.Size = new Size(100, 21);
            label4.TabIndex = 1;
            label4.Text = "Dakika sonra";
            // 
            // uıPanel4
            // 
            uıPanel4.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel4.Controls.Add(campFireCountUpDown);
            uıPanel4.Controls.Add(buyCampfireCheckBox);
            uıPanel4.Controls.Add(label5);
            uıPanel4.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel4.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel4.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel4.Location = new Point(15, 585);
            uıPanel4.Margin = new Padding(4, 5, 4, 5);
            uıPanel4.MinimumSize = new Size(1, 1);
            uıPanel4.Name = "uıPanel4";
            uıPanel4.Radius = 15;
            uıPanel4.RectColor = Color.White;
            uıPanel4.Size = new Size(208, 98);
            uıPanel4.TabIndex = 4;
            uıPanel4.Text = null;
            uıPanel4.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // campFireCountUpDown
            // 
            campFireCountUpDown.DoubleStep = 1D;
            campFireCountUpDown.DoubleValue = 5D;
            campFireCountUpDown.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            campFireCountUpDown.IntValue = 5;
            campFireCountUpDown.Location = new Point(13, 52);
            campFireCountUpDown.Margin = new Padding(4, 5, 4, 5);
            campFireCountUpDown.Minimum = 0D;
            campFireCountUpDown.MinimumSize = new Size(1, 16);
            campFireCountUpDown.Name = "campFireCountUpDown";
            campFireCountUpDown.Padding = new Padding(5);
            campFireCountUpDown.ShowText = false;
            campFireCountUpDown.Size = new Size(71, 29);
            campFireCountUpDown.TabIndex = 2;
            campFireCountUpDown.Text = "5";
            campFireCountUpDown.TextAlignment = ContentAlignment.MiddleRight;
            campFireCountUpDown.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            campFireCountUpDown.Watermark = "";
            // 
            // buyCampfireCheckBox
            // 
            buyCampfireCheckBox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            buyCampfireCheckBox.CheckBoxSize = 25;
            buyCampfireCheckBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            buyCampfireCheckBox.ForeColor = SystemColors.ControlLightLight;
            buyCampfireCheckBox.Location = new Point(13, 15);
            buyCampfireCheckBox.MinimumSize = new Size(1, 1);
            buyCampfireCheckBox.Name = "buyCampfireCheckBox";
            buyCampfireCheckBox.Size = new Size(178, 29);
            buyCampfireCheckBox.TabIndex = 0;
            buyCampfireCheckBox.Text = "Kamp Ateşi Satın Al";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F);
            label5.ForeColor = SystemColors.ControlLightLight;
            label5.Location = new Point(91, 56);
            label5.Name = "label5";
            label5.Size = new Size(42, 21);
            label5.TabIndex = 1;
            label5.Text = "Adet";
            // 
            // uıPanel5
            // 
            uıPanel5.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel5.Controls.Add(wormCountUpDown);
            uıPanel5.Controls.Add(buyWormCheckbox);
            uıPanel5.Controls.Add(label6);
            uıPanel5.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel5.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel5.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel5.Location = new Point(250, 585);
            uıPanel5.Margin = new Padding(4, 5, 4, 5);
            uıPanel5.MinimumSize = new Size(1, 1);
            uıPanel5.Name = "uıPanel5";
            uıPanel5.Radius = 15;
            uıPanel5.RectColor = Color.White;
            uıPanel5.Size = new Size(208, 98);
            uıPanel5.TabIndex = 4;
            uıPanel5.Text = null;
            uıPanel5.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // wormCountUpDown
            // 
            wormCountUpDown.DoubleStep = 1D;
            wormCountUpDown.DoubleValue = 5D;
            wormCountUpDown.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            wormCountUpDown.IntValue = 5;
            wormCountUpDown.Location = new Point(13, 52);
            wormCountUpDown.Margin = new Padding(4, 5, 4, 5);
            wormCountUpDown.Minimum = 0D;
            wormCountUpDown.MinimumSize = new Size(1, 16);
            wormCountUpDown.Name = "wormCountUpDown";
            wormCountUpDown.Padding = new Padding(5);
            wormCountUpDown.ShowText = false;
            wormCountUpDown.Size = new Size(71, 29);
            wormCountUpDown.TabIndex = 2;
            wormCountUpDown.Text = "5";
            wormCountUpDown.TextAlignment = ContentAlignment.MiddleRight;
            wormCountUpDown.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            wormCountUpDown.Watermark = "";
            // 
            // buyWormCheckbox
            // 
            buyWormCheckbox.CheckBoxColor = Color.FromArgb(99, 168, 71);
            buyWormCheckbox.CheckBoxSize = 25;
            buyWormCheckbox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            buyWormCheckbox.ForeColor = SystemColors.ControlLightLight;
            buyWormCheckbox.Location = new Point(13, 15);
            buyWormCheckbox.MinimumSize = new Size(1, 1);
            buyWormCheckbox.Name = "buyWormCheckbox";
            buyWormCheckbox.Size = new Size(178, 29);
            buyWormCheckbox.TabIndex = 0;
            buyWormCheckbox.Text = "Solucan Satın Al";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 12F);
            label6.ForeColor = SystemColors.ControlLightLight;
            label6.Location = new Point(91, 56);
            label6.Name = "label6";
            label6.Size = new Size(109, 21);
            label6.TabIndex = 1;
            label6.Text = "Adet(50xAdet)";
            // 
            // uıPanel6
            // 
            uıPanel6.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel6.Controls.Add(animationModeSwitch);
            uıPanel6.Controls.Add(label7);
            uıPanel6.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel6.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel6.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel6.Location = new Point(466, 585);
            uıPanel6.Margin = new Padding(4, 5, 4, 5);
            uıPanel6.MinimumSize = new Size(1, 1);
            uıPanel6.Name = "uıPanel6";
            uıPanel6.Radius = 15;
            uıPanel6.RectColor = Color.White;
            uıPanel6.Size = new Size(198, 98);
            uıPanel6.TabIndex = 4;
            uıPanel6.Text = null;
            uıPanel6.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // animationModeSwitch
            // 
            animationModeSwitch.ActiveColor = Color.FromArgb(99, 168, 71);
            animationModeSwitch.ActiveText = "Zırh Değiştir";
            animationModeSwitch.DisabledColor = Color.FromArgb(255, 139, 164);
            animationModeSwitch.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            animationModeSwitch.InActiveColor = Color.FromArgb(255, 139, 164);
            animationModeSwitch.InActiveText = "Binek Kullan";
            animationModeSwitch.Location = new Point(28, 48);
            animationModeSwitch.MinimumSize = new Size(1, 1);
            animationModeSwitch.Name = "animationModeSwitch";
            animationModeSwitch.Size = new Size(139, 29);
            animationModeSwitch.TabIndex = 5;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 14F);
            label7.ForeColor = SystemColors.ControlLightLight;
            label7.Location = new Point(24, 12);
            label7.Name = "label7";
            label7.Size = new Size(152, 25);
            label7.TabIndex = 1;
            label7.Text = "Animasyon İptali";
            // 
            // uıPanel7
            // 
            uıPanel7.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel7.Controls.Add(inventoryPageSelectUpDown);
            uıPanel7.Controls.Add(label8);
            uıPanel7.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel7.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel7.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel7.Location = new Point(391, 297);
            uıPanel7.Margin = new Padding(4, 5, 4, 5);
            uıPanel7.MinimumSize = new Size(1, 1);
            uıPanel7.Name = "uıPanel7";
            uıPanel7.Radius = 15;
            uıPanel7.RectColor = Color.White;
            uıPanel7.Size = new Size(273, 42);
            uıPanel7.TabIndex = 4;
            uıPanel7.Text = null;
            uıPanel7.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // inventoryPageSelectUpDown
            // 
            inventoryPageSelectUpDown.DoubleStep = 1D;
            inventoryPageSelectUpDown.DoubleValue = 1D;
            inventoryPageSelectUpDown.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            inventoryPageSelectUpDown.IntValue = 1;
            inventoryPageSelectUpDown.Location = new Point(171, 5);
            inventoryPageSelectUpDown.Margin = new Padding(4, 5, 4, 5);
            inventoryPageSelectUpDown.MaxLength = 4;
            inventoryPageSelectUpDown.Minimum = 0D;
            inventoryPageSelectUpDown.MinimumSize = new Size(1, 16);
            inventoryPageSelectUpDown.Name = "inventoryPageSelectUpDown";
            inventoryPageSelectUpDown.Padding = new Padding(5);
            inventoryPageSelectUpDown.ShowText = false;
            inventoryPageSelectUpDown.Size = new Size(71, 29);
            inventoryPageSelectUpDown.TabIndex = 2;
            inventoryPageSelectUpDown.Text = "1";
            inventoryPageSelectUpDown.TextAlignment = ContentAlignment.MiddleRight;
            inventoryPageSelectUpDown.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            inventoryPageSelectUpDown.Watermark = "";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 12F);
            label8.ForeColor = SystemColors.ControlLightLight;
            label8.Location = new Point(28, 9);
            label8.Name = "label8";
            label8.Size = new Size(127, 21);
            label8.TabIndex = 1;
            label8.Text = "Envanter Sayfası:";
            // 
            // uıPanel8
            // 
            uıPanel8.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel8.Controls.Add(maxFishSpeedTextBox);
            uıPanel8.Controls.Add(minFishSpeedTextBox);
            uıPanel8.Controls.Add(label12);
            uıPanel8.Controls.Add(label13);
            uıPanel8.Controls.Add(label11);
            uıPanel8.Controls.Add(label10);
            uıPanel8.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel8.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel8.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel8.Location = new Point(391, 205);
            uıPanel8.Margin = new Padding(4, 5, 4, 5);
            uıPanel8.MinimumSize = new Size(1, 1);
            uıPanel8.Name = "uıPanel8";
            uıPanel8.Radius = 15;
            uıPanel8.RectColor = Color.White;
            uıPanel8.Size = new Size(273, 82);
            uıPanel8.TabIndex = 4;
            uıPanel8.Text = null;
            uıPanel8.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // maxFishSpeedTextBox
            // 
            maxFishSpeedTextBox.DoubleValue = 250D;
            maxFishSpeedTextBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            maxFishSpeedTextBox.IntValue = 250;
            maxFishSpeedTextBox.Location = new Point(181, 34);
            maxFishSpeedTextBox.Margin = new Padding(4, 5, 4, 5);
            maxFishSpeedTextBox.MinimumSize = new Size(1, 16);
            maxFishSpeedTextBox.Name = "maxFishSpeedTextBox";
            maxFishSpeedTextBox.Padding = new Padding(5);
            maxFishSpeedTextBox.ShowText = false;
            maxFishSpeedTextBox.Size = new Size(70, 29);
            maxFishSpeedTextBox.TabIndex = 5;
            maxFishSpeedTextBox.Text = "250";
            maxFishSpeedTextBox.TextAlignment = ContentAlignment.MiddleLeft;
            maxFishSpeedTextBox.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            maxFishSpeedTextBox.Watermark = "Max";
            // 
            // minFishSpeedTextBox
            // 
            minFishSpeedTextBox.DoubleValue = 150D;
            minFishSpeedTextBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            minFishSpeedTextBox.IntValue = 150;
            minFishSpeedTextBox.Location = new Point(89, 34);
            minFishSpeedTextBox.Margin = new Padding(4, 5, 4, 5);
            minFishSpeedTextBox.MinimumSize = new Size(1, 16);
            minFishSpeedTextBox.Name = "minFishSpeedTextBox";
            minFishSpeedTextBox.Padding = new Padding(5);
            minFishSpeedTextBox.ShowText = false;
            minFishSpeedTextBox.Size = new Size(68, 29);
            minFishSpeedTextBox.TabIndex = 5;
            minFishSpeedTextBox.Text = "150";
            minFishSpeedTextBox.TextAlignment = ContentAlignment.MiddleLeft;
            minFishSpeedTextBox.Type = Sunny.UI.UITextBox.UIEditType.Integer;
            minFishSpeedTextBox.Watermark = "Min";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 12F);
            label12.ForeColor = SystemColors.ControlLightLight;
            label12.Location = new Point(181, 8);
            label12.Name = "label12";
            label12.Size = new Size(70, 21);
            label12.TabIndex = 1;
            label12.Text = "Max(ms)";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 12F);
            label13.ForeColor = SystemColors.ControlLightLight;
            label13.Location = new Point(161, 37);
            label13.Name = "label13";
            label13.Size = new Size(16, 21);
            label13.TabIndex = 1;
            label13.Text = "-";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 12F);
            label11.ForeColor = SystemColors.ControlLightLight;
            label11.Location = new Point(89, 8);
            label11.Name = "label11";
            label11.Size = new Size(68, 21);
            label11.TabIndex = 1;
            label11.Text = "Min(ms)";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 12F);
            label10.ForeColor = SystemColors.ControlLightLight;
            label10.Location = new Point(9, 21);
            label10.Name = "label10";
            label10.Size = new Size(73, 42);
            label10.TabIndex = 1;
            label10.Text = "Oltalama\r\n Hızı:";
            label10.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // fishFilterPanel
            // 
            fishFilterPanel.BackColor = Color.FromArgb(24, 24, 27);
            fishFilterPanel.FillColor = Color.FromArgb(24, 24, 27);
            fishFilterPanel.FillColor2 = Color.FromArgb(24, 24, 27);
            fishFilterPanel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            fishFilterPanel.Location = new Point(15, 723);
            fishFilterPanel.Margin = new Padding(0);
            fishFilterPanel.MinimumSize = new Size(1, 1);
            fishFilterPanel.Name = "fishFilterPanel";
            fishFilterPanel.Radius = 0;
            fishFilterPanel.RectColor = Color.Transparent;
            fishFilterPanel.Size = new Size(649, 1085);
            fishFilterPanel.TabIndex = 5;
            fishFilterPanel.Text = null;
            fishFilterPanel.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uıPanel9
            // 
            uıPanel9.BackColor = Color.FromArgb(30, 30, 35);
            uıPanel9.Controls.Add(botSettingsNameTextBox);
            uıPanel9.Controls.Add(loadBotSettingsButton);
            uıPanel9.Controls.Add(botSettingsListComboBox);
            uıPanel9.Controls.Add(deleteBotSettingsButton);
            uıPanel9.Controls.Add(label9);
            uıPanel9.Controls.Add(addBotSettingsButton);
            uıPanel9.FillColor = Color.FromArgb(30, 30, 35);
            uıPanel9.FillColor2 = Color.FromArgb(30, 30, 35);
            uıPanel9.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıPanel9.Location = new Point(15, 72);
            uıPanel9.Margin = new Padding(4, 5, 4, 5);
            uıPanel9.MinimumSize = new Size(1, 1);
            uıPanel9.Name = "uıPanel9";
            uıPanel9.Radius = 15;
            uıPanel9.RectColor = Color.White;
            uıPanel9.Size = new Size(649, 123);
            uıPanel9.TabIndex = 4;
            uıPanel9.Text = null;
            uıPanel9.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // botSettingsNameTextBox
            // 
            botSettingsNameTextBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            botSettingsNameTextBox.Location = new Point(175, 22);
            botSettingsNameTextBox.Margin = new Padding(4, 5, 4, 5);
            botSettingsNameTextBox.MinimumSize = new Size(1, 16);
            botSettingsNameTextBox.Name = "botSettingsNameTextBox";
            botSettingsNameTextBox.Padding = new Padding(5);
            botSettingsNameTextBox.ShowText = false;
            botSettingsNameTextBox.Size = new Size(268, 29);
            botSettingsNameTextBox.TabIndex = 5;
            botSettingsNameTextBox.TextAlignment = ContentAlignment.MiddleLeft;
            botSettingsNameTextBox.Watermark = "Ayar İsmi";
            // 
            // loadBotSettingsButton
            // 
            loadBotSettingsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            loadBotSettingsButton.Location = new Point(453, 61);
            loadBotSettingsButton.MinimumSize = new Size(1, 1);
            loadBotSettingsButton.Name = "loadBotSettingsButton";
            loadBotSettingsButton.Size = new Size(67, 29);
            loadBotSettingsButton.TabIndex = 4;
            loadBotSettingsButton.Text = "Yükle";
            loadBotSettingsButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // botSettingsListComboBox
            // 
            botSettingsListComboBox.DataSource = null;
            botSettingsListComboBox.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            botSettingsListComboBox.FillColor = Color.White;
            botSettingsListComboBox.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            botSettingsListComboBox.ItemHoverColor = Color.FromArgb(155, 200, 255);
            botSettingsListComboBox.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            botSettingsListComboBox.Location = new Point(175, 61);
            botSettingsListComboBox.Margin = new Padding(4, 5, 4, 5);
            botSettingsListComboBox.MinimumSize = new Size(63, 0);
            botSettingsListComboBox.Name = "botSettingsListComboBox";
            botSettingsListComboBox.Padding = new Padding(0, 0, 30, 2);
            botSettingsListComboBox.Size = new Size(268, 29);
            botSettingsListComboBox.SymbolSize = 24;
            botSettingsListComboBox.TabIndex = 0;
            botSettingsListComboBox.Text = "Seç";
            botSettingsListComboBox.TextAlignment = ContentAlignment.MiddleLeft;
            botSettingsListComboBox.Watermark = "";
            // 
            // deleteBotSettingsButton
            // 
            deleteBotSettingsButton.FillColor = Color.FromArgb(255, 139, 164);
            deleteBotSettingsButton.FillColor2 = Color.FromArgb(255, 139, 164);
            deleteBotSettingsButton.FillHoverColor = Color.FromArgb(255, 139, 164);
            deleteBotSettingsButton.FillPressColor = Color.FromArgb(244, 103, 136);
            deleteBotSettingsButton.FillSelectedColor = Color.FromArgb(244, 103, 136);
            deleteBotSettingsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            deleteBotSettingsButton.ForeColor = Color.Black;
            deleteBotSettingsButton.Location = new Point(524, 61);
            deleteBotSettingsButton.MinimumSize = new Size(1, 1);
            deleteBotSettingsButton.Name = "deleteBotSettingsButton";
            deleteBotSettingsButton.RectColor = Color.FromArgb(255, 139, 164);
            deleteBotSettingsButton.RectHoverColor = Color.FromArgb(255, 139, 164);
            deleteBotSettingsButton.RectPressColor = Color.FromArgb(244, 103, 136);
            deleteBotSettingsButton.RectSelectedColor = Color.FromArgb(244, 103, 136);
            deleteBotSettingsButton.Size = new Size(71, 29);
            deleteBotSettingsButton.TabIndex = 3;
            deleteBotSettingsButton.Text = "Sil";
            deleteBotSettingsButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 15F);
            label9.ForeColor = SystemColors.ControlLightLight;
            label9.Location = new Point(41, 42);
            label9.Name = "label9";
            label9.Size = new Size(109, 28);
            label9.TabIndex = 1;
            label9.Text = "Ön Ayarlar:";
            // 
            // addBotSettingsButton
            // 
            addBotSettingsButton.FillColor = Color.FromArgb(135, 193, 109);
            addBotSettingsButton.FillColor2 = Color.FromArgb(135, 193, 109);
            addBotSettingsButton.FillHoverColor = Color.FromArgb(135, 193, 109);
            addBotSettingsButton.FillPressColor = Color.FromArgb(99, 168, 71);
            addBotSettingsButton.FillSelectedColor = Color.FromArgb(99, 168, 71);
            addBotSettingsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            addBotSettingsButton.ForeColor = Color.Black;
            addBotSettingsButton.Location = new Point(451, 22);
            addBotSettingsButton.MinimumSize = new Size(1, 1);
            addBotSettingsButton.Name = "addBotSettingsButton";
            addBotSettingsButton.RectColor = Color.FromArgb(135, 193, 109);
            addBotSettingsButton.RectHoverColor = Color.FromArgb(135, 193, 109);
            addBotSettingsButton.RectPressColor = Color.FromArgb(99, 168, 71);
            addBotSettingsButton.RectSelectedColor = Color.FromArgb(99, 168, 71);
            addBotSettingsButton.Size = new Size(144, 29);
            addBotSettingsButton.TabIndex = 3;
            addBotSettingsButton.Text = "Ekle";
            addBotSettingsButton.TipsFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            // 
            // FishBotPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(24, 24, 27);
            Controls.Add(fishFilterPanel);
            Controls.Add(channelsLine);
            Controls.Add(uıPanel7);
            Controls.Add(uıPanel2);
            Controls.Add(uıPanel6);
            Controls.Add(uıPanel5);
            Controls.Add(uıPanel4);
            Controls.Add(uıPanel9);
            Controls.Add(uıPanel3);
            Controls.Add(uıPanel1);
            Controls.Add(uıPanel8);
            Controls.Add(clientPanel);
            Controls.Add(panelHeader);
            Name = "FishBotPage";
            Size = new Size(680, 1850);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            clientPanel.ResumeLayout(false);
            clientPanel.PerformLayout();
            uıPanel1.ResumeLayout(false);
            uıPanel1.PerformLayout();
            uıPanel2.ResumeLayout(false);
            uıPanel2.PerformLayout();
            uıPanel3.ResumeLayout(false);
            uıPanel3.PerformLayout();
            uıPanel4.ResumeLayout(false);
            uıPanel4.PerformLayout();
            uıPanel5.ResumeLayout(false);
            uıPanel5.PerformLayout();
            uıPanel6.ResumeLayout(false);
            uıPanel6.PerformLayout();
            uıPanel7.ResumeLayout(false);
            uıPanel7.PerformLayout();
            uıPanel8.ResumeLayout(false);
            uıPanel8.PerformLayout();
            uıPanel9.ResumeLayout(false);
            uıPanel9.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitle;
        private Panel panelHeader;
        private Label clientNameLabel;
        private Label label1;
        private Sunny.UI.UIComboBox gameWindowSelectComboBox;
        private Sunny.UI.UIButton refresGameWindowList;
        private Sunny.UI.UIButton highlightGameWindowButton;
        private Sunny.UI.UIPanel clientPanel;
        private Sunny.UI.UIPanel uıPanel1;
        private Sunny.UI.UICheckBox closeGameCheckBox;
        private Sunny.UI.UIUpDownTextBox closeGameMinuteSelectUpDown;
        private Label label2;
        private Sunny.UI.UIPanel uıPanel2;
        private Sunny.UI.UIUpDownTextBox changeChannelMinuteUpDown;
        private Sunny.UI.UICheckBox changeChannelCheckBox;
        private Label label3;
        private Sunny.UI.UICheckBox ch6CheckBox;
        private Sunny.UI.UICheckBox ch3CheckBox;
        private Sunny.UI.UICheckBox ch5CheckBox;
        private Sunny.UI.UICheckBox ch2CheckBox;
        private Sunny.UI.UICheckBox ch4CheckBox;
        private Sunny.UI.UICheckBox ch1CheckBox;
        private Sunny.UI.UICheckBox selectAllChannelsCheckBox;
        private Sunny.UI.UILine channelsLine;
        private Sunny.UI.UIPanel uıPanel3;
        private Sunny.UI.UIUpDownTextBox characterScreenUpDown;
        private Sunny.UI.UICheckBox characterScreenCheckBox;
        private Label label4;
        private Sunny.UI.UIPanel uıPanel4;
        private Sunny.UI.UIUpDownTextBox campFireCountUpDown;
        private Sunny.UI.UICheckBox buyCampfireCheckBox;
        private Label label5;
        private Sunny.UI.UIPanel uıPanel5;
        private Sunny.UI.UIUpDownTextBox wormCountUpDown;
        private Sunny.UI.UICheckBox buyWormCheckbox;
        private Label label6;
        private Sunny.UI.UIPanel uıPanel6;
        private Label label7;
        private Sunny.UI.UISwitch animationModeSwitch;
        private Sunny.UI.UIPanel uıPanel7;
        private Sunny.UI.UIUpDownTextBox inventoryPageSelectUpDown;
        private Label label8;
        private Sunny.UI.UIPanel uıPanel8;
        private Label fishBotTime;
        private Label fishBotStatus;
        private Sunny.UI.UILine uıLine1;
        private Sunny.UI.UIPanel fishFilterPanel;
        private Sunny.UI.UIButton selectGameWindow;
        private Sunny.UI.UIPanel uıPanel9;
        private Sunny.UI.UIComboBox botSettingsListComboBox;
        private Sunny.UI.UITextBox botSettingsNameTextBox;
        private Sunny.UI.UIButton loadBotSettingsButton;
        private Sunny.UI.UIButton deleteBotSettingsButton;
        private Label label9;
        private Sunny.UI.UIButton addBotSettingsButton;
        private Sunny.UI.UITextBox maxFishSpeedTextBox;
        private Sunny.UI.UITextBox minFishSpeedTextBox;
        private Label label12;
        private Label label11;
        private Label label10;
        private Label label13;
    }
}
