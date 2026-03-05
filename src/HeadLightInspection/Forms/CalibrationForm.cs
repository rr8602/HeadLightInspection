using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HeadLightInspection.ImageProcessing.Helpers;
using HeadLightInspection.ImageProcessing.Models;

namespace HeadLightInspection.Forms
{
    /// <summary>
    /// 교정 폼 - High Beam / Low Beam 탭으로 구성
    /// </summary>
    public partial class CalibrationForm : Form
    {
        private CalibrationData _calibrationData = null!;
        private readonly string _iniFilePath;

        // 현재 측정값 (외부에서 업데이트)
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point CurrentHotPoint { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentPixelValue { get; set; }

        // 탭 컨트롤
        private TabControl tabMain = null!;
        private TabPage tabHighBeam = null!;
        private TabPage tabLowBeam = null!;

        // High Beam CD 교정 버튼들
        private Button[] _highCdButtons = null!;
        private readonly int[] _highCdValues = { 10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000, 100000 };
        private int[] _highCdStatus = null!; // 0=미사용, 1=기본, 2=변경됨

        // High Beam 각도 교정 버튼들
        private Button btnHighAngleZero = null!;
        private Button btnHighAngleL1 = null!, btnHighAngleL2 = null!;
        private Button btnHighAngleR1 = null!, btnHighAngleR2 = null!;
        private Button btnHighAngleU1 = null!, btnHighAngleU2 = null!;
        private Button btnHighAngleD1 = null!, btnHighAngleD2 = null!;

        // Low Beam CD 교정 버튼들
        private Button[] _lowCdButtons = null!;
        private readonly int[] _lowCdValues = { 2000, 5000, 8000, 10000, 15000, 20000, 25000, 30000, 35000, 40000 };
        private int[] _lowCdStatus = null!;

        // Low Beam 각도 교정 버튼들
        private Button btnLowAngleZero = null!;
        private Button btnLowAngleL1 = null!, btnLowAngleL2 = null!;
        private Button btnLowAngleR1 = null!, btnLowAngleR2 = null!;
        private Button btnLowAngleU1 = null!, btnLowAngleU2 = null!;
        private Button btnLowAngleD1 = null!, btnLowAngleD2 = null!;

        // 측정값 표시 라벨
        private Label lblHighCurrentX = null!, lblHighCurrentY = null!, lblHighCurrentCd = null!;
        private Label lblLowCurrentX = null!, lblLowCurrentY = null!, lblLowCurrentCd = null!;

        // 갱신 타이머
        private System.Windows.Forms.Timer _updateTimer = null!;

        public CalibrationForm(string iniFilePath)
        {
            _iniFilePath = iniFilePath;
            _highCdStatus = new int[10];
            _lowCdStatus = new int[10];

            InitializeComponent();
            LoadCalibrationData();
        }

        private void InitializeComponent()
        {
            this.Text = "교정 (Calibration)";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 탭 컨트롤
            tabMain = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(870, 640),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // High Beam 탭
            tabHighBeam = new TabPage("상향등 교정 (High Beam)");
            CreateHighBeamTab();
            tabMain.TabPages.Add(tabHighBeam);

            // Low Beam 탭
            tabLowBeam = new TabPage("하향등 교정 (Low Beam)");
            CreateLowBeamTab();
            tabMain.TabPages.Add(tabLowBeam);

            this.Controls.Add(tabMain);

            // 타이머 설정
            _updateTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void CreateHighBeamTab()
        {
            // === CD 교정 섹션 ===
            var grpCd = new GroupBox
            {
                Text = "광도 교정 (CD Calibration)",
                Location = new Point(10, 10),
                Size = new Size(420, 350)
            };

            // Zero 버튼
            var btnHighCdZero = new Button
            {
                Text = "ZERO\n설정",
                Location = new Point(10, 25),
                Size = new Size(80, 50),
                BackColor = Color.LightBlue
            };
            btnHighCdZero.Click += (s, e) => SetHighCdZero();
            grpCd.Controls.Add(btnHighCdZero);

            // CD 교정 버튼들
            _highCdButtons = new Button[10];
            for (int i = 0; i < 10; i++)
            {
                int row = i / 5;
                int col = i % 5;
                _highCdButtons[i] = new Button
                {
                    Text = $"{_highCdValues[i]:N0}\ncd",
                    Location = new Point(10 + col * 82, 85 + row * 60),
                    Size = new Size(78, 55),
                    Tag = i,
                    BackColor = Color.LightGray
                };
                int index = i;
                _highCdButtons[i].Click += (s, e) => ClickHighCdButton(index);
                grpCd.Controls.Add(_highCdButtons[i]);
            }

            // CD 저장/초기화 버튼
            var btnHighCdSave = new Button
            {
                Text = "CD 저장",
                Location = new Point(10, 210),
                Size = new Size(100, 35),
                BackColor = Color.LightGreen
            };
            btnHighCdSave.Click += (s, e) => SaveHighCd();
            grpCd.Controls.Add(btnHighCdSave);

            var btnHighCdInit = new Button
            {
                Text = "CD 초기화",
                Location = new Point(120, 210),
                Size = new Size(100, 35),
                BackColor = Color.LightCoral
            };
            btnHighCdInit.Click += (s, e) => InitHighCd();
            grpCd.Controls.Add(btnHighCdInit);

            // 현재 측정값 표시
            var lblCurrentTitle = new Label
            {
                Text = "현재 측정값:",
                Location = new Point(10, 260),
                Size = new Size(100, 20),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold)
            };
            grpCd.Controls.Add(lblCurrentTitle);

            lblHighCurrentCd = new Label
            {
                Text = "PixelValue: ---",
                Location = new Point(10, 285),
                Size = new Size(200, 25),
                Font = new Font("맑은 고딕", 12, FontStyle.Bold),
                ForeColor = Color.Blue
            };
            grpCd.Controls.Add(lblHighCurrentCd);

            lblHighCurrentX = new Label
            {
                Text = "X: ---",
                Location = new Point(220, 285),
                Size = new Size(90, 25),
                Font = new Font("맑은 고딕", 10)
            };
            grpCd.Controls.Add(lblHighCurrentX);

            lblHighCurrentY = new Label
            {
                Text = "Y: ---",
                Location = new Point(320, 285),
                Size = new Size(90, 25),
                Font = new Font("맑은 고딕", 10)
            };
            grpCd.Controls.Add(lblHighCurrentY);

            tabHighBeam.Controls.Add(grpCd);

            // === 각도 교정 섹션 ===
            var grpAngle = new GroupBox
            {
                Text = "각도 교정 (Angle Calibration)",
                Location = new Point(440, 10),
                Size = new Size(410, 350)
            };

            // Zero 버튼 (중앙)
            btnHighAngleZero = new Button
            {
                Text = "ZERO",
                Location = new Point(165, 120),
                Size = new Size(80, 50),
                BackColor = Color.Yellow
            };
            btnHighAngleZero.Click += (s, e) => SetHighAngleZero();
            grpAngle.Controls.Add(btnHighAngleZero);

            // 좌측 버튼들
            btnHighAngleL1 = CreateAngleButton("LEFT1", new Point(85, 120), () => SetHighAngle("L1"));
            btnHighAngleL2 = CreateAngleButton("LEFT2", new Point(5, 120), () => SetHighAngle("L2"));
            grpAngle.Controls.Add(btnHighAngleL1);
            grpAngle.Controls.Add(btnHighAngleL2);

            // 우측 버튼들
            btnHighAngleR1 = CreateAngleButton("RIGHT1", new Point(245, 120), () => SetHighAngle("R1"));
            btnHighAngleR2 = CreateAngleButton("RIGHT2", new Point(325, 120), () => SetHighAngle("R2"));
            grpAngle.Controls.Add(btnHighAngleR1);
            grpAngle.Controls.Add(btnHighAngleR2);

            // 상측 버튼들
            btnHighAngleU1 = CreateAngleButton("UP1", new Point(165, 65), () => SetHighAngle("U1"));
            btnHighAngleU2 = CreateAngleButton("UP2", new Point(165, 15), () => SetHighAngle("U2"));
            grpAngle.Controls.Add(btnHighAngleU1);
            grpAngle.Controls.Add(btnHighAngleU2);

            // 하측 버튼들
            btnHighAngleD1 = CreateAngleButton("DOWN1", new Point(165, 175), () => SetHighAngle("D1"));
            btnHighAngleD2 = CreateAngleButton("DOWN2", new Point(165, 230), () => SetHighAngle("D2"));
            grpAngle.Controls.Add(btnHighAngleD1);
            grpAngle.Controls.Add(btnHighAngleD2);

            // 각도 저장/초기화 버튼
            var btnHighAngleSave = new Button
            {
                Text = "각도 저장",
                Location = new Point(10, 295),
                Size = new Size(100, 35),
                BackColor = Color.LightGreen
            };
            btnHighAngleSave.Click += (s, e) => SaveHighAngle();
            grpAngle.Controls.Add(btnHighAngleSave);

            var btnHighAngleInit = new Button
            {
                Text = "각도 초기화",
                Location = new Point(120, 295),
                Size = new Size(100, 35),
                BackColor = Color.LightCoral
            };
            btnHighAngleInit.Click += (s, e) => InitHighAngle();
            grpAngle.Controls.Add(btnHighAngleInit);

            tabHighBeam.Controls.Add(grpAngle);

            // === 교정 데이터 표시 섹션 ===
            var grpData = new GroupBox
            {
                Text = "현재 교정 데이터",
                Location = new Point(10, 370),
                Size = new Size(840, 220)
            };

            var lblHighDataInfo = new Label
            {
                Text = "교정 데이터가 로드되면 여기에 표시됩니다.",
                Location = new Point(10, 25),
                Size = new Size(820, 180),
                Font = new Font("Consolas", 9)
            };
            lblHighDataInfo.Name = "lblHighDataInfo";
            grpData.Controls.Add(lblHighDataInfo);

            tabHighBeam.Controls.Add(grpData);
        }

        private void CreateLowBeamTab()
        {
            // === CD 교정 섹션 ===
            var grpCd = new GroupBox
            {
                Text = "광도 교정 (CD Calibration)",
                Location = new Point(10, 10),
                Size = new Size(420, 350)
            };

            // Zero 버튼
            var btnLowCdZero = new Button
            {
                Text = "ZERO\n설정",
                Location = new Point(10, 25),
                Size = new Size(80, 50),
                BackColor = Color.LightBlue
            };
            btnLowCdZero.Click += (s, e) => SetLowCdZero();
            grpCd.Controls.Add(btnLowCdZero);

            // CD 교정 버튼들
            _lowCdButtons = new Button[10];
            for (int i = 0; i < 10; i++)
            {
                int row = i / 5;
                int col = i % 5;
                _lowCdButtons[i] = new Button
                {
                    Text = $"{_lowCdValues[i]:N0}\ncd",
                    Location = new Point(10 + col * 82, 85 + row * 60),
                    Size = new Size(78, 55),
                    Tag = i,
                    BackColor = Color.LightGray
                };
                int index = i;
                _lowCdButtons[i].Click += (s, e) => ClickLowCdButton(index);
                grpCd.Controls.Add(_lowCdButtons[i]);
            }

            // CD 저장/초기화 버튼
            var btnLowCdSave = new Button
            {
                Text = "CD 저장",
                Location = new Point(10, 210),
                Size = new Size(100, 35),
                BackColor = Color.LightGreen
            };
            btnLowCdSave.Click += (s, e) => SaveLowCd();
            grpCd.Controls.Add(btnLowCdSave);

            var btnLowCdInit = new Button
            {
                Text = "CD 초기화",
                Location = new Point(120, 210),
                Size = new Size(100, 35),
                BackColor = Color.LightCoral
            };
            btnLowCdInit.Click += (s, e) => InitLowCd();
            grpCd.Controls.Add(btnLowCdInit);

            // 현재 측정값 표시
            var lblCurrentTitle = new Label
            {
                Text = "현재 측정값:",
                Location = new Point(10, 260),
                Size = new Size(100, 20),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold)
            };
            grpCd.Controls.Add(lblCurrentTitle);

            lblLowCurrentCd = new Label
            {
                Text = "PixelValue: ---",
                Location = new Point(10, 285),
                Size = new Size(200, 25),
                Font = new Font("맑은 고딕", 12, FontStyle.Bold),
                ForeColor = Color.Blue
            };
            grpCd.Controls.Add(lblLowCurrentCd);

            lblLowCurrentX = new Label
            {
                Text = "X: ---",
                Location = new Point(220, 285),
                Size = new Size(90, 25),
                Font = new Font("맑은 고딕", 10)
            };
            grpCd.Controls.Add(lblLowCurrentX);

            lblLowCurrentY = new Label
            {
                Text = "Y: ---",
                Location = new Point(320, 285),
                Size = new Size(90, 25),
                Font = new Font("맑은 고딕", 10)
            };
            grpCd.Controls.Add(lblLowCurrentY);

            tabLowBeam.Controls.Add(grpCd);

            // === 각도 교정 섹션 ===
            var grpAngle = new GroupBox
            {
                Text = "각도 교정 (Angle Calibration)",
                Location = new Point(440, 10),
                Size = new Size(410, 350)
            };

            // Zero 버튼 (중앙)
            btnLowAngleZero = new Button
            {
                Text = "ZERO",
                Location = new Point(165, 120),
                Size = new Size(80, 50),
                BackColor = Color.Yellow
            };
            btnLowAngleZero.Click += (s, e) => SetLowAngleZero();
            grpAngle.Controls.Add(btnLowAngleZero);

            // 좌측 버튼들
            btnLowAngleL1 = CreateAngleButton("LEFT1", new Point(85, 120), () => SetLowAngle("L1"));
            btnLowAngleL2 = CreateAngleButton("LEFT2", new Point(5, 120), () => SetLowAngle("L2"));
            grpAngle.Controls.Add(btnLowAngleL1);
            grpAngle.Controls.Add(btnLowAngleL2);

            // 우측 버튼들
            btnLowAngleR1 = CreateAngleButton("RIGHT1", new Point(245, 120), () => SetLowAngle("R1"));
            btnLowAngleR2 = CreateAngleButton("RIGHT2", new Point(325, 120), () => SetLowAngle("R2"));
            grpAngle.Controls.Add(btnLowAngleR1);
            grpAngle.Controls.Add(btnLowAngleR2);

            // 상측 버튼들
            btnLowAngleU1 = CreateAngleButton("UP1", new Point(165, 65), () => SetLowAngle("U1"));
            btnLowAngleU2 = CreateAngleButton("UP2", new Point(165, 15), () => SetLowAngle("U2"));
            grpAngle.Controls.Add(btnLowAngleU1);
            grpAngle.Controls.Add(btnLowAngleU2);

            // 하측 버튼들
            btnLowAngleD1 = CreateAngleButton("DOWN1", new Point(165, 175), () => SetLowAngle("D1"));
            btnLowAngleD2 = CreateAngleButton("DOWN2", new Point(165, 230), () => SetLowAngle("D2"));
            grpAngle.Controls.Add(btnLowAngleD1);
            grpAngle.Controls.Add(btnLowAngleD2);

            // 각도 저장/초기화 버튼
            var btnLowAngleSave = new Button
            {
                Text = "각도 저장",
                Location = new Point(10, 295),
                Size = new Size(100, 35),
                BackColor = Color.LightGreen
            };
            btnLowAngleSave.Click += (s, e) => SaveLowAngle();
            grpAngle.Controls.Add(btnLowAngleSave);

            var btnLowAngleInit = new Button
            {
                Text = "각도 초기화",
                Location = new Point(120, 295),
                Size = new Size(100, 35),
                BackColor = Color.LightCoral
            };
            btnLowAngleInit.Click += (s, e) => InitLowAngle();
            grpAngle.Controls.Add(btnLowAngleInit);

            tabLowBeam.Controls.Add(grpAngle);

            // === 교정 데이터 표시 섹션 ===
            var grpData = new GroupBox
            {
                Text = "현재 교정 데이터",
                Location = new Point(10, 370),
                Size = new Size(840, 220)
            };

            var lblLowDataInfo = new Label
            {
                Text = "교정 데이터가 로드되면 여기에 표시됩니다.",
                Location = new Point(10, 25),
                Size = new Size(820, 180),
                Font = new Font("Consolas", 9)
            };
            lblLowDataInfo.Name = "lblLowDataInfo";
            grpData.Controls.Add(lblLowDataInfo);

            tabLowBeam.Controls.Add(grpData);
        }

