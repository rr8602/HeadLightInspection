using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace HeadLightInspection.ImageProcessing
{
    /// <summary>
    /// 램프 중심 정보
    /// </summary>
    public class LampCenterInfo
    {
        public const int MAX_LAMP_NUM = 10;

        /// <summary>
        /// 각 램프의 면적
        /// </summary>
        public int[] Area { get; } = new int[MAX_LAMP_NUM];

        /// <summary>
        /// 각 램프의 경계 사각형
        /// </summary>
        public Rect[] BoundingRect { get; } = new Rect[MAX_LAMP_NUM];

        /// <summary>
        /// 각 램프의 중심 위치
        /// </summary>
        public Point2f[] Center { get; } = new Point2f[MAX_LAMP_NUM];

        /// <summary>
        /// 크기 순서 (0: 가장 큼)
        /// </summary>
        public int[] SizeOrder { get; } = new int[MAX_LAMP_NUM];

        /// <summary>
        /// 모든 램프를 포함하는 경계 사각형
        /// </summary>
        public Rect Boundary { get; set; }

        /// <summary>
        /// 램프들의 무게 중심
        /// </summary>
        public Point2f Centroid { get; set; }

        /// <summary>
        /// 검출된 램프 수
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Moving Average 적용된 중심 위치
        /// </summary>
        public Point2f[] AvgCenter { get; } = new Point2f[MAX_LAMP_NUM + 2];

        /// <summary>
        /// Moving Average 샘플 수
        /// </summary>
        public int[] AvgSampleNum { get; } = new int[MAX_LAMP_NUM + 2];
    }

    /// <summary>
    /// 정대 결과
    /// </summary>
    public class AlignmentResult
    {
        /// <summary>
        /// 검출된 램프 수
        /// </summary>
        public int LampCount { get; set; }

        /// <summary>
        /// 경계 사각형 중심 (정대 기준점)
        /// </summary>
        public Point2f BoundaryCenter { get; set; }

        /// <summary>
        /// 무게 중심
        /// </summary>
        public Point2f Centroid { get; set; }

        /// <summary>
        /// 가장 왼쪽 램프 중심
        /// </summary>
        public Point2f LeftLampCenter { get; set; }

        /// <summary>
        /// 가장 오른쪽 램프 중심
        /// </summary>
        public Point2f RightLampCenter { get; set; }

        /// <summary>
        /// 정대 성공 여부
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 메시지
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 이진화 이미지 (디버그용)
        /// </summary>
        public Mat? BinaryImage { get; set; }
    }

    /// <summary>
    /// 헤드램프 정대 분석기
    /// </summary>
    public class HeadlampAligner : IDisposable
    {
        private Mat? _currentImage;
        private Mat? _binaryImage;
        private Mat? _processImage;
        private bool _disposed;

        private LampCenterInfo _centerInfo = new();

        // Moving Average 파라미터
        private int _avgWindowLen = 1;
        private double _avgThreshold = 0.0;

        public int ImageWidth => _currentImage?.Width ?? 0;
        public int ImageHeight => _currentImage?.Height ?? 0;

        /// <summary>
        /// 생성자
        /// </summary>
        public HeadlampAligner()
        {
            SetAvgParameters(1, 0.0);
        }

        /// <summary>
        /// Moving Average 파라미터 설정
        /// </summary>
        public void SetAvgParameters(int windowLen, double threshold)
        {
            _avgWindowLen = windowLen;
            _avgThreshold = threshold;

            // 초기화
            for (int i = 0; i < LampCenterInfo.MAX_LAMP_NUM + 2; i++)
            {
                _centerInfo.AvgSampleNum[i] = 0;
                _centerInfo.AvgCenter[i] = new Point2f(0, 0);
            }
        }

        /// <summary>
        /// 이미지 설정
        /// </summary>
        public void SetImage(Mat image)
        {
            _currentImage?.Dispose();
            _currentImage = image.Clone();
        }

        /// <summary>
        /// Bitmap에서 이미지 설정
        /// </summary>
        public void SetImage(System.Drawing.Bitmap bitmap)
        {
            _currentImage?.Dispose();
            _currentImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);

            // 그레이스케일 변환
            if (_currentImage.Channels() > 1)
            {
                var gray = new Mat();
                Cv2.CvtColor(_currentImage, gray, ColorConversionCodes.BGR2GRAY);
                _currentImage.Dispose();
                _currentImage = gray;
            }
        }

        /// <summary>
        /// 히스토그램 계산 및 지정 퍼센트 이상 픽셀값 구하기
        /// </summary>
        private int GetPercentPixel(double percent)
        {
            if (_currentImage == null) return 128;

            // 히스토그램 계산
            Mat hist = new Mat();
            int[] histSize = { 256 };
            Rangef[] ranges = { new Rangef(0, 256) };

            Cv2.CalcHist(new Mat[] { _currentImage }, new int[] { 0 }, null, hist, 1, histSize, ranges);

            // 전체 픽셀 수
            int totalPixels = _currentImage.Rows * _currentImage.Cols;
            int targetCount = (int)((100.0 - percent) / 100.0 * totalPixels);

            // 누적 히스토그램으로 임계값 찾기
            int cumulative = 0;
            for (int i = 255; i >= 0; i--)
            {
                cumulative += (int)hist.At<float>(i);
                if (cumulative >= (totalPixels - targetCount))
                {
                    hist.Dispose();
                    return Math.Max(i, 1);
                }
            }

            hist.Dispose();
            return 1;
        }

        /// <summary>
        /// 이진화 이미지 생성
        /// </summary>
        private void MakeBinary(int threshold)
        {
            if (_currentImage == null) return;

            _binaryImage?.Dispose();
            _binaryImage = new Mat();
            Cv2.Threshold(_currentImage, _binaryImage, threshold, 255, ThresholdTypes.Binary);
        }

        /// <summary>
        /// 램프 중심 검색 (정대)
        /// </summary>
        public AlignmentResult SearchLampCenter(int margin = 0, int boundaryX = 0, int boundaryY = 0)
        {
            var result = new AlignmentResult();

            if (_currentImage == null)
            {
                result.IsValid = false;
                result.Message = "이미지가 설정되지 않았습니다.";
                return result;
            }

            try
            {
                // 마진 적용 (테두리 영역 0으로 설정)
                if (margin > 0)
                {
                    ApplyMargin(margin);
                }

                // 경계 영역 적용
                if (boundaryX > 0 || boundaryY > 0)
                {
                    ApplyBoundary(boundaryX, boundaryY);
                }

                // 상위 1.3% 밝기에 해당하는 픽셀값 찾기
                int pixelValue = GetPercentPixel(1.3);

                // 90%로 임계값 조정
                pixelValue = (int)(pixelValue * 0.90);
                if (pixelValue <= 0) pixelValue = 1;

                // 이진화
                MakeBinary(pixelValue);

                // 모폴로지 연산
                _processImage?.Dispose();
                _processImage = new Mat();

                var smallKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
                var openKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(33, 33));
                var closeKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(55, 55));

                // Open -> Close -> Open -> Close
                var temp = new Mat();
                Cv2.MorphologyEx(_binaryImage!, temp, MorphTypes.Open, smallKernel);
                Cv2.MorphologyEx(temp, _processImage, MorphTypes.Close, closeKernel);
                Cv2.MorphologyEx(_processImage, _processImage, MorphTypes.Open, openKernel);
                Cv2.MorphologyEx(_processImage, _processImage, MorphTypes.Close, closeKernel);
                temp.Dispose();

                // Connected Components 분석 (1차: 큰 영역)
                var labels = new Mat();
                var stats = new Mat();
                var centroids = new Mat();

                int numLabels = Cv2.ConnectedComponentsWithStats(_processImage, labels, stats, centroids);

                // 램프 중심 추출
                int count = 0;
                for (int i = 1; i < numLabels && count < LampCenterInfo.MAX_LAMP_NUM; i++) // 0은 배경
                {
                    int left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                    int top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                    int width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                    int height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);
                    int area = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

                    // 이미지 경계에 닿은 영역은 제외
                    if (left == 0 || top == 0 ||
                        left + width >= _currentImage.Width ||
                        top + height >= _currentImage.Height)
                        continue;

                    _centerInfo.BoundingRect[count] = new Rect(left, top, width, height);
                    _centerInfo.Area[count] = area;
                    _centerInfo.Center[count] = new Point2f(
                        (float)centroids.At<double>(i, 0),
                        (float)centroids.At<double>(i, 1));
                    count++;
                }
                _centerInfo.Count = count;

                // 크기 순서 정렬
                CalculateSizeOrder();

                // Connected Components 분석 (2차: 상세)
                var labels2 = new Mat();
                var stats2 = new Mat();
                var centroids2 = new Mat();

                int numLabels2 = Cv2.ConnectedComponentsWithStats(_binaryImage!, labels2, stats2, centroids2);

                // 경계 사각형 및 무게 중심 계산
                CalculateBoundaryAndCentroid(stats2, centroids2, numLabels2);

                labels.Dispose();
                stats.Dispose();
                centroids.Dispose();
                labels2.Dispose();
                stats2.Dispose();
                centroids2.Dispose();

                // Moving Average 적용
                CalculateAvgLampPositions();

                // 결과 설정
                result.LampCount = _centerInfo.Count;
                result.BoundaryCenter = GetBoundaryCenter(true);
                result.Centroid = GetLampCentroid(true);
                result.LeftLampCenter = GetLampCenterLeft(true);
                result.RightLampCenter = GetLampCenterRight(true);
                result.IsValid = _centerInfo.Count > 0;
                result.Message = _centerInfo.Count > 0 ? $"{_centerInfo.Count}개 램프 검출" : "램프를 찾을 수 없습니다.";
                result.BinaryImage = _binaryImage?.Clone();
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Message = $"정대 오류: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 마진 적용 (테두리 영역 0으로)
        /// </summary>
        private void ApplyMargin(int margin)
        {
            if (_currentImage == null) return;

            // 상단
            _currentImage.RowRange(0, margin).SetTo(0);
            // 하단
            _currentImage.RowRange(_currentImage.Height - margin, _currentImage.Height).SetTo(0);
            // 좌측
            _currentImage.ColRange(0, margin).SetTo(0);
            // 우측
            _currentImage.ColRange(_currentImage.Width - margin, _currentImage.Width).SetTo(0);
        }

        /// <summary>
        /// 경계 영역 적용
        /// </summary>
        private void ApplyBoundary(int boundaryX, int boundaryY)
        {
            if (_currentImage == null || (boundaryX == 0 && boundaryY == 0)) return;

            int centerX = _currentImage.Width / 2;
            int centerY = _currentImage.Height / 2;

            // 하단 절반에서 중앙 영역만 남기고 0으로 설정
            for (int y = centerY; y < _currentImage.Height; y++)
            {
                for (int x = 0; x < _currentImage.Width; x++)
                {
                    if (y < centerY + boundaryY)
                    {
                        if (x >= centerX - boundaryX && x <= centerX + boundaryX)
                            continue;
                    }
                    _currentImage.Set(y, x, (byte)0);
                }
            }
        }

        /// <summary>
        /// 크기 순서 계산
        /// </summary>
        private void CalculateSizeOrder()
        {
            // 초기화
            for (int i = 0; i < LampCenterInfo.MAX_LAMP_NUM; i++)
                _centerInfo.SizeOrder[i] = -1;

            // 크기 순서 계산
            for (int i = 0; i < _centerInfo.Count; i++)
            {
                int order = 0;
                for (int j = 0; j < _centerInfo.Count; j++)
                {
                    if (i != j && _centerInfo.Area[i] < _centerInfo.Area[j])
                        order++;
                }
                _centerInfo.SizeOrder[i] = order;
            }
        }

        /// <summary>
        /// 경계 사각형 및 무게 중심 계산
        /// </summary>
        private void CalculateBoundaryAndCentroid(Mat stats, Mat centroids, int numLabels)
        {
            if (_currentImage == null) return;

            int roiMargin = 50;
            int minArea = 225; // 15x15

            int bdLeft = _currentImage.Width;
            int bdTop = _currentImage.Height;
            int bdRight = 0;
            int bdBottom = 0;
            int sumArea = 0;
            Point2f centroid = new Point2f(0, 0);

            for (int i = 1; i < numLabels; i++)
            {
                int left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                int top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                int width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                int height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);
                int area = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);
                int right = left + width;
                int bottom = top + height;

                // ROI 범위 체크
                if (left < roiMargin || top < roiMargin ||
                    right > _currentImage.Width - roiMargin * 2 ||
                    bottom > _currentImage.Height - roiMargin * 2)
                    continue;

                // 최소 면적 체크
                if (area < minArea)
                    continue;

                float cx = (float)centroids.At<double>(i, 0);
                float cy = (float)centroids.At<double>(i, 1);

                // 경계 사각형 업데이트
                if (bdLeft > left) bdLeft = left;
                if (bdTop > top) bdTop = top;
                if (bdRight < right) bdRight = right;
                if (bdBottom < bottom) bdBottom = bottom;

                // 가중 무게 중심 계산
                centroid.X = (centroid.X * sumArea + cx * area) / (sumArea + area);
                centroid.Y = (centroid.Y * sumArea + cy * area) / (sumArea + area);
                sumArea += area;
            }

            _centerInfo.Boundary = new Rect(bdLeft, bdTop, bdRight - bdLeft, bdBottom - bdTop);
            _centerInfo.Centroid = centroid;
        }

        /// <summary>
        /// Moving Average 적용
        /// </summary>
        private void CalculateAvgLampPositions()
        {
            if (_avgWindowLen < 0 || _centerInfo.Count <= 0 || _avgThreshold < 0.0)
                return;

            // 각 램프 중심에 대해 Moving Average 적용
            for (int idx = 0; idx < _centerInfo.Count; idx++)
            {
                ApplyMovingAverage(idx, _centerInfo.Center[idx]);
            }

            // Boundary center
            Point2f boundaryCenter = new Point2f(
                _centerInfo.Boundary.X + _centerInfo.Boundary.Width / 2f,
                _centerInfo.Boundary.Y + _centerInfo.Boundary.Height / 2f);
            ApplyMovingAverage(LampCenterInfo.MAX_LAMP_NUM, boundaryCenter);

            // Centroid
            ApplyMovingAverage(LampCenterInfo.MAX_LAMP_NUM + 1, _centerInfo.Centroid);
        }

        private void ApplyMovingAverage(int idx, Point2f current)
        {
            if (_centerInfo.AvgSampleNum[idx] < _avgWindowLen)
                _centerInfo.AvgSampleNum[idx]++;

            if (_centerInfo.AvgSampleNum[idx] == 1)
            {
                _centerInfo.AvgCenter[idx] = current;
            }
            else
            {
                double dist = Math.Sqrt(
                    Math.Pow(_centerInfo.AvgCenter[idx].X - current.X, 2) +
                    Math.Pow(_centerInfo.AvgCenter[idx].Y - current.Y, 2));

                if (dist <= _avgThreshold)
                {
                    int n = _centerInfo.AvgSampleNum[idx];
                    _centerInfo.AvgCenter[idx].X =
                        (_centerInfo.AvgCenter[idx].X * (n - 1) + current.X) / n;
                    _centerInfo.AvgCenter[idx].Y =
                        (_centerInfo.AvgCenter[idx].Y * (n - 1) + current.Y) / n;
                }
                else
                {
                    // 임계값 초과 - 리셋
                    _centerInfo.AvgCenter[idx] = current;
                    _centerInfo.AvgSampleNum[idx] = 1;
                }
            }
        }

        #region Lamp Center Getters

        /// <summary>
        /// 가장 왼쪽 램프 중심
        /// </summary>
        public Point2f GetLampCenterLeft(bool useAverage = false)
        {
            if (_centerInfo.Count <= 0)
                return new Point2f(-1, -1);

            int idx = 0;
            for (int i = 1; i < _centerInfo.Count; i++)
            {
                if (_centerInfo.Center[i].X < _centerInfo.Center[idx].X)
                    idx = i;
            }

            return useAverage ? _centerInfo.AvgCenter[idx] : _centerInfo.Center[idx];
        }

        /// <summary>
        /// 가장 오른쪽 램프 중심
        /// </summary>
        public Point2f GetLampCenterRight(bool useAverage = false)
        {
            if (_centerInfo.Count <= 0)
                return new Point2f(-1, -1);

            int idx = 0;
            for (int i = 1; i < _centerInfo.Count; i++)
            {
                if (_centerInfo.Center[i].X > _centerInfo.Center[idx].X)
                    idx = i;
            }

            return useAverage ? _centerInfo.AvgCenter[idx] : _centerInfo.Center[idx];
        }

        /// <summary>
        /// 가장 위쪽 램프 중심
        /// </summary>
        public Point2f GetLampCenterTop(bool useAverage = false)
        {
            if (_centerInfo.Count <= 0)
                return new Point2f(-1, -1);

            int idx = 0;
            for (int i = 1; i < _centerInfo.Count; i++)
            {
                if (_centerInfo.Center[i].Y < _centerInfo.Center[idx].Y)
                    idx = i;
            }

            return useAverage ? _centerInfo.AvgCenter[idx] : _centerInfo.Center[idx];
        }

        /// <summary>
        /// 가장 아래쪽 램프 중심
        /// </summary>
        public Point2f GetLampCenterBottom(bool useAverage = false)
        {
            if (_centerInfo.Count <= 0)
                return new Point2f(-1, -1);

            int idx = 0;
            for (int i = 1; i < _centerInfo.Count; i++)
            {
                if (_centerInfo.Center[i].Y > _centerInfo.Center[idx].Y)
                    idx = i;
            }

            return useAverage ? _centerInfo.AvgCenter[idx] : _centerInfo.Center[idx];
        }

        /// <summary>
        /// 경계 사각형 중심
        /// </summary>
        public Point2f GetBoundaryCenter(bool useAverage = false)
        {
            if (useAverage && _centerInfo.Count > 0)
                return _centerInfo.AvgCenter[LampCenterInfo.MAX_LAMP_NUM];

            if (_centerInfo.Boundary.Width > 0 && _centerInfo.Boundary.Height > 0)
            {
                return new Point2f(
                    _centerInfo.Boundary.X + _centerInfo.Boundary.Width / 2f,
                    _centerInfo.Boundary.Y + _centerInfo.Boundary.Height / 2f);
            }

            // 경계가 없으면 가장 큰 램프 중심 반환
            return GetLampCenterBySizeOrder(0, useAverage);
        }

        /// <summary>
        /// 무게 중심
        /// </summary>
        public Point2f GetLampCentroid(bool useAverage = false)
        {
            if (useAverage && _centerInfo.Count > 0)
                return _centerInfo.AvgCenter[LampCenterInfo.MAX_LAMP_NUM + 1];

            return _centerInfo.Centroid;
        }

        /// <summary>
        /// 크기 순서로 램프 중심 가져오기
        /// </summary>
        public Point2f GetLampCenterBySizeOrder(int order, bool useAverage = false)
        {
            if (_centerInfo.Count <= 0 || order >= _centerInfo.Count)
                return new Point2f(-1, -1);

            // order에 해당하는 인덱스 찾기
            for (int i = 0; i < _centerInfo.Count; i++)
            {
                if (_centerInfo.SizeOrder[i] == order)
                {
                    return useAverage ? _centerInfo.AvgCenter[i] : _centerInfo.Center[i];
                }
            }

            return new Point2f(-1, -1);
        }

        /// <summary>
        /// 검출된 램프 수
        /// </summary>
        public int GetLampCount() => _centerInfo.Count;

        /// <summary>
        /// 경계 사각형
        /// </summary>
        public Rect GetBoundary() => _centerInfo.Boundary;

        #endregion

        public void Dispose()
        {
            if (_disposed) return;

            _currentImage?.Dispose();
            _binaryImage?.Dispose();
            _processImage?.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~HeadlampAligner()
        {
            Dispose();
        }
    }
}
