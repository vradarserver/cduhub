namespace Cduhub.WindowsGui
{
    partial class CduDisplayControl
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
            if(disposing && (components != null)) {
                components.Dispose();
            }
            if(disposing) {
                if(_PixelBuffer != null) {
                    _PixelBuffer.Dispose();
                    _PixelBuffer = null;
                }
                _FallbackLargeFont.Dispose();
                _FallbackSmallFont.Dispose();
                _FallbackColourBrush.Dispose();
                DisposeOfColourBrushes();
                _PixelBuffer?.Dispose();
                _PictureBoxImage.Dispose();
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
            this.components = new System.ComponentModel.Container();
            this._PictureBox = new System.Windows.Forms.PictureBox();
            this._ContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._ContextMenu_CopyToClipboard = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._PictureBox)).BeginInit();
            this._ContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // _PictureBox
            // 
            this._PictureBox.BackColor = System.Drawing.Color.Black;
            this._PictureBox.ContextMenuStrip = this._ContextMenu;
            this._PictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._PictureBox.Location = new System.Drawing.Point(0, 0);
            this._PictureBox.Name = "_PictureBox";
            this._PictureBox.Size = new System.Drawing.Size(580, 480);
            this._PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._PictureBox.TabIndex = 0;
            this._PictureBox.TabStop = false;
            // 
            // _ContextMenu
            // 
            this._ContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ContextMenu_CopyToClipboard});
            this._ContextMenu.Name = "_ContextMenu";
            this._ContextMenu.Size = new System.Drawing.Size(172, 26);
            // 
            // _ContextMenu_CopyToClipboard
            // 
            this._ContextMenu_CopyToClipboard.Name = "_ContextMenu_CopyToClipboard";
            this._ContextMenu_CopyToClipboard.Size = new System.Drawing.Size(180, 22);
            this._ContextMenu_CopyToClipboard.Text = "Copy to Clipboard";
            this._ContextMenu_CopyToClipboard.Click += new System.EventHandler(this.ContextMenuItem_CopyToClipoard_Clicked);
            // 
            // CduDisplayControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._PictureBox);
            this.Name = "CduDisplayControl";
            this.Size = new System.Drawing.Size(580, 480);
            ((System.ComponentModel.ISupportInitialize)(this._PictureBox)).EndInit();
            this._ContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _PictureBox;
        private System.Windows.Forms.ContextMenuStrip _ContextMenu;
        private System.Windows.Forms.ToolStripMenuItem _ContextMenu_CopyToClipboard;
    }
}
