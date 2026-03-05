using System;
using OpenCvSharp;

namespace HeadLightInspection.ImageProcessing.Models
{
    /// <summary>
    /// 판정 결과
    /// </summary>
    public enum JudgmentStatus
    {
        NotTested = 0,
        Pass,       // 합격 (OK)
        Fail,       // 불합격 (NG)
        Warning     // 경고 (범위 근접)
    }

    /// <summary>
    /// 헤드램프 위치
    /// </summary>
    public enum HeadlampSide
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// 판정 결과 상세
    /// </summary>
    public class JudgmentResult
    {
        /// <summary>
        /// 전체 판정 결과
        /// </summary>
        public JudgmentStatus Status { get; set; } = JudgmentStatus.NotTested;

        /// <summary>
        /// 헤드램프 위치
        /// </summary>
        public HeadlampSide Side { get; set; }

        /// <summary>
        /// 빔 타입
        /// </summary>
        public BeamType BeamType { get; set; }

        #region 측정값 (픽셀)

        /// <summary>
        /// Hot Point 위치 (픽셀)
        /// </summary>
        public OpenCvSharp.Point HotPoint { get; set; }

        /// <summary>
        /// Hot Point 밝기값
        /// </summary>
        public int HotPointValue { get; set; }

        /// <summary>
        /// Cross Point 위치 (픽셀)
        /// </summary>
        public Point2f? CrossPoint { get; set; }

        #endregion

        #region 측정값 (각도, 분 단위)

        /// <summary>
        /// 수평 편차 (분 단위, 좌: -, 우: +)
        /// </summary>
        public double HorizontalDeviation { get; set; }

        /// <summary>
        /// 수직 편차 (분 단위, 상: -, 하: +)
        /// </summary>
        public double VerticalDeviation { get; set; }

        /// <summary>
        /// 광도 (cd)
        /// </summary>
        public int Candela { get; set; }

        #endregion

        #region 개별 항목 판정

        /// <summary>
        /// 수평 각도 판정
        /// </summary>
        public JudgmentStatus HorizontalStatus { get; set; } = JudgmentStatus.NotTested;

        /// <summary>
        /// 수직 각도 판정
        /// </summary>
        public JudgmentStatus VerticalStatus { get; set; } = JudgmentStatus.NotTested;

        /// <summary>
        /// 광도 판정
        /// </summary>
        public JudgmentStatus CandelaStatus { get; set; } = JudgmentStatus.NotTested;

        #endregion

        /// <summary>
        /// 판정 메시지
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 판정 시간
        /// </summary>
        public DateTime JudgmentTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 결과 문자열
        /// </summary>
        public override string ToString()
        {
            string result = Status switch
            {
                JudgmentStatus.Pass => "OK",
                JudgmentStatus.Fail => "NG",
                JudgmentStatus.Warning => "WARNING",
                _ => "NOT TESTED"
            };

            return $"[{Side} {BeamType}] {result} - H:{HorizontalDeviation:F1}' V:{VerticalDeviation:F1}' CD:{Candela}";
        }
    }

    /// <summary>
    /// 판정 로직
    /// </summary>
    public class JudgmentEngine
    {
        private readonly CalibrationData _calibration;
        private readonly StandardData _standard;

        public JudgmentEngine(CalibrationData calibration, StandardData standard)
        {
            _calibration = calibration;
            _standard = standard;
        }

        /// <summary>
        /// High Beam 판정
        /// </summary>
        public JudgmentResult JudgeHighBeam(AnalysisResult analysis, HeadlampSide side)
        {
            var result = new JudgmentResult
            {
                Side = side,
                BeamType = BeamType.HighBeam,
                HotPoint = analysis.HotPoint,
                HotPointValue = analysis.HotPointValue
            };

            // 픽셀을 각도로 변환
            var (angleX, angleY) = _calibration.PixelToAngleHigh(analysis.HotPoint.X, analysis.HotPoint.Y);
            result.HorizontalDeviation = angleX * 60; // 도 → 분
            result.VerticalDeviation = angleY * 60;

            // 판정 기준 선택 (좌/우)
            int limitUp, limitDown, limitLeft, limitRight;
            if (side == HeadlampSide.Left)
            {
                limitUp = _standard.HighLeftUp;
                limitDown = _standard.HighLeftDown;
                limitLeft = _standard.HighLeftLeft;
                limitRight = _standard.HighLeftRight;
            }
            else
            {
                limitUp = _standard.HighRightUp;
                limitDown = _standard.HighRightDown;
                limitLeft = _standard.HighRightLeft;
                limitRight = _standard.HighRightRight;
            }

            // 수평 판정
            if (result.HorizontalDeviation >= -limitLeft && result.HorizontalDeviation <= limitRight)
            {
                result.HorizontalStatus = JudgmentStatus.Pass;
            }
            else
            {
                result.HorizontalStatus = JudgmentStatus.Fail;
            }

            // 수직 판정
            if (result.VerticalDeviation >= -limitUp && result.VerticalDeviation <= limitDown)
            {
                result.VerticalStatus = JudgmentStatus.Pass;
            }
            else
            {
                result.VerticalStatus = JudgmentStatus.Fail;
            }

            // 광도 측정 및 판정
            result.Candela = _calibration.PixelToCandelaHigh(analysis.HotPointValue);

            // High Beam 광도 기준: 2등식은 HighCD2DS, 4등식은 HighCD4DS
            int candelaLimit = _standard.HighCD4DS; // 4등식 기준 사용
            if (result.Candela >= candelaLimit)
            {
                result.CandelaStatus = JudgmentStatus.Pass;
            }
            else
            {
                result.CandelaStatus = JudgmentStatus.Fail;
            }

            // 전체 판정
            if (result.HorizontalStatus == JudgmentStatus.Pass &&
                result.VerticalStatus == JudgmentStatus.Pass &&
                result.CandelaStatus == JudgmentStatus.Pass)
            {
                result.Status = JudgmentStatus.Pass;
                result.Message = "합격";
            }
            else
            {
                result.Status = JudgmentStatus.Fail;
                result.Message = "불합격 - ";
                if (result.HorizontalStatus == JudgmentStatus.Fail)
                    result.Message += "수평 범위 초과 ";
                if (result.VerticalStatus == JudgmentStatus.Fail)
                    result.Message += "수직 범위 초과 ";
                if (result.CandelaStatus == JudgmentStatus.Fail)
                    result.Message += "광도 미달 ";
            }

            return result;
        }

