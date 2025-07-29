namespace Cduhub.WindowsGui
{
    partial class AboutForm
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.label1 = new System.Windows.Forms.Label();
            this._TextBox_ThisVersion = new System.Windows.Forms.TextBox();
            this._TextBox_LatestVersion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._Button_Close = new System.Windows.Forms.Button();
            this._TextBox_LatestReleaseUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this._LinkLabel_OpenReleaseUrl = new System.Windows.Forms.LinkLabel();
            this._TextBox_License = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "This version:";
            // 
            // _TextBox_ThisVersion
            // 
            this._TextBox_ThisVersion.Font = new System.Drawing.Font("Consolas", 9.75F);
            this._TextBox_ThisVersion.Location = new System.Drawing.Point(100, 12);
            this._TextBox_ThisVersion.Name = "_TextBox_ThisVersion";
            this._TextBox_ThisVersion.ReadOnly = true;
            this._TextBox_ThisVersion.Size = new System.Drawing.Size(175, 23);
            this._TextBox_ThisVersion.TabIndex = 1;
            // 
            // _TextBox_LatestVersion
            // 
            this._TextBox_LatestVersion.Font = new System.Drawing.Font("Consolas", 9.75F);
            this._TextBox_LatestVersion.Location = new System.Drawing.Point(100, 41);
            this._TextBox_LatestVersion.Name = "_TextBox_LatestVersion";
            this._TextBox_LatestVersion.ReadOnly = true;
            this._TextBox_LatestVersion.Size = new System.Drawing.Size(175, 23);
            this._TextBox_LatestVersion.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Latest version:";
            // 
            // _Button_Close
            // 
            this._Button_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._Button_Close.AutoSize = true;
            this._Button_Close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._Button_Close.Location = new System.Drawing.Point(630, 430);
            this._Button_Close.Name = "_Button_Close";
            this._Button_Close.Size = new System.Drawing.Size(75, 25);
            this._Button_Close.TabIndex = 4;
            this._Button_Close.Text = "Close";
            this._Button_Close.UseVisualStyleBackColor = true;
            // 
            // _TextBox_LatestReleaseUrl
            // 
            this._TextBox_LatestReleaseUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._TextBox_LatestReleaseUrl.Font = new System.Drawing.Font("Consolas", 9.75F);
            this._TextBox_LatestReleaseUrl.Location = new System.Drawing.Point(100, 70);
            this._TextBox_LatestReleaseUrl.Name = "_TextBox_LatestReleaseUrl";
            this._TextBox_LatestReleaseUrl.ReadOnly = true;
            this._TextBox_LatestReleaseUrl.Size = new System.Drawing.Size(563, 23);
            this._TextBox_LatestReleaseUrl.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Release URL:";
            // 
            // _LinkLabel_OpenReleaseUrl
            // 
            this._LinkLabel_OpenReleaseUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._LinkLabel_OpenReleaseUrl.AutoSize = true;
            this._LinkLabel_OpenReleaseUrl.Location = new System.Drawing.Point(669, 73);
            this._LinkLabel_OpenReleaseUrl.Name = "_LinkLabel_OpenReleaseUrl";
            this._LinkLabel_OpenReleaseUrl.Size = new System.Drawing.Size(36, 15);
            this._LinkLabel_OpenReleaseUrl.TabIndex = 7;
            this._LinkLabel_OpenReleaseUrl.TabStop = true;
            this._LinkLabel_OpenReleaseUrl.Text = "Open";
            this._LinkLabel_OpenReleaseUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_OpenReleaseUrl_LinkClicked);
            // 
            // _TextBox_License
            // 
            this._TextBox_License.Font = new System.Drawing.Font("Consolas", 9.75F);
            this._TextBox_License.Location = new System.Drawing.Point(100, 99);
            this._TextBox_License.Multiline = true;
            this._TextBox_License.Name = "_TextBox_License";
            this._TextBox_License.ReadOnly = true;
            this._TextBox_License.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._TextBox_License.Size = new System.Drawing.Size(605, 153);
            this._TextBox_License.TabIndex = 8;
            this._TextBox_License.Text = resources.GetString("_TextBox_License.Text");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 102);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "License:";
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBox1.Location = new System.Drawing.Point(100, 258);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(605, 153);
            this.textBox1.TabIndex = 10;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 258);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Credit:";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._Button_Close;
            this.ClientSize = new System.Drawing.Size(717, 467);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._TextBox_License);
            this.Controls.Add(this._LinkLabel_OpenReleaseUrl);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._TextBox_LatestReleaseUrl);
            this.Controls.Add(this._Button_Close);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._TextBox_LatestVersion);
            this.Controls.Add(this._TextBox_ThisVersion);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About CDU Hub";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _TextBox_ThisVersion;
        private System.Windows.Forms.TextBox _TextBox_LatestVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button _Button_Close;
        private System.Windows.Forms.TextBox _TextBox_LatestReleaseUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel _LinkLabel_OpenReleaseUrl;
        private System.Windows.Forms.TextBox _TextBox_License;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
    }
}