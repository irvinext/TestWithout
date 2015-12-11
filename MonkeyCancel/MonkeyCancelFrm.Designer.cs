namespace MonkeyCancel
{
    partial class MonkeyCancelFrm
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonWriteDump = new System.Windows.Forms.Button();
            this.buttonSaveEdges = new System.Windows.Forms.Button();
            this.flowLayoutPanelInstruments = new System.Windows.Forms.FlowLayoutPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelPriceServer = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelOrderServer = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFillServer = new System.Windows.Forms.ToolStripStatusLabel();
            this.pricesUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.buttonWriteDump);
            this.panel1.Controls.Add(this.buttonSaveEdges);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(924, 46);
            this.panel1.TabIndex = 0;
            // 
            // buttonWriteDump
            // 
            this.buttonWriteDump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonWriteDump.Location = new System.Drawing.Point(864, 12);
            this.buttonWriteDump.Name = "buttonWriteDump";
            this.buttonWriteDump.Size = new System.Drawing.Size(48, 23);
            this.buttonWriteDump.TabIndex = 3;
            this.buttonWriteDump.Text = "Dump";
            this.buttonWriteDump.UseVisualStyleBackColor = true;
            this.buttonWriteDump.Click += new System.EventHandler(this.buttonWriteDump_Click);
            // 
            // buttonSaveEdges
            // 
            this.buttonSaveEdges.Location = new System.Drawing.Point(12, 12);
            this.buttonSaveEdges.Name = "buttonSaveEdges";
            this.buttonSaveEdges.Size = new System.Drawing.Size(48, 23);
            this.buttonSaveEdges.TabIndex = 2;
            this.buttonSaveEdges.Text = "Save";
            this.buttonSaveEdges.UseVisualStyleBackColor = true;
            this.buttonSaveEdges.Click += new System.EventHandler(this.buttonSaveEdges_Click);
            // 
            // flowLayoutPanelInstruments
            // 
            this.flowLayoutPanelInstruments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanelInstruments.AutoScroll = true;
            this.flowLayoutPanelInstruments.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelInstruments.Location = new System.Drawing.Point(0, 52);
            this.flowLayoutPanelInstruments.Name = "flowLayoutPanelInstruments";
            this.flowLayoutPanelInstruments.Size = new System.Drawing.Size(924, 114);
            this.flowLayoutPanelInstruments.TabIndex = 1;
            this.flowLayoutPanelInstruments.WrapContents = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelPriceServer,
            this.toolStripStatusLabelOrderServer,
            this.toolStripStatusLabelFillServer});
            this.statusStrip1.Location = new System.Drawing.Point(0, 169);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(924, 28);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelPriceServer
            // 
            this.toolStripStatusLabelPriceServer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelPriceServer.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusLabelPriceServer.Name = "toolStripStatusLabelPriceServer";
            this.toolStripStatusLabelPriceServer.Padding = new System.Windows.Forms.Padding(2);
            this.toolStripStatusLabelPriceServer.Size = new System.Drawing.Size(95, 23);
            this.toolStripStatusLabelPriceServer.Text = "Price Server";
            // 
            // toolStripStatusLabelOrderServer
            // 
            this.toolStripStatusLabelOrderServer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelOrderServer.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusLabelOrderServer.Name = "toolStripStatusLabelOrderServer";
            this.toolStripStatusLabelOrderServer.Padding = new System.Windows.Forms.Padding(2);
            this.toolStripStatusLabelOrderServer.Size = new System.Drawing.Size(101, 23);
            this.toolStripStatusLabelOrderServer.Text = "Order Server";
            // 
            // toolStripStatusLabelFillServer
            // 
            this.toolStripStatusLabelFillServer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabelFillServer.ForeColor = System.Drawing.Color.Red;
            this.toolStripStatusLabelFillServer.Name = "toolStripStatusLabelFillServer";
            this.toolStripStatusLabelFillServer.Padding = new System.Windows.Forms.Padding(2);
            this.toolStripStatusLabelFillServer.Size = new System.Drawing.Size(80, 23);
            this.toolStripStatusLabelFillServer.Text = "Fill Server";
            // 
            // pricesUpdateTimer
            // 
            this.pricesUpdateTimer.Enabled = true;
            this.pricesUpdateTimer.Interval = 250;
            this.pricesUpdateTimer.Tick += new System.EventHandler(this.pricesUpdateTimer_Tick);
            // 
            // MonkeyCancelFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 197);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.flowLayoutPanelInstruments);
            this.Controls.Add(this.panel1);
            this.Name = "MonkeyCancelFrm";
            this.Text = "MonkeyCancel";
            this.panel1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonSaveEdges;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelPriceServer;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelOrderServer;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFillServer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelInstruments;
        private System.Windows.Forms.Button buttonWriteDump;
        private System.Windows.Forms.Timer pricesUpdateTimer;
    }
}

