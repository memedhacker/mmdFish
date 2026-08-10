namespace Aether.Pages
{
    partial class UpgradePage
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
            clientNameLabel = new Label();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Calibri", 18F, FontStyle.Bold, GraphicsUnit.Point, 162);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(15, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(248, 29);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⚔️ Oto Artı Basma Ayarları";
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(30, 30, 35);
            panelHeader.Controls.Add(clientNameLabel);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(680, 55);
            panelHeader.TabIndex = 0;
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
            // UpgradePage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(24, 24, 27);
            Controls.Add(panelHeader);
            Name = "UpgradePage";
            Size = new Size(680, 600);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitle;
        private Panel panelHeader;
        private Label clientNameLabel;
    }
}
