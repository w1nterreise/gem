namespace Gem {
    partial class Gem {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Gem));
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.pnlControl = new System.Windows.Forms.Panel();
            this.btnClearCache = new System.Windows.Forms.Button();
            this.btnOpacity = new System.Windows.Forms.Button();
            this.btnPin = new System.Windows.Forms.CheckBox();
            this.btnHome = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsLblSTT = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsLblMemory = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsLblCache = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.pnlControl.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 0);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(800, 450);
            this.webView.TabIndex = 1;
            this.webView.ZoomFactor = 1D;
            this.webView.CoreWebView2InitializationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>(this.webView_CoreWebView2InitializationCompleted);
            this.webView.WebMessageReceived += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs>(this.webView_WebMessageReceived);
            // 
            // pnlControl
            // 
            this.pnlControl.BackColor = System.Drawing.SystemColors.Control;
            this.pnlControl.Controls.Add(this.btnClearCache);
            this.pnlControl.Controls.Add(this.btnOpacity);
            this.pnlControl.Controls.Add(this.btnPin);
            this.pnlControl.Controls.Add(this.btnHome);
            this.pnlControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControl.Location = new System.Drawing.Point(0, 0);
            this.pnlControl.Name = "pnlControl";
            this.pnlControl.Size = new System.Drawing.Size(800, 24);
            this.pnlControl.TabIndex = 2;
            // 
            // btnClearCache
            // 
            this.btnClearCache.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClearCache.Image = global::Gem.Properties.Resources.clear;
            this.btnClearCache.Location = new System.Drawing.Point(760, 0);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(40, 24);
            this.btnClearCache.TabIndex = 3;
            this.btnClearCache.UseVisualStyleBackColor = true;
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            // 
            // btnOpacity
            // 
            this.btnOpacity.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnOpacity.ForeColor = System.Drawing.SystemColors.GrayText;
            this.btnOpacity.Location = new System.Drawing.Point(80, 0);
            this.btnOpacity.Name = "btnOpacity";
            this.btnOpacity.Size = new System.Drawing.Size(40, 24);
            this.btnOpacity.TabIndex = 2;
            this.btnOpacity.UseVisualStyleBackColor = true;
            // 
            // btnPin
            // 
            this.btnPin.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnPin.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnPin.Image = global::Gem.Properties.Resources.pin;
            this.btnPin.Location = new System.Drawing.Point(40, 0);
            this.btnPin.Name = "btnPin";
            this.btnPin.Size = new System.Drawing.Size(40, 24);
            this.btnPin.TabIndex = 1;
            this.btnPin.UseVisualStyleBackColor = true;
            this.btnPin.CheckedChanged += new System.EventHandler(this.btnPin_CheckedChanged);
            // 
            // btnHome
            // 
            this.btnHome.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnHome.Image = global::Gem.Properties.Resources.home;
            this.btnHome.Location = new System.Drawing.Point(0, 0);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(40, 24);
            this.btnHome.TabIndex = 0;
            this.btnHome.UseVisualStyleBackColor = true;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLblSTT,
            this.tsSpring,
            this.tsLblMemory,
            this.tsLblCache});
            this.statusStrip.Location = new System.Drawing.Point(0, 428);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // tsLblSTT
            // 
            this.tsLblSTT.AutoToolTip = true;
            this.tsLblSTT.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tsLblSTT.Image = global::Gem.Properties.Resources.mic;
            this.tsLblSTT.Name = "tsLblSTT";
            this.tsLblSTT.Size = new System.Drawing.Size(20, 20);
            this.tsLblSTT.Visible = false;
            // 
            // tsSpring
            // 
            this.tsSpring.AutoToolTip = true;
            this.tsSpring.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsSpring.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tsSpring.Name = "tsSpring";
            this.tsSpring.Size = new System.Drawing.Size(785, 17);
            this.tsSpring.Spring = true;
            // 
            // tsLblMemory
            // 
            this.tsLblMemory.AutoToolTip = true;
            this.tsLblMemory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsLblMemory.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tsLblMemory.Name = "tsLblMemory";
            this.tsLblMemory.Size = new System.Drawing.Size(0, 17);
            // 
            // tsLblCache
            // 
            this.tsLblCache.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsLblCache.ForeColor = System.Drawing.SystemColors.GrayText;
            this.tsLblCache.Name = "tsLblCache";
            this.tsLblCache.Size = new System.Drawing.Size(0, 17);
            // 
            // Gem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.pnlControl);
            this.Controls.Add(this.webView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Gem";
            this.Text = "Gem 1.1 - Aileron";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Gem_FormClosing);
            this.Load += new System.EventHandler(this.Gem_Load);
            this.Resize += new System.EventHandler(this.Gem_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.pnlControl.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTips;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Panel pnlControl;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.Button btnOpacity;
        private System.Windows.Forms.CheckBox btnPin;
        private System.Windows.Forms.Button btnHome;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsLblSTT;
        private System.Windows.Forms.ToolStripStatusLabel tsSpring;
        private System.Windows.Forms.ToolStripStatusLabel tsLblMemory;
        private System.Windows.Forms.ToolStripStatusLabel tsLblCache;
    }
}

