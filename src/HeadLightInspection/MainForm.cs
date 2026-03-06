using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using HeadLightInspection.Camera;
using HeadLightInspection.Forms;
using HeadLightInspection.ImageProcessing;
using HeadLightInspection.ImageProcessing.Data;
using HeadLightInspection.ImageProcessing.Models;

namespace HeadLightInspection
{
    public partial class MainForm : Form
    {
        private IDSCamera? _camera;
        private Bitmap? _originalImage;
        private HeadlampAnalyzer? _analyzer;
        private HeadlampAligner? _aligner;
        private AnalysisResult? _lastResult;
        private AlignmentResult? _lastAlignmentResult;
        private readonly object _imageLock = new object();

        // 데이터 관리 및 판정
        private DataManager? _dataManager;
        private JudgmentEngine? _judgmentEngine;
        private ModelParameter? _currentModelParam;
        private StandardData? _currentStandardData;
        private JudgmentResult? _lastJudgmentResult;

        // 정대 상태
        private OpenCvSharp.Point2f _alignedCenter;

        public MainForm()
        {
            InitializeComponent();
            this.FormClosing += MainForm_FormClosing;

            _analyzer = new HeadlampAnalyzer();
            // 필터 파라미터 설정 (윈도우 길이, 거리 임계값)
            _analyzer.SetHotPointFilterParams(windowLen: 5, threshold: 30.0);
            _analyzer.SetCrossPointFilterParams(windowLen: 5, threshold: 30.0);
            _analyzer.SetLineFilterParams(windowLen: 5, threshold: 20.0);

            _aligner = new HeadlampAligner();
            _aligner.SetAvgParameters(5, 30.0);

            cmbBeamType.SelectedIndex = 0;
            cmbCutoffAlgorithm.SelectedIndex = 1; // 기본값: Edge
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 데이터 관리자 초기화 및 로드
            InitializeDataManager();

            UpdateCameraList();
            UpdateModelList();
            UpdateUI();

            // 헤드램프 위치 초기 선택
            cmbHeadlampSide.SelectedIndex = 0;
        }

        private void InitializeDataManager()
        {
            try
            {
                _dataManager = new DataManager();
                _dataManager.LoadAll();

                // 기본 판정 엔진 생성
                _judgmentEngine = new JudgmentEngine(
                    _dataManager.Calibration,
                    _dataManager.GetStandardData("DEFAULT"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 실패: {ex.Message}", "경고",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // 기본값 사용
                _dataManager = new DataManager();
                _judgmentEngine = new JudgmentEngine(
                    CalibrationData.CreateDefault(),
                    StandardData.CreateDefault());
            }
        }

        private void UpdateModelList()
        {
            cmbModel.Items.Clear();

            if (_dataManager != null)
            {
                foreach (var modelName in _dataManager.ModelNames)
                {
                    cmbModel.Items.Add(modelName);
                }

                if (cmbModel.Items.Count > 0)
                {
                    cmbModel.SelectedIndex = 0;
                }
            }
        }

        private void cmbModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dataManager == null || cmbModel.SelectedItem == null) return;

            string modelName = cmbModel.SelectedItem.ToString() ?? "DEFAULT";

            // 모델 파라미터 로드 및 적용
            _currentModelParam = _dataManager.GetModelParameter(modelName);
            _currentStandardData = _dataManager.GetStandardData(modelName);

            // HeadlampAnalyzer에 파라미터 적용
            _analyzer?.ApplyModelParameter(_currentModelParam);

            // JudgmentEngine 업데이트
            _judgmentEngine = new JudgmentEngine(
                _dataManager.Calibration,
                _currentStandardData);

            // 필터 리셋
            _analyzer?.ResetFilters();

            // 판정 결과 초기화
            ResetJudgmentDisplay();
        }

        private void ResetJudgmentDisplay()
        {
            lblJudgmentResult.Text = "---";
            lblJudgmentResult.ForeColor = Color.Gray;
            lblHorizontalValue.Text = "--- '";
            lblHorizontalValue.ForeColor = Color.Black;
            lblVerticalValue.Text = "--- '";
            lblVerticalValue.ForeColor = Color.Black;
            lblCandelaValue.Text = "--- cd";
            lblCandelaValue.ForeColor = Color.Black;
        }

