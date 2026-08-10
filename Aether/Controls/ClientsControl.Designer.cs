namespace Aether.Controls
{
    partial class ClientsControl
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
            clientListFlowPanel = new DoubleBufferedFlowLayoutPanel();
            customScrollBar1 = new CustomScrollBar();
            SuspendLayout();
            // 
            // clientListFlowPanel
            // 
            clientListFlowPanel.AutoScroll = true;
            clientListFlowPanel.BackColor = Color.FromArgb(24, 24, 27);
            clientListFlowPanel.Dock = DockStyle.Fill;
            clientListFlowPanel.FlowDirection = FlowDirection.TopDown;
            clientListFlowPanel.Location = new Point(0, 0);
            clientListFlowPanel.Name = "clientListFlowPanel";
            clientListFlowPanel.Size = new Size(288, 588);
            clientListFlowPanel.TabIndex = 0;
            clientListFlowPanel.WrapContents = false;
            // 
            // customScrollBar1
            // 
            customScrollBar1.BackColor = Color.Transparent;
            customScrollBar1.Dock = DockStyle.Right;
            customScrollBar1.Location = new Point(288, 0);
            customScrollBar1.Name = "customScrollBar1";
            customScrollBar1.Size = new Size(12, 588);
            customScrollBar1.TabIndex = 1;
            customScrollBar1.TargetControl = clientListFlowPanel;
            // 
            // ClientsControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(clientListFlowPanel);
            Controls.Add(customScrollBar1);
            Name = "ClientsControl";
            Size = new Size(300, 588);
            ResumeLayout(false);
        }

        #endregion

        private DoubleBufferedFlowLayoutPanel clientListFlowPanel;
        private CustomScrollBar customScrollBar1;
    }
}
