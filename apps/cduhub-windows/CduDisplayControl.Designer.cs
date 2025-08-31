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
                _FallbackLargeFont.Dispose();
                _FallbackSmallFont.Dispose();
                DisposeOfColourBrushes();
                _PixelBuffer.Dispose();
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
            this._PictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // _PictureBox
            // 
            this._PictureBox.BackColor = System.Drawing.Color.Black;
            this._PictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._PictureBox.Location = new System.Drawing.Point(0, 0);
            this._PictureBox.Name = "_PictureBox";
            this._PictureBox.Size = new System.Drawing.Size(580, 480);
            this._PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._PictureBox.TabIndex = 0;
            this._PictureBox.TabStop = false;
            // 
            // CduDisplayControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this._PictureBox);
            this.Name = "CduDisplayControl";
            this.Size = new System.Drawing.Size(580, 480);
            ((System.ComponentModel.ISupportInitialize)(this._PictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _PictureBox;
    }
}