        private void PerformJudgment(AnalysisResult result, BeamType beamType)
        {
            if (_judgmentEngine == null) return;

            // 헤드램프 위치 (좌/우)
            HeadlampSide side = cmbHeadlampSide.SelectedIndex == 0 ?
                HeadlampSide.Left : HeadlampSide.Right;

            // 판정 수행
            if (beamType == BeamType.HighBeam)
            {
                _lastJudgmentResult = _judgmentEngine.JudgeHighBeam(result, side);
            }
            else
            {
                _lastJudgmentResult = _judgmentEngine.JudgeLowBeam(result, side);
            }

            // UI 업데이트
            UpdateJudgmentDisplay(_lastJudgmentResult);
        }

        private void UpdateJudgmentDisplay(JudgmentResult result)
        {
            // 전체 판정 결과
            switch (result.Status)
            {
                case JudgmentStatus.Pass:
                    lblJudgmentResult.Text = "OK";
                    lblJudgmentResult.ForeColor = Color.Green;
                    break;
                case JudgmentStatus.Fail:
                    lblJudgmentResult.Text = "NG";
                    lblJudgmentResult.ForeColor = Color.Red;
                    break;
                case JudgmentStatus.Warning:
                    lblJudgmentResult.Text = "WARNING";
                    lblJudgmentResult.ForeColor = Color.Orange;
                    break;
                default:
                    lblJudgmentResult.Text = "---";
                    lblJudgmentResult.ForeColor = Color.Gray;
                    break;
            }

            // 수평 편차 (분 단위)
            lblHorizontalValue.Text = $"{result.HorizontalDeviation:F1} '";
            lblHorizontalValue.ForeColor = result.HorizontalStatus == JudgmentStatus.Pass ?
                Color.Black : Color.Red;

            // 수직 편차 (분 단위)
            lblVerticalValue.Text = $"{result.VerticalDeviation:F1} '";
            lblVerticalValue.ForeColor = result.VerticalStatus == JudgmentStatus.Pass ?
                Color.Black : Color.Red;

            // 광도
            lblCandelaValue.Text = $"{result.Candela} cd";
            lblCandelaValue.ForeColor = result.CandelaStatus == JudgmentStatus.Pass ?
                Color.Black : Color.Red;
        }

        private void UpdateCameraList()
        {
            cmbCameras.Items.Clear();
            var cameras = IDSCamera.GetAvailableCameras();

            if (cameras.Count == 0)
            {
                cmbCameras.Items.Add("카메라 없음");
                cmbCameras.SelectedIndex = 0;
                cmbCameras.Enabled = false;
            }
            else
            {
                foreach (var cam in cameras)
                {
                    cmbCameras.Items.Add(cam);
                }
                cmbCameras.SelectedIndex = 0;
                cmbCameras.Enabled = true;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_camera == null || !_camera.IsConnected)
            {
                ConnectCamera();
            }
            else
            {
                DisconnectCamera();
            }
        }

        private void ConnectCamera()
        {
            if (cmbCameras.SelectedIndex < 0) return;

            _camera = new IDSCamera();
            _camera.ImageReceived += Camera_ImageReceived;
            _camera.ErrorOccurred += Camera_ErrorOccurred;
            _camera.CounterChanged += Camera_CounterChanged;

            if (_camera.Open(cmbCameras.SelectedIndex))
            {
                lblStatus.Text = $"연결됨 ({_camera.ImageWidth}x{_camera.ImageHeight})";
                lblStatus.ForeColor = Color.Green;

                // 노출 범위 설정
                var (min, max) = _camera.GetExposureRange();
                trkExposure.Minimum = (int)min;
                trkExposure.Maximum = (int)Math.Min(max, 100000); // 최대 100ms
                trkExposure.Value = (int)_camera.GetExposureTime();
                lblExposure.Text = $"{trkExposure.Value} us";

                UpdateUI();
            }
            else
            {
                _camera.Dispose();
                _camera = null;
            }
        }