        private Button CreateAngleButton(string text, Point location, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(75, 45),
                BackColor = Color.LightSteelBlue
            };
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void LoadCalibrationData()
        {
            try
            {
                _calibrationData = CalibrationData.LoadFromIni(_iniFilePath);
                UpdateButtonStates();
                UpdateDataDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"교정 데이터 로드 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _calibrationData = new CalibrationData();
            }
        }

        private void UpdateButtonStates()
        {
            // High CD 버튼 상태 업데이트
            for (int i = 0; i < 10; i++)
            {
                if (_calibrationData.HighCD[i] != 0)
                {
                    _highCdStatus[i] = 1;
                    _highCdButtons[i].BackColor = Color.LightSteelBlue;
                }
                else
                {
                    _highCdStatus[i] = 0;
                    _highCdButtons[i].BackColor = Color.LightGray;
                }
            }

            // Low CD 버튼 상태 업데이트
            for (int i = 0; i < 10; i++)
            {
                if (_calibrationData.LowCD[i] != 0)
                {
                    _lowCdStatus[i] = 1;
                    _lowCdButtons[i].BackColor = Color.LightSteelBlue;
                }
                else
                {
                    _lowCdStatus[i] = 0;
                    _lowCdButtons[i].BackColor = Color.LightGray;
                }
            }
        }

