using System;
using System.Collections.Generic;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using HeadLightInspection.ImageProcessing.Models;

namespace HeadLightInspection.ImageProcessing
{
    /// <summary>
    /// 헤드램프 분석 결과
    /// </summary>
    public class AnalysisResult
    {
        public OpenCvSharp.Point HotPoint { get; set; }
        public int HotPointValue { get; set; }
        public List<LineSegment> CutoffLines { get; set; } = new();
        public List<Point2f> CrossPoints { get; set; } = new();

        /// <summary>
        /// 최종 측정 기준점 (오프셋 적용 후)
        /// </summary>
        public Point2f MeasurementPoint { get; set; }

        /// <summary>
        /// 사용된 알고리즘
        /// </summary>
        public CutoffAlgorithm UsedAlgorithm { get; set; }

        /// <summary>
        /// 이전값 사용 여부
        /// </summary>
        public bool UsedPreviousValue { get; set; }

        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 직선 정보
    /// </summary>
    public class LineSegment
    {
        public double A { get; set; }  // ax + by + c = 0
        public double B { get; set; }
        public double C { get; set; }
        public OpenCvSharp.Point Left { get; set; }
        public OpenCvSharp.Point Right { get; set; }
        public int InlierCount { get; set; }
        public double AverageDistance { get; set; }
    }

    /// <summary>
    /// 빔 타입
    /// </summary>
    public enum BeamType
    {
        LowBeam = 0,
        HighBeam = 1
    }

    /// <summary>
    /// 헤드램프 이미지 분석기
    /// </summary>
    public class HeadlampAnalyzer : IDisposable
    {
        private const int RANSAC_TRY_NUM = 200;
        private const int RANSAC_MIN_SAMPLES = 6;

        private Mat? _currentImage;
        private Mat? _grayImage;
        private bool _disposed;

        // Moving Average 필터 - Hot Point
        private int _hpAvgWindowLen = 5;      // 평균 윈도우 길이 (기본 5프레임)
        private int _hpAvgSampleNum = 0;      // 현재까지 쌓인 샘플 수
        private double _hpThAvg = 30.0;       // 거리 임계값 (이 이상 떨어지면 점프로 간주)
        private Point2f _avgHotPos;

        // Moving Average 필터 - Cross Point
        private int _cpAvgWindowLen = 5;
        private int _cpAvgSampleNum = 0;
        private double _cpThAvg = 30.0;
        private Point2f _avgCrossPoint;

        // Moving Average 필터 - Cutoff Lines (각 라인별)
        private int _lineAvgWindowLen = 5;
        private int[] _lineAvgSampleNum = new int[2];
        private double _lineThAvg = 20.0;
        private LineSegment?[] _avgLines = new LineSegment?[2];

        // 현재 모델 파라미터
        private ModelParameter? _currentModelParameter;

        // 이전값 저장 (검출 실패 시 사용)
        private Point2f _previousMeasurementPoint;
        private int _previousValueFailCount = 0;
        private bool _hasPreviousValue = false;

        // 자동 오프셋 (CUT_ALGO_COMB용)
        private int _autoOffsetX = 0;
        private int _autoOffsetY = 0;
        private bool _autoOffsetCalculated = false;
        private int _autoOffsetFrameCount = 0;
        private const int AUTO_OFFSET_MIN_FRAMES = 5;

        public int ImageWidth => _currentImage?.Width ?? 0;
        public int ImageHeight => _currentImage?.Height ?? 0;

        /// <summary>
        /// 모델 파라미터 적용
        /// </summary>
        public void ApplyModelParameter(ModelParameter param)
        {
            _currentModelParameter = param;

            // 필터 파라미터 적용
            SetHotPointFilterParams(param.HotPointFilterCount, param.HotPointThreshold);
            SetCrossPointFilterParams(param.CutoffLineFilterCount, param.CutoffLineThreshold);
            SetLineFilterParams(param.CutoffLineFilterCount, param.CutoffLineThreshold);

            ResetFilters();
        }

