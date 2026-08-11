namespace Aether.Controls
{
    partial class ClientCard
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
            lblClientName = new Label();
            cardPanel = new Sunny.UI.UIPanel();
            uıCheckBox1 = new Sunny.UI.UICheckBox();
            pictureBox1 = new PictureBox();
            selectedGameWindow = new Label();
            cardPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // lblClientName
            // 
            lblClientName.AutoSize = true;
            lblClientName.BackColor = Color.Transparent;
            lblClientName.Font = new Font("Calibri", 18F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblClientName.ForeColor = Color.White;
            lblClientName.Location = new Point(54, 14);
            lblClientName.Name = "lblClientName";
            lblClientName.Size = new Size(88, 29);
            lblClientName.TabIndex = 0;
            lblClientName.Text = "Client 0";
            // 
            // cardPanel
            // 
            cardPanel.BackColor = Color.Transparent;
            cardPanel.Controls.Add(uıCheckBox1);
            cardPanel.Controls.Add(pictureBox1);
            cardPanel.Controls.Add(selectedGameWindow);
            cardPanel.Controls.Add(lblClientName);
            cardPanel.FillColor = Color.Transparent;
            cardPanel.FillColor2 = Color.Transparent;
            cardPanel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            cardPanel.Location = new Point(0, 0);
            cardPanel.Margin = new Padding(4, 5, 4, 5);
            cardPanel.MinimumSize = new Size(1, 1);
            cardPanel.Name = "cardPanel";
            cardPanel.Radius = 15;
            cardPanel.RectColor = Color.FromArgb(89, 189, 255);
            cardPanel.Size = new Size(272, 76);
            cardPanel.TabIndex = 1;
            cardPanel.Text = null;
            cardPanel.TextAlignment = ContentAlignment.MiddleCenter;
            cardPanel.MouseLeave += cardPanel_MouseLeave;
            cardPanel.MouseHover += cardPanel_MouseHover;
            // 
            // uıCheckBox1
            // 
            uıCheckBox1.CheckBoxSize = 25;
            uıCheckBox1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 162);
            uıCheckBox1.ForeColor = Color.FromArgb(48, 48, 48);
            uıCheckBox1.Location = new Point(7, 19);
            uıCheckBox1.MinimumSize = new Size(1, 1);
            uıCheckBox1.Name = "uıCheckBox1";
            uıCheckBox1.Size = new Size(41, 35);
            uıCheckBox1.TabIndex = 3;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Image = Properties.Resources.play_button;
            pictureBox1.Location = new Point(222, 24);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(30, 30);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // selectedGameWindow
            // 
            selectedGameWindow.AutoSize = true;
            selectedGameWindow.Font = new Font("Microsoft Sans Serif", 8F);
            selectedGameWindow.ForeColor = Color.Red;
            selectedGameWindow.Location = new Point(54, 43);
            selectedGameWindow.Name = "selectedGameWindow";
            selectedGameWindow.Size = new Size(81, 13);
            selectedGameWindow.TabIndex = 1;
            selectedGameWindow.Text = "Client Seçilmedi";
            // 
            // ClientCard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(cardPanel);
            Name = "ClientCard";
            Size = new Size(276, 76);
            cardPanel.ResumeLayout(false);
            cardPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label lblClientName;
        private Sunny.UI.UIPanel cardPanel;
        private Label selectedGameWindow;
        private PictureBox pictureBox1;
        private Sunny.UI.UICheckBox uıCheckBox1;
    }
}
