
namespace GlobalCommander
{
    partial class GlobalCommander
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
            this.DeviceList = new System.Windows.Forms.ListBox();
            this.PointList = new System.Windows.Forms.ListBox();
            this.PropertyList = new System.Windows.Forms.ListBox();
            this.cmdPopulateDevices = new System.Windows.Forms.Button();
            this.cmdPopulatePoints = new System.Windows.Forms.Button();
            this.cmdPopulateProperties = new System.Windows.Forms.Button();
            this.cmdCommand = new System.Windows.Forms.Button();
            this.lblCmdVal = new System.Windows.Forms.Label();
            this.txtCmdVal = new System.Windows.Forms.TextBox();
            this.o1 = new System.Windows.Forms.RadioButton();
            this.o2 = new System.Windows.Forms.RadioButton();
            this.o3 = new System.Windows.Forms.RadioButton();
            this.o4 = new System.Windows.Forms.RadioButton();
            this.o5 = new System.Windows.Forms.RadioButton();
            this.o6 = new System.Windows.Forms.RadioButton();
            this.o7 = new System.Windows.Forms.RadioButton();
            this.o10 = new System.Windows.Forms.RadioButton();
            this.o8 = new System.Windows.Forms.RadioButton();
            this.o11 = new System.Windows.Forms.RadioButton();
            this.o9 = new System.Windows.Forms.RadioButton();
            this.o12 = new System.Windows.Forms.RadioButton();
            this.o14 = new System.Windows.Forms.RadioButton();
            this.o15 = new System.Windows.Forms.RadioButton();
            this.o13 = new System.Windows.Forms.RadioButton();
            this.o16 = new System.Windows.Forms.RadioButton();
            this.progBar = new System.Windows.Forms.ProgressBar();
            this.cmdViewProps = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.txtDeviceFilter = new System.Windows.Forms.TextBox();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.txtPointFilter = new System.Windows.Forms.TextBox();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.PatienceLabel = new System.Windows.Forms.Label();
            this.PatienceTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.SuspendLayout();
            // 
            // DeviceList
            // 
            this.DeviceList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DeviceList.FormattingEnabled = true;
            this.DeviceList.Location = new System.Drawing.Point(0, 0);
            this.DeviceList.Name = "DeviceList";
            this.DeviceList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.DeviceList.Size = new System.Drawing.Size(293, 530);
            this.DeviceList.TabIndex = 0;
            this.DeviceList.SelectedIndexChanged += new System.EventHandler(this.DeviceList_SelectedIndexChanged);
            // 
            // PointList
            // 
            this.PointList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PointList.FormattingEnabled = true;
            this.PointList.Location = new System.Drawing.Point(0, 0);
            this.PointList.Name = "PointList";
            this.PointList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.PointList.Size = new System.Drawing.Size(400, 530);
            this.PointList.TabIndex = 1;
            this.PointList.SelectedIndexChanged += new System.EventHandler(this.PointList_SelectedIndexChanged);
            // 
            // PropertyList
            // 
            this.PropertyList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyList.FormattingEnabled = true;
            this.PropertyList.Location = new System.Drawing.Point(0, 0);
            this.PropertyList.Name = "PropertyList";
            this.PropertyList.Size = new System.Drawing.Size(365, 530);
            this.PropertyList.TabIndex = 2;
            // 
            // cmdPopulateDevices
            // 
            this.cmdPopulateDevices.Location = new System.Drawing.Point(0, 2);
            this.cmdPopulateDevices.Name = "cmdPopulateDevices";
            this.cmdPopulateDevices.Size = new System.Drawing.Size(140, 36);
            this.cmdPopulateDevices.TabIndex = 3;
            this.cmdPopulateDevices.Text = "Populate Devices";
            this.cmdPopulateDevices.UseVisualStyleBackColor = true;
            this.cmdPopulateDevices.Click += new System.EventHandler(this.cmdPopulateDevices_Click);
            // 
            // cmdPopulatePoints
            // 
            this.cmdPopulatePoints.Location = new System.Drawing.Point(0, 2);
            this.cmdPopulatePoints.Name = "cmdPopulatePoints";
            this.cmdPopulatePoints.Size = new System.Drawing.Size(140, 36);
            this.cmdPopulatePoints.TabIndex = 4;
            this.cmdPopulatePoints.Text = "Populate Points";
            this.cmdPopulatePoints.UseVisualStyleBackColor = true;
            this.cmdPopulatePoints.Click += new System.EventHandler(this.cmdPopulatePoints_Click);
            // 
            // cmdPopulateProperties
            // 
            this.cmdPopulateProperties.Location = new System.Drawing.Point(0, 2);
            this.cmdPopulateProperties.Name = "cmdPopulateProperties";
            this.cmdPopulateProperties.Size = new System.Drawing.Size(140, 36);
            this.cmdPopulateProperties.TabIndex = 5;
            this.cmdPopulateProperties.Text = "Populate Properties";
            this.cmdPopulateProperties.UseVisualStyleBackColor = true;
            this.cmdPopulateProperties.Click += new System.EventHandler(this.cmdPopulateProperties_Click);
            // 
            // cmdCommand
            // 
            this.cmdCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCommand.Location = new System.Drawing.Point(1087, 412);
            this.cmdCommand.Name = "cmdCommand";
            this.cmdCommand.Size = new System.Drawing.Size(179, 36);
            this.cmdCommand.TabIndex = 7;
            this.cmdCommand.Text = "Globally Command";
            this.cmdCommand.UseVisualStyleBackColor = true;
            this.cmdCommand.Click += new System.EventHandler(this.cmdCommand_Click);
            // 
            // lblCmdVal
            // 
            this.lblCmdVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCmdVal.AutoSize = true;
            this.lblCmdVal.Location = new System.Drawing.Point(1087, 382);
            this.lblCmdVal.Name = "lblCmdVal";
            this.lblCmdVal.Size = new System.Drawing.Size(87, 13);
            this.lblCmdVal.TabIndex = 7;
            this.lblCmdVal.Text = "Command Value:";
            // 
            // txtCmdVal
            // 
            this.txtCmdVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCmdVal.Location = new System.Drawing.Point(1180, 379);
            this.txtCmdVal.Name = "txtCmdVal";
            this.txtCmdVal.Size = new System.Drawing.Size(86, 20);
            this.txtCmdVal.TabIndex = 6;
            // 
            // o1
            // 
            this.o1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o1.AutoSize = true;
            this.o1.Location = new System.Drawing.Point(1103, 2);
            this.o1.Name = "o1";
            this.o1.Size = new System.Drawing.Size(65, 17);
            this.o1.TabIndex = 12;
            this.o1.Text = "Priority 1";
            this.o1.UseVisualStyleBackColor = true;
            // 
            // o2
            // 
            this.o2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o2.AutoSize = true;
            this.o2.Location = new System.Drawing.Point(1103, 25);
            this.o2.Name = "o2";
            this.o2.Size = new System.Drawing.Size(65, 17);
            this.o2.TabIndex = 13;
            this.o2.Text = "Priority 2";
            this.o2.UseVisualStyleBackColor = true;
            // 
            // o3
            // 
            this.o3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o3.AutoSize = true;
            this.o3.Location = new System.Drawing.Point(1103, 48);
            this.o3.Name = "o3";
            this.o3.Size = new System.Drawing.Size(65, 17);
            this.o3.TabIndex = 14;
            this.o3.Text = "Priority 3";
            this.o3.UseVisualStyleBackColor = true;
            // 
            // o4
            // 
            this.o4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o4.AutoSize = true;
            this.o4.Location = new System.Drawing.Point(1103, 71);
            this.o4.Name = "o4";
            this.o4.Size = new System.Drawing.Size(65, 17);
            this.o4.TabIndex = 15;
            this.o4.Text = "Priority 4";
            this.o4.UseVisualStyleBackColor = true;
            // 
            // o5
            // 
            this.o5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o5.AutoSize = true;
            this.o5.Location = new System.Drawing.Point(1103, 94);
            this.o5.Name = "o5";
            this.o5.Size = new System.Drawing.Size(65, 17);
            this.o5.TabIndex = 16;
            this.o5.Text = "Priority 5";
            this.o5.UseVisualStyleBackColor = true;
            // 
            // o6
            // 
            this.o6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o6.AutoSize = true;
            this.o6.Location = new System.Drawing.Point(1103, 117);
            this.o6.Name = "o6";
            this.o6.Size = new System.Drawing.Size(65, 17);
            this.o6.TabIndex = 17;
            this.o6.Text = "Priority 6";
            this.o6.UseVisualStyleBackColor = true;
            // 
            // o7
            // 
            this.o7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o7.AutoSize = true;
            this.o7.Location = new System.Drawing.Point(1103, 140);
            this.o7.Name = "o7";
            this.o7.Size = new System.Drawing.Size(65, 17);
            this.o7.TabIndex = 18;
            this.o7.Text = "Priority 7";
            this.o7.UseVisualStyleBackColor = true;
            // 
            // o10
            // 
            this.o10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o10.AutoSize = true;
            this.o10.Location = new System.Drawing.Point(1103, 209);
            this.o10.Name = "o10";
            this.o10.Size = new System.Drawing.Size(71, 17);
            this.o10.TabIndex = 19;
            this.o10.Text = "Priority 10";
            this.o10.UseVisualStyleBackColor = true;
            // 
            // o8
            // 
            this.o8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o8.AutoSize = true;
            this.o8.Checked = true;
            this.o8.Location = new System.Drawing.Point(1103, 163);
            this.o8.Name = "o8";
            this.o8.Size = new System.Drawing.Size(65, 17);
            this.o8.TabIndex = 20;
            this.o8.TabStop = true;
            this.o8.Text = "Priority 8";
            this.o8.UseVisualStyleBackColor = true;
            // 
            // o11
            // 
            this.o11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o11.AutoSize = true;
            this.o11.Location = new System.Drawing.Point(1103, 232);
            this.o11.Name = "o11";
            this.o11.Size = new System.Drawing.Size(71, 17);
            this.o11.TabIndex = 21;
            this.o11.Text = "Priority 11";
            this.o11.UseVisualStyleBackColor = true;
            // 
            // o9
            // 
            this.o9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o9.AutoSize = true;
            this.o9.Location = new System.Drawing.Point(1103, 186);
            this.o9.Name = "o9";
            this.o9.Size = new System.Drawing.Size(65, 17);
            this.o9.TabIndex = 22;
            this.o9.Text = "Priority 9";
            this.o9.UseVisualStyleBackColor = true;
            // 
            // o12
            // 
            this.o12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o12.AutoSize = true;
            this.o12.Location = new System.Drawing.Point(1103, 255);
            this.o12.Name = "o12";
            this.o12.Size = new System.Drawing.Size(71, 17);
            this.o12.TabIndex = 23;
            this.o12.Text = "Priority 12";
            this.o12.UseVisualStyleBackColor = true;
            // 
            // o14
            // 
            this.o14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o14.AutoSize = true;
            this.o14.Location = new System.Drawing.Point(1103, 301);
            this.o14.Name = "o14";
            this.o14.Size = new System.Drawing.Size(71, 17);
            this.o14.TabIndex = 24;
            this.o14.Text = "Priority 14";
            this.o14.UseVisualStyleBackColor = true;
            // 
            // o15
            // 
            this.o15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o15.AutoSize = true;
            this.o15.Location = new System.Drawing.Point(1103, 324);
            this.o15.Name = "o15";
            this.o15.Size = new System.Drawing.Size(71, 17);
            this.o15.TabIndex = 25;
            this.o15.Text = "Priority 15";
            this.o15.UseVisualStyleBackColor = true;
            // 
            // o13
            // 
            this.o13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o13.AutoSize = true;
            this.o13.Location = new System.Drawing.Point(1103, 278);
            this.o13.Name = "o13";
            this.o13.Size = new System.Drawing.Size(71, 17);
            this.o13.TabIndex = 26;
            this.o13.Text = "Priority 13";
            this.o13.UseVisualStyleBackColor = true;
            // 
            // o16
            // 
            this.o16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.o16.AutoSize = true;
            this.o16.Location = new System.Drawing.Point(1103, 347);
            this.o16.Name = "o16";
            this.o16.Size = new System.Drawing.Size(71, 17);
            this.o16.TabIndex = 27;
            this.o16.Text = "Priority 16";
            this.o16.UseVisualStyleBackColor = true;
            // 
            // progBar
            // 
            this.progBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progBar.Location = new System.Drawing.Point(12, 605);
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(1251, 16);
            this.progBar.TabIndex = 10;
            // 
            // cmdViewProps
            // 
            this.cmdViewProps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdViewProps.Location = new System.Drawing.Point(1087, 460);
            this.cmdViewProps.Name = "cmdViewProps";
            this.cmdViewProps.Size = new System.Drawing.Size(179, 36);
            this.cmdViewProps.TabIndex = 9;
            this.cmdViewProps.Text = "View Properties in Scope";
            this.cmdViewProps.UseVisualStyleBackColor = true;
            this.cmdViewProps.Click += new System.EventHandler(this.cmdViewProps_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer1.Size = new System.Drawing.Size(1066, 573);
            this.splitContainer1.SplitterDistance = 697;
            this.splitContainer1.TabIndex = 28;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer2.Size = new System.Drawing.Size(697, 573);
            this.splitContainer2.SplitterDistance = 293;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.DeviceList);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.txtDeviceFilter);
            this.splitContainer3.Panel2.Controls.Add(this.cmdPopulateDevices);
            this.splitContainer3.Size = new System.Drawing.Size(293, 573);
            this.splitContainer3.SplitterDistance = 530;
            this.splitContainer3.TabIndex = 1;
            // 
            // txtDeviceFilter
            // 
            this.txtDeviceFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeviceFilter.Location = new System.Drawing.Point(152, 11);
            this.txtDeviceFilter.Name = "txtDeviceFilter";
            this.txtDeviceFilter.Size = new System.Drawing.Size(113, 20);
            this.txtDeviceFilter.TabIndex = 4;
            this.txtDeviceFilter.TextChanged += new System.EventHandler(this.txtDeviceFilter_TextChanged);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.PointList);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.txtPointFilter);
            this.splitContainer4.Panel2.Controls.Add(this.cmdPopulatePoints);
            this.splitContainer4.Size = new System.Drawing.Size(400, 573);
            this.splitContainer4.SplitterDistance = 530;
            this.splitContainer4.TabIndex = 2;
            // 
            // txtPointFilter
            // 
            this.txtPointFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPointFilter.Location = new System.Drawing.Point(146, 11);
            this.txtPointFilter.Name = "txtPointFilter";
            this.txtPointFilter.Size = new System.Drawing.Size(209, 20);
            this.txtPointFilter.TabIndex = 5;
            this.txtPointFilter.TextChanged += new System.EventHandler(this.txtPointFilter_TextChanged);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer5.IsSplitterFixed = true;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.PropertyList);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.cmdPopulateProperties);
            this.splitContainer5.Size = new System.Drawing.Size(365, 573);
            this.splitContainer5.SplitterDistance = 530;
            this.splitContainer5.TabIndex = 3;
            // 
            // PatienceLabel
            // 
            this.PatienceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PatienceLabel.AutoSize = true;
            this.PatienceLabel.Location = new System.Drawing.Point(59, 589);
            this.PatienceLabel.Name = "PatienceLabel";
            this.PatienceLabel.Size = new System.Drawing.Size(852, 13);
            this.PatienceLabel.TabIndex = 29;
            this.PatienceLabel.Text = "Please be patient - the global commander has absolutely no prior knowledge of the" +
    " nework, or the duplicity of bacnet objects in each device, and must poll each d" +
    "evice individually.";
            this.PatienceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.PatienceLabel.Visible = false;
            // 
            // PatienceTimer
            // 
            this.PatienceTimer.Interval = 1000;
            this.PatienceTimer.Tick += new System.EventHandler(this.PatienceTimer_Tick);
            // 
            // GlobalCommander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1276, 628);
            this.Controls.Add(this.PatienceLabel);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.progBar);
            this.Controls.Add(this.o16);
            this.Controls.Add(this.o12);
            this.Controls.Add(this.o6);
            this.Controls.Add(this.o13);
            this.Controls.Add(this.o9);
            this.Controls.Add(this.o15);
            this.Controls.Add(this.o3);
            this.Controls.Add(this.o11);
            this.Controls.Add(this.o5);
            this.Controls.Add(this.o8);
            this.Controls.Add(this.o14);
            this.Controls.Add(this.o2);
            this.Controls.Add(this.o10);
            this.Controls.Add(this.o4);
            this.Controls.Add(this.o7);
            this.Controls.Add(this.o1);
            this.Controls.Add(this.txtCmdVal);
            this.Controls.Add(this.lblCmdVal);
            this.Controls.Add(this.cmdViewProps);
            this.Controls.Add(this.cmdCommand);
            this.Name = "GlobalCommander";
            this.Text = "Yabe Global Commander";
            this.Load += new System.EventHandler(this.GlobalCommander_Load);
            this.Shown += new System.EventHandler(this.GlobalCommander_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox DeviceList;
        private System.Windows.Forms.ListBox PointList;
        private System.Windows.Forms.ListBox PropertyList;
        private System.Windows.Forms.Button cmdPopulateDevices;
        private System.Windows.Forms.Button cmdPopulatePoints;
        private System.Windows.Forms.Button cmdPopulateProperties;
        private System.Windows.Forms.Button cmdCommand;
        private System.Windows.Forms.Label lblCmdVal;
        private System.Windows.Forms.TextBox txtCmdVal;
        private System.Windows.Forms.RadioButton o1;
        private System.Windows.Forms.RadioButton o2;
        private System.Windows.Forms.RadioButton o3;
        private System.Windows.Forms.RadioButton o4;
        private System.Windows.Forms.RadioButton o5;
        private System.Windows.Forms.RadioButton o6;
        private System.Windows.Forms.RadioButton o7;
        private System.Windows.Forms.RadioButton o10;
        private System.Windows.Forms.RadioButton o8;
        private System.Windows.Forms.RadioButton o11;
        private System.Windows.Forms.RadioButton o9;
        private System.Windows.Forms.RadioButton o12;
        private System.Windows.Forms.RadioButton o14;
        private System.Windows.Forms.RadioButton o15;
        private System.Windows.Forms.RadioButton o13;
        private System.Windows.Forms.RadioButton o16;
        private System.Windows.Forms.ProgressBar progBar;
        private System.Windows.Forms.Button cmdViewProps;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.TextBox txtDeviceFilter;
        private System.Windows.Forms.TextBox txtPointFilter;
        private System.Windows.Forms.Label PatienceLabel;
        private System.Windows.Forms.Timer PatienceTimer;
    }
}