        private void DisconnectCamera()
        {
            if (_camera != null)
            {
                _camera.Close();
                _camera.Dispose();
                _camera = null;

                lblStatus.Text = "연결 안됨";
                lblStatus.ForeColor = Color.Red;
                UpdateUI();
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (_camera == null) return;

            if (_camera.IsAcquiring)
            {
                _camera.StopAcquisition();
            }
            else
            {
                // 취득 시작 전 필터 리셋
                _analyzer?.ResetFilters();
                _camera.StartAcquisition();
            }

            UpdateUI();
        }

        private void Camera_ImageReceived(object? sender, Bitmap image)
        {
            // UI 스레드에서 PictureBox 업데이트
            if (picAlignment.InvokeRequired)
            {
                picAlignment.Invoke(() => ProcessReceivedImage(image));
            }
            else
            {
                ProcessReceivedImage(image);
            }
        }

        private void ProcessReceivedImage(Bitmap image)
        {
            lock (_imageLock)
            {
                _originalImage?.Dispose();
                _originalImage = new Bitmap(image);
            }

            // 실시간 분석 수행 (오버레이 체크 시)
            if (chkShowOverlay.Checked)
            {
                try
                {
                    // 정대 분석 수행 (항상)
                    ProcessAlignmentImage(image);

                    // 측정 분석 수행 (항상)
                    ProcessMeasurementImage(image);
                }
                catch
                {
                    // 오류 시 원본 이미지 표시
                    UpdateBothPictureBoxes(CopyBitmap(image));
                }
            }
            else
            {
                // 오버레이 없이 원본 이미지만 표시
                UpdateBothPictureBoxes(CopyBitmap(image));
            }

            image.Dispose();
        }

        /// <summary>
        /// 정대 분석 및 정대 탭 업데이트
        /// </summary>
        private void ProcessAlignmentImage(Bitmap image)
        {
            if (_aligner == null) return;

            _aligner.SetImage(image);
            _lastAlignmentResult = _aligner.SearchLampCenter(margin: 40);

            Bitmap alignmentOverlay;
            if (_lastAlignmentResult.IsValid)
            {
                alignmentOverlay = DrawAlignmentOverlay(image, _lastAlignmentResult);
                _alignedCenter = _lastAlignmentResult.BoundaryCenter;

                // 정대 결과 라벨 업데이트
                lblAlignStatus.Text = "상태: 정대 완료";
                lblAlignStatus.ForeColor = Color.Green;
                lblAlignLampCount.Text = $"램프: {_lastAlignmentResult.LampCount}개";

                if (_lastAlignmentResult.LeftLampCenter.X > 0)
                {
                    lblAlignLeftLamp.Text = $"좌측 램프 (L): ({_lastAlignmentResult.LeftLampCenter.X:F0}, {_lastAlignmentResult.LeftLampCenter.Y:F0})";
                }
                else
                {
                    lblAlignLeftLamp.Text = "좌측 램프 (L): ---";
                }

                if (_lastAlignmentResult.RightLampCenter.X > 0)
                {
                    lblAlignRightLamp.Text = $"우측 램프 (R): ({_lastAlignmentResult.RightLampCenter.X:F0}, {_lastAlignmentResult.RightLampCenter.Y:F0})";
                }
                else
                {
                    lblAlignRightLamp.Text = "우측 램프 (R): ---";
                }

                lblAlignBoundaryCenter.Text = $"경계 중심 (Center): ({_lastAlignmentResult.BoundaryCenter.X:F1}, {_lastAlignmentResult.BoundaryCenter.Y:F1})";
                lblAlignCentroid.Text = $"무게 중심 (Centroid): ({_lastAlignmentResult.Centroid.X:F1}, {_lastAlignmentResult.Centroid.Y:F1})";
            }
            else
            {
                alignmentOverlay = CopyBitmap(image);

                lblAlignStatus.Text = "상태: 램프 없음";
                lblAlignStatus.ForeColor = Color.Gray;
                lblAlignLampCount.Text = "램프: 0개";
                lblAlignLeftLamp.Text = "좌측 램프 (L): ---";
                lblAlignRightLamp.Text = "우측 램프 (R): ---";
                lblAlignBoundaryCenter.Text = "경계 중심 (Center): ---";
                lblAlignCentroid.Text = "무게 중심 (Centroid): ---";
            }

            // 정대 탭 PictureBox 업데이트
            var oldImage = picAlignment.Image;
            picAlignment.Image = alignmentOverlay;
            oldImage?.Dispose();
        }

        /// <summary>
        /// 측정 분석 및 측정 탭 업데이트
        /// </summary>
        private void ProcessMeasurementImage(Bitmap image)
        {
            if (_analyzer == null) return;

            _analyzer.SetImage(image);
            var beamType = cmbBeamType.SelectedIndex == 0 ? BeamType.LowBeam : BeamType.HighBeam;

            // 빔 타입에 따라 다른 분석 메서드 사용
            if (beamType == BeamType.LowBeam)
            {
                // 하향등: 알고리즘 선택 가능
                var lampPosition = cmbHeadlampSide.SelectedIndex == 0 ? LampPosition.Left : LampPosition.Right;
                var algorithm = GetSelectedCutoffAlgorithm();
                _lastResult = _analyzer.AnalyzeLowBeam(lampPosition, algorithm);
            }
            else
            {
                // 상향등: Hot Point만 사용
                _lastResult = _analyzer.AnalyzeHighBeam();
            }

            Bitmap measurementOverlay;
            if (_lastResult.IsValid)
            {
                measurementOverlay = DrawMeasurementOverlay(image, _lastResult);

                // 측정 결과 라벨 업데이트
                lblMeasureHotPoint.Text = $"Hot Point: ({_lastResult.HotPoint.X}, {_lastResult.HotPoint.Y})";
                lblMeasureHotValue.Text = $"Hot Value: {_lastResult.HotPointValue}";

                if (_lastResult.CrossPoints.Count > 0)
                {
                    var cp = _lastResult.CrossPoints[0];
                    lblMeasureCrossPoint.Text = $"Cross Point: ({cp.X:F1}, {cp.Y:F1})";
                }
                else
                {
                    lblMeasureCrossPoint.Text = "Cross Point: ---";
                }

                // 측정점 라벨 업데이트 (알고리즘별)
                var mp = _lastResult.MeasurementPoint;
                string algoName = _lastResult.UsedAlgorithm.ToString();
                string prevInfo = _lastResult.UsedPreviousValue ? " (이전값)" : "";
                lblMeasurementPoint.Text = $"측정점: ({mp.X:F1}, {mp.Y:F1})\n[{algoName}]{prevInfo}";

                // 판정 수행 및 표시
                PerformJudgment(_lastResult, beamType);
            }
            else
            {
                measurementOverlay = CopyBitmap(image);

                lblMeasureHotPoint.Text = "Hot Point: ---";
                lblMeasureHotValue.Text = "Hot Value: ---";
                lblMeasureCrossPoint.Text = "Cross Point: ---";
                lblMeasurementPoint.Text = "측정점: ---";
            }

            // 측정 탭 PictureBox 업데이트
            var oldImage = picMeasurement.Image;
            picMeasurement.Image = measurementOverlay;
            oldImage?.Dispose();
        }

        /// <summary>
        /// 두 PictureBox에 동일한 이미지 표시 (오버레이 없을 때)
        /// </summary>
        private void UpdateBothPictureBoxes(Bitmap image)
        {
            var oldAlignImage = picAlignment.Image;
            var oldMeasureImage = picMeasurement.Image;

            picAlignment.Image = image;
            picMeasurement.Image = new Bitmap(image);

            oldAlignImage?.Dispose();
            oldMeasureImage?.Dispose();
        }

        /// <summary>
        /// 8bpp 인덱스 이미지를 24bpp RGB로 변환
        /// </summary>
        private Bitmap ConvertTo24bpp(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(source, 0, 0, source.Width, source.Height);
            }
            return result;
        }

