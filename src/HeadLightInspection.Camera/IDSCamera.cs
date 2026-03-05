using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using uEye;
using uEye.Defines;
using uEye.Types;

namespace HeadLightInspection.Camera
{
    /// <summary>
    /// uEye SDK를 사용한 IDS 카메라 제어 클래스
    /// </summary>
    public class IDSCamera : IDisposable
    {
        #region Native API (P/Invoke)

        private const string UEYE_DLL = "ueye_api_64.dll";

        // Blacklevel 명령어
        private const uint IS_BLACKLEVEL_CMD_GET_CAPS = 1;
        private const uint IS_BLACKLEVEL_CMD_GET_OFFSET_RANGE = 6;
        private const uint IS_BLACKLEVEL_CMD_GET_OFFSET = 7;
        private const uint IS_BLACKLEVEL_CMD_SET_OFFSET = 8;
        private const uint IS_BLACKLEVEL_CMD_GET_MODE = 3;
        private const uint IS_BLACKLEVEL_CMD_SET_MODE = 4;

        // Auto Parameter 명령어
        private const uint IS_SET_ENABLE_AUTO_GAIN = 0x8800;
        private const uint IS_GET_ENABLE_AUTO_GAIN = 0x8801;
        private const uint IS_SET_ENABLE_AUTO_SHUTTER = 0x8802;
        private const uint IS_GET_ENABLE_AUTO_SHUTTER = 0x8803;
        private const uint IS_SET_AUTO_REFERENCE = 0x8000;
        private const uint IS_GET_AUTO_REFERENCE = 0x8001;
        private const uint IS_SET_AUTO_GAIN_MAX = 0x8002;
        private const uint IS_GET_AUTO_GAIN_MAX = 0x8003;
        private const uint IS_SET_AUTO_SHUTTER_MAX = 0x8004;
        private const uint IS_GET_AUTO_SHUTTER_MAX = 0x8005;

        [StructLayout(LayoutKind.Sequential)]
        private struct IS_RANGE_S32
        {
            public int s32Min;
            public int s32Max;
            public int s32Inc;
        }