        /// <summary>
        /// 현재 적용된 모델 파라미터
        /// </summary>
        public ModelParameter? CurrentModelParameter => _currentModelParameter;

        /// <summary>
        /// Hot Point 필터 파라미터 설정
        /// </summary>
        public void SetHotPointFilterParams(int windowLen, double threshold)
        {
            _hpAvgWindowLen = windowLen;
            _hpThAvg = threshold;
            _hpAvgSampleNum = 0;
        }

        /// <summary>
        /// Cross Point 필터 파라미터 설정
        /// </summary>
        public void SetCrossPointFilterParams(int windowLen, double threshold)
        {
            _cpAvgWindowLen = windowLen;
            _cpThAvg = threshold;
            _cpAvgSampleNum = 0;
        }

        /// <summary>
        /// Cutoff Line 필터 파라미터 설정
        /// </summary>
        public void SetLineFilterParams(int windowLen, double threshold)
        {
            _lineAvgWindowLen = windowLen;
            _lineThAvg = threshold;
            _lineAvgSampleNum[0] = 0;
            _lineAvgSampleNum[1] = 0;
            _avgLines[0] = null;
            _avgLines[1] = null;
        }

        /// <summary>
        /// 모든 필터 리셋
        /// </summary>
        public void ResetFilters()
        {
            _hpAvgSampleNum = 0;
            _cpAvgSampleNum = 0;
            _lineAvgSampleNum[0] = 0;
            _lineAvgSampleNum[1] = 0;
            _avgLines[0] = null;
            _avgLines[1] = null;

            // 이전값 리셋
            _hasPreviousValue = false;
            _previousValueFailCount = 0;

            // 자동 오프셋 리셋
            _autoOffsetCalculated = false;
            _autoOffsetFrameCount = 0;
            _autoOffsetX = 0;
            _autoOffsetY = 0;
        }

        /// <summary>
        /// Bitmap 이미지 설정
        /// </summary>
        public void SetImage(Bitmap bitmap)
        {
            _currentImage?.Dispose();
            _grayImage?.Dispose();

            _currentImage = BitmapConverter.ToMat(bitmap);

            // 그레이스케일 변환
            int channels = _currentImage.Channels();
            if (channels == 4)
            {
                _grayImage = new Mat();
                Cv2.CvtColor(_currentImage, _grayImage, ColorConversionCodes.BGRA2GRAY);
            }
            else if (channels == 3)
            {
                _grayImage = new Mat();
                Cv2.CvtColor(_currentImage, _grayImage, ColorConversionCodes.BGR2GRAY);
            }
            else
            {
                _grayImage = _currentImage.Clone();
            }
        }

