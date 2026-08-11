namespace Aether.Pages
{
    partial class HomePage
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
            lblTitle = new System.Windows.Forms.Label();
            panelHeader = new System.Windows.Forms.Panel();
            clientNameLabel = new System.Windows.Forms.Label();
            panelHeader.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Location = new System.Drawing.Point(15, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(120, 29);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🏠 Ana Sayfa";
            // 
            // panelHeader
            // 
            panelHeader.BackColor = System.Drawing.Color.FromArgb(30, 30, 35);
            panelHeader.Controls.Add(clientNameLabel);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelHeader.Location = new System.Drawing.Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new System.Drawing.Size(680, 55);
            panelHeader.TabIndex = 0;
            // 
            // clientNameLabel
            // 
            clientNameLabel.AutoSize = true;
            clientNameLabel.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            clientNameLabel.ForeColor = System.Drawing.Color.FromArgb(135, 193, 109);
            clientNameLabel.Location = new System.Drawing.Point(563, 12);
            clientNameLabel.Name = "clientNameLabel";
            clientNameLabel.Size = new System.Drawing.Size(101, 32);
            clientNameLabel.TabIndex = 1;
            clientNameLabel.Text = "Client 0";
            // 
            // HomePage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(24, 24, 27);
            Controls.Add(panelHeader);
            Name = "HomePage";
            Size = new System.Drawing.Size(680, 600);
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label clientNameLabel;
    }
}