        [DllImport(UEYE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int is_Blacklevel(int hCam, uint nCommand, ref int pParam, uint cbSizeOfParam);

        [DllImport(UEYE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int is_Blacklevel(int hCam, uint nCommand, ref IS_RANGE_S32 pParam, uint cbSizeOfParam);

        [DllImport(UEYE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int is_SetAutoParameter(int hCam, int param, ref double pval1, ref double pval2);

        [DllImport(UEYE_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int is_GetCameraHandle(IntPtr hCamera, out int hCam);

        #endregion

        #region Events
        public event EventHandler<Bitmap>? ImageReceived;
        public event EventHandler<(uint frameCount, uint errorCount)>? CounterChanged;
        public event EventHandler<string>? ErrorOccurred;
        #endregion

        #region Private Fields
        private uEye.Camera? _camera;
        private int _cameraHandle;  // 네이티브 API 호출용 카메라 핸들
        private volatile bool _isRunning;
        private uint _frameCounter;
        private uint _errorCounter;
        private bool _disposed;

        private int _memoryId;
        #endregion

        #region Properties
        public bool IsConnected => _camera != null && _camera.Information.GetCameraInfo(out _) == Status.Success;
        public bool IsAcquiring => _isRunning;
        public uint FrameCounter => _frameCounter;
        public uint ErrorCounter => _errorCounter;
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        public int BitsPerPixel { get; private set; }
        public bool IsMonoMode { get; private set; }
        #endregion

        #region Constructor
        public IDSCamera()
        {
            Debug.WriteLine("[IDSCamera] Created");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 사용 가능한 카메라 목록 조회
        /// </summary>
        public static List<string> GetAvailableCameras()
        {
            var cameras = new List<string>();

            uEye.Info.Camera.GetCameraList(out uEye.Types.CameraInformation[] cameraList);

            if (cameraList != null)
            {
                foreach (var cam in cameraList)
                {
                    cameras.Add($"{cam.Model} (ID: {cam.CameraID}, S/N: {cam.SerialNumber})");
                }
            }

            return cameras;
        }

        /// <summary>
        /// 카메라 연결
        /// </summary>
        public bool Open(int cameraIndex = 0)
        {
            try
            {
                // 카메라 목록 조회
                uEye.Info.Camera.GetCameraList(out uEye.Types.CameraInformation[] cameraList);

                if (cameraList == null || cameraList.Length == 0)
                {
                    OnError("카메라를 찾을 수 없습니다.");
                    return false;
                }

                if (cameraIndex >= cameraList.Length)
                {
                    OnError($"카메라 인덱스 {cameraIndex}가 유효하지 않습니다.");
                    return false;
                }

                // 카메라 초기화
                _camera = new uEye.Camera();
                _cameraHandle = (int)cameraList[cameraIndex].CameraID;
                Status status = _camera.Init(_cameraHandle);

                if (status != Status.Success)
                {
                    OnError($"카메라 초기화 실패: {status}");
                    _camera = null;
                    _cameraHandle = 0;
                    return false;
                }

                // Init 후 실제 핸들 값 얻기 (ID와 다를 수 있음)
                _camera.Information.GetCameraInfo(out uEye.Types.CameraInfo info);
                _cameraHandle = (int)info.CameraID;
                Debug.WriteLine($"[IDSCamera] Camera opened: {cameraList[cameraIndex].Model}, Handle: {_cameraHandle}");

                // Mono8 모드 설정 (그레이스케일)
                Status pixelStatus = _camera.PixelFormat.Set(uEye.Defines.ColorMode.Mono8);
                if (pixelStatus != Status.Success)
                {
                    // Mono8 지원 안 하면 BGR8로 fallback
                    Debug.WriteLine($"[IDSCamera] Mono8 not supported (Status: {pixelStatus}), falling back to BGR8");
                    _camera.PixelFormat.Set(uEye.Defines.ColorMode.BGR8Packed);
                    BitsPerPixel = 24;
                    IsMonoMode = false;
                }
                else
                {
                    Debug.WriteLine($"[IDSCamera] Mono8 mode set successfully");
                    BitsPerPixel = 8;
                    IsMonoMode = true;
                }

                // 실제 설정된 컬러 모드 확인
                _camera.PixelFormat.Get(out uEye.Defines.ColorMode currentMode);
                Debug.WriteLine($"[IDSCamera] Current color mode: {currentMode}, BitsPerPixel: {BitsPerPixel}, IsMonoMode: {IsMonoMode}");

                // 이미지 크기 가져오기
                _camera.Size.AOI.Get(out Rectangle aoi);
                ImageWidth = aoi.Width;
                ImageHeight = aoi.Height;

                Debug.WriteLine($"[IDSCamera] Image size: {ImageWidth}x{ImageHeight}");

                // 메모리 할당
                AllocateMemory();

                return true;
            }
            catch (Exception ex)
            {
                OnError($"카메라 연결 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 카메라 연결 해제
        /// </summary>
        public void Close()
        {
            StopAcquisition();

            if (_camera != null)
            {
                try
                {
                    FreeMemory();
                    _camera.Exit();
                    _camera = null;
                    Debug.WriteLine("[IDSCamera] Camera closed");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[IDSCamera] Close error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 이미지 취득 시작
        /// </summary>
        public bool StartAcquisition()
        {
            if (_camera == null)
            {
                OnError("카메라가 연결되어 있지 않습니다.");
                return false;
            }

            if (_isRunning) return true;

            try
            {
                _frameCounter = 0;
                _errorCounter = 0;

                // 프레임 이벤트 등록
                _camera.EventFrame += Camera_EventFrame;

                // 라이브 비디오 시작
                Status status = _camera.Acquisition.Capture();
                if (status != Status.Success)
                {
                    OnError($"취득 시작 실패: {status}");
                    _camera.EventFrame -= Camera_EventFrame;
                    return false;
                }

                _isRunning = true;

                Debug.WriteLine("[IDSCamera] Acquisition started");
                return true;
            }
            catch (Exception ex)
            {
                OnError($"취득 시작 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 이미지 취득 중지
        /// </summary>
        public void StopAcquisition()
        {
            if (!_isRunning) return;

            _isRunning = false;

            if (_camera != null)
            {
                try
                {
                    _camera.EventFrame -= Camera_EventFrame;
                    _camera.Acquisition.Stop();
                    Debug.WriteLine("[IDSCamera] Acquisition stopped");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[IDSCamera] Stop error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 노출 시간 설정 (밀리초)
        /// </summary>
        public bool SetExposureTime(double exposureTimeMs)
        {
            if (_camera == null) return false;

            try
            {
                // 범위 확인
                _camera.Timing.Exposure.GetRange(out double min, out double max, out double inc);
                exposureTimeMs = Math.Clamp(exposureTimeMs, min, max);

                Status status = _camera.Timing.Exposure.Set(exposureTimeMs);
                if (status == Status.Success)
                {
                    Debug.WriteLine($"[IDSCamera] Exposure set to {exposureTimeMs} ms");
                    return true;
                }
                else
                {
                    OnError($"노출 설정 실패: {status}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnError($"노출 설정 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 노출 시간 가져오기 (밀리초)
        /// </summary>
        public double GetExposureTime()
        {
            if (_camera == null) return 0;

            try
            {
                _camera.Timing.Exposure.Get(out double exposure);
                return exposure;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 노출 범위 가져오기 (밀리초)
        /// </summary>
        public (double min, double max) GetExposureRange()
        {
            if (_camera == null) return (0, 0);

            try
            {
                _camera.Timing.Exposure.GetRange(out double min, out double max, out double inc);
                return (min, max);
            }
            catch
            {
                return (0, 0);
            }
        }

        /// <summary>
        /// 게인(Gain) 설정 (0~100)
        /// </summary>
        public bool SetGain(int gain)
        {
            if (_camera == null) return false;

            try
            {
                gain = Math.Clamp(gain, 0, 100);
                Status status = _camera.Gain.Hardware.Scaled.SetMaster(gain);
                if (status == Status.Success)
                {
                    Debug.WriteLine($"[IDSCamera] Gain set to {gain}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"[IDSCamera] Gain set failed: {status}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] Gain set error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 게인(Gain) 가져오기
        /// </summary>
        public int GetGain()
        {
            if (_camera == null) return 0;

            try
            {
                _camera.Gain.Hardware.Scaled.GetMaster(out int gain);
                return gain;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 자동 밝기 제어 비활성화 및 수동 모드 설정
        /// </summary>
        public bool DisableAutoFeatures()
        {
            if (_camera == null) return false;

            try
            {
                // 자동 게인 비활성화
                _camera.AutoFeatures.Software.Gain.SetEnable(false);
                // 자동 화이트밸런스 비활성화
                _camera.AutoFeatures.Software.WhiteBalance.SetEnable(false);

                Debug.WriteLine("[IDSCamera] Auto features disabled");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] DisableAutoFeatures error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 완전 암흑 모드 설정
        /// IDS Camera Manager 기준: 밝기=0, 노출한계=최소, 이득한계=0, 노출시간=최소, 게인=0, 흑색레벨=0
        /// </summary>
        public bool SetDarkMode()
        {
            if (_camera == null) return false;

            try
            {
                // 1. 자동 기능 비활성화
                SetAutoGainEnabled(false);
                SetAutoExposureEnabled(false);

                // 2. 자동 밝기 목표값 0 (IDS Camera Manager: 밝기)
                SetAutoBrightnessReference(0);

                // 3. 자동 노출 한계 최소값 (IDS Camera Manager: 노출 한계)
                var (minExp, _) = GetExposureRange();
                SetAutoExposureMax(minExp);

                // 4. 자동 게인 한계 0 (IDS Camera Manager: 이득 한계)
                SetAutoGainMax(0);

                // 5. 노출 시간 최소값 (IDS Camera Manager: 노출시간)
                _camera.Timing.Exposure.GetRange(out double minExposure, out _, out _);
                _camera.Timing.Exposure.Set(minExposure);
                Debug.WriteLine($"[IDSCamera] Exposure set to min: {minExposure}ms");

                // 6. 마스터 게인 0 (IDS Camera Manager: 게인)
                _camera.Gain.Hardware.Scaled.SetMaster(0);
                Debug.WriteLine("[IDSCamera] Master gain set to 0");

                // 7. 흑색레벨 0 (IDS Camera Manager: 흑색레벨 상쇄)
                SetBlacklevelOffset(0);

                // 8. RGB 게인도 0으로 (컬러 카메라인 경우)
                try
                {
                    _camera.Gain.Hardware.Scaled.SetRed(0);
                    _camera.Gain.Hardware.Scaled.SetGreen(0);
                    _camera.Gain.Hardware.Scaled.SetBlue(0);
                }
                catch { }

                // 9. 감마 기본값 (1.0 = 100)
                try { _camera.Gamma.Software.Set(100); } catch { }

                // 10. 부스트 게인 비활성화
                try { _camera.Gain.Hardware.Boost.SetEnable(false); } catch { }

                Debug.WriteLine("[IDSCamera] Dark mode enabled - all brightness settings minimized");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SetDarkMode error: {ex.Message}");
                return false;
            }
        }

        #region 흑색레벨 (Blacklevel) 제어

        /// <summary>
        /// 흑색레벨 오프셋 범위 조회
        /// </summary>
        public (int min, int max, int inc) GetBlacklevelOffsetRange()
        {
            if (_cameraHandle == 0) return (0, 255, 1);

            try
            {
                IS_RANGE_S32 range = new IS_RANGE_S32();
                int result = is_Blacklevel(_cameraHandle, IS_BLACKLEVEL_CMD_GET_OFFSET_RANGE,
                    ref range, (uint)Marshal.SizeOf(typeof(IS_RANGE_S32)));

                if (result == 0) // IS_SUCCESS
                {
                    Debug.WriteLine($"[IDSCamera] Blacklevel range: {range.s32Min} ~ {range.s32Max}");
                    return (range.s32Min, range.s32Max, range.s32Inc);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] GetBlacklevelOffsetRange error: {ex.Message}");
            }

            return (0, 255, 1);
        }

        /// <summary>
        /// 흑색레벨 오프셋 설정
        /// </summary>
        public bool SetBlacklevelOffset(int offset)
        {
            if (_cameraHandle == 0) return false;

            try
            {
                int result = is_Blacklevel(_cameraHandle, IS_BLACKLEVEL_CMD_SET_OFFSET,
                    ref offset, sizeof(int));
                Debug.WriteLine($"[IDSCamera] Blacklevel offset set to {offset}: result={result}");
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SetBlacklevelOffset error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 흑색레벨 오프셋 조회
        /// </summary>
        public int GetBlacklevelOffset()
        {
            if (_cameraHandle == 0) return 0;

            try
            {
                int offset = 0;
                int result = is_Blacklevel(_cameraHandle, IS_BLACKLEVEL_CMD_GET_OFFSET,
                    ref offset, sizeof(int));
                if (result == 0) return offset;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] GetBlacklevelOffset error: {ex.Message}");
            }

            return 0;
        }

        #endregion

        #region 자동 밝기/노출/게인 제어

        /// <summary>
        /// 자동 밝기 기준값 설정 (IDS Camera Manager: 밝기 0~255)
        /// </summary>
        public bool SetAutoBrightnessReference(int reference)
        {
            if (_cameraHandle == 0) return false;

            try
            {
                double val1 = reference;
                double val2 = 0;
                int result = is_SetAutoParameter(_cameraHandle, (int)IS_SET_AUTO_REFERENCE, ref val1, ref val2);
                Debug.WriteLine($"[IDSCamera] Auto brightness reference set to {reference}: result={result}");
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SetAutoBrightnessReference error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 자동 밝기 기준값 조회
        /// </summary>
        public int GetAutoBrightnessReference()
        {
            if (_cameraHandle == 0) return 128;

            try
            {
                double val1 = 0;
                double val2 = 0;
                int result = is_SetAutoParameter(_cameraHandle, (int)IS_GET_AUTO_REFERENCE, ref val1, ref val2);
                if (result == 0) return (int)val1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] GetAutoBrightnessReference error: {ex.Message}");
            }

            return 128;
        }

        /// <summary>
        /// 자동 노출 활성화/비활성화
        /// </summary>
        public bool SetAutoExposureEnabled(bool enabled)
        {
            if (_cameraHandle == 0) return false;

            try
            {
                double val1 = enabled ? 1 : 0;
                double val2 = 0;
                int result = is_SetAutoParameter(_cameraHandle, (int)IS_SET_ENABLE_AUTO_SHUTTER, ref val1, ref val2);
                Debug.WriteLine($"[IDSCamera] Auto exposure {(enabled ? "enabled" : "disabled")}: result={result}");
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SetAutoExposureEnabled error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 자동 노출 최대값 설정 (IDS Camera Manager: 노출 한계)
        /// </summary>
        public bool SetAutoExposureMax(double maxMs)
        {
            if (_cameraHandle == 0) return false;

            try
            {
                double val1 = maxMs;
                double val2 = 0;
                int result = is_SetAutoParameter(_cameraHandle, (int)IS_SET_AUTO_SHUTTER_MAX, ref val1, ref val2);
                Debug.WriteLine($"[IDSCamera] Auto exposure max set to {maxMs}ms: result={result}");
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SetAutoExposureMax error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 자동 게인 활성화/비활성화
        /// </summary>
        public bool SetAutoGainEnabled(bool enabled)
        {
            if (_cameraHandle == 0) return false;

            try
            {
                double val1 = enabled ? 1 : 0;
                double val2 = 0;
                int result = is_SetAutoParameter(_cameraHandle, (int)IS_SET_ENABLE_AUTO_GAIN, ref val1, ref val2);
                Debug.WriteLine($"[IDSCamera] Auto gain {(enabled ? "enabled" : "disabled")}: result={result}");
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SetAutoGainEnabled error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 자동 게인 최대값 설정 (IDS Camera Manager: 이득 한계)
        /// </summary>
        public bool SetAutoGainMax(int maxPercent)
        {
            if (_cameraHandle == 0) return false;

            try
            {
                double val1 = maxPercent;
                double val2 = 0;
                int result = is_SetAutoParameter(_cameraHandle, (int)IS_SET_AUTO_GAIN_MAX, ref val1, ref val2);
                Debug.WriteLine($"[IDSCamera] Auto gain max set to {maxPercent}%: result={result}");
                return result == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SetAutoGainMax error: {ex.Message}");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// IDS Camera Manager에서 저장한 파라미터 파일 로드
        /// </summary>
        /// <param name="filePath">파라미터 파일 경로 (.ini)</param>
        public bool LoadParameterFile(string filePath)
        {
            if (_camera == null) return false;

            try
            {
                Status status = _camera.Parameter.Load(filePath);
                Debug.WriteLine($"[IDSCamera] Parameter file loaded: {filePath} - {status}");
                return status == Status.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] LoadParameterFile error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 현재 카메라 설정을 파일로 저장
        /// </summary>
        public bool SaveParameterFile(string filePath)
        {
            if (_camera == null) return false;

            try
            {
                Status status = _camera.Parameter.Save(filePath);
                Debug.WriteLine($"[IDSCamera] Parameter file saved: {filePath} - {status}");
                return status == Status.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] SaveParameterFile error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 감마 설정
        /// </summary>
        public bool SetGamma(int gamma)
        {
            if (_camera == null) return false;

            try
            {
                Status status = _camera.Gamma.Software.Set(gamma);
                Debug.WriteLine($"[IDSCamera] Gamma set to {gamma}: {status}");
                return status == Status.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] Gamma set error: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Private Methods
        private void AllocateMemory()
        {
            if (_camera == null) return;

            _camera.Memory.Allocate(out _memoryId, false);
            _camera.Memory.SetActive(_memoryId);

            Debug.WriteLine($"[IDSCamera] Memory allocated: ID={_memoryId}");
        }

        private void FreeMemory()
        {
            if (_camera == null) return;

            try
            {
                _camera.Memory.Free(_memoryId);
                Debug.WriteLine("[IDSCamera] Memory freed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IDSCamera] FreeMemory error: {ex.Message}");
            }
        }

        private void Camera_EventFrame(object? sender, EventArgs e)
        {
            if (!_isRunning || _camera == null) return;

            try
            {
                // 현재 메모리에서 이미지 가져오기
                _camera.Memory.Lock(_memoryId);

                try
                {
                    // 메모리에서 바이트 배열로 복사
                    int stride = (ImageWidth * BitsPerPixel / 8 + 3) & ~3; // 4바이트 정렬
                    int imageSize = stride * ImageHeight;

                    _camera.Memory.CopyToArray(_memoryId, out byte[] imageData);

                    if (imageData != null && imageData.Length > 0)
                    {
                        Bitmap bitmap;

                        if (IsMonoMode)
                        {
                            // Mono8: 8bit 그레이스케일 이미지 생성
                            bitmap = new Bitmap(ImageWidth, ImageHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                            // 그레이스케일 팔레트 설정
                            ColorPalette palette = bitmap.Palette;
                            for (int i = 0; i < 256; i++)
                            {
                                palette.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                            }
                            bitmap.Palette = palette;

                            BitmapData bmpData = bitmap.LockBits(
                                new Rectangle(0, 0, ImageWidth, ImageHeight),
                                ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                            // 행별로 데이터 복사 (stride 고려)
                            for (int y = 0; y < ImageHeight; y++)
                            {
                                Marshal.Copy(imageData, y * ImageWidth, bmpData.Scan0 + y * bmpData.Stride, ImageWidth);
                            }
                            bitmap.UnlockBits(bmpData);
                        }
                        else
                        {
                            // BGR8: 24bit 컬러 이미지
                            bitmap = new Bitmap(ImageWidth, ImageHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                            BitmapData bmpData = bitmap.LockBits(
                                new Rectangle(0, 0, ImageWidth, ImageHeight),
                                ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                            Marshal.Copy(imageData, 0, bmpData.Scan0, Math.Min(imageData.Length, bmpData.Stride * ImageHeight));
                            bitmap.UnlockBits(bmpData);
                        }

                        _frameCounter++;

                        // 이벤트 발생
                        ImageReceived?.Invoke(this, bitmap);
                    }
                }
                finally
                {
                    _camera.Memory.Unlock(_memoryId);
                }

                CounterChanged?.Invoke(this, (_frameCounter, _errorCounter));
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    _errorCounter++;
                    Debug.WriteLine($"[IDSCamera] Acquisition error: {ex.Message}");
                    CounterChanged?.Invoke(this, (_frameCounter, _errorCounter));
                }
            }
        }

        private void OnError(string message)
        {
            Debug.WriteLine($"[IDSCamera] Error: {message}");
            ErrorOccurred?.Invoke(this, message);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Close();
            }

            _disposed = true;
        }

        ~IDSCamera()
        {
            Dispose(false);
        }
        #endregion
    }
}
