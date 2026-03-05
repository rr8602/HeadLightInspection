using System;

namespace HeadLightInspection.ImageProcessing.Models
{
    /// <summary>
    /// Hot Point 알고리즘 타입
    /// </summary>
    public enum KernelType
    {
        AVR3X3 = 0,
        AVR5X5,
        AVR7X7,
        AVR9X9,
        GAU3X3,
        GAU5X5,
        NONE
    }

    /// <summary>
    /// 핸들 위치
    /// </summary>
    public enum HandlePosition
    {
        Left = 0,   // 좌핸들
        Right = 1   // 우핸들
    }

    /// <summary>
    /// 차종별 모델 파라미터
    /// C++ tbl_Spec 테이블 및 stParam 구조체 참조
    /// </summary>
    public class ModelParameter
    {
        /// <summary>
        /// 모델명 (차종)
        /// </summary>
        public string ModelName { get; set; } = "DEFAULT";

        // Hot Point 필터링 파라미터
        /// <summary>
        /// Hot Point 필터 윈도우 길이
        /// </summary>
        public int HotPointFilterCount { get; set; } = 50;

        /// <summary>
        /// Hot Point 거리 임계값 (픽셀)
        /// </summary>
        public int HotPointThreshold { get; set; } = 30;

        /// <summary>
        /// Hot Point 알고리즘
        /// </summary>
        public KernelType HotPointAlgorithm { get; set; } = KernelType.AVR5X5;

        // Cutoff Line 필터링 파라미터
        /// <summary>
        /// Cutoff Line 필터 윈도우 길이
        /// </summary>
        public int CutoffLineFilterCount { get; set; } = 50;

        /// <summary>
        /// Cutoff Line 거리 임계값 (픽셀)
        /// </summary>
        public int CutoffLineThreshold { get; set; } = 30;

        /// <summary>
        /// Cutoff Line 알고리즘
        /// </summary>
        public KernelType CutoffLineAlgorithm { get; set; } = KernelType.AVR5X5;

        // 오프셋 (좌/우 헤드램프별)
        /// <summary>
        /// 좌측 헤드램프 X 오프셋
        /// </summary>
        public int OffsetLeftX { get; set; } = 0;

        /// <summary>
        /// 좌측 헤드램프 Y 오프셋
        /// </summary>
        public int OffsetLeftY { get; set; } = 0;

        /// <summary>
        /// 우측 헤드램프 X 오프셋
        /// </summary>
        public int OffsetRightX { get; set; } = 0;

        /// <summary>
        /// 우측 헤드램프 Y 오프셋
        /// </summary>
        public int OffsetRightY { get; set; } = 0;

        /// <summary>
        /// 노출 시간 (밀리초)
        /// </summary>
        public double ExposureTime { get; set; } = 10.0;

        /// <summary>
        /// 핸들 위치 (좌/우핸들)
        /// </summary>
        public HandlePosition HandlePosition { get; set; } = HandlePosition.Left;

        // RANSAC 파라미터
        /// <summary>
        /// RANSAC 시도 횟수
        /// </summary>
        public int RansacTryCount { get; set; } = 200;

        /// <summary>
        /// RANSAC 거리 허용 오차
        /// </summary>
        public double RansacTolerance { get; set; } = 3.0;

        // ROI (Region of Interest)
        /// <summary>
        /// ROI X 좌표
        /// </summary>
        public int RoiX { get; set; } = 0;

        /// <summary>
        /// ROI Y 좌표
        /// </summary>
        public int RoiY { get; set; } = 0;

        /// <summary>
        /// ROI 너비
        /// </summary>
        public int RoiWidth { get; set; } = 0;

        /// <summary>
        /// ROI 높이
        /// </summary>
        public int RoiHeight { get; set; } = 0;

        /// <summary>
        /// 기본 파라미터 생성
        /// </summary>
        public static ModelParameter CreateDefault()
        {
            return new ModelParameter
            {
                ModelName = "DEFAULT",
                HotPointFilterCount = 50,
                HotPointThreshold = 30,
                HotPointAlgorithm = KernelType.AVR5X5,
                CutoffLineFilterCount = 50,
                CutoffLineThreshold = 30,
                CutoffLineAlgorithm = KernelType.AVR5X5,
                ExposureTime = 10.0,
                HandlePosition = HandlePosition.Left,
                RansacTryCount = 200,
                RansacTolerance = 3.0
            };
        }

        /// <summary>
        /// 파라미터 복사
        /// </summary>
        public ModelParameter Clone()
        {
            return new ModelParameter
            {
                ModelName = this.ModelName,
                HotPointFilterCount = this.HotPointFilterCount,
                HotPointThreshold = this.HotPointThreshold,
                HotPointAlgorithm = this.HotPointAlgorithm,
                CutoffLineFilterCount = this.CutoffLineFilterCount,
                CutoffLineThreshold = this.CutoffLineThreshold,
                CutoffLineAlgorithm = this.CutoffLineAlgorithm,
                OffsetLeftX = this.OffsetLeftX,
                OffsetLeftY = this.OffsetLeftY,
                OffsetRightX = this.OffsetRightX,
                OffsetRightY = this.OffsetRightY,
                ExposureTime = this.ExposureTime,
                HandlePosition = this.HandlePosition,
                RansacTryCount = this.RansacTryCount,
                RansacTolerance = this.RansacTolerance,
                RoiX = this.RoiX,
                RoiY = this.RoiY,
                RoiWidth = this.RoiWidth,
                RoiHeight = this.RoiHeight
            };
        }
    }
}
