using System;
using System.IO;
using HeadLightInspection.ImageProcessing.Helpers;

namespace HeadLightInspection.ImageProcessing.Models
{
    /// <summary>
    /// Calibration 데이터
    /// C++ stCalibration 구조체 및 Calibration.ini 참조
    /// 픽셀 좌표를 각도(도/분)로 변환하는 데 사용
    /// </summary>
    public class CalibrationData
    {
        #region High Beam Calibration

        /// <summary>
        /// High Beam 기준점 X (픽셀)
        /// </summary>
        public int HighZeroX { get; set; } = 640;

        /// <summary>
        /// High Beam 기준점 Y (픽셀)
        /// </summary>
        public int HighZeroY { get; set; } = 480;

        // 좌측 각도 캘리브레이션 (1도, 2도 위치)
        public int HighAngleL1X { get; set; }
        public int HighAngleL1Y { get; set; }
        public int HighAngleL2X { get; set; }
        public int HighAngleL2Y { get; set; }

        // 우측 각도 캘리브레이션
        public int HighAngleR1X { get; set; }
        public int HighAngleR1Y { get; set; }
        public int HighAngleR2X { get; set; }
        public int HighAngleR2Y { get; set; }

        // 상단 각도 캘리브레이션
        public int HighAngleU1X { get; set; }
        public int HighAngleU1Y { get; set; }
        public int HighAngleU2X { get; set; }
        public int HighAngleU2Y { get; set; }

        // 하단 각도 캘리브레이션
        public int HighAngleD1X { get; set; }
        public int HighAngleD1Y { get; set; }
        public int HighAngleD2X { get; set; }
        public int HighAngleD2Y { get; set; }

        /// <summary>
        /// High Beam 광도 캘리브레이션 (cd 값)
        /// </summary>
        public int[] HighCD { get; set; } = new int[10];

        public int HighCDZero { get; set; }

        #endregion

        #region Low Beam Calibration

        /// <summary>
        /// Low Beam 기준점 X (픽셀)
        /// </summary>
        public int LowZeroX { get; set; } = 640;

        /// <summary>
        /// Low Beam 기준점 Y (픽셀)
        /// </summary>
        public int LowZeroY { get; set; } = 480;

        // 좌측 각도 캘리브레이션
        public int LowAngleL1X { get; set; }
        public int LowAngleL1Y { get; set; }
        public int LowAngleL2X { get; set; }
        public int LowAngleL2Y { get; set; }

        // 우측 각도 캘리브레이션
        public int LowAngleR1X { get; set; }
        public int LowAngleR1Y { get; set; }
        public int LowAngleR2X { get; set; }
        public int LowAngleR2Y { get; set; }

        // 상단 각도 캘리브레이션
        public int LowAngleU1X { get; set; }
        public int LowAngleU1Y { get; set; }
        public int LowAngleU2X { get; set; }
        public int LowAngleU2Y { get; set; }

        // 하단 각도 캘리브레이션
        public int LowAngleD1X { get; set; }
        public int LowAngleD1Y { get; set; }
        public int LowAngleD2X { get; set; }
        public int LowAngleD2Y { get; set; }

        /// <summary>
        /// Low Beam 광도 캘리브레이션 (cd 값)
        /// </summary>
        public int[] LowCD { get; set; } = new int[10];

        public int LowCDZero { get; set; }
        public int LowCD2000 { get; set; }
        public int LowCD5000 { get; set; }
        public int LowCD8000 { get; set; }
        public int LowCD10000 { get; set; }
        public int LowCD15000 { get; set; }

        #endregion

        #region 광도 캘리브레이션 테이블

        /// <summary>
        /// High Beam 광도 캘리브레이션 테이블 (cd 값)
        /// 인덱스 0~9: 10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000, 100000 cd
        /// </summary>
        public int[] HighCDKeys { get; set; } = { 10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000, 100000 };

