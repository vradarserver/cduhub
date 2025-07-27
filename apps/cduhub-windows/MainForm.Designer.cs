using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cduhub.WindowsGui
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                if(components != null) {
                    components.Dispose();
                }
                UnhookConnectedFlightSimulators();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this._Label_UsbDeviceState = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._ListView_ConnectedFlightSimulators = new Cduhub.WindowsGui.ListViewEx();
            this._Col_FlightSim_Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._Col_FlightSim_Aircraft = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._Col_FlightSim_ConnectionState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._Col_FlightSim_LastMessageUtc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._Col_FlightSim_CountMessages = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._RefreshDisplayTimer = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this._LinkLabel_ConfigFolder = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "USB device:";
            // 
            // _Label_UsbDeviceState
            // 
            this._Label_UsbDeviceState.AutoSize = true;
            this._Label_UsbDeviceState.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._Label_UsbDeviceState.Location = new System.Drawing.Point(86, 12);
            this._Label_UsbDeviceState.Margin = new System.Windows.Forms.Padding(3);
            this._Label_UsbDeviceState.Name = "_Label_UsbDeviceState";
            this._Label_UsbDeviceState.Size = new System.Drawing.Size(14, 15);
            this._Label_UsbDeviceState.TabIndex = 1;
            this._Label_UsbDeviceState.Text = "-";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._ListView_ConnectedFlightSimulators);
            this.groupBox1.Location = new System.Drawing.Point(12, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(560, 165);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Flight Simulators";
            // 
            // _ListView_ConnectedFlightSimulators
            // 
            this._ListView_ConnectedFlightSimulators.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ListView_ConnectedFlightSimulators.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._Col_FlightSim_Name,
            this._Col_FlightSim_Aircraft,
            this._Col_FlightSim_ConnectionState,
            this._Col_FlightSim_LastMessageUtc,
            this._Col_FlightSim_CountMessages});
            this._ListView_ConnectedFlightSimulators.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._ListView_ConnectedFlightSimulators.FullRowSelect = true;
            this._ListView_ConnectedFlightSimulators.GridLines = true;
            this._ListView_ConnectedFlightSimulators.HideSelection = false;
            this._ListView_ConnectedFlightSimulators.Location = new System.Drawing.Point(6, 22);
            this._ListView_ConnectedFlightSimulators.MultiSelect = false;
            this._ListView_ConnectedFlightSimulators.Name = "_ListView_ConnectedFlightSimulators";
            this._ListView_ConnectedFlightSimulators.Size = new System.Drawing.Size(548, 137);
            this._ListView_ConnectedFlightSimulators.TabIndex = 0;
            this._ListView_ConnectedFlightSimulators.UseCompatibleStateImageBehavior = false;
            this._ListView_ConnectedFlightSimulators.View = System.Windows.Forms.View.Details;
            // 
            // _Col_FlightSim_Name
            // 
            this._Col_FlightSim_Name.Text = "Flight Simulator";
            // 
            // _Col_FlightSim_Aircraft
            // 
            this._Col_FlightSim_Aircraft.Text = "Aircraft";
            // 
            // _Col_FlightSim_ConnectionState
            // 
            this._Col_FlightSim_ConnectionState.Text = "State";
            this._Col_FlightSim_ConnectionState.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // _Col_FlightSim_LastMessageUtc
            // 
            this._Col_FlightSim_LastMessageUtc.Text = "Last Message";
            // 
            // _Col_FlightSim_CountMessages
            // 
            this._Col_FlightSim_CountMessages.Text = "Messages";
            this._Col_FlightSim_CountMessages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // _RefreshDisplayTimer
            // 
            this._RefreshDisplayTimer.Enabled = true;
            this._RefreshDisplayTimer.Interval = 250;
            this._RefreshDisplayTimer.Tick += new System.EventHandler(this.RefreshDisplayTimer_Tick);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Configs:";
            // 
            // _LinkLabel_ConfigFolder
            // 
            this._LinkLabel_ConfigFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._LinkLabel_ConfigFolder.Location = new System.Drawing.Point(69, 209);
            this._LinkLabel_ConfigFolder.Name = "_LinkLabel_ConfigFolder";
            this._LinkLabel_ConfigFolder.Size = new System.Drawing.Size(503, 15);
            this._LinkLabel_ConfigFolder.TabIndex = 4;
            this._LinkLabel_ConfigFolder.TabStop = true;
            this._LinkLabel_ConfigFolder.Text = "-";
            this._LinkLabel_ConfigFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_ConfigFolder_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 241);
            this.Controls.Add(this._LinkLabel_ConfigFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._Label_UsbDeviceState);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CDU Hub";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Label _Label_UsbDeviceState;
        private GroupBox groupBox1;
        private Cduhub.WindowsGui.ListViewEx _ListView_ConnectedFlightSimulators;
        private ColumnHeader _Col_FlightSim_Name;
        private ColumnHeader _Col_FlightSim_LastMessageUtc;
        private ColumnHeader _Col_FlightSim_CountMessages;
        private ColumnHeader _Col_FlightSim_Aircraft;
        private ColumnHeader _Col_FlightSim_ConnectionState;
        private Timer _RefreshDisplayTimer;
        private Label label2;
        private LinkLabel _LinkLabel_ConfigFolder;
    }
}