        /// <summary>
        /// Hot Point 검출 (가장 밝은 점)
        /// </summary>
        public (OpenCvSharp.Point position, int value) FindHotPoint(int margin = 0)
        {
            if (_grayImage == null)
                return (new OpenCvSharp.Point(0, 0), 0);

            Mat searchArea = _grayImage;
            int offsetX = 0, offsetY = 0;

            if (margin > 0)
            {
                int x = margin;
                int y = margin;
                int w = _grayImage.Width - 2 * margin;
                int h = _grayImage.Height - 2 * margin;

                if (w > 0 && h > 0)
                {
                    searchArea = new Mat(_grayImage, new Rect(x, y, w, h));
                    offsetX = x;
                    offsetY = y;
                }
            }

            Cv2.MinMaxLoc(searchArea, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            var hotPoint = new OpenCvSharp.Point(maxLoc.X + offsetX, maxLoc.Y + offsetY);

            if (margin > 0)
                searchArea.Dispose();

            return (hotPoint, (int)maxVal);
        }

        /// <summary>
        /// 수평 에지 검출
        /// </summary>
        public List<OpenCvSharp.Point> FindHorizontalEdges(int threshold = 30)
        {
            if (_grayImage == null)
                return new List<OpenCvSharp.Point>();

            var edges = new List<OpenCvSharp.Point>();

            using var sobelY = new Mat();
            Cv2.Sobel(_grayImage, sobelY, MatType.CV_16S, 0, 1, 3);

            using var absSobel = new Mat();
            Cv2.ConvertScaleAbs(sobelY, absSobel);

            for (int y = 1; y < _grayImage.Height - 1; y++)
            {
                for (int x = 0; x < _grayImage.Width; x++)
                {
                    byte val = absSobel.At<byte>(y, x);
                    if (val > threshold)
                    {
                        edges.Add(new OpenCvSharp.Point(x, y));
                    }
                }
            }

            return edges;
        }

        /// <summary>
        /// Cutoff Line 검출 (RANSAC)
        /// </summary>
        public List<LineSegment> FindCutoffLines(double tolerance = 3.0, int maxLines = 2)
        {
            var lines = new List<LineSegment>();
            var edges = FindHorizontalEdges();

            if (edges.Count < RANSAC_MIN_SAMPLES)
                return lines;

            var remainingEdges = new List<OpenCvSharp.Point>(edges);
            var random = new Random();

            for (int lineIdx = 0; lineIdx < maxLines && remainingEdges.Count >= RANSAC_MIN_SAMPLES; lineIdx++)
            {
                LineSegment? bestLine = null;
                int bestInlierCount = 0;

                for (int iter = 0; iter < RANSAC_TRY_NUM; iter++)
                {
                    int idx1 = random.Next(remainingEdges.Count);
                    int idx2 = random.Next(remainingEdges.Count);
                    if (idx1 == idx2) continue;

                    var p1 = remainingEdges[idx1];
                    var p2 = remainingEdges[idx2];

                    double a = p2.Y - p1.Y;
                    double b = p1.X - p2.X;
                    double c = p2.X * p1.Y - p1.X * p2.Y;
                    double norm = Math.Sqrt(a * a + b * b);

                    if (norm < 1e-6) continue;

                    a /= norm;
                    b /= norm;
                    c /= norm;

                    var inliers = new List<OpenCvSharp.Point>();
                    double totalDist = 0;

                    foreach (var pt in remainingEdges)
                    {
                        double dist = Math.Abs(a * pt.X + b * pt.Y + c);
                        if (dist < tolerance)
                        {
                            inliers.Add(pt);
                            totalDist += dist;
                        }
                    }

                    if (inliers.Count > bestInlierCount)
                    {
                        bestInlierCount = inliers.Count;
                        var refined = FitLineToPoints(inliers);

                        bestLine = new LineSegment
                        {
                            A = refined.a,
                            B = refined.b,
                            C = refined.c,
                            Left = new OpenCvSharp.Point(inliers.Min(p => p.X),
                                (int)(-(refined.a * inliers.Min(p => p.X) + refined.c) / refined.b)),
                            Right = new OpenCvSharp.Point(inliers.Max(p => p.X),
                                (int)(-(refined.a * inliers.Max(p => p.X) + refined.c) / refined.b)),
                            InlierCount = inliers.Count,
                            AverageDistance = inliers.Count > 0 ? totalDist / inliers.Count : 0
                        };
                    }
                }

                if (bestLine != null && bestInlierCount >= RANSAC_MIN_SAMPLES)
                {
                    lines.Add(bestLine);

                    remainingEdges.RemoveAll(pt =>
                    {
                        double dist = Math.Abs(bestLine.A * pt.X + bestLine.B * pt.Y + bestLine.C);
                        return dist < tolerance * 2;
                    });
                }
            }

            return lines;
        }

        /// <summary>
        /// 점들에 직선 피팅 (최소자승법)
        /// </summary>
        private (double a, double b, double c) FitLineToPoints(List<OpenCvSharp.Point> points)
        {
            if (points.Count < 2)
                return (0, 1, 0);

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            int n = points.Count;

            foreach (var pt in points)
            {
                sumX += pt.X;
                sumY += pt.Y;
                sumXY += pt.X * pt.Y;
                sumX2 += pt.X * pt.X;
            }

            double denom = n * sumX2 - sumX * sumX;
            if (Math.Abs(denom) < 1e-6)
            {
                return (1, 0, -sumX / n);
            }

            double slope = (n * sumXY - sumX * sumY) / denom;
            double intercept = (sumY - slope * sumX) / n;

            double a = slope;
            double b = -1;
            double c = intercept;
            double norm = Math.Sqrt(a * a + b * b);

            return (a / norm, b / norm, c / norm);
        }

        /// <summary>
        /// 두 직선의 교차점 계산
        /// </summary>
        public Point2f? FindCrossPoint(LineSegment line1, LineSegment line2)
        {
            double denom = line1.A * line2.B - line2.A * line1.B;

            // 평행선 검출 (임계값 높임)
            if (Math.Abs(denom) < 0.01)
                return null;

            double x = (line1.B * line2.C - line2.B * line1.C) / denom;
            double y = (line2.A * line1.C - line1.A * line2.C) / denom;

            // 이미지 범위 검증 (여유 마진 포함)
            int margin = 200; // 이미지 밖 200픽셀까지 허용

            if (_grayImage != null)
            {
                if (x < -margin || x > _grayImage.Width + margin ||
                    y < -margin || y > _grayImage.Height + margin)
                {
                    System.Diagnostics.Debug.WriteLine($"[FindCrossPoint] Out of bounds: ({x:F1}, {y:F1}) - discarded");
                    return null;
                }
            }

            return new Point2f((float)x, (float)y);
        }

        /// <summary>
        /// Hot Point Moving Average 필터링
        /// </summary>
        private OpenCvSharp.Point ApplyHotPointFilter(OpenCvSharp.Point current)
        {
            if (_hpAvgWindowLen < 1 || _hpThAvg < 0.0)
                return current;

            if (_hpAvgSampleNum < _hpAvgWindowLen)
                _hpAvgSampleNum++;

            if (_hpAvgSampleNum == 1)
            {
                _avgHotPos = new Point2f(current.X, current.Y);
            }
            else
            {
                double dist = Math.Sqrt(
                    (_avgHotPos.X - current.X) * (_avgHotPos.X - current.X) +
                    (_avgHotPos.Y - current.Y) * (_avgHotPos.Y - current.Y));

                if (dist <= _hpThAvg)
                {
                    // Moving average 업데이트
                    _avgHotPos.X = (float)((_avgHotPos.X * (_hpAvgSampleNum - 1) + current.X) / _hpAvgSampleNum);
                    _avgHotPos.Y = (float)((_avgHotPos.Y * (_hpAvgSampleNum - 1) + current.Y) / _hpAvgSampleNum);
                }
                else
                {
                    // 임계값 초과 - 점프로 간주, 재시작
                    _avgHotPos = new Point2f(current.X, current.Y);
                    _hpAvgSampleNum = 1;
                }
            }

            return new OpenCvSharp.Point((int)_avgHotPos.X, (int)_avgHotPos.Y);
        }

        /// <summary>
        /// Cross Point Moving Average 필터링
        /// </summary>
        private Point2f ApplyCrossPointFilter(Point2f current)
        {
            if (_cpAvgWindowLen < 1 || _cpThAvg < 0.0)
                return current;

            if (_cpAvgSampleNum < _cpAvgWindowLen)
                _cpAvgSampleNum++;

            if (_cpAvgSampleNum == 1)
            {
                _avgCrossPoint = current;
            }
            else
            {
                double dist = Math.Sqrt(
                    (_avgCrossPoint.X - current.X) * (_avgCrossPoint.X - current.X) +
                    (_avgCrossPoint.Y - current.Y) * (_avgCrossPoint.Y - current.Y));

                if (dist <= _cpThAvg)
                {
                    _avgCrossPoint.X = (float)((_avgCrossPoint.X * (_cpAvgSampleNum - 1) + current.X) / _cpAvgSampleNum);
                    _avgCrossPoint.Y = (float)((_avgCrossPoint.Y * (_cpAvgSampleNum - 1) + current.Y) / _cpAvgSampleNum);
                }
                else
                {
                    _avgCrossPoint = current;
                    _cpAvgSampleNum = 1;
                }
            }

            return _avgCrossPoint;
        }

        /// <summary>
        /// Cutoff Line Moving Average 필터링
        /// </summary>
        private LineSegment ApplyLineFilter(LineSegment current, int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= 2)
                return current;

            if (_lineAvgWindowLen < 1 || _lineThAvg < 0.0)
                return current;

            if (_lineAvgSampleNum[lineIndex] < _lineAvgWindowLen)
                _lineAvgSampleNum[lineIndex]++;

            if (_lineAvgSampleNum[lineIndex] == 1 || _avgLines[lineIndex] == null)
            {
                _avgLines[lineIndex] = new LineSegment
                {
                    A = current.A,
                    B = current.B,
                    C = current.C,
                    Left = current.Left,
                    Right = current.Right,
                    InlierCount = current.InlierCount,
                    AverageDistance = current.AverageDistance
                };
            }
            else
            {
                var avg = _avgLines[lineIndex]!;

                // Left, Right 포인트의 거리 체크
                double distLeft = Math.Sqrt(
                    (avg.Left.X - current.Left.X) * (avg.Left.X - current.Left.X) +
                    (avg.Left.Y - current.Left.Y) * (avg.Left.Y - current.Left.Y));
                double distRight = Math.Sqrt(
                    (avg.Right.X - current.Right.X) * (avg.Right.X - current.Right.X) +
                    (avg.Right.Y - current.Right.Y) * (avg.Right.Y - current.Right.Y));

                if (distLeft <= _lineThAvg && distRight <= _lineThAvg)
                {
                    int n = _lineAvgSampleNum[lineIndex];
                    avg.A = (avg.A * (n - 1) + current.A) / n;
                    avg.B = (avg.B * (n - 1) + current.B) / n;
                    avg.C = (avg.C * (n - 1) + current.C) / n;
                    avg.Left = new OpenCvSharp.Point(
                        (int)((avg.Left.X * (n - 1) + current.Left.X) / n),
                        (int)((avg.Left.Y * (n - 1) + current.Left.Y) / n));
                    avg.Right = new OpenCvSharp.Point(
                        (int)((avg.Right.X * (n - 1) + current.Right.X) / n),
                        (int)((avg.Right.Y * (n - 1) + current.Right.Y) / n));
                }
                else
                {
                    // 점프 - 재시작
                    _avgLines[lineIndex] = new LineSegment
                    {
                        A = current.A,
                        B = current.B,
                        C = current.C,
                        Left = current.Left,
                        Right = current.Right,
                        InlierCount = current.InlierCount,
                        AverageDistance = current.AverageDistance
                    };
                    _lineAvgSampleNum[lineIndex] = 1;
                }
            }

            return _avgLines[lineIndex]!;
        }