        /// <summary>
        /// Low Beam 광도 캘리브레이션 테이블 (cd 값)
        /// 인덱스 0~9: 2000, 5000, 8000, 10000, 15000, 20000, 25000, 30000, 35000, 40000 cd
        /// </summary>
        public int[] LowCDKeys { get; set; } = { 2000, 5000, 8000, 10000, 15000, 20000, 25000, 30000, 35000, 40000 };

        #endregion

        #region 변환 메서드

        /// <summary>
        /// 1도당 픽셀 수 계산 (수평, High Beam 좌측 기준)
        /// </summary>
        public double GetPixelsPerDegreeHighLeft()
        {
            int diff = Math.Abs(HighZeroX - HighAngleL1X);
            return diff > 0 ? diff : 100; // 기본값 100
        }

        /// <summary>
        /// 1도당 픽셀 수 계산 (수평, High Beam 우측 기준)
        /// </summary>
        public double GetPixelsPerDegreeHighRight()
        {
            int diff = Math.Abs(HighZeroX - HighAngleR1X);
            return diff > 0 ? diff : 100;
        }

        /// <summary>
        /// 1도당 픽셀 수 계산 (수직, High Beam 상단 기준)
        /// </summary>
        public double GetPixelsPerDegreeHighUp()
        {
            int diff = Math.Abs(HighZeroY - HighAngleU1Y);
            return diff > 0 ? diff : 100;
        }

        /// <summary>
        /// 1도당 픽셀 수 계산 (수직, High Beam 하단 기준)
        /// </summary>
        public double GetPixelsPerDegreeHighDown()
        {
            int diff = Math.Abs(HighZeroY - HighAngleD1Y);
            return diff > 0 ? diff : 100;
        }

        /// <summary>
        /// 1도당 픽셀 수 계산 (수평, Low Beam 좌측 기준)
        /// </summary>
        public double GetPixelsPerDegreeLowLeft()
        {
            int diff = Math.Abs(LowZeroX - LowAngleL1X);
            return diff > 0 ? diff : 100;
        }

        /// <summary>
        /// 1도당 픽셀 수 계산 (수평, Low Beam 우측 기준)
        /// </summary>
        public double GetPixelsPerDegreeLowRight()
        {
            int diff = Math.Abs(LowZeroX - LowAngleR1X);
            return diff > 0 ? diff : 100;
        }

        /// <summary>
        /// 픽셀 좌표를 각도로 변환 (High Beam)
        /// </summary>
        /// <param name="pixelX">픽셀 X 좌표</param>
        /// <param name="pixelY">픽셀 Y 좌표</param>
        /// <returns>각도 (도 단위)</returns>
        public (double angleX, double angleY) PixelToAngleHigh(int pixelX, int pixelY)
        {
            double pixelsPerDegreeX = pixelX < HighZeroX ? GetPixelsPerDegreeHighLeft() : GetPixelsPerDegreeHighRight();
            double pixelsPerDegreeY = pixelY < HighZeroY ? GetPixelsPerDegreeHighUp() : GetPixelsPerDegreeHighDown();

            double angleX = (pixelX - HighZeroX) / pixelsPerDegreeX;
            double angleY = (pixelY - HighZeroY) / pixelsPerDegreeY;

            return (angleX, angleY);
        }

        /// <summary>
        /// 픽셀 좌표를 각도로 변환 (Low Beam)
        /// </summary>
        public (double angleX, double angleY) PixelToAngleLow(int pixelX, int pixelY)
        {
            double pixelsPerDegreeX = pixelX < LowZeroX ? GetPixelsPerDegreeLowLeft() : GetPixelsPerDegreeLowRight();
            double pixelsPerDegreeY = 100; // Low Beam Y 방향은 별도 계산 필요

            double angleX = (pixelX - LowZeroX) / pixelsPerDegreeX;
            double angleY = (pixelY - LowZeroY) / pixelsPerDegreeY;

            return (angleX, angleY);
        }

