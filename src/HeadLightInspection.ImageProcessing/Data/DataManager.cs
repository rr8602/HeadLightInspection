using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HeadLightInspection.ImageProcessing.Models;

namespace HeadLightInspection.ImageProcessing.Data
{
    /// <summary>
    /// 파라미터, 캘리브레이션, 판정 기준 데이터 관리
    /// C++ DBMng 클래스 참조
    /// </summary>
    public class DataManager
    {
        private readonly string _dataPath;
        private Dictionary<string, ModelParameter> _modelParameters;
        private Dictionary<string, StandardData> _standardDataList;
        private CalibrationData _calibration;

        /// <summary>
        /// 로드된 모델 목록
        /// </summary>
        public List<string> ModelNames => new List<string>(_modelParameters.Keys);

        /// <summary>
        /// 현재 캘리브레이션 데이터
        /// </summary>
        public CalibrationData Calibration => _calibration;

        public DataManager(string? dataPath = null)
        {
            _dataPath = dataPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _modelParameters = new Dictionary<string, ModelParameter>();
            _standardDataList = new Dictionary<string, StandardData>();
            _calibration = CalibrationData.CreateDefault();

            // 데이터 폴더 생성
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        #region 모델 파라미터 (tbl_Model)

        /// <summary>
        /// 모든 모델 파라미터 로드
        /// </summary>
        public void LoadAllModelParameters()
        {
            string filePath = Path.Combine(_dataPath, "ModelParameters.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var list = JsonSerializer.Deserialize<List<ModelParameter>>(json);
                    _modelParameters.Clear();

                    if (list != null)
                    {
                        foreach (var param in list)
                        {
                            _modelParameters[param.ModelName] = param;
                        }
                    }
                }
                catch (Exception)
                {
                    CreateDefaultModelParameters();
                }
            }
            else
            {
                CreateDefaultModelParameters();
                SaveAllModelParameters();
            }
        }

        /// <summary>
        /// 특정 모델 파라미터 조회
        /// </summary>
        public ModelParameter GetModelParameter(string modelName)
        {
            if (_modelParameters.TryGetValue(modelName, out var param))
            {
                return param;
            }

            // DEFAULT 반환
            if (_modelParameters.TryGetValue("DEFAULT", out var defaultParam))
            {
                return defaultParam;
            }

            return ModelParameter.CreateDefault();
        }

        /// <summary>
        /// 모델 파라미터 저장
        /// </summary>
        public void SaveModelParameter(ModelParameter param)
        {
            _modelParameters[param.ModelName] = param;
            SaveAllModelParameters();
        }

        /// <summary>
        /// 모든 모델 파라미터 저장
        /// </summary>
        public void SaveAllModelParameters()
        {
            string filePath = Path.Combine(_dataPath, "ModelParameters.json");
            var list = new List<ModelParameter>(_modelParameters.Values);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(filePath, json);
        }

        private void CreateDefaultModelParameters()
        {
            _modelParameters.Clear();
            _modelParameters["DEFAULT"] = ModelParameter.CreateDefault();

            // 샘플 모델 추가
            var sample1 = ModelParameter.CreateDefault();
            sample1.ModelName = "MODEL_A_LHD";
            sample1.HandlePosition = HandlePosition.Left;
            _modelParameters[sample1.ModelName] = sample1;

            var sample2 = ModelParameter.CreateDefault();
            sample2.ModelName = "MODEL_A_RHD";
            sample2.HandlePosition = HandlePosition.Right;
            _modelParameters[sample2.ModelName] = sample2;
        }

        #endregion

        #region 판정 기준 (tbl_StdData)

        /// <summary>
        /// 모든 판정 기준 로드
        /// </summary>
        public void LoadAllStandardData()
        {
            string filePath = Path.Combine(_dataPath, "StandardData.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var list = JsonSerializer.Deserialize<List<StandardData>>(json);
                    _standardDataList.Clear();

                    if (list != null)
                    {
                        foreach (var std in list)
                        {
                            _standardDataList[std.ModelName] = std;
                        }
                    }
                }
                catch (Exception)
                {
                    CreateDefaultStandardData();
                }
            }
            else
            {
                CreateDefaultStandardData();
                SaveAllStandardData();
            }
        }

        /// <summary>
        /// 특정 모델의 판정 기준 조회
        /// </summary>
        public StandardData GetStandardData(string modelName)
        {
            if (_standardDataList.TryGetValue(modelName, out var std))
            {
                return std;
            }

            // DEFAULT 반환
            if (_standardDataList.TryGetValue("DEFAULT", out var defaultStd))
            {
                return defaultStd;
            }

            return StandardData.CreateDefault();
        }

        /// <summary>
        /// 판정 기준 저장
        /// </summary>
        public void SaveStandardData(StandardData std)
        {
            _standardDataList[std.ModelName] = std;
            SaveAllStandardData();
        }

        /// <summary>
        /// 모든 판정 기준 저장
        /// </summary>
        public void SaveAllStandardData()
        {
            string filePath = Path.Combine(_dataPath, "StandardData.json");
            var list = new List<StandardData>(_standardDataList.Values);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(list, options);
            File.WriteAllText(filePath, json);
        }

        private void CreateDefaultStandardData()
        {
            _standardDataList.Clear();
            _standardDataList["DEFAULT"] = StandardData.CreateDefault();

            // 샘플 추가
            var sample1 = StandardData.CreateDefault();
            sample1.ModelName = "MODEL_A_LHD";
            _standardDataList[sample1.ModelName] = sample1;

            var sample2 = StandardData.CreateDefault();
            sample2.ModelName = "MODEL_A_RHD";
            _standardDataList[sample2.ModelName] = sample2;
        }

        #endregion

        #region 캘리브레이션 (Calibration.ini)

        /// <summary>
        /// 캘리브레이션 데이터 로드
        /// </summary>
        public void LoadCalibration()
        {
            string filePath = Path.Combine(_dataPath, "Calibration.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    _calibration = JsonSerializer.Deserialize<CalibrationData>(json) ?? CalibrationData.CreateDefault();
                }
                catch (Exception)
                {
                    _calibration = CalibrationData.CreateDefault();
                }
            }
            else
            {
                _calibration = CalibrationData.CreateDefault();
                SaveCalibration();
            }
        }

        /// <summary>
        /// 캘리브레이션 데이터 저장
        /// </summary>
        public void SaveCalibration()
        {
            string filePath = Path.Combine(_dataPath, "Calibration.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_calibration, options);
            File.WriteAllText(filePath, json);
        }

        #endregion

        #region 전체 로드/저장

        /// <summary>
        /// 모든 데이터 로드
        /// </summary>
        public void LoadAll()
        {
            LoadAllModelParameters();
            LoadAllStandardData();
            LoadCalibration();
        }

        /// <summary>
        /// 모든 데이터 저장
        /// </summary>
        public void SaveAll()
        {
            SaveAllModelParameters();
            SaveAllStandardData();
            SaveCalibration();
        }

        #endregion
    }
}
