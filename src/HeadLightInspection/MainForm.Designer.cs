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
            pictureBox = new PictureBox();
            panelControls = new Panel();
            grpJudgment = new GroupBox();
            lblJudgmentResult = new Label();
            lblCandelaValue = new Label();
            lblCandelaLabel = new Label();
            lblVerticalValue = new Label();
            lblVerticalLabel = new Label();
            lblHorizontalValue = new Label();
            lblHorizontalLabel = new Label();
            grpMode = new GroupBox();
            lblAlignStatus = new Label();
            rbModeMeasurement = new RadioButton();
            rbModeAlignment = new RadioButton();
            grpModel = new GroupBox();
            cmbHeadlampSide = new ComboBox();
            lblSideLabel = new Label();
            cmbModel = new ComboBox();
            lblModelLabel = new Label();
            grpAnalysis = new GroupBox();
            lblAnalysisResult = new Label();
            chkShowOverlay = new CheckBox();
            cmbBeamType = new ComboBox();
            lblBeamType = new Label();
            btnAnalyze = new Button();
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
            lblCameraLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            panelControls.SuspendLayout();
            grpJudgment.SuspendLayout();
            grpMode.SuspendLayout();
            grpModel.SuspendLayout();
            grpAnalysis.SuspendLayout();
            grpStatus.SuspendLayout();
            grpAcquisition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBlacklevel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numExposure).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkExposure).BeginInit();
            grpCamera.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox
            // 
            pictureBox.BackColor = Color.Black;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Location = new Point(0, 0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(779, 950);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            // 
            // panelControls
            // 
            panelControls.AutoScroll = true;
            panelControls.Controls.Add(grpJudgment);
            panelControls.Controls.Add(grpMode);
            panelControls.Controls.Add(grpModel);
            panelControls.Controls.Add(grpAnalysis);
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
            // grpJudgment
            // 
            grpJudgment.Controls.Add(lblJudgmentResult);
            grpJudgment.Controls.Add(lblCandelaValue);
            grpJudgment.Controls.Add(lblCandelaLabel);
            grpJudgment.Controls.Add(lblVerticalValue);
            grpJudgment.Controls.Add(lblVerticalLabel);
            grpJudgment.Controls.Add(lblHorizontalValue);
            grpJudgment.Controls.Add(lblHorizontalLabel);
            grpJudgment.Dock = DockStyle.Top;
            grpJudgment.Location = new Point(10, 790);
            grpJudgment.Name = "grpJudgment";
            grpJudgment.Padding = new Padding(10);
            grpJudgment.Size = new Size(281, 150);
            grpJudgment.TabIndex = 5;
            grpJudgment.TabStop = false;
            grpJudgment.Text = "판정 결과";
            // 
            // lblJudgmentResult
            // 
            lblJudgmentResult.Font = new Font("맑은 고딕", 24F, FontStyle.Bold);
            lblJudgmentResult.ForeColor = Color.Gray;
            lblJudgmentResult.Location = new Point(15, 25);
            lblJudgmentResult.Name = "lblJudgmentResult";
            lblJudgmentResult.Size = new Size(230, 45);
            lblJudgmentResult.TabIndex = 0;
            lblJudgmentResult.Text = "---";
            lblJudgmentResult.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCandelaValue
            // 
            lblCandelaValue.AutoSize = true;
            lblCandelaValue.Location = new Point(100, 120);
            lblCandelaValue.Name = "lblCandelaValue";
            lblCandelaValue.Size = new Size(39, 15);
            lblCandelaValue.TabIndex = 6;
            lblCandelaValue.Text = "--- cd";
            // 
            // lblCandelaLabel
            // 
            lblCandelaLabel.AutoSize = true;
            lblCandelaLabel.Location = new Point(15, 120);
            lblCandelaLabel.Name = "lblCandelaLabel";
            lblCandelaLabel.Size = new Size(34, 15);
            lblCandelaLabel.TabIndex = 5;
            lblCandelaLabel.Text = "광도:";
            // 
            // lblVerticalValue
            // 
            lblVerticalValue.AutoSize = true;
            lblVerticalValue.Location = new Point(100, 95);
            lblVerticalValue.Name = "lblVerticalValue";
            lblVerticalValue.Size = new Size(29, 15);
            lblVerticalValue.TabIndex = 4;
            lblVerticalValue.Text = "--- '";
            // 
            // lblVerticalLabel
            // 
            lblVerticalLabel.AutoSize = true;
            lblVerticalLabel.Location = new Point(15, 95);
            lblVerticalLabel.Name = "lblVerticalLabel";
            lblVerticalLabel.Size = new Size(62, 15);
            lblVerticalLabel.TabIndex = 3;
            lblVerticalLabel.Text = "수직 편차:";
            // 
            // lblHorizontalValue
            // 
            lblHorizontalValue.AutoSize = true;
            lblHorizontalValue.Location = new Point(100, 75);
            lblHorizontalValue.Name = "lblHorizontalValue";
            lblHorizontalValue.Size = new Size(29, 15);
            lblHorizontalValue.TabIndex = 2;
            lblHorizontalValue.Text = "--- '";
            // 
            // lblHorizontalLabel
            // 
            lblHorizontalLabel.AutoSize = true;
            lblHorizontalLabel.Location = new Point(15, 75);
            lblHorizontalLabel.Name = "lblHorizontalLabel";
            lblHorizontalLabel.Size = new Size(62, 15);
            lblHorizontalLabel.TabIndex = 1;
            lblHorizontalLabel.Text = "수평 편차:";
            // 
            // grpMode
            // 
            grpMode.Controls.Add(lblAlignStatus);
            grpMode.Controls.Add(rbModeMeasurement);
            grpMode.Controls.Add(rbModeAlignment);
            grpMode.Dock = DockStyle.Top;
            grpMode.Location = new Point(10, 690);
            grpMode.Name = "grpMode";
            grpMode.Padding = new Padding(10);
            grpMode.Size = new Size(281, 100);
            grpMode.TabIndex = 6;
            grpMode.TabStop = false;
            grpMode.Text = "검사 모드";
            // 
            // lblAlignStatus
            // 
            lblAlignStatus.AutoSize = true;
            lblAlignStatus.Location = new Point(15, 55);
            lblAlignStatus.Name = "lblAlignStatus";
            lblAlignStatus.Size = new Size(90, 15);
            lblAlignStatus.TabIndex = 2;
            lblAlignStatus.Text = "정대 상태: 대기";
            // 
            // rbModeMeasurement
            // 
            rbModeMeasurement.AutoSize = true;
            rbModeMeasurement.Location = new Point(120, 25);
            rbModeMeasurement.Name = "rbModeMeasurement";
            rbModeMeasurement.Size = new Size(76, 19);
            rbModeMeasurement.TabIndex = 1;
            rbModeMeasurement.Text = "측정 (PA)";
            rbModeMeasurement.UseVisualStyleBackColor = true;
            rbModeMeasurement.CheckedChanged += rbMode_CheckedChanged;
            // 
            // rbModeAlignment
            // 
            rbModeAlignment.AutoSize = true;
            rbModeAlignment.Checked = true;
            rbModeAlignment.Location = new Point(15, 25);
            rbModeAlignment.Name = "rbModeAlignment";
            rbModeAlignment.Size = new Size(75, 19);
            rbModeAlignment.TabIndex = 0;
            rbModeAlignment.TabStop = true;
            rbModeAlignment.Text = "정대 (FC)";
            rbModeAlignment.UseVisualStyleBackColor = true;
            rbModeAlignment.CheckedChanged += rbMode_CheckedChanged;
            // 
            // grpModel
            // 
            grpModel.Controls.Add(cmbHeadlampSide);
            grpModel.Controls.Add(lblSideLabel);
            grpModel.Controls.Add(cmbModel);
            grpModel.Controls.Add(lblModelLabel);
            grpModel.Dock = DockStyle.Top;
            grpModel.Location = new Point(10, 600);
            grpModel.Name = "grpModel";
            grpModel.Padding = new Padding(10);
            grpModel.Size = new Size(281, 90);
            grpModel.TabIndex = 4;
            grpModel.TabStop = false;
            grpModel.Text = "차종 선택";
            // 
            // cmbHeadlampSide
            // 
            cmbHeadlampSide.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHeadlampSide.FormattingEnabled = true;
            cmbHeadlampSide.Items.AddRange(new object[] { "좌측 (Left)", "우측 (Right)" });
            cmbHeadlampSide.Location = new Point(60, 55);
            cmbHeadlampSide.Name = "cmbHeadlampSide";
            cmbHeadlampSide.Size = new Size(185, 23);
            cmbHeadlampSide.TabIndex = 3;
            // 
            // lblSideLabel
            // 
            lblSideLabel.AutoSize = true;
            lblSideLabel.Location = new Point(15, 58);
            lblSideLabel.Name = "lblSideLabel";
            lblSideLabel.Size = new Size(34, 15);
            lblSideLabel.TabIndex = 2;
            lblSideLabel.Text = "위치:";
            // 
            // cmbModel
            // 
            cmbModel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModel.FormattingEnabled = true;
            cmbModel.Location = new Point(60, 25);
            cmbModel.Name = "cmbModel";
            cmbModel.Size = new Size(185, 23);
            cmbModel.TabIndex = 1;
            cmbModel.SelectedIndexChanged += cmbModel_SelectedIndexChanged;
            // 
            // lblModelLabel
            // 
            lblModelLabel.AutoSize = true;
            lblModelLabel.Location = new Point(15, 28);
            lblModelLabel.Name = "lblModelLabel";
            lblModelLabel.Size = new Size(34, 15);
            lblModelLabel.TabIndex = 0;
            lblModelLabel.Text = "차종:";
            // 
            // grpAnalysis
            // 
            grpAnalysis.Controls.Add(lblAnalysisResult);
            grpAnalysis.Controls.Add(chkShowOverlay);
            grpAnalysis.Controls.Add(cmbBeamType);
            grpAnalysis.Controls.Add(lblBeamType);
            grpAnalysis.Controls.Add(btnAnalyze);
            grpAnalysis.Dock = DockStyle.Top;
            grpAnalysis.Location = new Point(10, 460);
            grpAnalysis.Name = "grpAnalysis";
            grpAnalysis.Padding = new Padding(10);
            grpAnalysis.Size = new Size(281, 140);
            grpAnalysis.TabIndex = 3;
            grpAnalysis.TabStop = false;
            grpAnalysis.Text = "분석";
            // 
            // lblAnalysisResult
            // 
            lblAnalysisResult.AutoSize = true;
            lblAnalysisResult.Location = new Point(15, 115);
            lblAnalysisResult.Name = "lblAnalysisResult";
            lblAnalysisResult.Size = new Size(0, 15);
            lblAnalysisResult.TabIndex = 4;
            // 
            // chkShowOverlay
            // 
            chkShowOverlay.AutoSize = true;
            chkShowOverlay.Checked = true;
            chkShowOverlay.CheckState = CheckState.Checked;
            chkShowOverlay.Location = new Point(15, 85);
            chkShowOverlay.Name = "chkShowOverlay";
            chkShowOverlay.Size = new Size(102, 19);
            chkShowOverlay.TabIndex = 3;
            chkShowOverlay.Text = "결과 오버레이";
            chkShowOverlay.UseVisualStyleBackColor = true;
            // 
            // cmbBeamType
            // 
            cmbBeamType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBeamType.FormattingEnabled = true;
            cmbBeamType.Items.AddRange(new object[] { "Low Beam", "High Beam" });
            cmbBeamType.Location = new Point(80, 25);
            cmbBeamType.Name = "cmbBeamType";
            cmbBeamType.Size = new Size(100, 23);
            cmbBeamType.TabIndex = 1;
            cmbBeamType.SelectedIndexChanged += cmbBeamType_SelectedIndexChanged;
            // 
            // lblBeamType
            // 
            lblBeamType.AutoSize = true;
            lblBeamType.Location = new Point(15, 28);
            lblBeamType.Name = "lblBeamType";
            lblBeamType.Size = new Size(50, 15);
            lblBeamType.TabIndex = 0;
            lblBeamType.Text = "빔 타입:";
            // 
            // btnAnalyze
            // 
            btnAnalyze.Enabled = false;
            btnAnalyze.Location = new Point(15, 55);
            btnAnalyze.Name = "btnAnalyze";
            btnAnalyze.Size = new Size(230, 25);
            btnAnalyze.TabIndex = 2;
            btnAnalyze.Text = "분석";
            btnAnalyze.UseVisualStyleBackColor = true;
            btnAnalyze.Click += btnAnalyze_Click;
            // 
            // grpStatus
            // 
            grpStatus.Controls.Add(lblErrorCount);
            grpStatus.Controls.Add(lblFrameCount);
            grpStatus.Controls.Add(lblStatus);
            grpStatus.Controls.Add(lblStatusLabel);
            grpStatus.Dock = DockStyle.Top;
            grpStatus.Location = new Point(10, 340);
            grpStatus.Name = "grpStatus";
            grpStatus.Padding = new Padding(10);
            grpStatus.Size = new Size(281, 120);
            grpStatus.TabIndex = 2;
            grpStatus.TabStop = false;
            grpStatus.Text = "상태";
            // 
            // lblErrorCount
            // 
            lblErrorCount.AutoSize = true;
            lblErrorCount.Location = new Point(15, 80);
            lblErrorCount.Name = "lblErrorCount";
            lblErrorCount.Size = new Size(45, 15);
            lblErrorCount.TabIndex = 3;
            lblErrorCount.Text = "오류: 0";
            // 
            // lblFrameCount
            // 
            lblFrameCount.AutoSize = true;
            lblFrameCount.Location = new Point(15, 55);
            lblFrameCount.Name = "lblFrameCount";
            lblFrameCount.Size = new Size(57, 15);
            lblFrameCount.TabIndex = 2;
            lblFrameCount.Text = "프레임: 0";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(60, 30);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(59, 15);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "연결 안됨";
            // 
            // lblStatusLabel
            // 
            lblStatusLabel.AutoSize = true;
            lblStatusLabel.Location = new Point(15, 30);
            lblStatusLabel.Name = "lblStatusLabel";
            lblStatusLabel.Size = new Size(34, 15);
            lblStatusLabel.TabIndex = 0;
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
            grpAcquisition.Location = new Point(10, 200);
            grpAcquisition.Name = "grpAcquisition";
            grpAcquisition.Padding = new Padding(10);
            grpAcquisition.Size = new Size(281, 250);
            grpAcquisition.TabIndex = 1;
            grpAcquisition.TabStop = false;
            grpAcquisition.Text = "취득";
            // 
            // btnStartStop
            //
            btnStartStop.Enabled = false;
            btnStartStop.Location = new Point(15, 200);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(110, 35);
            btnStartStop.TabIndex = 3;
            btnStartStop.Text = "시작";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;
            //
            // btnDarkMode
            //
            btnDarkMode.Enabled = false;
            btnDarkMode.Location = new Point(130, 200);
            btnDarkMode.Name = "btnDarkMode";
            btnDarkMode.Size = new Size(115, 35);
            btnDarkMode.TabIndex = 7;
            btnDarkMode.Text = "암흑 모드";
            btnDarkMode.UseVisualStyleBackColor = true;
            btnDarkMode.Click += btnDarkMode_Click;
            //
            // chkAutoMode
            //
            chkAutoMode.AutoSize = true;
            chkAutoMode.Checked = true;
            chkAutoMode.CheckState = CheckState.Checked;
            chkAutoMode.Enabled = false;
            chkAutoMode.Location = new Point(15, 152);
            chkAutoMode.Name = "chkAutoMode";
            chkAutoMode.Size = new Size(170, 19);
            chkAutoMode.TabIndex = 14;
            chkAutoMode.Text = "자동 노출/게인 (Auto AES)";
            chkAutoMode.UseVisualStyleBackColor = true;
            chkAutoMode.CheckedChanged += chkAutoMode_CheckedChanged;
            //
            // lblBlacklevel
            //
            lblBlacklevel.AutoSize = true;
            lblBlacklevel.Location = new Point(145, 127);
            lblBlacklevel.Name = "lblBlacklevel";
            lblBlacklevel.Size = new Size(13, 15);
            lblBlacklevel.TabIndex = 13;
            lblBlacklevel.Text = "0";
            //
            // numBlacklevel
            //
            numBlacklevel.Enabled = false;
            numBlacklevel.Location = new Point(60, 125);
            numBlacklevel.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numBlacklevel.Name = "numBlacklevel";
            numBlacklevel.Size = new Size(80, 23);
            numBlacklevel.TabIndex = 12;
            numBlacklevel.ValueChanged += numBlacklevel_ValueChanged;
            //
            // lblBlacklevelLabel
            //
            lblBlacklevelLabel.AutoSize = true;
            lblBlacklevelLabel.Location = new Point(15, 127);
            lblBlacklevelLabel.Name = "lblBlacklevelLabel";
            lblBlacklevelLabel.Size = new Size(42, 15);
            lblBlacklevelLabel.TabIndex = 11;
            lblBlacklevelLabel.Text = "흑레벨:";
            // 
            // lblGain
            // 
            lblGain.AutoSize = true;
            lblGain.Location = new Point(145, 100);
            lblGain.Name = "lblGain";
            lblGain.Size = new Size(24, 15);
            lblGain.TabIndex = 10;
            lblGain.Text = "0%";
            // 
            // numGain
            // 
            numGain.Enabled = false;
            numGain.Location = new Point(60, 98);
            numGain.Name = "numGain";
            numGain.Size = new Size(80, 23);
            numGain.TabIndex = 9;
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
            lblExposure.TabIndex = 2;
            lblExposure.Text = "0μs";
            // 
            // numExposure
            // 
            numExposure.Enabled = false;
            numExposure.Location = new Point(160, 55);
            numExposure.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numExposure.Name = "numExposure";
            numExposure.Size = new Size(85, 23);
            numExposure.TabIndex = 4;
            numExposure.ValueChanged += numExposure_ValueChanged;
            // 
            // trkExposure
            // 
            trkExposure.Enabled = false;
            trkExposure.Location = new Point(15, 50);
            trkExposure.Maximum = 100000;
            trkExposure.Name = "trkExposure";
            trkExposure.Size = new Size(140, 45);
            trkExposure.TabIndex = 1;
            trkExposure.TickFrequency = 10000;
            trkExposure.Scroll += trkExposure_Scroll;
            // 
            // lblExposureLabel
            // 
            lblExposureLabel.AutoSize = true;
            lblExposureLabel.Location = new Point(15, 30);
            lblExposureLabel.Name = "lblExposureLabel";
            lblExposureLabel.Size = new Size(62, 15);
            lblExposureLabel.TabIndex = 0;
            lblExposureLabel.Text = "노출 시간:";
            // 
            // btnCalibration
            //
            btnCalibration = new Button();
            btnCalibration.Location = new Point(15, 155);
            btnCalibration.Name = "btnCalibration";
            btnCalibration.Size = new Size(242, 30);
            btnCalibration.TabIndex = 5;
            btnCalibration.Text = "교정";
            btnCalibration.UseVisualStyleBackColor = true;
            btnCalibration.Click += btnCalibration_Click;
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
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // cmbCameras
            // 
            cmbCameras.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCameras.FormattingEnabled = true;
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
            btnConnect.TabIndex = 3;
            btnConnect.Text = "연결";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            //
            // btnLoadParams
            //
            btnLoadParams.Enabled = false;
            btnLoadParams.Location = new Point(15, 120);
            btnLoadParams.Name = "btnLoadParams";
            btnLoadParams.Size = new Size(242, 30);
            btnLoadParams.TabIndex = 4;
            btnLoadParams.Text = "파라미터 파일 로드...";
            btnLoadParams.UseVisualStyleBackColor = true;
            btnLoadParams.Click += btnLoadParams_Click;
            //
            // lblCameraLabel
            // 
            lblCameraLabel.AutoSize = true;
            lblCameraLabel.Location = new Point(15, 30);
            lblCameraLabel.Name = "lblCameraLabel";
            lblCameraLabel.Size = new Size(46, 15);
            lblCameraLabel.TabIndex = 0;
            lblCameraLabel.Text = "카메라:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1080, 950);
            Controls.Add(pictureBox);
            Controls.Add(panelControls);
            MinimumSize = new Size(900, 700);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HeadLight Inspection - 카메라 테스트";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            panelControls.ResumeLayout(false);
            grpJudgment.ResumeLayout(false);
            grpJudgment.PerformLayout();
            grpMode.ResumeLayout(false);
            grpMode.PerformLayout();
            grpModel.ResumeLayout(false);
            grpModel.PerformLayout();
            grpAnalysis.ResumeLayout(false);
            grpAnalysis.PerformLayout();
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

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.GroupBox grpCamera;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox cmbCameras;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnLoadParams;
        private System.Windows.Forms.Label lblCameraLabel;
        private System.Windows.Forms.GroupBox grpAcquisition;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Label lblExposure;
        private System.Windows.Forms.TrackBar trkExposure;
        private System.Windows.Forms.NumericUpDown numExposure;
        private System.Windows.Forms.Label lblExposureLabel;
        private System.Windows.Forms.Button btnDarkMode;
        private System.Windows.Forms.Label lblGainLabel;
        private System.Windows.Forms.NumericUpDown numGain;
        private System.Windows.Forms.Label lblGain;
        private System.Windows.Forms.Label lblBlacklevelLabel;
        private System.Windows.Forms.NumericUpDown numBlacklevel;
        private System.Windows.Forms.Label lblBlacklevel;
        private System.Windows.Forms.CheckBox chkAutoMode;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.Label lblErrorCount;
        private System.Windows.Forms.Label lblFrameCount;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.GroupBox grpAnalysis;
        private System.Windows.Forms.Label lblAnalysisResult;
        private System.Windows.Forms.CheckBox chkShowOverlay;
        private System.Windows.Forms.ComboBox cmbBeamType;
        private System.Windows.Forms.Label lblBeamType;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.GroupBox grpModel;
        private System.Windows.Forms.ComboBox cmbModel;
        private System.Windows.Forms.Label lblModelLabel;
        private System.Windows.Forms.ComboBox cmbHeadlampSide;
        private System.Windows.Forms.Label lblSideLabel;
        private System.Windows.Forms.GroupBox grpJudgment;
        private System.Windows.Forms.Label lblJudgmentResult;
        private System.Windows.Forms.Label lblHorizontalLabel;
        private System.Windows.Forms.Label lblHorizontalValue;
        private System.Windows.Forms.Label lblVerticalLabel;
        private System.Windows.Forms.Label lblVerticalValue;
        private System.Windows.Forms.Label lblCandelaLabel;
        private System.Windows.Forms.Label lblCandelaValue;
        private System.Windows.Forms.GroupBox grpMode;
        private System.Windows.Forms.RadioButton rbModeAlignment;
        private System.Windows.Forms.RadioButton rbModeMeasurement;
        private System.Windows.Forms.Label lblAlignStatus;
        private System.Windows.Forms.Button btnCalibration;
    }
}