        /// <summary>
        /// 전체 분석 수행
        /// </summary>
        public AnalysisResult Analyze(BeamType beamType = BeamType.LowBeam)
        {
            var result = new AnalysisResult();

            if (_grayImage == null)
            {
                result.IsValid = false;
                result.Message = "이미지가 설정되지 않았습니다.";
                return result;
            }

            try
            {
                // 1. Hot Point 검출 + 필터링
                var (hotPos, hotVal) = FindHotPoint(margin: 10);
                result.HotPoint = ApplyHotPointFilter(hotPos);
                result.HotPointValue = hotVal;

                // 2. Cutoff Line 검출 (Low Beam만) + 필터링
                if (beamType == BeamType.LowBeam)
                {
                    var rawLines = FindCutoffLines(tolerance: 3.0, maxLines: 2);

                    // 각 라인에 필터링 적용
                    result.CutoffLines = new List<LineSegment>();
                    for (int i = 0; i < rawLines.Count && i < 2; i++)
                    {
                        var filteredLine = ApplyLineFilter(rawLines[i], i);
                        result.CutoffLines.Add(filteredLine);
                    }

                    // 3. Cross Point 검출 + 필터링
                    if (result.CutoffLines.Count >= 2)
                    {
                        var crossPt = FindCrossPoint(result.CutoffLines[0], result.CutoffLines[1]);
                        if (crossPt.HasValue)
                        {
                            var filteredCrossPt = ApplyCrossPointFilter(crossPt.Value);
                            result.CrossPoints.Add(filteredCrossPt);
                        }
                    }
                }

                result.IsValid = true;
                result.Message = "분석 완료";
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Message = $"분석 오류: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 하향등 전용 분석 (알고리즘 선택 지원)
        /// </summary>
        /// <param name="lampPosition">좌/우 램프 위치</param>
        /// <param name="algorithm">사용할 알고리즘 (null이면 모델 파라미터에서 가져옴)</param>
        public AnalysisResult AnalyzeLowBeam(LampPosition lampPosition, CutoffAlgorithm? algorithm = null)
        {
            var result = new AnalysisResult();

            if (_grayImage == null)
            {
                result.IsValid = false;
                result.Message = "이미지가 설정되지 않았습니다.";
                return result;
            }

            // 알고리즘 결정
            CutoffAlgorithm algoToUse = algorithm ?? _currentModelParameter?.CutoffAlgorithmType ?? CutoffAlgorithm.Edge;
            result.UsedAlgorithm = algoToUse;

            try
            {
                // 1. Hot Point 검출 + 필터링
                var (hotPos, hotVal) = FindHotPoint(margin: 10);
                result.HotPoint = ApplyHotPointFilter(hotPos);
                result.HotPointValue = hotVal;

                // 2. Cutoff Line 검출 + 필터링
                var rawLines = FindCutoffLines(tolerance: _currentModelParameter?.RansacTolerance ?? 3.0, maxLines: 2);
                result.CutoffLines = new List<LineSegment>();
                for (int i = 0; i < rawLines.Count && i < 2; i++)
                {
                    var filteredLine = ApplyLineFilter(rawLines[i], i);
                    result.CutoffLines.Add(filteredLine);
                }

                // 3. Cross Point 검출
                Point2f? crossPoint = null;
                if (result.CutoffLines.Count >= 2)
                {
                    crossPoint = FindCrossPoint(result.CutoffLines[0], result.CutoffLines[1]);
                    if (crossPoint.HasValue)
                    {
                        var filteredCrossPt = ApplyCrossPointFilter(crossPoint.Value);
                        result.CrossPoints.Add(filteredCrossPt);
                    }
                }

                // 4. 알고리즘별 측정점 계산
                Point2f measurementPoint;
                bool detected = false;

                switch (algoToUse)
                {
                    case CutoffAlgorithm.None:
                        (measurementPoint, detected) = CalculateAlgoNone(result.HotPoint, lampPosition);
                        break;

                    case CutoffAlgorithm.Edge:
                        (measurementPoint, detected) = CalculateAlgoEdge(result.HotPoint, crossPoint, lampPosition);
                        break;

                    case CutoffAlgorithm.Fog:
                        (measurementPoint, detected) = CalculateAlgoFog(result.HotPoint, result.CutoffLines, crossPoint, lampPosition);
                        break;

                    case CutoffAlgorithm.Combined:
                        (measurementPoint, detected) = CalculateAlgoCombined(result.HotPoint, result.CutoffLines, crossPoint, lampPosition);
                        break;

                    default:
                        (measurementPoint, detected) = CalculateAlgoNone(result.HotPoint, lampPosition);
                        break;
                }

                // 5. 이전값 처리
                if (!detected)
                {
                    measurementPoint = HandlePreviousValue(out bool usedPrevious);
                    result.UsedPreviousValue = usedPrevious;
                    detected = usedPrevious;
                }
                else
                {
                    // 성공 시 이전값 저장
                    _previousMeasurementPoint = measurementPoint;
                    _hasPreviousValue = true;
                    _previousValueFailCount = 0;
                    result.UsedPreviousValue = false;
                }

                result.MeasurementPoint = measurementPoint;
                result.IsValid = detected;
                result.Message = detected
                    ? $"[{algoToUse}] 측정 완료" + (result.UsedPreviousValue ? " (이전값 사용)" : "")
                    : "측정 실패";
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Message = $"분석 오류: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 상향등 분석 (Hot Point만 사용)
        /// </summary>
        public AnalysisResult AnalyzeHighBeam()
        {
            var result = new AnalysisResult();

            if (_grayImage == null)
            {
                result.IsValid = false;
                result.Message = "이미지가 설정되지 않았습니다.";
                return result;
            }

            try
            {
                // Hot Point 검출 + 필터링
                var (hotPos, hotVal) = FindHotPoint(margin: 10);
                result.HotPoint = ApplyHotPointFilter(hotPos);
                result.HotPointValue = hotVal;

                // 상향등은 Hot Point가 곧 측정점
                result.MeasurementPoint = new Point2f(result.HotPoint.X, result.HotPoint.Y);
                result.IsValid = true;
                result.Message = "상향등 측정 완료";
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Message = $"분석 오류: {ex.Message}";
            }

            return result;
        }

        #region Cutoff Algorithms

        /// <summary>
        /// CUT_ALGO_NONE: Hot Point + 고정 오프셋
        /// </summary>
        private (Point2f point, bool detected) CalculateAlgoNone(OpenCvSharp.Point hotPoint, LampPosition lampPosition)
        {
            int offsetX, offsetY;
            GetOffset(lampPosition, out offsetX, out offsetY);

            var result = new Point2f(hotPoint.X + offsetX, hotPoint.Y + offsetY);
            return (result, true);
        }

        /// <summary>
        /// CUT_ALGO_EDGE: Cutoff Line 교점 + 오프셋
        /// </summary>
        private (Point2f point, bool detected) CalculateAlgoEdge(OpenCvSharp.Point hotPoint, Point2f? crossPoint, LampPosition lampPosition)
        {
            if (!crossPoint.HasValue)
            {
                // 교점 검출 실패 - 이전값 처리로 넘김
                return (new Point2f(0, 0), false);
            }

            int offsetX, offsetY;
            GetOffset(lampPosition, out offsetX, out offsetY);

            var result = new Point2f(crossPoint.Value.X + offsetX, crossPoint.Value.Y + offsetY);
            return (result, true);
        }

        /// <summary>
        /// CUT_ALGO_FOG: 1st Line에서 Hot Point X좌표의 Y값 계산
        /// </summary>
        private (Point2f point, bool detected) CalculateAlgoFog(OpenCvSharp.Point hotPoint, List<LineSegment> lines, Point2f? crossPoint, LampPosition lampPosition)
        {
            if (lines.Count == 0)
            {
                // 라인 검출 실패
                return (new Point2f(0, 0), false);
            }

            // 1st Line (첫 번째 검출된 라인)
            var firstLine = lines[0];

            // Hot Point X좌표에서 1st Line의 Y값 계산
            // ax + by + c = 0 → y = -(ax + c) / b
            float y;
            if (Math.Abs(firstLine.B) > 0.0001)
            {
                y = (float)(-(firstLine.A * hotPoint.X + firstLine.C) / firstLine.B);
            }
            else
            {
                // 수직선인 경우 (드묾)
                y = hotPoint.Y;
            }

            // 교점이 검출되었다면 교점의 Y값 사용
            if (crossPoint.HasValue)
            {
                y = crossPoint.Value.Y;
            }

            int offsetX, offsetY;
            GetOffset(lampPosition, out offsetX, out offsetY);

            var result = new Point2f(hotPoint.X + offsetX, y + offsetY);
            return (result, true);
        }

        /// <summary>
        /// CUT_ALGO_COMB: 자동 오프셋 계산 + Fog 방식
        /// </summary>
        private (Point2f point, bool detected) CalculateAlgoCombined(OpenCvSharp.Point hotPoint, List<LineSegment> lines, Point2f? crossPoint, LampPosition lampPosition)
        {
            // 1단계: 자동 오프셋 계산 (초기 몇 프레임)
            if (!_autoOffsetCalculated)
            {
                _autoOffsetFrameCount++;

                if (crossPoint.HasValue && _autoOffsetFrameCount > AUTO_OFFSET_MIN_FRAMES)
                {
                    // Hot Point와 Cross Point의 차이로 오프셋 계산
                    int calcOffsetX = (int)(crossPoint.Value.X - hotPoint.X);

                    // 오프셋이 합리적인 범위 내인지 확인 (100 픽셀 이내)
                    if (Math.Abs(calcOffsetX) < 100)
                    {
                        _autoOffsetX = calcOffsetX;
                        _autoOffsetY = 0; // Y 오프셋은 0으로 (Fog 방식에서 Y는 라인에서 계산)
                        _autoOffsetCalculated = true;
                    }
                    else
                    {
                        // 범위 초과 시 기본값
                        _autoOffsetX = Math.Sign(calcOffsetX) * 50;
                        _autoOffsetCalculated = true;
                    }
                }
            }

            // 2단계: Fog 방식으로 측정점 계산
            if (lines.Count == 0)
            {
                return (new Point2f(0, 0), false);
            }

            var firstLine = lines[0];
            float y;
            if (Math.Abs(firstLine.B) > 0.0001)
            {
                y = (float)(-(firstLine.A * hotPoint.X + firstLine.C) / firstLine.B);
            }
            else
            {
                y = hotPoint.Y;
            }

            if (crossPoint.HasValue)
            {
                y = crossPoint.Value.Y;
            }

            // 자동 오프셋 적용 (기본 오프셋에 추가)
            int offsetX, offsetY;
            GetOffset(lampPosition, out offsetX, out offsetY);

            var result = new Point2f(hotPoint.X + offsetX + _autoOffsetX, y + offsetY + _autoOffsetY);
            return (result, true);
        }

        /// <summary>
        /// 좌/우 램프별 오프셋 가져오기
        /// </summary>
        private void GetOffset(LampPosition lampPosition, out int offsetX, out int offsetY)
        {
            if (_currentModelParameter == null)
            {
                offsetX = 0;
                offsetY = 0;
                return;
            }

            if (lampPosition == LampPosition.Left)
            {
                offsetX = _currentModelParameter.OffsetLeftX;
                offsetY = _currentModelParameter.OffsetLeftY;
            }
            else
            {
                offsetX = _currentModelParameter.OffsetRightX;
                offsetY = _currentModelParameter.OffsetRightY;
            }
        }

        /// <summary>
        /// 이전값 처리
        /// </summary>
        private Point2f HandlePreviousValue(out bool usedPrevious)
        {
            bool usePrev = _currentModelParameter?.UsePreviousValue ?? true;
            int maxCount = _currentModelParameter?.PreviousValueMaxCount ?? 10;

            if (usePrev && _hasPreviousValue && _previousValueFailCount < maxCount)
            {
                _previousValueFailCount++;
                usedPrevious = true;
                return _previousMeasurementPoint;
            }

            usedPrevious = false;
            return new Point2f(0, 0);
        }

        #endregion

        public void Dispose()
        {
            if (_disposed) return;

            _currentImage?.Dispose();
            _grayImage?.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~HeadlampAnalyzer()
        {
            Dispose();
        }
    }
}