        /// <summary>
        /// 비트맵 복사 (8bpp 인덱스 이미지의 경우 24bpp로 변환하여 그레이스케일 유지)
        /// </summary>
        private Bitmap CopyBitmap(Bitmap source)
        {
            if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                return ConvertTo24bpp(source);
            }
            return new Bitmap(source);
        }

        /// <summary>
        /// 측정 결과 오버레이
        /// </summary>
        private Bitmap DrawMeasurementOverlay(Bitmap source, AnalysisResult result)
        {
            Bitmap overlay;
            if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                overlay = ConvertTo24bpp(source);
            }
            else
            {
                overlay = new Bitmap(source);
            }

            using (var g = Graphics.FromImage(overlay))
            {
                // 빔 타입 및 헤드램프 위치 확인
                var beamType = cmbBeamType.SelectedIndex == 0 ? BeamType.LowBeam : BeamType.HighBeam;
                var side = cmbHeadlampSide.SelectedIndex == 0 ? HeadlampSide.Left : HeadlampSide.Right;

                // 기준점 및 허용 범위 원 표시
                if (_dataManager != null && _judgmentEngine != null)
                {
                    var calibration = _dataManager.Calibration;

                    // 기준점 (캘리브레이션 Zero 포인트)
                    int centerX = beamType == BeamType.HighBeam ? calibration.HighZeroX : calibration.LowZeroX;
                    int centerY = beamType == BeamType.HighBeam ? calibration.HighZeroY : calibration.LowZeroY;

                    // 기준점 십자선 (흰색)
                    using (var pen = new Pen(Color.White, 1))
                    {
                        g.DrawLine(pen, centerX - 30, centerY, centerX + 30, centerY);
                        g.DrawLine(pen, centerX, centerY - 30, centerX, centerY + 30);
                    }

                    // 허용 범위 타원 (녹색)
                    var (radiusX, radiusY) = _judgmentEngine.GetToleranceRadius(beamType, side);

                    if (radiusX > 0 && radiusY > 0)
                    {
                        using (var pen = new Pen(Color.LimeGreen, 2))
                        {
                            g.DrawEllipse(pen, centerX - radiusX, centerY - radiusY, radiusX * 2, radiusY * 2);
                        }
                    }
                }

                // Hot Point 표시 (빨간 원)
                using (var pen = new Pen(Color.Red, 2))
                {
                    g.DrawEllipse(pen, result.HotPoint.X - 10, result.HotPoint.Y - 10, 20, 20);
                }
                using (var brush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(brush, result.HotPoint.X - 3, result.HotPoint.Y - 3, 6, 6);
                    g.DrawString($"Hot: {result.HotPointValue}", SystemFonts.DefaultFont, brush,
                        result.HotPoint.X + 15, result.HotPoint.Y);
                }

                // Cutoff Lines 표시
                var colors = new[] { Color.Lime, Color.Yellow, Color.Magenta };
                for (int i = 0; i < result.CutoffLines.Count && i < colors.Length; i++)
                {
                    var line = result.CutoffLines[i];
                    using (var pen = new Pen(colors[i], 2))
                    {
                        g.DrawLine(pen, line.Left.X, line.Left.Y, line.Right.X, line.Right.Y);
                    }
                }

                // Cross Point 표시
                foreach (var crossPt in result.CrossPoints)
                {
                    int cx = (int)crossPt.X;
                    int cy = (int)crossPt.Y;
                    using (var pen = new Pen(Color.Cyan, 2))
                    {
                        g.DrawLine(pen, cx - 10, cy, cx + 10, cy);
                        g.DrawLine(pen, cx, cy - 10, cx, cy + 10);
                    }
                    using (var brush = new SolidBrush(Color.Cyan))
                    {
                        g.DrawString($"({crossPt.X:F1}, {crossPt.Y:F1})", SystemFonts.DefaultFont, brush, cx + 10, cy - 15);
                    }
                }

                // 최종 측정점 표시 (주황색 사각형)
                if (result.MeasurementPoint.X > 0 && result.MeasurementPoint.Y > 0)
                {
                    int mx = (int)result.MeasurementPoint.X;
                    int my = (int)result.MeasurementPoint.Y;
                    using (var pen = new Pen(Color.Orange, 3))
                    {
                        g.DrawRectangle(pen, mx - 15, my - 15, 30, 30);
                        g.DrawLine(pen, mx - 20, my, mx + 20, my);
                        g.DrawLine(pen, mx, my - 20, mx, my + 20);
                    }
                    using (var brush = new SolidBrush(Color.Orange))
                    {
                        string label = result.UsedPreviousValue ? "측정점 (이전값)" : "측정점";
                        g.DrawString(label, new Font("Arial", 9, FontStyle.Bold), brush, mx + 20, my - 25);
                    }
                }
            }
            return overlay;
        }

        /// <summary>
        /// 정대 모드 오버레이 그리기
        /// </summary>
        private Bitmap DrawAlignmentOverlay(Bitmap source, AlignmentResult result)
        {
            Bitmap overlay;
            if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                overlay = ConvertTo24bpp(source);
            }
            else
            {
                overlay = new Bitmap(source);
            }

            using (var g = Graphics.FromImage(overlay))
            {
                // 경계 중심점 (정대 기준점) - 노란색 큰 십자선
                if (result.BoundaryCenter.X > 0 && result.BoundaryCenter.Y > 0)
                {
                    int cx = (int)result.BoundaryCenter.X;
                    int cy = (int)result.BoundaryCenter.Y;

                    using (var pen = new Pen(Color.Yellow, 3))
                    {
                        g.DrawLine(pen, cx - 50, cy, cx + 50, cy);
                        g.DrawLine(pen, cx, cy - 50, cx, cy + 50);
                    }

                    using (var brush = new SolidBrush(Color.Yellow))
                    {
                        g.DrawString($"CENTER ({cx}, {cy})", SystemFonts.DefaultFont, brush, cx + 15, cy - 25);
                    }
                }

                // 무게 중심 - 마젠타
                if (result.Centroid.X > 0 && result.Centroid.Y > 0)
                {
                    int cx = (int)result.Centroid.X;
                    int cy = (int)result.Centroid.Y;

                    using (var pen = new Pen(Color.Magenta, 2))
                    {
                        g.DrawEllipse(pen, cx - 15, cy - 15, 30, 30);
                        g.DrawLine(pen, cx - 20, cy, cx + 20, cy);
                        g.DrawLine(pen, cx, cy - 20, cx, cy + 20);
                    }
                }

                // 좌측 램프 - 녹색
                if (result.LeftLampCenter.X > 0)
                {
                    int lx = (int)result.LeftLampCenter.X;
                    int ly = (int)result.LeftLampCenter.Y;

                    using (var pen = new Pen(Color.LimeGreen, 2))
                    {
                        g.DrawRectangle(pen, lx - 20, ly - 20, 40, 40);
                    }
                    using (var brush = new SolidBrush(Color.LimeGreen))
                    {
                        g.DrawString("L", new Font("Arial", 12, FontStyle.Bold), brush, lx - 7, ly - 10);
                    }
                }

                // 우측 램프 - 청록색
                if (result.RightLampCenter.X > 0)
                {
                    int rx = (int)result.RightLampCenter.X;
                    int ry = (int)result.RightLampCenter.Y;

                    using (var pen = new Pen(Color.Cyan, 2))
                    {
                        g.DrawRectangle(pen, rx - 20, ry - 20, 40, 40);
                    }
                    using (var brush = new SolidBrush(Color.Cyan))
                    {
                        g.DrawString("R", new Font("Arial", 12, FontStyle.Bold), brush, rx - 7, ry - 10);
                    }
                }

                // 이미지 중앙 기준선 - 흰색 점선
                int imgCenterX = overlay.Width / 2;
                int imgCenterY = overlay.Height / 2;

                using (var pen = new Pen(Color.White, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawLine(pen, imgCenterX, 0, imgCenterX, overlay.Height);
                    g.DrawLine(pen, 0, imgCenterY, overlay.Width, imgCenterY);
                }
            }

            return overlay;
        }

        private void Camera_ErrorOccurred(object? sender, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => MessageBox.Show(message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error));
            }
            else
            {
                MessageBox.Show(message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Camera_CounterChanged(object? sender, (uint frameCount, uint errorCount) counters)
        {
            if (lblFrameCount.InvokeRequired)
            {
                lblFrameCount.Invoke(() =>
                {
                    lblFrameCount.Text = $"프레임: {counters.frameCount}";
                    lblErrorCount.Text = $"오류: {counters.errorCount}";
                });
            }
            else
            {
                lblFrameCount.Text = $"프레임: {counters.frameCount}";
                lblErrorCount.Text = $"오류: {counters.errorCount}";
            }
        }

        private void trkExposure_Scroll(object sender, EventArgs e)
        {
            // NumericUpDown 동기화
            if (numExposure.Value != trkExposure.Value)
                numExposure.Value = trkExposure.Value;

            if (_camera != null && _camera.IsConnected)
            {
                _camera.SetExposureTime(trkExposure.Value);
            }

            UpdateExposureLabel();
        }

        private void UpdateExposureLabel()
        {
            double us = trkExposure.Value;
            double ms = us / 1000.0;
            lblExposure.Text = us >= 1000 ? $"{ms:F1}ms" : $"{us}μs";
        }

        private void numExposure_ValueChanged(object sender, EventArgs e)
        {
            // 무한 루프 방지: 트랙바 값과 다를 때만 업데이트
            if (trkExposure.Value != (int)numExposure.Value)
            {
                trkExposure.Value = (int)numExposure.Value;
                if (_camera != null && _camera.IsConnected)
                {
                    _camera.SetExposureTime((int)numExposure.Value);
                }
                UpdateExposureLabel();
            }
        }

        private void SyncExposureControls(int value)
        {
            // 트랙바와 NumericUpDown 동기화
            if (trkExposure.Value != value)
                trkExposure.Value = value;
            if (numExposure.Value != value)
                numExposure.Value = value;
            UpdateExposureLabel();
        }

        private void numGain_ValueChanged(object sender, EventArgs e)
        {
            if (_camera != null && _camera.IsConnected)
            {
                _camera.SetGain((int)numGain.Value);
            }
            lblGain.Text = $"{numGain.Value}%";
        }

        private void numBlacklevel_ValueChanged(object sender, EventArgs e)
        {
            if (_camera != null && _camera.IsConnected)
            {
                _camera.SetBlacklevelOffset((int)numBlacklevel.Value);
            }
            lblBlacklevel.Text = $"{numBlacklevel.Value}";
        }

        private void chkAutoMode_CheckedChanged(object sender, EventArgs e)
        {
            if (_camera != null && _camera.IsConnected)
            {
                bool autoEnabled = chkAutoMode.Checked;
                _camera.SetAutoExposureEnabled(autoEnabled);
                _camera.SetAutoGainEnabled(autoEnabled);

                if (!autoEnabled)
                {
                    // 자동 모드 OFF 시 한계값도 최소로 설정
                    _camera.SetAutoBrightnessReference(0);
                    var (minExp, _) = _camera.GetExposureRange();
                    _camera.SetAutoExposureMax(minExp);
                    _camera.SetAutoGainMax(0);
                }
            }
        }

        private void btnDarkMode_Click(object sender, EventArgs e)
        {
            if (_camera != null && _camera.IsConnected)
            {
                // 암흑 모드 설정
                _camera.SetDarkMode();

                // UI 동기화
                chkAutoMode.Checked = false;  // 자동 모드 OFF
                SyncExposureControls(0);
                numGain.Value = 0;
                lblGain.Text = "0%";
                numBlacklevel.Value = 0;
                lblBlacklevel.Text = "0";
            }
        }

        private void btnLoadParams_Click(object sender, EventArgs e)
        {
            if (_camera == null || !_camera.IsConnected) return;

            using var dialog = new OpenFileDialog
            {
                Title = "카메라 파라미터 파일 선택",
                Filter = "파라미터 파일 (*.ini)|*.ini|모든 파일 (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (_camera.LoadParameterFile(dialog.FileName))
                {
                    // 노출/게인/흑색레벨 UI 동기화
                    var exposure = _camera.GetExposureTime();
                    SyncExposureControls((int)exposure);

                    var gain = _camera.GetGain();
                    numGain.Value = Math.Clamp(gain, 0, 100);
                    lblGain.Text = $"{gain}%";

                    var blacklevel = _camera.GetBlacklevelOffset();
                    numBlacklevel.Value = Math.Clamp(blacklevel, 0, 255);
                    lblBlacklevel.Text = $"{blacklevel}";

                    MessageBox.Show($"파라미터 파일을 로드했습니다.\n{dialog.FileName}",
                        "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("파라미터 파일 로드에 실패했습니다.",
                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            UpdateCameraList();
        }

        private void cmbBeamType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 빔 타입 변경 시 필터 리셋
            _analyzer?.ResetFilters();

            // 하향등(Low Beam)일 때만 알고리즘 선택 활성화
            bool isLowBeam = cmbBeamType.SelectedIndex == 0;
            cmbCutoffAlgorithm.Enabled = isLowBeam;
            lblCutoffAlgorithm.Enabled = isLowBeam;

            // 이전값 사용은 하향등 + Edge 알고리즘일 때만 활성화
            bool isEdge = cmbCutoffAlgorithm.SelectedIndex == 1;
            chkUsePreviousValue.Enabled = isLowBeam && isEdge;
            if (!isLowBeam || !isEdge)
            {
                chkUsePreviousValue.Checked = false;
            }

            // 상향등일 때는 라벨 업데이트
            if (!isLowBeam)
            {
                lblMeasurementPoint.Text = "측정점: Hot Point";
            }
        }

        private void cmbCutoffAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 알고리즘 변경 시 필터 리셋
            _analyzer?.ResetFilters();

            // Edge 알고리즘일 때만 이전값 사용 체크박스 활성화
            // (다른 알고리즘은 항상 값을 반환하므로 이전값 사용 불필요)
            bool isEdge = cmbCutoffAlgorithm.SelectedIndex == 1; // Edge
            chkUsePreviousValue.Enabled = isEdge;
            if (!isEdge)
            {
                chkUsePreviousValue.Checked = false;
            }
        }

        private void chkUsePreviousValue_CheckedChanged(object sender, EventArgs e)
        {
            // 이전값 사용 설정 변경 - ModelParameter에 반영
            if (_currentModelParam != null)
            {
                _currentModelParam.UsePreviousValue = chkUsePreviousValue.Checked;
                _analyzer?.ApplyModelParameter(_currentModelParam);
            }
        }

        /// <summary>
        /// 현재 선택된 Cutoff 알고리즘 가져오기
        /// </summary>
        private CutoffAlgorithm GetSelectedCutoffAlgorithm()
        {
            return cmbCutoffAlgorithm.SelectedIndex switch
            {
                0 => CutoffAlgorithm.None,
                1 => CutoffAlgorithm.Edge,
                2 => CutoffAlgorithm.Fog,
                3 => CutoffAlgorithm.Combined,
                _ => CutoffAlgorithm.Edge
            };
        }

        private void UpdateUI()
        {
            bool connected = _camera?.IsConnected ?? false;
            bool acquiring = _camera?.IsAcquiring ?? false;

            btnConnect.Text = connected ? "연결 해제" : "연결";
            btnStartStop.Text = acquiring ? "정지" : "시작";
            btnStartStop.Enabled = connected;
            trkExposure.Enabled = connected && !acquiring;
            numExposure.Enabled = connected && !acquiring;
            numGain.Enabled = connected && !acquiring;
            btnDarkMode.Enabled = connected && !acquiring;
            btnLoadParams.Enabled = connected && !acquiring;
            numBlacklevel.Enabled = connected && !acquiring;
            chkAutoMode.Enabled = connected && !acquiring;
            cmbCameras.Enabled = !connected;
            btnRefresh.Enabled = !connected;
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DisconnectCamera();
            _originalImage?.Dispose();
            _analyzer?.Dispose();
            _aligner?.Dispose();
            _calibrationForm?.Dispose();
        }

        #region Calibration

        private CalibrationForm? _calibrationForm;
        private string _calibrationIniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Calibration.ini");

        private void btnCalibration_Click(object sender, EventArgs e)
        {
            // 이미 열려 있으면 활성화
            if (_calibrationForm != null && !_calibrationForm.IsDisposed)
            {
                _calibrationForm.Activate();
                return;
            }

            // 새 교정 폼 열기
            _calibrationForm = new CalibrationForm(_calibrationIniPath);
            _calibrationForm.FormClosed += (s, args) => _calibrationForm = null;
            _calibrationForm.Show();

            // 실시간 측정값 전달을 위한 타이머 시작
            var calibTimer = new System.Windows.Forms.Timer { Interval = 200 };
            calibTimer.Tick += (s, args) =>
            {
                if (_calibrationForm == null || _calibrationForm.IsDisposed)
                {
                    calibTimer.Stop();
                    calibTimer.Dispose();
                    return;
                }

                // 현재 분석 결과가 있으면 교정 폼에 전달
                if (_lastResult != null && _lastResult.IsValid)
                {
                    _calibrationForm.UpdateMeasurement(
                        new Point(_lastResult.HotPoint.X, _lastResult.HotPoint.Y),
                        _lastResult.HotPointValue);
                }
            };
            calibTimer.Start();
        }

        #endregion
    }
}
