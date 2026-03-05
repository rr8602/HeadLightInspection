using System;

namespace HeadLightInspection.ImageProcessing.Models
{
    /// <summary>
    /// 합/불 판정 기준 데이터
    /// C++ tbl_StdData 테이블 및 stStdData 구조체 참조
    /// 모델(차종)별로 다른 판정 기준을 가짐
    /// </summary>
    public class StandardData
    {
        /// <summary>
        /// 모델명 (차종)
        /// </summary>
        public string ModelName { get; set; } = "DEFAULT";

        #region High Beam 판정 기준

        /// <summary>
        /// High Beam 광도 기준 (2등식, cd)
        /// </summary>
        public int HighCD2DS { get; set; } = 15000;

        /// <summary>
        /// High Beam 광도 기준 (4등식, cd)
        /// </summary>
        public int HighCD4DS { get; set; } = 12000;

        // 좌측 헤드램프 각도 허용 범위 (분 단위)
        /// <summary>
        /// 좌측 상한
        /// </summary>
        public int HighLeftUp { get; set; } = 30;
        /// <summary>
        /// 좌측 하한
        /// </summary>
        public int HighLeftDown { get; set; } = 30;
        /// <summary>
        /// 좌측 좌한
        /// </summary>
        public int HighLeftLeft { get; set; } = 60;
        /// <summary>
        /// 좌측 우한
        /// </summary>
        public int HighLeftRight { get; set; } = 60;

        // 우측 헤드램프 각도 허용 범위 (분 단위)
        public int HighRightUp { get; set; } = 30;
        public int HighRightDown { get; set; } = 30;
        public int HighRightLeft { get; set; } = 60;
        public int HighRightRight { get; set; } = 60;

        #endregion

        #region Low Beam 판정 기준

        /// <summary>
        /// Low Beam 광도 최대값 (cd)
        /// </summary>
        public int LowCDMax { get; set; } = 40000;

        /// <summary>
        /// Low Beam 광도 최소값 (cd)
        /// </summary>
        public int LowCDMin { get; set; } = 10000;

        /// <summary>
        /// Low Beam 수평 각도 최소값 (분)
        /// </summary>
        public int LowHorizontalMin { get; set; } = -60;

        /// <summary>
        /// Low Beam 수평 각도 최대값 (분)
        /// </summary>
        public int LowHorizontalMax { get; set; } = 60;

        /// <summary>
        /// Low Beam Cutoff 각도 최소값 (분)
        /// </summary>
        public int LowCutoffMin { get; set; } = -30;

        /// <summary>
        /// Low Beam Cutoff 각도 최대값 (분)
        /// </summary>
        public int LowCutoffMax { get; set; } = 30;

        // Low Beam 높이 범위 (3단계)
        public int LowHeightMin1 { get; set; } = 0;
        public int LowHeightMax1 { get; set; } = 100;
        public int LowHeightMin2 { get; set; } = 0;
        public int LowHeightMax2 { get; set; } = 100;
        public int LowHeightMin3 { get; set; } = 0;
        public int LowHeightMax3 { get; set; } = 100;

        // Low Beam 범위 (3단계, 분 단위)
        public double LowRangeMin1 { get; set; } = -30;
        public double LowRangeMax1 { get; set; } = 30;
        public double LowRangeMin2 { get; set; } = -30;
        public double LowRangeMax2 { get; set; } = 30;
        public double LowRangeMin3 { get; set; } = -30;
        public double LowRangeMax3 { get; set; } = 30;

        #endregion

        #region 수동 검사 기준 (좌/우 헤드램프별)

        // 좌측 헤드램프
        public int ManualLeftUp { get; set; } = 30;
        public int ManualLeftDown { get; set; } = 30;
        public int ManualLeftLeft { get; set; } = 60;
        public int ManualLeftRight { get; set; } = 60;

        // 우측 헤드램프
        public int ManualRightUp { get; set; } = 30;
        public int ManualRightDown { get; set; } = 30;
        public int ManualRightLeft { get; set; } = 60;
        public int ManualRightRight { get; set; } = 60;

        #endregion

        /// <summary>
        /// 기본 판정 기준 생성
        /// </summary>
        public static StandardData CreateDefault()
        {
            return new StandardData
            {
                ModelName = "DEFAULT",
                HighCD2DS = 15000,
                HighCD4DS = 12000,
                HighLeftUp = 30,
                HighLeftDown = 30,
                HighLeftLeft = 60,
                HighLeftRight = 60,
                HighRightUp = 30,
                HighRightDown = 30,
                HighRightLeft = 60,
                HighRightRight = 60,
                LowCDMax = 40000,
                LowCDMin = 10000,
                LowHorizontalMin = -60,
                LowHorizontalMax = 60,
                LowCutoffMin = -30,
                LowCutoffMax = 30
            };
        }
    }
}
