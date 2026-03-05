using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HeadLightInspection.ImageProcessing.Helpers
{
    /// <summary>
    /// INI 파일 읽기/쓰기 헬퍼
    /// </summary>
    public static class IniFileHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(
            string section, string key, string defaultValue,
            StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(
            string section, string key, string value, string filePath);

        /// <summary>
        /// INI 파일에서 정수 값 읽기
        /// </summary>
        public static int ReadInt(string section, string key, string filePath, int defaultValue = 0)
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, defaultValue.ToString(), sb, 255, filePath);

            if (int.TryParse(sb.ToString(), out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// INI 파일에서 문자열 값 읽기
        /// </summary>
        public static string ReadString(string section, string key, string filePath, string defaultValue = "")
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, defaultValue, sb, 255, filePath);
            return sb.ToString();
        }

        /// <summary>
        /// INI 파일에 정수 값 쓰기
        /// </summary>
        public static bool WriteInt(string section, string key, int value, string filePath)
        {
            return WritePrivateProfileString(section, key, value.ToString(), filePath) != 0;
        }

        /// <summary>
        /// INI 파일에 문자열 값 쓰기
        /// </summary>
        public static bool WriteString(string section, string key, string value, string filePath)
        {
            return WritePrivateProfileString(section, key, value, filePath) != 0;
        }

        /// <summary>
        /// 기본 Calibration.ini 파일 생성
        /// </summary>
        public static void CreateDefaultCalibrationFile(string filePath)
        {
            // HIGH_CD 섹션
            WriteInt("HIGH_CD", "ZERO", 1000, filePath);
            WriteInt("HIGH_CD", "10000", 75, filePath);
            WriteInt("HIGH_CD", "20000", 747, filePath);
            WriteInt("HIGH_CD", "30000", 1568, filePath);
            WriteInt("HIGH_CD", "40000", 2382, filePath);
            WriteInt("HIGH_CD", "50000", 3196, filePath);
            WriteInt("HIGH_CD", "60000", 3982, filePath);
            WriteInt("HIGH_CD", "70000", 4672, filePath);
            WriteInt("HIGH_CD", "80000", 5194, filePath);
            WriteInt("HIGH_CD", "90000", 0, filePath);
            WriteInt("HIGH_CD", "100000", 0, filePath);

            // HIGH_ANGLE 섹션
            WriteInt("HIGH_ANGLE", "ZEROX", 640, filePath);
            WriteInt("HIGH_ANGLE", "ZEROY", 480, filePath);
            WriteInt("HIGH_ANGLE", "LEFT1_X", 605, filePath);
            WriteInt("HIGH_ANGLE", "LEFT1_Y", 480, filePath);
            WriteInt("HIGH_ANGLE", "LEFT2_X", 570, filePath);
            WriteInt("HIGH_ANGLE", "LEFT2_Y", 480, filePath);
            WriteInt("HIGH_ANGLE", "RIGHT1_X", 675, filePath);
            WriteInt("HIGH_ANGLE", "RIGHT1_Y", 480, filePath);
            WriteInt("HIGH_ANGLE", "RIGHT2_X", 710, filePath);
            WriteInt("HIGH_ANGLE", "RIGHT2_Y", 480, filePath);
            WriteInt("HIGH_ANGLE", "UP1_X", 640, filePath);
            WriteInt("HIGH_ANGLE", "UP1_Y", 445, filePath);
            WriteInt("HIGH_ANGLE", "UP2_X", 640, filePath);
            WriteInt("HIGH_ANGLE", "UP2_Y", 410, filePath);
            WriteInt("HIGH_ANGLE", "DOWN1_X", 640, filePath);
            WriteInt("HIGH_ANGLE", "DOWN1_Y", 515, filePath);
            WriteInt("HIGH_ANGLE", "DOWN2_X", 640, filePath);
            WriteInt("HIGH_ANGLE", "DOWN2_Y", 550, filePath);

            // LOW_CD 섹션
            WriteInt("LOW_CD", "ZERO", 500, filePath);
            WriteInt("LOW_CD", "2000", 1000, filePath);
            WriteInt("LOW_CD", "5000", 1500, filePath);
            WriteInt("LOW_CD", "8000", 2000, filePath);
            WriteInt("LOW_CD", "10000", 2500, filePath);
            WriteInt("LOW_CD", "15000", 3000, filePath);
            WriteInt("LOW_CD", "20000", 0, filePath);
            WriteInt("LOW_CD", "25000", 0, filePath);
            WriteInt("LOW_CD", "30000", 0, filePath);
            WriteInt("LOW_CD", "35000", 0, filePath);
            WriteInt("LOW_CD", "40000", 0, filePath);

            // LOW_ANGLE 섹션
            WriteInt("LOW_ANGLE", "ZEROX", 640, filePath);
            WriteInt("LOW_ANGLE", "ZEROY", 512, filePath);
            WriteInt("LOW_ANGLE", "LEFT1_X", 600, filePath);
            WriteInt("LOW_ANGLE", "LEFT1_Y", 512, filePath);
            WriteInt("LOW_ANGLE", "LEFT2_X", 560, filePath);
            WriteInt("LOW_ANGLE", "LEFT2_Y", 512, filePath);
            WriteInt("LOW_ANGLE", "RIGHT1_X", 680, filePath);
            WriteInt("LOW_ANGLE", "RIGHT1_Y", 512, filePath);
            WriteInt("LOW_ANGLE", "RIGHT2_X", 720, filePath);
            WriteInt("LOW_ANGLE", "RIGHT2_Y", 512, filePath);
            WriteInt("LOW_ANGLE", "UP1_X", 640, filePath);
            WriteInt("LOW_ANGLE", "UP1_Y", 472, filePath);
            WriteInt("LOW_ANGLE", "UP2_X", 640, filePath);
            WriteInt("LOW_ANGLE", "UP2_Y", 432, filePath);
            WriteInt("LOW_ANGLE", "DOWN1_X", 640, filePath);
            WriteInt("LOW_ANGLE", "DOWN1_Y", 552, filePath);
            WriteInt("LOW_ANGLE", "DOWN2_X", 640, filePath);
            WriteInt("LOW_ANGLE", "DOWN2_Y", 592, filePath);
        }
    }
}
