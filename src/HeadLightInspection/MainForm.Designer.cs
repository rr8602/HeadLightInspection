namespace HeadLightInspection
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tabMain = new TabControl();
            tabAlignment = new TabPage();
            picAlignment = new PictureBox();
            panelAlignInfo = new Panel();
            grpAlignResult = new GroupBox();
            lblAlignStatus = new Label();
            lblAlignLampCount = new Label();
            lblAlignLeftLamp = new Label();
            lblAlignRightLamp = new Label();
            lblAlignBoundaryCenter = new Label();
            lblAlignCentroid = new Label();
            tabMeasurement = new TabPage();
            picMeasurement = new PictureBox();
            panelMeasureInfo = new Panel();
            grpMeasureResult = new GroupBox();
            lblMeasureHotPoint = new Label();
            lblMeasureHotValue = new Label();
            lblMeasureCrossPoint = new Label();
            grpJudgment = new GroupBox();
            lblJudgmentResult = new Label();
            lblHorizontalLabel = new Label();
            lblHorizontalValue = new Label();
            lblVerticalLabel = new Label();
            lblVerticalValue = new Label();
            lblCandelaLabel = new Label();
            lblCandelaValue = new Label();
            grpBeamSelect = new GroupBox();
            cmbBeamType = new ComboBox();
            lblBeamType = new Label();
            panelControls = new Panel();
            grpDisplay = new GroupBox();
            chkShowOverlay = new CheckBox();
            grpModel = new GroupBox();
            cmbHeadlampSide = new ComboBox();
            lblSideLabel = new Label();
            cmbModel = new ComboBox();
            lblModelLabel = new Label();
            grpStatus = new GroupBox();
            lblErrorCount = new Label();
            lblFrameCount = new Label();
            lblStatus = new Label();
            lblStatusLabel = new Label();
            grpAcquisition = new GroupBox();
            btnStartStop = new Button();
            btnDarkMode = new Button();
            chkAutoMode = new CheckBox();
            lblBlacklevel = new Label();
            numBlacklevel = new NumericUpDown();
            lblBlacklevelLabel = new Label();
            lblGain = new Label();
            numGain = new NumericUpDown();
            lblGainLabel = new Label();
            lblExposure = new Label();
            numExposure = new NumericUpDown();
            trkExposure = new TrackBar();
            lblExposureLabel = new Label();
            grpCamera = new GroupBox();
            btnRefresh = new Button();
            cmbCameras = new ComboBox();
            btnConnect = new Button();
            btnLoadParams = new Button();
            btnCalibration = new Button();
            lblCameraLabel = new Label();
            tabMain.SuspendLayout();
            tabAlignment.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAlignment).BeginInit();
            panelAlignInfo.SuspendLayout();
            grpAlignResult.SuspendLayout();
            tabMeasurement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picMeasurement).BeginInit();
            panelMeasureInfo.SuspendLayout();
            grpMeasureResult.SuspendLayout();
            grpJudgment.SuspendLayout();
            grpBeamSelect.SuspendLayout();
            panelControls.SuspendLayout();
            grpDisplay.SuspendLayout();
            grpModel.SuspendLayout();
            grpStatus.SuspendLayout();
            grpAcquisition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBlacklevel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numExposure).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkExposure).BeginInit();
            grpCamera.SuspendLayout();
            SuspendLayout();
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabAlignment);
            tabMain.Controls.Add(tabMeasurement);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(779, 950);
            tabMain.TabIndex = 0;
            // 
            // tabAlignment
            // 
            tabAlignment.Controls.Add(picAlignment);
            tabAlignment.Controls.Add(panelAlignInfo);
            tabAlignment.Location = new Point(4, 24);
            tabAlignment.Name = "tabAlignment";
            tabAlignment.Padding = new Padding(3);
            tabAlignment.Size = new Size(771, 922);
            tabAlignment.TabIndex = 0;
            tabAlignment.Text = "정대 (Alignment)";
            tabAlignment.UseVisualStyleBackColor = true;
            // 
            // picAlignment
            // 
            picAlignment.BackColor = Color.Black;
            picAlignment.Dock = DockStyle.Fill;
            picAlignment.Location = new Point(3, 3);
            picAlignment.Name = "picAlignment";
            picAlignment.Size = new Size(765, 766);
            picAlignment.SizeMode = PictureBoxSizeMode.Zoom;
            picAlignment.TabIndex = 0;
            picAlignment.TabStop = false;
            // 
            // panelAlignInfo
            // 
            panelAlignInfo.Controls.Add(grpAlignResult);
            panelAlignInfo.Dock = DockStyle.Bottom;
            panelAlignInfo.Location = new Point(3, 769);
            panelAlignInfo.Name = "panelAlignInfo";
            panelAlignInfo.Size = new Size(765, 150);
            panelAlignInfo.TabIndex = 1;
            // 
            // grpAlignResult
            // 
            grpAlignResult.Controls.Add(lblAlignStatus);
            grpAlignResult.Controls.Add(lblAlignLampCount);
            grpAlignResult.Controls.Add(lblAlignLeftLamp);
            grpAlignResult.Controls.Add(lblAlignRightLamp);
            grpAlignResult.Controls.Add(lblAlignBoundaryCenter);
            grpAlignResult.Controls.Add(lblAlignCentroid);
            grpAlignResult.Dock = DockStyle.Fill;
            grpAlignResult.Location = new Point(0, 0);
            grpAlignResult.Name = "grpAlignResult";
            grpAlignResult.Padding = new Padding(10);
            grpAlignResult.Size = new Size(765, 150);
            grpAlignResult.TabIndex = 0;
            grpAlignResult.TabStop = false;
            grpAlignResult.Text = "정대 결과";
            // 
            // lblAlignStatus
            // 
            lblAlignStatus.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblAlignStatus.ForeColor = Color.Gray;
            lblAlignStatus.Location = new Point(5, 22);
            lblAlignStatus.Name = "lblAlignStatus";
            lblAlignStatus.Size = new Size(200, 20);
            lblAlignStatus.TabIndex = 0;
            lblAlignStatus.Text = "상태: 대기";
            // 
            // lblAlignLampCount
            // 
            lblAlignLampCount.Location = new Point(270, 26);
            lblAlignLampCount.Name = "lblAlignLampCount";
            lblAlignLampCount.Size = new Size(120, 20);
            lblAlignLampCount.TabIndex = 1;
            lblAlignLampCount.Text = "램프: ---";
            // 
            // lblAlignLeftLamp
            // 
            lblAlignLeftLamp.ForeColor = Color.LimeGreen;
            lblAlignLeftLamp.Location = new Point(5, 56);
            lblAlignLeftLamp.Name = "lblAlignLeftLamp";
            lblAlignLeftLamp.Size = new Size(250, 20);
            lblAlignLeftLamp.TabIndex = 2;
            lblAlignLeftLamp.Text = "좌측 램프 (L): ---";
            // 
            // lblAlignRightLamp
            // 
            lblAlignRightLamp.ForeColor = Color.Cyan;
            lblAlignRightLamp.Location = new Point(270, 56);
            lblAlignRightLamp.Name = "lblAlignRightLamp";
            lblAlignRightLamp.Size = new Size(250, 20);
            lblAlignRightLamp.TabIndex = 3;
            lblAlignRightLamp.Text = "우측 램프 (R): ---";
            // 
            // lblAlignBoundaryCenter
            // 
            lblAlignBoundaryCenter.Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblAlignBoundaryCenter.ForeColor = Color.DarkKhaki;
            lblAlignBoundaryCenter.Location = new Point(5, 86);
            lblAlignBoundaryCenter.Name = "lblAlignBoundaryCenter";
            lblAlignBoundaryCenter.Size = new Size(250, 20);
            lblAlignBoundaryCenter.TabIndex = 4;
            lblAlignBoundaryCenter.Text = "경계 중심 (Center): ---";
            // 
            // lblAlignCentroid
            // 
            lblAlignCentroid.ForeColor = Color.Magenta;
            lblAlignCentroid.Location = new Point(270, 86);
            lblAlignCentroid.Name = "lblAlignCentroid";
            lblAlignCentroid.Size = new Size(250, 20);
            lblAlignCentroid.TabIndex = 5;
            lblAlignCentroid.Text = "무게 중심 (Centroid): ---";
            // 
            // tabMeasurement
            // 
            tabMeasurement.Controls.Add(picMeasurement);
            tabMeasurement.Controls.Add(panelMeasureInfo);
            tabMeasurement.Location = new Point(4, 24);
            tabMeasurement.Name = "tabMeasurement";
            tabMeasurement.Padding = new Padding(3);
            tabMeasurement.Size = new Size(771, 922);
            tabMeasurement.TabIndex = 1;
            tabMeasurement.Text = "측정 (Measurement)";
            tabMeasurement.UseVisualStyleBackColor = true;
            // 
            // picMeasurement
            // 
            picMeasurement.BackColor = Color.Black;
            picMeasurement.Dock = DockStyle.Fill;
            picMeasurement.Location = new Point(3, 3);
            picMeasurement.Name = "picMeasurement";
            picMeasurement.Size = new Size(765, 716);
            picMeasurement.SizeMode = PictureBoxSizeMode.Zoom;
            picMeasurement.TabIndex = 0;
            picMeasurement.TabStop = false;
            // 
            // panelMeasureInfo
            // 
            panelMeasureInfo.Controls.Add(grpMeasureResult);
            panelMeasureInfo.Controls.Add(grpJudgment);
            panelMeasureInfo.Controls.Add(grpBeamSelect);
            panelMeasureInfo.Dock = DockStyle.Bottom;
            panelMeasureInfo.Location = new Point(3, 719);
            panelMeasureInfo.Name = "panelMeasureInfo";
            panelMeasureInfo.Size = new Size(765, 200);
            panelMeasureInfo.TabIndex = 1;
            // 
            // grpMeasureResult
            // 
            grpMeasureResult.Controls.Add(lblMeasureHotPoint);
            grpMeasureResult.Controls.Add(lblMeasureHotValue);
            grpMeasureResult.Controls.Add(lblMeasureCrossPoint);
            grpMeasureResult.Location = new Point(160, 5);
            grpMeasureResult.Name = "grpMeasureResult";
            grpMeasureResult.Size = new Size(300, 90);
            grpMeasureResult.TabIndex = 0;
            grpMeasureResult.TabStop = false;
            grpMeasureResult.Text = "측정 결과";
            // 
            // lblMeasureHotPoint
            // 
            lblMeasureHotPoint.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblMeasureHotPoint.ForeColor = Color.Red;
            lblMeasureHotPoint.Location = new Point(10, 22);
            lblMeasureHotPoint.Name = "lblMeasureHotPoint";
            lblMeasureHotPoint.Size = new Size(280, 18);
            lblMeasureHotPoint.TabIndex = 0;
            lblMeasureHotPoint.Text = "Hot Point: ---";
            // 
            // lblMeasureHotValue
            // 
            lblMeasureHotValue.Location = new Point(10, 44);
            lblMeasureHotValue.Name = "lblMeasureHotValue";
            lblMeasureHotValue.Size = new Size(280, 18);
            lblMeasureHotValue.TabIndex = 1;
            lblMeasureHotValue.Text = "Hot Value: ---";
            // 
            // lblMeasureCrossPoint
            // 
            lblMeasureCrossPoint.ForeColor = Color.Cyan;
            lblMeasureCrossPoint.Location = new Point(10, 66);
            lblMeasureCrossPoint.Name = "lblMeasureCrossPoint";
            lblMeasureCrossPoint.Size = new Size(280, 18);
            lblMeasureCrossPoint.TabIndex = 2;
            lblMeasureCrossPoint.Text = "Cross Point: ---";
            // 
            // grpJudgment
            // 
            grpJudgment.Controls.Add(lblJudgmentResult);
            grpJudgment.Controls.Add(lblHorizontalLabel);
            grpJudgment.Controls.Add(lblHorizontalValue);
            grpJudgment.Controls.Add(lblVerticalLabel);
            grpJudgment.Controls.Add(lblVerticalValue);
            grpJudgment.Controls.Add(lblCandelaLabel);
            grpJudgment.Controls.Add(lblCandelaValue);
            grpJudgment.Location = new Point(470, 5);
            grpJudgment.Name = "grpJudgment";
            grpJudgment.Size = new Size(300, 190);
            grpJudgment.TabIndex = 1;
            grpJudgment.TabStop = false;
            grpJudgment.Text = "판정 결과";
            // 
            // lblJudgmentResult
            // 
            lblJudgmentResult.Font = new Font("맑은 고딕", 28F, FontStyle.Bold);
            lblJudgmentResult.ForeColor = Color.Gray;
            lblJudgmentResult.Location = new Point(10, 25);
            lblJudgmentResult.Name = "lblJudgmentResult";
            lblJudgmentResult.Size = new Size(280, 50);
            lblJudgmentResult.TabIndex = 0;
            lblJudgmentResult.Text = "---";
            lblJudgmentResult.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblHorizontalLabel
            // 
            lblHorizontalLabel.Location = new Point(15, 85);
            lblHorizontalLabel.Name = "lblHorizontalLabel";
            lblHorizontalLabel.Size = new Size(70, 20);
            lblHorizontalLabel.TabIndex = 1;
            lblHorizontalLabel.Text = "수평 (LR):";
            // 
            // lblHorizontalValue
            // 
            lblHorizontalValue.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblHorizontalValue.Location = new Point(90, 85);
            lblHorizontalValue.Name = "lblHorizontalValue";
            lblHorizontalValue.Size = new Size(80, 20);
            lblHorizontalValue.TabIndex = 2;
            lblHorizontalValue.Text = "--- '";
            // 
            // lblVerticalLabel
            // 
            lblVerticalLabel.Location = new Point(15, 110);
            lblVerticalLabel.Name = "lblVerticalLabel";
            lblVerticalLabel.Size = new Size(70, 20);
            lblVerticalLabel.TabIndex = 3;
            lblVerticalLabel.Text = "수직 (UD):";
            // 
            // lblVerticalValue
            // 
            lblVerticalValue.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblVerticalValue.Location = new Point(90, 110);
            lblVerticalValue.Name = "lblVerticalValue";
            lblVerticalValue.Size = new Size(80, 20);
            lblVerticalValue.TabIndex = 4;
            lblVerticalValue.Text = "--- '";
            // 
            // lblCandelaLabel
            // 
            lblCandelaLabel.Location = new Point(15, 135);
            lblCandelaLabel.Name = "lblCandelaLabel";
            lblCandelaLabel.Size = new Size(70, 20);
            lblCandelaLabel.TabIndex = 5;
            lblCandelaLabel.Text = "광도 (CD):";
            // 
            // lblCandelaValue
            // 
            lblCandelaValue.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblCandelaValue.Location = new Point(90, 135);
            lblCandelaValue.Name = "lblCandelaValue";
            lblCandelaValue.Size = new Size(100, 20);
            lblCandelaValue.TabIndex = 6;
            lblCandelaValue.Text = "--- cd";
            // 
            // grpBeamSelect
            // 
            grpBeamSelect.Controls.Add(cmbBeamType);
            grpBeamSelect.Controls.Add(lblBeamType);
            grpBeamSelect.Location = new Point(5, 5);
            grpBeamSelect.Name = "grpBeamSelect";
            grpBeamSelect.Size = new Size(150, 60);
            grpBeamSelect.TabIndex = 2;
            grpBeamSelect.TabStop = false;
            grpBeamSelect.Text = "빔 타입";
            // 
            // cmbBeamType
            // 
            cmbBeamType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBeamType.Items.AddRange(new object[] { "Low Beam", "High Beam" });
            cmbBeamType.Location = new Point(50, 22);
            cmbBeamType.Name = "cmbBeamType";
            cmbBeamType.Size = new Size(90, 23);
            cmbBeamType.TabIndex = 0;
            cmbBeamType.SelectedIndexChanged += cmbBeamType_SelectedIndexChanged;
            // 
            // lblBeamType
            // 
            lblBeamType.Location = new Point(10, 25);
            lblBeamType.Name = "lblBeamType";
            lblBeamType.Size = new Size(35, 20);
            lblBeamType.TabIndex = 1;
            lblBeamType.Text = "타입:";
            // 
            // panelControls
            // 
            panelControls.AutoScroll = true;
            panelControls.Controls.Add(grpDisplay);
            panelControls.Controls.Add(grpModel);
            panelControls.Controls.Add(grpStatus);
            panelControls.Controls.Add(grpAcquisition);
            panelControls.Controls.Add(grpCamera);
            panelControls.Dock = DockStyle.Right;
            panelControls.Location = new Point(779, 0);
            panelControls.Name = "panelControls";
            panelControls.Padding = new Padding(10);
            panelControls.Size = new Size(301, 950);
            panelControls.TabIndex = 1;
            // 
            // grpDisplay
            // 
            grpDisplay.Controls.Add(chkShowOverlay);
            grpDisplay.Dock = DockStyle.Top;
            grpDisplay.Location = new Point(10, 650);
            grpDisplay.Name = "grpDisplay";
            grpDisplay.Size = new Size(281, 60);
            grpDisplay.TabIndex = 0;
            grpDisplay.TabStop = false;
            grpDisplay.Text = "표시 옵션";
            // 
            // chkShowOverlay
            // 
            chkShowOverlay.AutoSize = true;
            chkShowOverlay.Checked = true;
            chkShowOverlay.CheckState = CheckState.Checked;
            chkShowOverlay.Location = new Point(15, 25);
            chkShowOverlay.Name = "chkShowOverlay";
            chkShowOverlay.Size = new Size(158, 19);
            chkShowOverlay.TabIndex = 0;
            chkShowOverlay.Text = "분석 결과 오버레이 표시";
            // 
            // grpModel
            // 
            grpModel.Controls.Add(cmbHeadlampSide);
            grpModel.Controls.Add(lblSideLabel);
            grpModel.Controls.Add(cmbModel);
            grpModel.Controls.Add(lblModelLabel);
            grpModel.Dock = DockStyle.Top;
            grpModel.Location = new Point(10, 560);
            grpModel.Name = "grpModel";
            grpModel.Size = new Size(281, 90);
            grpModel.TabIndex = 1;
            grpModel.TabStop = false;
            grpModel.Text = "차종 선택";
            // 
            // cmbHeadlampSide
            // 
            cmbHeadlampSide.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHeadlampSide.Items.AddRange(new object[] { "좌측 (Left)", "우측 (Right)" });
            cmbHeadlampSide.Location = new Point(60, 55);
            cmbHeadlampSide.Name = "cmbHeadlampSide";
            cmbHeadlampSide.Size = new Size(185, 23);
            cmbHeadlampSide.TabIndex = 0;
            // 
            // lblSideLabel
            // 
            lblSideLabel.AutoSize = true;
            lblSideLabel.Location = new Point(15, 58);
            lblSideLabel.Name = "lblSideLabel";
            lblSideLabel.Size = new Size(34, 15);
            lblSideLabel.TabIndex = 1;
            lblSideLabel.Text = "위치:";
            // 
            // cmbModel
            // 
            cmbModel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModel.Location = new Point(60, 25);
            cmbModel.Name = "cmbModel";
            cmbModel.Size = new Size(185, 23);
            cmbModel.TabIndex = 2;
            cmbModel.SelectedIndexChanged += cmbModel_SelectedIndexChanged;
            // 
            // lblModelLabel
            // 
            lblModelLabel.AutoSize = true;
            lblModelLabel.Location = new Point(15, 28);
            lblModelLabel.Name = "lblModelLabel";
            lblModelLabel.Size = new Size(34, 15);
            lblModelLabel.TabIndex = 3;
            lblModelLabel.Text = "차종:";
            // 
            // grpStatus
            // 
            grpStatus.Controls.Add(lblErrorCount);
            grpStatus.Controls.Add(lblFrameCount);
            grpStatus.Controls.Add(lblStatus);
            grpStatus.Controls.Add(lblStatusLabel);
            grpStatus.Dock = DockStyle.Top;
            grpStatus.Location = new Point(10, 460);
            grpStatus.Name = "grpStatus";
            grpStatus.Size = new Size(281, 100);
            grpStatus.TabIndex = 2;
            grpStatus.TabStop = false;
            grpStatus.Text = "상태";
            // 
            // lblErrorCount
            // 
            lblErrorCount.AutoSize = true;
            lblErrorCount.Location = new Point(15, 75);
            lblErrorCount.Name = "lblErrorCount";
            lblErrorCount.Size = new Size(45, 15);
            lblErrorCount.TabIndex = 0;
            lblErrorCount.Text = "오류: 0";
            // 
            // lblFrameCount
            // 
            lblFrameCount.AutoSize = true;
            lblFrameCount.Location = new Point(15, 55);
            lblFrameCount.Name = "lblFrameCount";
            lblFrameCount.Size = new Size(57, 15);
            lblFrameCount.TabIndex = 1;
            lblFrameCount.Text = "프레임: 0";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(60, 30);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(59, 15);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "연결 안됨";
            // 
            // lblStatusLabel
            // 
            lblStatusLabel.AutoSize = true;
            lblStatusLabel.Location = new Point(15, 30);
            lblStatusLabel.Name = "lblStatusLabel";
            lblStatusLabel.Size = new Size(34, 15);
            lblStatusLabel.TabIndex = 3;
            lblStatusLabel.Text = "상태:";
            // 
            // grpAcquisition
            // 
            grpAcquisition.Controls.Add(btnStartStop);
            grpAcquisition.Controls.Add(btnDarkMode);
            grpAcquisition.Controls.Add(chkAutoMode);
            grpAcquisition.Controls.Add(lblBlacklevel);
            grpAcquisition.Controls.Add(numBlacklevel);
            grpAcquisition.Controls.Add(lblBlacklevelLabel);
            grpAcquisition.Controls.Add(lblGain);
            grpAcquisition.Controls.Add(numGain);
            grpAcquisition.Controls.Add(lblGainLabel);
            grpAcquisition.Controls.Add(lblExposure);
            grpAcquisition.Controls.Add(numExposure);
            grpAcquisition.Controls.Add(trkExposure);
            grpAcquisition.Controls.Add(lblExposureLabel);
            grpAcquisition.Dock = DockStyle.Top;
            grpAcquisition.Location = new Point(10, 210);
            grpAcquisition.Name = "grpAcquisition";
            grpAcquisition.Size = new Size(281, 250);
            grpAcquisition.TabIndex = 3;
            grpAcquisition.TabStop = false;
            grpAcquisition.Text = "취득";
            // 
            // btnStartStop
            // 
            btnStartStop.Enabled = false;
            btnStartStop.Location = new Point(15, 185);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(110, 35);
            btnStartStop.TabIndex = 0;
            btnStartStop.Text = "시작";
            btnStartStop.Click += btnStartStop_Click;
            // 
            // btnDarkMode
            // 
            btnDarkMode.Enabled = false;
            btnDarkMode.Location = new Point(130, 185);
            btnDarkMode.Name = "btnDarkMode";
            btnDarkMode.Size = new Size(115, 35);
            btnDarkMode.TabIndex = 1;
            btnDarkMode.Text = "암흑 모드";
            btnDarkMode.Click += btnDarkMode_Click;
            // 
            // chkAutoMode
            // 
            chkAutoMode.AutoSize = true;
            chkAutoMode.Checked = true;
            chkAutoMode.CheckState = CheckState.Checked;
            chkAutoMode.Enabled = false;
            chkAutoMode.Location = new Point(15, 155);
            chkAutoMode.Name = "chkAutoMode";
            chkAutoMode.Size = new Size(170, 19);
            chkAutoMode.TabIndex = 2;
            chkAutoMode.Text = "자동 노출/게인 (Auto AES)";
            chkAutoMode.CheckedChanged += chkAutoMode_CheckedChanged;
            // 
            // lblBlacklevel
            // 
            lblBlacklevel.AutoSize = true;
            lblBlacklevel.Location = new Point(145, 127);
            lblBlacklevel.Name = "lblBlacklevel";
            lblBlacklevel.Size = new Size(14, 15);
            lblBlacklevel.TabIndex = 3;
            lblBlacklevel.Text = "0";
            // 
            // numBlacklevel
            // 
            numBlacklevel.Enabled = false;
            numBlacklevel.Location = new Point(60, 125);
            numBlacklevel.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numBlacklevel.Name = "numBlacklevel";
            numBlacklevel.Size = new Size(80, 23);
            numBlacklevel.TabIndex = 4;
            numBlacklevel.ValueChanged += numBlacklevel_ValueChanged;
            // 
            // lblBlacklevelLabel
            // 
            lblBlacklevelLabel.AutoSize = true;
            lblBlacklevelLabel.Location = new Point(15, 127);
            lblBlacklevelLabel.Name = "lblBlacklevelLabel";
            lblBlacklevelLabel.Size = new Size(46, 15);
            lblBlacklevelLabel.TabIndex = 5;
            lblBlacklevelLabel.Text = "흑레벨:";
            // 
            // lblGain
            // 
            lblGain.AutoSize = true;
            lblGain.Location = new Point(145, 100);
            lblGain.Name = "lblGain";
            lblGain.Size = new Size(24, 15);
            lblGain.TabIndex = 6;
            lblGain.Text = "0%";
            // 
            // numGain
            // 
            numGain.Enabled = false;
            numGain.Location = new Point(60, 98);
            numGain.Name = "numGain";
            numGain.Size = new Size(80, 23);
            numGain.TabIndex = 7;
            numGain.ValueChanged += numGain_ValueChanged;
            // 
            // lblGainLabel
            // 
            lblGainLabel.AutoSize = true;
            lblGainLabel.Location = new Point(15, 100);
            lblGainLabel.Name = "lblGainLabel";
            lblGainLabel.Size = new Size(34, 15);
            lblGainLabel.TabIndex = 8;
            lblGainLabel.Text = "게인:";
            // 
            // lblExposure
            // 
            lblExposure.AutoSize = true;
            lblExposure.Location = new Point(160, 80);
            lblExposure.Name = "lblExposure";
            lblExposure.Size = new Size(26, 15);
            lblExposure.TabIndex = 9;
            lblExposure.Text = "0μs";
            // 
            // numExposure
            // 
            numExposure.Enabled = false;
            numExposure.Location = new Point(160, 55);
            numExposure.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numExposure.Name = "numExposure";
            numExposure.Size = new Size(85, 23);
            numExposure.TabIndex = 10;
            numExposure.ValueChanged += numExposure_ValueChanged;
            // 
            // trkExposure
            // 
            trkExposure.Enabled = false;
            trkExposure.Location = new Point(15, 50);
            trkExposure.Maximum = 100000;
            trkExposure.Name = "trkExposure";
            trkExposure.Size = new Size(140, 45);
            trkExposure.TabIndex = 11;
            trkExposure.TickFrequency = 10000;
            trkExposure.Scroll += trkExposure_Scroll;
            // 
            // lblExposureLabel
            // 
            lblExposureLabel.AutoSize = true;
            lblExposureLabel.Location = new Point(15, 30);
            lblExposureLabel.Name = "lblExposureLabel";
            lblExposureLabel.Size = new Size(62, 15);
            lblExposureLabel.TabIndex = 12;
            lblExposureLabel.Text = "노출 시간:";
            // 
            // grpCamera
            // 
            grpCamera.Controls.Add(btnRefresh);
            grpCamera.Controls.Add(cmbCameras);
            grpCamera.Controls.Add(btnConnect);
            grpCamera.Controls.Add(btnLoadParams);
            grpCamera.Controls.Add(btnCalibration);
            grpCamera.Controls.Add(lblCameraLabel);
            grpCamera.Dock = DockStyle.Top;
            grpCamera.Location = new Point(10, 10);
            grpCamera.Name = "grpCamera";
            grpCamera.Padding = new Padding(10);
            grpCamera.Size = new Size(281, 200);
            grpCamera.TabIndex = 0;
            grpCamera.TabStop = false;
            grpCamera.Text = "카메라";
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(200, 50);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(57, 23);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "Refresh";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // cmbCameras
            // 
            cmbCameras.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCameras.Location = new Point(15, 50);
            cmbCameras.Name = "cmbCameras";
            cmbCameras.Size = new Size(180, 23);
            cmbCameras.TabIndex = 1;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(15, 85);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(242, 30);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "연결";
            btnConnect.Click += btnConnect_Click;
            // 
            // btnLoadParams
            // 
            btnLoadParams.Enabled = false;
            btnLoadParams.Location = new Point(15, 120);
            btnLoadParams.Name = "btnLoadParams";
            btnLoadParams.Size = new Size(242, 30);
            btnLoadParams.TabIndex = 3;
            btnLoadParams.Text = "파라미터 파일 로드...";
            btnLoadParams.Click += btnLoadParams_Click;
            // 
            // btnCalibration
            // 
            btnCalibration.Location = new Point(15, 155);
            btnCalibration.Name = "btnCalibration";
            btnCalibration.Size = new Size(242, 30);
            btnCalibration.TabIndex = 4;
            btnCalibration.Text = "교정 설정 (Calibration)...";
            btnCalibration.Click += btnCalibration_Click;
            // 
            // lblCameraLabel
            // 
            lblCameraLabel.AutoSize = true;
            lblCameraLabel.Location = new Point(15, 30);
            lblCameraLabel.Name = "lblCameraLabel";
            lblCameraLabel.Size = new Size(46, 15);
            lblCameraLabel.TabIndex = 5;
            lblCameraLabel.Text = "카메라:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1080, 950);
            Controls.Add(tabMain);
            Controls.Add(panelControls);
            MinimumSize = new Size(1000, 800);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HeadLight Inspection";
            Load += MainForm_Load;
            tabMain.ResumeLayout(false);
            tabAlignment.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picAlignment).EndInit();
            panelAlignInfo.ResumeLayout(false);
            grpAlignResult.ResumeLayout(false);
            tabMeasurement.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picMeasurement).EndInit();
            panelMeasureInfo.ResumeLayout(false);
            grpMeasureResult.ResumeLayout(false);
            grpJudgment.ResumeLayout(false);
            grpBeamSelect.ResumeLayout(false);
            panelControls.ResumeLayout(false);
            grpDisplay.ResumeLayout(false);
            grpDisplay.PerformLayout();
            grpModel.ResumeLayout(false);
            grpModel.PerformLayout();
            grpStatus.ResumeLayout(false);
            grpStatus.PerformLayout();
            grpAcquisition.ResumeLayout(false);
            grpAcquisition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBlacklevel).EndInit();
            ((System.ComponentModel.ISupportInitialize)numGain).EndInit();
            ((System.ComponentModel.ISupportInitialize)numExposure).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkExposure).EndInit();
            grpCamera.ResumeLayout(false);
            grpCamera.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // 메인 탭 컨트롤
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabAlignment;
        private System.Windows.Forms.TabPage tabMeasurement;

        // 정대 탭 컨트롤
        private System.Windows.Forms.PictureBox picAlignment;
        private System.Windows.Forms.Panel panelAlignInfo;
        private System.Windows.Forms.GroupBox grpAlignResult;
        private System.Windows.Forms.Label lblAlignLeftLamp;
        private System.Windows.Forms.Label lblAlignRightLamp;
        private System.Windows.Forms.Label lblAlignBoundaryCenter;
        private System.Windows.Forms.Label lblAlignCentroid;
        private System.Windows.Forms.Label lblAlignStatus;
        private System.Windows.Forms.Label lblAlignLampCount;

        // 측정 탭 컨트롤
        private System.Windows.Forms.PictureBox picMeasurement;
        private System.Windows.Forms.Panel panelMeasureInfo;
        private System.Windows.Forms.GroupBox grpMeasureResult;
        private System.Windows.Forms.Label lblMeasureHotPoint;
        private System.Windows.Forms.Label lblMeasureHotValue;
        private System.Windows.Forms.Label lblMeasureCrossPoint;
        private System.Windows.Forms.GroupBox grpJudgment;
        private System.Windows.Forms.Label lblJudgmentResult;
        private System.Windows.Forms.Label lblHorizontalLabel;
        private System.Windows.Forms.Label lblHorizontalValue;
        private System.Windows.Forms.Label lblVerticalLabel;
        private System.Windows.Forms.Label lblVerticalValue;
        private System.Windows.Forms.Label lblCandelaLabel;
        private System.Windows.Forms.Label lblCandelaValue;
        private System.Windows.Forms.GroupBox grpBeamSelect;
        private System.Windows.Forms.ComboBox cmbBeamType;
        private System.Windows.Forms.Label lblBeamType;

        // 우측 패널 컨트롤
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.GroupBox grpCamera;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox cmbCameras;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnLoadParams;
        private System.Windows.Forms.Button btnCalibration;
        private System.Windows.Forms.Label lblCameraLabel;
        private System.Windows.Forms.GroupBox grpAcquisition;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnDarkMode;
        private System.Windows.Forms.CheckBox chkAutoMode;
        private System.Windows.Forms.Label lblBlacklevel;
        private System.Windows.Forms.NumericUpDown numBlacklevel;
        private System.Windows.Forms.Label lblBlacklevelLabel;
        private System.Windows.Forms.Label lblGain;
        private System.Windows.Forms.NumericUpDown numGain;
        private System.Windows.Forms.Label lblGainLabel;
        private System.Windows.Forms.Label lblExposure;
        private System.Windows.Forms.NumericUpDown numExposure;
        private System.Windows.Forms.TrackBar trkExposure;
        private System.Windows.Forms.Label lblExposureLabel;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.Label lblErrorCount;
        private System.Windows.Forms.Label lblFrameCount;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.GroupBox grpModel;
        private System.Windows.Forms.ComboBox cmbHeadlampSide;
        private System.Windows.Forms.Label lblSideLabel;
        private System.Windows.Forms.ComboBox cmbModel;
        private System.Windows.Forms.Label lblModelLabel;
        private System.Windows.Forms.GroupBox grpDisplay;
        private System.Windows.Forms.CheckBox chkShowOverlay;
    }
}