        /// <summary>
        /// 각도를 픽셀 좌표로 변환 (High Beam)
        /// </summary>
        public (int pixelX, int pixelY) AngleToPixelHigh(double angleX, double angleY)
        {
            double pixelsPerDegreeX = angleX < 0 ? GetPixelsPerDegreeHighLeft() : GetPixelsPerDegreeHighRight();
            double pixelsPerDegreeY = angleY < 0 ? GetPixelsPerDegreeHighUp() : GetPixelsPerDegreeHighDown();

            int pixelX = HighZeroX + (int)(angleX * pixelsPerDegreeX);
            int pixelY = HighZeroY + (int)(angleY * pixelsPerDegreeY);

            return (pixelX, pixelY);
        }

        #endregion

        /// <summary>
        /// 픽셀 밝기 값을 광도(cd)로 변환 (High Beam)
        /// </summary>
        /// <param name="pixelValue">Hot Point의 픽셀 밝기 값</param>
        /// <returns>광도 (cd)</returns>
        public int PixelToCandela(int pixelValue, bool isHighBeam = true)
        {
            int[] keys = isHighBeam ? HighCDKeys : LowCDKeys;
            int[] values = isHighBeam ? HighCD : LowCD;
            int zeroValue = isHighBeam ? HighCDZero : LowCDZero;

            // 유효한 캘리브레이션 데이터 카운트
            int count = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > 0) count++;
                else break;
            }

            if (count == 0) return pixelValue; // 캘리브레이션 없으면 원본 반환

            // 제로 보정
            pixelValue = Math.Max(0, pixelValue - zeroValue);

            // 범위 외 처리
            if (pixelValue <= values[0])
            {
                // 최소값 이하: 비례 계산
                if (values[0] == 0) return 0;
                return (int)(pixelValue * ((double)keys[0] / values[0]));
            }
            else if (pixelValue >= values[count - 1])
            {
                // 최대값 이상: 비례 계산
                if (values[count - 1] == 0) return keys[count - 1];
                return (int)(pixelValue * ((double)keys[count - 1] / values[count - 1]));
            }

            // 선형 보간
            for (int i = 1; i < count; i++)
            {
                if (pixelValue <= values[i])
                {
                    // 기울기 계산
                    double span = (double)(keys[i] - keys[i - 1]) / (values[i] - values[i - 1]);
                    // 보간값 계산
                    return keys[i - 1] + (int)((pixelValue - values[i - 1]) * span);
                }
            }