        /// <summary>
        /// Low Beam 판정
        /// </summary>
        public JudgmentResult JudgeLowBeam(AnalysisResult analysis, HeadlampSide side)
        {
            var result = new JudgmentResult
            {
                Side = side,
                BeamType = BeamType.LowBeam,
                HotPoint = analysis.HotPoint,
                HotPointValue = analysis.HotPointValue
            };

            // Cross Point가 있으면 Cross Point 기준, 없으면 Hot Point 기준
            Point2f measurePoint;
            if (analysis.CrossPoints.Count > 0)
            {
                result.CrossPoint = analysis.CrossPoints[0];
                measurePoint = analysis.CrossPoints[0];
            }
            else
            {
                measurePoint = new Point2f(analysis.HotPoint.X, analysis.HotPoint.Y);
            }

            // 픽셀을 각도로 변환
            var (angleX, angleY) = _calibration.PixelToAngleLow((int)measurePoint.X, (int)measurePoint.Y);
            result.HorizontalDeviation = angleX * 60; // 도 → 분
            result.VerticalDeviation = angleY * 60;

            // 수평 판정
            if (result.HorizontalDeviation >= _standard.LowHorizontalMin &&
                result.HorizontalDeviation <= _standard.LowHorizontalMax)
            {
                result.HorizontalStatus = JudgmentStatus.Pass;
            }
            else
            {
                result.HorizontalStatus = JudgmentStatus.Fail;
            }

            // 수직(Cutoff) 판정
            if (result.VerticalDeviation >= _standard.LowCutoffMin &&
                result.VerticalDeviation <= _standard.LowCutoffMax)
            {
                result.VerticalStatus = JudgmentStatus.Pass;
            }
            else
            {
                result.VerticalStatus = JudgmentStatus.Fail;
            }

            // 광도 측정 및 판정
            result.Candela = _calibration.PixelToCandelaLow(analysis.HotPointValue);

            // Low Beam 광도 기준: LowCDMin ~ LowCDMax 범위
            if (result.Candela >= _standard.LowCDMin && result.Candela <= _standard.LowCDMax)
            {
                result.CandelaStatus = JudgmentStatus.Pass;
            }
            else
            {
                result.CandelaStatus = JudgmentStatus.Fail;
            }

            // 전체 판정
            if (result.HorizontalStatus == JudgmentStatus.Pass &&
                result.VerticalStatus == JudgmentStatus.Pass &&
                result.CandelaStatus == JudgmentStatus.Pass)
            {
                result.Status = JudgmentStatus.Pass;
                result.Message = "합격";
            }
            else
            {
                result.Status = JudgmentStatus.Fail;
                result.Message = "불합격 - ";
                if (result.HorizontalStatus == JudgmentStatus.Fail)
                    result.Message += "수평 범위 초과 ";
                if (result.VerticalStatus == JudgmentStatus.Fail)
                    result.Message += "Cutoff 범위 초과 ";
                if (result.CandelaStatus == JudgmentStatus.Fail)
                    result.Message += "광도 범위 초과 ";
            }

            return result;
        }

        /// <summary>
        /// 허용 범위 원 반지름 계산 (픽셀)
        /// </summary>
        public (int radiusX, int radiusY) GetToleranceRadius(BeamType beamType, HeadlampSide side)
        {
            double pixelsPerDegree;
            int limitHorizontal, limitVertical;

            if (beamType == BeamType.HighBeam)
            {
                pixelsPerDegree = _calibration.GetPixelsPerDegreeHighLeft();

                if (side == HeadlampSide.Left)
                {
                    limitHorizontal = Math.Max(_standard.HighLeftLeft, _standard.HighLeftRight);
                    limitVertical = Math.Max(_standard.HighLeftUp, _standard.HighLeftDown);
                }
                else
                {
                    limitHorizontal = Math.Max(_standard.HighRightLeft, _standard.HighRightRight);
                    limitVertical = Math.Max(_standard.HighRightUp, _standard.HighRightDown);
                }
            }
            else
            {
                pixelsPerDegree = _calibration.GetPixelsPerDegreeLowLeft();
                limitHorizontal = Math.Max(Math.Abs(_standard.LowHorizontalMin), _standard.LowHorizontalMax);
                limitVertical = Math.Max(Math.Abs(_standard.LowCutoffMin), _standard.LowCutoffMax);
            }

            // 분 → 도 → 픽셀
            int radiusX = (int)(limitHorizontal / 60.0 * pixelsPerDegree);
            int radiusY = (int)(limitVertical / 60.0 * pixelsPerDegree);

            return (radiusX, radiusY);
        }
    }
}