        private void UpdateDataDisplay()
        {
            // High Beam 데이터 표시
            var lblHighData = tabHighBeam.Controls.Find("lblHighDataInfo", true);
            if (lblHighData.Length > 0)
            {
                var data = _calibrationData;
                lblHighData[0].Text =
                    $"=== CD 교정 ===\n" +
                    $"ZERO: {data.HighCDZero}\n" +
                    $"10K={data.HighCD[0]}, 20K={data.HighCD[1]}, 30K={data.HighCD[2]}, 40K={data.HighCD[3]}, 50K={data.HighCD[4]}\n" +
                    $"60K={data.HighCD[5]}, 70K={data.HighCD[6]}, 80K={data.HighCD[7]}, 90K={data.HighCD[8]}, 100K={data.HighCD[9]}\n\n" +
                    $"=== 각도 교정 ===\n" +
                    $"ZERO: ({data.HighZeroX}, {data.HighZeroY})\n" +
                    $"LEFT1: ({data.HighAngleL1X}, {data.HighAngleL1Y})  LEFT2: ({data.HighAngleL2X}, {data.HighAngleL2Y})\n" +
                    $"RIGHT1: ({data.HighAngleR1X}, {data.HighAngleR1Y})  RIGHT2: ({data.HighAngleR2X}, {data.HighAngleR2Y})\n" +
                    $"UP1: ({data.HighAngleU1X}, {data.HighAngleU1Y})  UP2: ({data.HighAngleU2X}, {data.HighAngleU2Y})\n" +
                    $"DOWN1: ({data.HighAngleD1X}, {data.HighAngleD1Y})  DOWN2: ({data.HighAngleD2X}, {data.HighAngleD2Y})";
            }

            // Low Beam 데이터 표시
            var lblLowData = tabLowBeam.Controls.Find("lblLowDataInfo", true);
            if (lblLowData.Length > 0)
            {
                var data = _calibrationData;
                lblLowData[0].Text =
                    $"=== CD 교정 ===\n" +
                    $"ZERO: {data.LowCDZero}\n" +
                    $"2K={data.LowCD[0]}, 5K={data.LowCD[1]}, 8K={data.LowCD[2]}, 10K={data.LowCD[3]}, 15K={data.LowCD[4]}\n" +
                    $"20K={data.LowCD[5]}, 25K={data.LowCD[6]}, 30K={data.LowCD[7]}, 35K={data.LowCD[8]}, 40K={data.LowCD[9]}\n\n" +
                    $"=== 각도 교정 ===\n" +
                    $"ZERO: ({data.LowZeroX}, {data.LowZeroY})\n" +
                    $"LEFT1: ({data.LowAngleL1X}, {data.LowAngleL1Y})  LEFT2: ({data.LowAngleL2X}, {data.LowAngleL2Y})\n" +
                    $"RIGHT1: ({data.LowAngleR1X}, {data.LowAngleR1Y})  RIGHT2: ({data.LowAngleR2X}, {data.LowAngleR2Y})\n" +
                    $"UP1: ({data.LowAngleU1X}, {data.LowAngleU1Y})  UP2: ({data.LowAngleU2X}, {data.LowAngleU2Y})\n" +
                    $"DOWN1: ({data.LowAngleD1X}, {data.LowAngleD1Y})  DOWN2: ({data.LowAngleD2X}, {data.LowAngleD2Y})";
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // 현재 측정값 업데이트
            if (lblHighCurrentCd != null)
            {
                lblHighCurrentCd.Text = $"PixelValue: {CurrentPixelValue}";
                lblHighCurrentX.Text = $"X: {CurrentHotPoint.X}";
                lblHighCurrentY.Text = $"Y: {CurrentHotPoint.Y}";
            }

            if (lblLowCurrentCd != null)
            {
                lblLowCurrentCd.Text = $"PixelValue: {CurrentPixelValue}";
                lblLowCurrentX.Text = $"X: {CurrentHotPoint.X}";
                lblLowCurrentY.Text = $"Y: {CurrentHotPoint.Y}";
            }
        }

        #region High Beam CD Methods

        private void SetHighCdZero()
        {
            if (MessageBox.Show("현재 픽셀값을 ZERO로 설정하시겠습니까?", "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _calibrationData.HighCDZero = CurrentPixelValue;
                UpdateDataDisplay();
            }
        }

        private void ClickHighCdButton(int index)
        {
            // 상태 순환: 0(미사용) -> 1(기본) -> 2(변경) -> 0
            if (_highCdStatus[index] == 0)
            {
                // 미사용 -> 파일에서 로드
                _highCdStatus[index] = 1;
                var tempData = CalibrationData.LoadFromIni(_iniFilePath);
                _calibrationData.HighCD[index] = tempData.HighCD[index];
                _highCdButtons[index].BackColor = Color.LightSteelBlue;
            }
            else if (_highCdStatus[index] == 1)
            {
                // 기본 -> 현재값으로 변경
                _highCdStatus[index] = 2;
                _calibrationData.HighCD[index] = CurrentPixelValue;
                _highCdButtons[index].BackColor = Color.LightGreen;
            }
            else
            {
                // 변경 -> 미사용
                _highCdStatus[index] = 0;
                _calibrationData.HighCD[index] = 0;
                _highCdButtons[index].BackColor = Color.LightGray;
            }

            UpdateDataDisplay();
        }

        private void SaveHighCd()
        {
            if (MessageBox.Show("High Beam CD 교정 데이터를 저장하시겠습니까?", "저장 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IniFileHelper.WriteInt("HIGH_CD", "ZERO", _calibrationData.HighCDZero, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "10000", _calibrationData.HighCD[0], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "20000", _calibrationData.HighCD[1], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "30000", _calibrationData.HighCD[2], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "40000", _calibrationData.HighCD[3], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "50000", _calibrationData.HighCD[4], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "60000", _calibrationData.HighCD[5], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "70000", _calibrationData.HighCD[6], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "80000", _calibrationData.HighCD[7], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "90000", _calibrationData.HighCD[8], _iniFilePath);
                IniFileHelper.WriteInt("HIGH_CD", "100000", _calibrationData.HighCD[9], _iniFilePath);

                MessageBox.Show("저장되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateButtonStates();
            }
        }

        private void InitHighCd()
        {
            if (MessageBox.Show("High Beam CD 교정 데이터를 파일에서 다시 로드하시겠습니까?", "초기화 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var tempData = CalibrationData.LoadFromIni(_iniFilePath);
                _calibrationData.HighCDZero = tempData.HighCDZero;
                for (int i = 0; i < 10; i++)
                {
                    _calibrationData.HighCD[i] = tempData.HighCD[i];
                }
                UpdateButtonStates();
                UpdateDataDisplay();
            }
        }

        #endregion

        #region High Beam Angle Methods

        private void SetHighAngleZero()
        {
            if (MessageBox.Show("현재 핫포인트를 ZERO로 설정하시겠습니까?\n(다른 각도 위치도 자동 계산됩니다)", "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int gap = 36; // 기본 간격

                _calibrationData.HighZeroX = CurrentHotPoint.X;
                _calibrationData.HighZeroY = CurrentHotPoint.Y;

                // 자동 각도 계산
                _calibrationData.HighAngleL1X = CurrentHotPoint.X - gap;
                _calibrationData.HighAngleL2X = CurrentHotPoint.X - gap * 2;
                _calibrationData.HighAngleR1X = CurrentHotPoint.X + gap;
                _calibrationData.HighAngleR2X = CurrentHotPoint.X + gap * 2;

                _calibrationData.HighAngleU1Y = CurrentHotPoint.Y - gap;
                _calibrationData.HighAngleU2Y = CurrentHotPoint.Y - gap * 2;
                _calibrationData.HighAngleD1Y = CurrentHotPoint.Y + gap;
                _calibrationData.HighAngleD2Y = CurrentHotPoint.Y + gap * 2;

                UpdateDataDisplay();
            }
        }

        private void SetHighAngle(string position)
        {
            switch (position)
            {
                case "L1":
                    _calibrationData.HighAngleL1X = CurrentHotPoint.X;
                    _calibrationData.HighAngleL1Y = CurrentHotPoint.Y;
                    break;
                case "L2":
                    _calibrationData.HighAngleL2X = CurrentHotPoint.X;
                    _calibrationData.HighAngleL2Y = CurrentHotPoint.Y;
                    break;
                case "R1":
                    _calibrationData.HighAngleR1X = CurrentHotPoint.X;
                    _calibrationData.HighAngleR1Y = CurrentHotPoint.Y;
                    break;
                case "R2":
                    _calibrationData.HighAngleR2X = CurrentHotPoint.X;
                    _calibrationData.HighAngleR2Y = CurrentHotPoint.Y;
                    break;
                case "U1":
                    _calibrationData.HighAngleU1X = CurrentHotPoint.X;
                    _calibrationData.HighAngleU1Y = CurrentHotPoint.Y;
                    break;
                case "U2":
                    _calibrationData.HighAngleU2X = CurrentHotPoint.X;
                    _calibrationData.HighAngleU2Y = CurrentHotPoint.Y;
                    break;
                case "D1":
                    _calibrationData.HighAngleD1X = CurrentHotPoint.X;
                    _calibrationData.HighAngleD1Y = CurrentHotPoint.Y;
                    break;
                case "D2":
                    _calibrationData.HighAngleD2X = CurrentHotPoint.X;
                    _calibrationData.HighAngleD2Y = CurrentHotPoint.Y;
                    break;
            }
            UpdateDataDisplay();
        }

        private void SaveHighAngle()
        {
            if (MessageBox.Show("High Beam 각도 교정 데이터를 저장하시겠습니까?", "저장 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IniFileHelper.WriteInt("HIGH_ANGLE", "ZEROX", _calibrationData.HighZeroX, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "ZEROY", _calibrationData.HighZeroY, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT1_X", _calibrationData.HighAngleL1X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT1_Y", _calibrationData.HighAngleL1Y, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT2_X", _calibrationData.HighAngleL2X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT2_Y", _calibrationData.HighAngleL2Y, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT1_X", _calibrationData.HighAngleR1X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT1_Y", _calibrationData.HighAngleR1Y, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT2_X", _calibrationData.HighAngleR2X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT2_Y", _calibrationData.HighAngleR2Y, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "UP1_X", _calibrationData.HighAngleU1X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "UP1_Y", _calibrationData.HighAngleU1Y, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "UP2_X", _calibrationData.HighAngleU2X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "UP2_Y", _calibrationData.HighAngleU2Y, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN1_X", _calibrationData.HighAngleD1X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN1_Y", _calibrationData.HighAngleD1Y, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN2_X", _calibrationData.HighAngleD2X, _iniFilePath);
                IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN2_Y", _calibrationData.HighAngleD2Y, _iniFilePath);

                MessageBox.Show("저장되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitHighAngle()
        {
            if (MessageBox.Show("High Beam 각도 교정 데이터를 파일에서 다시 로드하시겠습니까?", "초기화 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var tempData = CalibrationData.LoadFromIni(_iniFilePath);
                _calibrationData.HighZeroX = tempData.HighZeroX;
                _calibrationData.HighZeroY = tempData.HighZeroY;
                _calibrationData.HighAngleL1X = tempData.HighAngleL1X;
                _calibrationData.HighAngleL1Y = tempData.HighAngleL1Y;
                _calibrationData.HighAngleL2X = tempData.HighAngleL2X;
                _calibrationData.HighAngleL2Y = tempData.HighAngleL2Y;
                _calibrationData.HighAngleR1X = tempData.HighAngleR1X;
                _calibrationData.HighAngleR1Y = tempData.HighAngleR1Y;
                _calibrationData.HighAngleR2X = tempData.HighAngleR2X;
                _calibrationData.HighAngleR2Y = tempData.HighAngleR2Y;
                _calibrationData.HighAngleU1X = tempData.HighAngleU1X;
                _calibrationData.HighAngleU1Y = tempData.HighAngleU1Y;
                _calibrationData.HighAngleU2X = tempData.HighAngleU2X;
                _calibrationData.HighAngleU2Y = tempData.HighAngleU2Y;
                _calibrationData.HighAngleD1X = tempData.HighAngleD1X;
                _calibrationData.HighAngleD1Y = tempData.HighAngleD1Y;
                _calibrationData.HighAngleD2X = tempData.HighAngleD2X;
                _calibrationData.HighAngleD2Y = tempData.HighAngleD2Y;
                UpdateDataDisplay();
            }
        }

        #endregion

        #region Low Beam CD Methods

        private void SetLowCdZero()
        {
            if (MessageBox.Show("현재 픽셀값을 ZERO로 설정하시겠습니까?", "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _calibrationData.LowCDZero = CurrentPixelValue;
                UpdateDataDisplay();
            }
        }

        private void ClickLowCdButton(int index)
        {
            if (_lowCdStatus[index] == 0)
            {
                _lowCdStatus[index] = 1;
                var tempData = CalibrationData.LoadFromIni(_iniFilePath);
                _calibrationData.LowCD[index] = tempData.LowCD[index];
                _lowCdButtons[index].BackColor = Color.LightSteelBlue;
            }
            else if (_lowCdStatus[index] == 1)
            {
                _lowCdStatus[index] = 2;
                _calibrationData.LowCD[index] = CurrentPixelValue;
                _lowCdButtons[index].BackColor = Color.LightGreen;
            }
            else
            {
                _lowCdStatus[index] = 0;
                _calibrationData.LowCD[index] = 0;
                _lowCdButtons[index].BackColor = Color.LightGray;
            }

            UpdateDataDisplay();
        }

        private void SaveLowCd()
        {
            if (MessageBox.Show("Low Beam CD 교정 데이터를 저장하시겠습니까?", "저장 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IniFileHelper.WriteInt("LOW_CD", "ZERO", _calibrationData.LowCDZero, _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "2000", _calibrationData.LowCD[0], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "5000", _calibrationData.LowCD[1], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "8000", _calibrationData.LowCD[2], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "10000", _calibrationData.LowCD[3], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "15000", _calibrationData.LowCD[4], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "20000", _calibrationData.LowCD[5], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "25000", _calibrationData.LowCD[6], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "30000", _calibrationData.LowCD[7], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "35000", _calibrationData.LowCD[8], _iniFilePath);
                IniFileHelper.WriteInt("LOW_CD", "40000", _calibrationData.LowCD[9], _iniFilePath);

                MessageBox.Show("저장되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateButtonStates();
            }
        }

        private void InitLowCd()
        {
            if (MessageBox.Show("Low Beam CD 교정 데이터를 파일에서 다시 로드하시겠습니까?", "초기화 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var tempData = CalibrationData.LoadFromIni(_iniFilePath);
                _calibrationData.LowCDZero = tempData.LowCDZero;
                for (int i = 0; i < 10; i++)
                {
                    _calibrationData.LowCD[i] = tempData.LowCD[i];
                }
                UpdateButtonStates();
                UpdateDataDisplay();
            }
        }

        #endregion

        #region Low Beam Angle Methods

        private void SetLowAngleZero()
        {
            if (MessageBox.Show("현재 핫포인트를 ZERO로 설정하시겠습니까?\n(다른 각도 위치도 자동 계산됩니다)", "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int gap = 40; // Low beam 기본 간격

                _calibrationData.LowZeroX = CurrentHotPoint.X;
                _calibrationData.LowZeroY = CurrentHotPoint.Y;

                _calibrationData.LowAngleL1X = CurrentHotPoint.X - gap;
                _calibrationData.LowAngleL2X = CurrentHotPoint.X - gap * 2;
                _calibrationData.LowAngleR1X = CurrentHotPoint.X + gap;
                _calibrationData.LowAngleR2X = CurrentHotPoint.X + gap * 2;

                _calibrationData.LowAngleU1Y = CurrentHotPoint.Y - gap;
                _calibrationData.LowAngleU2Y = CurrentHotPoint.Y - gap * 2;
                _calibrationData.LowAngleD1Y = CurrentHotPoint.Y + gap;
                _calibrationData.LowAngleD2Y = CurrentHotPoint.Y + gap * 2;

                UpdateDataDisplay();
            }
        }

        private void SetLowAngle(string position)
        {
            switch (position)
            {
                case "L1":
                    _calibrationData.LowAngleL1X = CurrentHotPoint.X;
                    _calibrationData.LowAngleL1Y = CurrentHotPoint.Y;
                    break;
                case "L2":
                    _calibrationData.LowAngleL2X = CurrentHotPoint.X;
                    _calibrationData.LowAngleL2Y = CurrentHotPoint.Y;
                    break;
                case "R1":
                    _calibrationData.LowAngleR1X = CurrentHotPoint.X;
                    _calibrationData.LowAngleR1Y = CurrentHotPoint.Y;
                    break;
                case "R2":
                    _calibrationData.LowAngleR2X = CurrentHotPoint.X;
                    _calibrationData.LowAngleR2Y = CurrentHotPoint.Y;
                    break;
                case "U1":
                    _calibrationData.LowAngleU1X = CurrentHotPoint.X;
                    _calibrationData.LowAngleU1Y = CurrentHotPoint.Y;
                    break;
                case "U2":
                    _calibrationData.LowAngleU2X = CurrentHotPoint.X;
                    _calibrationData.LowAngleU2Y = CurrentHotPoint.Y;
                    break;
                case "D1":
                    _calibrationData.LowAngleD1X = CurrentHotPoint.X;
                    _calibrationData.LowAngleD1Y = CurrentHotPoint.Y;
                    break;
                case "D2":
                    _calibrationData.LowAngleD2X = CurrentHotPoint.X;
                    _calibrationData.LowAngleD2Y = CurrentHotPoint.Y;
                    break;
            }
            UpdateDataDisplay();
        }

        private void SaveLowAngle()
        {
            if (MessageBox.Show("Low Beam 각도 교정 데이터를 저장하시겠습니까?", "저장 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                IniFileHelper.WriteInt("LOW_ANGLE", "ZEROX", _calibrationData.LowZeroX, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "ZEROY", _calibrationData.LowZeroY, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "LEFT1_X", _calibrationData.LowAngleL1X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "LEFT1_Y", _calibrationData.LowAngleL1Y, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "LEFT2_X", _calibrationData.LowAngleL2X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "LEFT2_Y", _calibrationData.LowAngleL2Y, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT1_X", _calibrationData.LowAngleR1X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT1_Y", _calibrationData.LowAngleR1Y, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT2_X", _calibrationData.LowAngleR2X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT2_Y", _calibrationData.LowAngleR2Y, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "UP1_X", _calibrationData.LowAngleU1X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "UP1_Y", _calibrationData.LowAngleU1Y, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "UP2_X", _calibrationData.LowAngleU2X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "UP2_Y", _calibrationData.LowAngleU2Y, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "DOWN1_X", _calibrationData.LowAngleD1X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "DOWN1_Y", _calibrationData.LowAngleD1Y, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "DOWN2_X", _calibrationData.LowAngleD2X, _iniFilePath);
                IniFileHelper.WriteInt("LOW_ANGLE", "DOWN2_Y", _calibrationData.LowAngleD2Y, _iniFilePath);

                MessageBox.Show("저장되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitLowAngle()
        {
            if (MessageBox.Show("Low Beam 각도 교정 데이터를 파일에서 다시 로드하시겠습니까?", "초기화 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var tempData = CalibrationData.LoadFromIni(_iniFilePath);
                _calibrationData.LowZeroX = tempData.LowZeroX;
                _calibrationData.LowZeroY = tempData.LowZeroY;
                _calibrationData.LowAngleL1X = tempData.LowAngleL1X;
                _calibrationData.LowAngleL1Y = tempData.LowAngleL1Y;
                _calibrationData.LowAngleL2X = tempData.LowAngleL2X;
                _calibrationData.LowAngleL2Y = tempData.LowAngleL2Y;
                _calibrationData.LowAngleR1X = tempData.LowAngleR1X;
                _calibrationData.LowAngleR1Y = tempData.LowAngleR1Y;
                _calibrationData.LowAngleR2X = tempData.LowAngleR2X;
                _calibrationData.LowAngleR2Y = tempData.LowAngleR2Y;
                _calibrationData.LowAngleU1X = tempData.LowAngleU1X;
                _calibrationData.LowAngleU1Y = tempData.LowAngleU1Y;
                _calibrationData.LowAngleU2X = tempData.LowAngleU2X;
                _calibrationData.LowAngleU2Y = tempData.LowAngleU2Y;
                _calibrationData.LowAngleD1X = tempData.LowAngleD1X;
                _calibrationData.LowAngleD1Y = tempData.LowAngleD1Y;
                _calibrationData.LowAngleD2X = tempData.LowAngleD2X;
                _calibrationData.LowAngleD2Y = tempData.LowAngleD2Y;
                UpdateDataDisplay();
            }
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// 외부에서 측정값 업데이트
        /// </summary>
        public void UpdateMeasurement(Point hotPoint, int pixelValue)
        {
            CurrentHotPoint = hotPoint;
            CurrentPixelValue = pixelValue;
        }

        /// <summary>
        /// 현재 교정 데이터 반환
        /// </summary>
        public CalibrationData GetCalibrationData()
        {
            return _calibrationData;
        }
    }
}