            return pixelValue;
        }

        /// <summary>
        /// 픽셀 밝기 값을 광도(cd)로 변환 (High Beam)
        /// </summary>
        public int PixelToCandelaHigh(int pixelValue)
        {
            return PixelToCandela(pixelValue, true);
        }

        /// <summary>
        /// 픽셀 밝기 값을 광도(cd)로 변환 (Low Beam)
        /// </summary>
        public int PixelToCandelaLow(int pixelValue)
        {
            return PixelToCandela(pixelValue, false);
        }


        #region 기본값 생성

        /// <summary>
        /// 기본 캘리브레이션 생성
        /// </summary>
        public static CalibrationData CreateDefault(int imageWidth = 1280, int imageHeight = 960)
        {
            int centerX = imageWidth / 2;
            int centerY = imageHeight / 2;
            int pixelsPerDegree = 100; // 1도당 100픽셀 (기본값)

            return new CalibrationData
            {
                HighZeroX = centerX,
                HighZeroY = centerY,
                HighAngleL1X = centerX - pixelsPerDegree,
                HighAngleL1Y = centerY,
                HighAngleL2X = centerX - pixelsPerDegree * 2,
                HighAngleL2Y = centerY,
                HighAngleR1X = centerX + pixelsPerDegree,
                HighAngleR1Y = centerY,
                HighAngleR2X = centerX + pixelsPerDegree * 2,
                HighAngleR2Y = centerY,
                HighAngleU1X = centerX,
                HighAngleU1Y = centerY - pixelsPerDegree,
                HighAngleU2X = centerX,
                HighAngleU2Y = centerY - pixelsPerDegree * 2,
                HighAngleD1X = centerX,
                HighAngleD1Y = centerY + pixelsPerDegree,
                HighAngleD2X = centerX,
                HighAngleD2Y = centerY + pixelsPerDegree * 2,

                LowZeroX = centerX,
                LowZeroY = centerY,
                LowAngleL1X = centerX - pixelsPerDegree,
                LowAngleL1Y = centerY,
                LowAngleL2X = centerX - pixelsPerDegree * 2,
                LowAngleL2Y = centerY,
                LowAngleR1X = centerX + pixelsPerDegree,
                LowAngleR1Y = centerY,
                LowAngleR2X = centerX + pixelsPerDegree * 2,
                LowAngleR2Y = centerY
            };
        }

        #endregion

        #region INI 파일 로드/저장

        /// <summary>
        /// Calibration.ini 파일에서 로드
        /// </summary>
        public static CalibrationData LoadFromIni(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // 파일 없으면 기본값으로 생성
                IniFileHelper.CreateDefaultCalibrationFile(filePath);
            }

            var data = new CalibrationData();

            // HIGH_CD 섹션
            data.HighCDZero = IniFileHelper.ReadInt("HIGH_CD", "ZERO", filePath, 1000);
            data.HighCD[0] = IniFileHelper.ReadInt("HIGH_CD", "10000", filePath);
            data.HighCD[1] = IniFileHelper.ReadInt("HIGH_CD", "20000", filePath);
            data.HighCD[2] = IniFileHelper.ReadInt("HIGH_CD", "30000", filePath);
            data.HighCD[3] = IniFileHelper.ReadInt("HIGH_CD", "40000", filePath);
            data.HighCD[4] = IniFileHelper.ReadInt("HIGH_CD", "50000", filePath);
            data.HighCD[5] = IniFileHelper.ReadInt("HIGH_CD", "60000", filePath);
            data.HighCD[6] = IniFileHelper.ReadInt("HIGH_CD", "70000", filePath);
            data.HighCD[7] = IniFileHelper.ReadInt("HIGH_CD", "80000", filePath);
            data.HighCD[8] = IniFileHelper.ReadInt("HIGH_CD", "90000", filePath);
            data.HighCD[9] = IniFileHelper.ReadInt("HIGH_CD", "100000", filePath);

            // HIGH_ANGLE 섹션
            data.HighZeroX = IniFileHelper.ReadInt("HIGH_ANGLE", "ZEROX", filePath, 640);
            data.HighZeroY = IniFileHelper.ReadInt("HIGH_ANGLE", "ZEROY", filePath, 480);
            data.HighAngleL1X = IniFileHelper.ReadInt("HIGH_ANGLE", "LEFT1_X", filePath);
            data.HighAngleL1Y = IniFileHelper.ReadInt("HIGH_ANGLE", "LEFT1_Y", filePath);
            data.HighAngleL2X = IniFileHelper.ReadInt("HIGH_ANGLE", "LEFT2_X", filePath);
            data.HighAngleL2Y = IniFileHelper.ReadInt("HIGH_ANGLE", "LEFT2_Y", filePath);
            data.HighAngleR1X = IniFileHelper.ReadInt("HIGH_ANGLE", "RIGHT1_X", filePath);
            data.HighAngleR1Y = IniFileHelper.ReadInt("HIGH_ANGLE", "RIGHT1_Y", filePath);
            data.HighAngleR2X = IniFileHelper.ReadInt("HIGH_ANGLE", "RIGHT2_X", filePath);
            data.HighAngleR2Y = IniFileHelper.ReadInt("HIGH_ANGLE", "RIGHT2_Y", filePath);
            data.HighAngleU1X = IniFileHelper.ReadInt("HIGH_ANGLE", "UP1_X", filePath);
            data.HighAngleU1Y = IniFileHelper.ReadInt("HIGH_ANGLE", "UP1_Y", filePath);
            data.HighAngleU2X = IniFileHelper.ReadInt("HIGH_ANGLE", "UP2_X", filePath);
            data.HighAngleU2Y = IniFileHelper.ReadInt("HIGH_ANGLE", "UP2_Y", filePath);
            data.HighAngleD1X = IniFileHelper.ReadInt("HIGH_ANGLE", "DOWN1_X", filePath);
            data.HighAngleD1Y = IniFileHelper.ReadInt("HIGH_ANGLE", "DOWN1_Y", filePath);
            data.HighAngleD2X = IniFileHelper.ReadInt("HIGH_ANGLE", "DOWN2_X", filePath);
            data.HighAngleD2Y = IniFileHelper.ReadInt("HIGH_ANGLE", "DOWN2_Y", filePath);

            // LOW_CD 섹션
            data.LowCDZero = IniFileHelper.ReadInt("LOW_CD", "ZERO", filePath, 500);
            data.LowCD[0] = IniFileHelper.ReadInt("LOW_CD", "2000", filePath);
            data.LowCD[1] = IniFileHelper.ReadInt("LOW_CD", "5000", filePath);
            data.LowCD[2] = IniFileHelper.ReadInt("LOW_CD", "8000", filePath);
            data.LowCD[3] = IniFileHelper.ReadInt("LOW_CD", "10000", filePath);
            data.LowCD[4] = IniFileHelper.ReadInt("LOW_CD", "15000", filePath);
            data.LowCD[5] = IniFileHelper.ReadInt("LOW_CD", "20000", filePath);
            data.LowCD[6] = IniFileHelper.ReadInt("LOW_CD", "25000", filePath);
            data.LowCD[7] = IniFileHelper.ReadInt("LOW_CD", "30000", filePath);
            data.LowCD[8] = IniFileHelper.ReadInt("LOW_CD", "35000", filePath);
            data.LowCD[9] = IniFileHelper.ReadInt("LOW_CD", "40000", filePath);

            // LOW_ANGLE 섹션
            data.LowZeroX = IniFileHelper.ReadInt("LOW_ANGLE", "ZEROX", filePath, 640);
            data.LowZeroY = IniFileHelper.ReadInt("LOW_ANGLE", "ZEROY", filePath, 512);
            data.LowAngleL1X = IniFileHelper.ReadInt("LOW_ANGLE", "LEFT1_X", filePath);
            data.LowAngleL1Y = IniFileHelper.ReadInt("LOW_ANGLE", "LEFT1_Y", filePath);
            data.LowAngleL2X = IniFileHelper.ReadInt("LOW_ANGLE", "LEFT2_X", filePath);
            data.LowAngleL2Y = IniFileHelper.ReadInt("LOW_ANGLE", "LEFT2_Y", filePath);
            data.LowAngleR1X = IniFileHelper.ReadInt("LOW_ANGLE", "RIGHT1_X", filePath);
            data.LowAngleR1Y = IniFileHelper.ReadInt("LOW_ANGLE", "RIGHT1_Y", filePath);
            data.LowAngleR2X = IniFileHelper.ReadInt("LOW_ANGLE", "RIGHT2_X", filePath);
            data.LowAngleR2Y = IniFileHelper.ReadInt("LOW_ANGLE", "RIGHT2_Y", filePath);
            data.LowAngleU1X = IniFileHelper.ReadInt("LOW_ANGLE", "UP1_X", filePath);
            data.LowAngleU1Y = IniFileHelper.ReadInt("LOW_ANGLE", "UP1_Y", filePath);
            data.LowAngleU2X = IniFileHelper.ReadInt("LOW_ANGLE", "UP2_X", filePath);
            data.LowAngleU2Y = IniFileHelper.ReadInt("LOW_ANGLE", "UP2_Y", filePath);
            data.LowAngleD1X = IniFileHelper.ReadInt("LOW_ANGLE", "DOWN1_X", filePath);
            data.LowAngleD1Y = IniFileHelper.ReadInt("LOW_ANGLE", "DOWN1_Y", filePath);
            data.LowAngleD2X = IniFileHelper.ReadInt("LOW_ANGLE", "DOWN2_X", filePath);
            data.LowAngleD2Y = IniFileHelper.ReadInt("LOW_ANGLE", "DOWN2_Y", filePath);

            return data;
        }

        /// <summary>
        /// Calibration.ini 파일로 저장
        /// </summary>
        public void SaveToIni(string filePath)
        {
            // HIGH_CD 섹션
            IniFileHelper.WriteInt("HIGH_CD", "ZERO", HighCDZero, filePath);
            IniFileHelper.WriteInt("HIGH_CD", "10000", HighCD[0], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "20000", HighCD[1], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "30000", HighCD[2], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "40000", HighCD[3], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "50000", HighCD[4], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "60000", HighCD[5], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "70000", HighCD[6], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "80000", HighCD[7], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "90000", HighCD[8], filePath);
            IniFileHelper.WriteInt("HIGH_CD", "100000", HighCD[9], filePath);

            // HIGH_ANGLE 섹션
            IniFileHelper.WriteInt("HIGH_ANGLE", "ZEROX", HighZeroX, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "ZEROY", HighZeroY, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT1_X", HighAngleL1X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT1_Y", HighAngleL1Y, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT2_X", HighAngleL2X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "LEFT2_Y", HighAngleL2Y, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT1_X", HighAngleR1X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT1_Y", HighAngleR1Y, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT2_X", HighAngleR2X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "RIGHT2_Y", HighAngleR2Y, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "UP1_X", HighAngleU1X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "UP1_Y", HighAngleU1Y, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "UP2_X", HighAngleU2X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "UP2_Y", HighAngleU2Y, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN1_X", HighAngleD1X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN1_Y", HighAngleD1Y, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN2_X", HighAngleD2X, filePath);
            IniFileHelper.WriteInt("HIGH_ANGLE", "DOWN2_Y", HighAngleD2Y, filePath);

            // LOW_CD 섹션
            IniFileHelper.WriteInt("LOW_CD", "ZERO", LowCDZero, filePath);
            IniFileHelper.WriteInt("LOW_CD", "2000", LowCD[0], filePath);
            IniFileHelper.WriteInt("LOW_CD", "5000", LowCD[1], filePath);
            IniFileHelper.WriteInt("LOW_CD", "8000", LowCD[2], filePath);
            IniFileHelper.WriteInt("LOW_CD", "10000", LowCD[3], filePath);
            IniFileHelper.WriteInt("LOW_CD", "15000", LowCD[4], filePath);
            IniFileHelper.WriteInt("LOW_CD", "20000", LowCD[5], filePath);
            IniFileHelper.WriteInt("LOW_CD", "25000", LowCD[6], filePath);
            IniFileHelper.WriteInt("LOW_CD", "30000", LowCD[7], filePath);
            IniFileHelper.WriteInt("LOW_CD", "35000", LowCD[8], filePath);
            IniFileHelper.WriteInt("LOW_CD", "40000", LowCD[9], filePath);

            // LOW_ANGLE 섹션
            IniFileHelper.WriteInt("LOW_ANGLE", "ZEROX", LowZeroX, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "ZEROY", LowZeroY, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "LEFT1_X", LowAngleL1X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "LEFT1_Y", LowAngleL1Y, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "LEFT2_X", LowAngleL2X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "LEFT2_Y", LowAngleL2Y, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT1_X", LowAngleR1X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT1_Y", LowAngleR1Y, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT2_X", LowAngleR2X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "RIGHT2_Y", LowAngleR2Y, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "UP1_X", LowAngleU1X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "UP1_Y", LowAngleU1Y, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "UP2_X", LowAngleU2X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "UP2_Y", LowAngleU2Y, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "DOWN1_X", LowAngleD1X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "DOWN1_Y", LowAngleD1Y, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "DOWN2_X", LowAngleD2X, filePath);
            IniFileHelper.WriteInt("LOW_ANGLE", "DOWN2_Y", LowAngleD2Y, filePath);
        }

        #endregion
    }
}
