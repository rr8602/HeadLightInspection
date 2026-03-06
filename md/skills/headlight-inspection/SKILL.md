---
name: headlight-inspection
description: 자동차 HeadLight Lamp 검사 프로그램 개발을 위한 스킬. IDS uEye SDK를 사용한 C# 카메라 제어, OpenCV 기반 이미지 처리, 헤드램프 분석 로직 구현 시 사용. "헤드라이트 검사", "카메라 연결", "이미지 분석" 관련 작업 시 트리거.
---

# HeadLight Inspection Development Skill

## 프로젝트 개요

자동차 HeadLight Lamp 검사 프로그램을 C#으로 개발하는 프로젝트.
기존 C++ MFC 코드(E:\HLT\KI-HLT)를 기반으로 IDS uEye SDK를 사용하여 재구현.

## 기술 스택

- **언어**: C# (.NET 9.0)
- **UI**: Windows Forms
- **카메라 SDK**: IDS uEye SDK (uEyeDotNet.dll)
- **이미지 처리**: OpenCvSharp4 (4.10.0)
- **기존 코드 참조**: E:\HLT\KI-HLT (C++ MFC)

## 용어 정의

| 용어 | 설명 |
|------|------|
| **정대 (Alignment/FC)** | 차량의 센터(램프 중심)를 잡는 과정. 측정 전 수행. CHeadlampFC 기반 |
| **측정 (Measurement/PA)** | Hot Point, Cutoff Line, Cross Point를 검출하는 과정. CHeadlampPA 기반 |
| **Hot Point** | 이미지에서 가장 밝은 픽셀 위치. 헤드램프 광원의 중심점을 나타냄 |
| **Cutoff Line** | Low Beam에서 빛이 차단되는 경계선. 상향으로 빛이 퍼지지 않도록 설계된 라인 |
| **Cross Point** | 두 Cutoff Line이 교차하는 지점. 헤드램프 광축 조정의 기준점 |
| **Low Beam** | 하향등. 마주 오는 차량에 눈부심을 방지하는 조명 |
| **High Beam** | 상향등. 전방을 넓게 비추는 조명 |
| **Boundary Center** | 정대 시 좌/우 램프 경계 중심점. 램프 영역의 중앙 좌표 |
| **Centroid** | 정대 시 램프 영역의 무게중심. 밝기 가중 평균 |
| **Grayscale** | 흑백 이미지. 컬러(RGB/BGRA)를 밝기 값만으로 변환한 단일 채널 이미지 |
| **RANSAC** | Random Sample Consensus. 노이즈가 많은 데이터에서 모델(직선)을 찾는 알고리즘 |
| **Inlier** | RANSAC에서 찾은 모델(직선)에 가까운 점들 |
| **Sobel** | 이미지의 가장자리(Edge)를 검출하는 필터. Y방향 Sobel은 수평 에지 검출 |

## SDK 위치

```
IDS uEye SDK: C:\Program Files\IDS\uEye\
├── interfaces\dotnet\
│   └── uEyeDotNet.dll              # .NET 바인딩
├── develop\bin\
│   ├── ueye_api_64.dll             # 카메라 API
│   └── ueye_tools_64.dll           # 유틸리티
└── Cockpit\                        # 카메라 테스트 도구
```

## 프로젝트 구조

```
E:\HeadLightInspection\
├── .claude\skills\headlight-inspection\   # Claude Skill
├── src\
│   ├── HeadLightInspection.sln
│   ├── HeadLightInspection\               # 메인 WinForms 앱
│   │   ├── Program.cs
│   │   ├── MainForm.cs
│   │   ├── MainForm.Designer.cs
│   │   └── Forms\
│   │       └── CalibrationForm.cs         # 교정 UI (C++ ViewCalcHigh/ViewCalcLow)
│   ├── HeadLightInspection.Camera\        # 카메라 라이브러리
│   │   └── IDSCamera.cs                   # uEye SDK 래퍼 (Mono8 모드 지원)
│   ├── HeadLightInspection.ImageProcessing\  # 이미지 처리 (완료)
│   │   ├── HeadlampAnalyzer.cs            # 측정(PA): Hot Point, Cutoff Line, Cross Point, 필터링
│   │   ├── HeadlampAligner.cs             # 정대(FC): 램프 중심 검출, 경계/무게중심 계산
│   │   ├── Data\                          # 데이터 관리
│   │   │   └── DataManager.cs             # 파라미터/캘리브레이션 로드/저장 (JSON)
│   │   ├── Helpers\                       # 유틸리티
│   │   │   └── IniFileHelper.cs           # INI 파일 읽기/쓰기 (C++ Calibration.ini 호환)
│   │   └── Models\                        # 데이터 모델
│   │       ├── ModelParameter.cs          # 차종별 파라미터 (C++ stParam)
│   │       ├── CalibrationData.cs         # 캘리브레이션 데이터 (C++ stCalibration)
│   │       ├── StandardData.cs            # 합/불 판정 기준 (C++ stStdData)
│   │       └── JudgmentResult.cs          # 판정 결과 및 엔진
│   └── HeadLightInspection.Core\          # 공통 모델 (예정)
│       ├── Models\
│       └── Interfaces\
└── references\
    └── cpp-source\                        # 원본 C++ 참조
```

## 지침

### 1단계: IDS 카메라 연결 및 출력 (완료)

1. uEye SDK 참조 추가
   - `uEyeDotNet.dll` 참조
   - 빌드 후 `ueye_api_64.dll`, `ueye_tools_64.dll` 복사

2. 카메라 초기화 순서
   ```csharp
   using uEye;
   using uEye.Defines;
   using uEye.Types;

   // 카메라 목록 조회
   uEye.Info.Camera.GetCameraList(out CameraInformation[] cameraList);

   // 카메라 초기화
   var camera = new uEye.Camera();
   camera.Init((int)cameraList[0].CameraID);

   // Mono8 모드 설정 (그레이스케일 출력)
   Status pixelStatus = camera.PixelFormat.Set(uEye.Defines.ColorMode.Mono8);
   if (pixelStatus != Status.Success)
   {
       // Mono8 지원 안 하면 BGR8로 fallback
       camera.PixelFormat.Set(uEye.Defines.ColorMode.BGR8Packed);
   }

   // 이미지 크기 가져오기
   camera.Size.AOI.Get(out Rectangle aoi);
   ```

3. **Mono8 모드 (그레이스케일)**
   - 카메라에서 직접 8비트 그레이스케일 출력
   - BGR 변환 불필요 → 처리 속도 향상
   - `IsMonoMode` 속성으로 현재 모드 확인
   ```csharp
   if (IsMonoMode)
   {
       // 8비트 인덱스 컬러 (그레이스케일)
       var bitmap = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
       // 팔레트 설정 (0~255 그레이스케일)
       ColorPalette palette = bitmap.Palette;
       for (int i = 0; i < 256; i++)
           palette.Entries[i] = Color.FromArgb(i, i, i);
       bitmap.Palette = palette;
   }
   ```

3. 메모리 할당 및 취득 시작
   ```csharp
   // 메모리 할당
   camera.Memory.Allocate(out int memoryId, false);
   camera.Memory.SetActive(memoryId);

   // 프레임 이벤트 등록
   camera.EventFrame += Camera_EventFrame;

   // 라이브 취득 시작
   camera.Acquisition.Capture();
   ```

### 2단계: 이미지 수신 및 PictureBox 표시 (완료)

1. EventFrame 이벤트로 프레임 수신
2. Memory.CopyToArray로 바이트 배열 획득
3. Bitmap 생성 후 PictureBox.Image에 할당

```csharp
private void Camera_EventFrame(object? sender, EventArgs e)
{
    camera.Memory.Lock(memoryId);
    try
    {
        camera.Memory.CopyToArray(memoryId, out byte[] imageData);

        // Bitmap 생성
        var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var bmpData = bitmap.LockBits(...);
        Marshal.Copy(imageData, 0, bmpData.Scan0, imageData.Length);
        bitmap.UnlockBits(bmpData);

        // UI 스레드에서 표시
        pictureBox.Invoke(() => {
            pictureBox.Image?.Dispose();
            pictureBox.Image = bitmap;
        });
    }
    finally
    {
        camera.Memory.Unlock(memoryId);
    }
}
```

### 3단계: 노출(Exposure) 및 게인 제어 (완료)

```csharp
// 노출 범위 조회
camera.Timing.Exposure.GetRange(out double min, out double max, out double inc);

// 노출 설정 (밀리초 단위)
camera.Timing.Exposure.Set(exposureTimeMs);

// 현재 노출 값 읽기
camera.Timing.Exposure.Get(out double currentExposure);

// 게인 설정 (0~100)
camera.Gain.Hardware.Scaled.SetMaster(gain);
```

**암흑 모드 설정** (광도 측정용):
- 광도(cd) 측정 시 센서 포화(255 고정)를 방지하기 위해 노출/게인을 최소로 설정
- **핵심: 자동 모드 OFF + 한계값 최소 설정이 필수**

```csharp
// 암흑 모드 - P/Invoke로 네이티브 API 직접 호출
public bool SetDarkMode()
{
    // 1. 자동 기능 비활성화 (핵심!)
    SetAutoGainEnabled(false);
    SetAutoExposureEnabled(false);

    // 2. 자동 한계값 최소 설정 (핵심!)
    SetAutoBrightnessReference(0);      // 밝기 기준값 = 0
    SetAutoExposureMax(minExposure);    // 노출 한계 = 최소
    SetAutoGainMax(0);                  // 게인 한계 = 0

    // 3. 실제 값 설정
    _camera.Timing.Exposure.Set(minExposure);  // 노출 = 최소 (약 0.145ms)
    _camera.Gain.Hardware.Scaled.SetMaster(0); // 게인 = 0
    SetBlacklevelOffset(0);                    // 흑색레벨 = 0

    return true;
}

// P/Invoke 선언 (ueye_api_64.dll)
[DllImport("ueye_api_64.dll")]
private static extern int is_Blacklevel(int hCam, uint nCommand, ref int pParam, uint cbSizeOfParam);

[DllImport("ueye_api_64.dll")]
private static extern int is_SetAutoParameter(int hCam, int param, ref double pval1, ref double pval2);
```

**암흑 모드가 안 되는 이유:**
- 자동 모드만 OFF해도 **한계값(Limit)**이 높으면 카메라가 해당 값을 기본값처럼 사용
- 반드시 `SetAutoExposureMax()`, `SetAutoGainMax()`, `SetAutoBrightnessReference()` 설정 필요

**UI 구성:**
- **자동 노출/게인 체크박스**: 자동 모드 ON/OFF (해제 시 한계값도 자동 최소 설정)
- 암흑 모드 버튼: 모든 밝기 설정을 최소값으로 일괄 설정
- 파라미터 파일 로드 버튼: IDS Camera Manager에서 저장한 .ini 파일 불러오기
- 게인 조절: NumericUpDown (0~100)
- 흑색레벨 조절: NumericUpDown (0~255)

### 4단계: 정대 (Alignment) 기능 (완료)

차량의 센터(램프 중심)를 잡는 기능. C++ `CHeadlampFC` 기반.

1. **HeadlampAligner 클래스** (`HeadlampAligner.cs`)
   ```csharp
   public class HeadlampAligner : IDisposable
   {
       // Moving Average 필터 설정
       public void SetAvgParameters(int windowLen, double threshold);

       // 이미지 설정
       public void SetImage(Mat image);
       public void SetImage(Bitmap bitmap);

       // 램프 중심 검출 (메인 알고리즘)
       public AlignmentResult SearchLampCenter(int margin = 0, int boundaryX = 0, int boundaryY = 0);

       // 결과 조회 (Moving Average 적용 여부 선택)
       public Point2f GetLampCenterLeft(bool useAverage = false);
       public Point2f GetLampCenterRight(bool useAverage = false);
       public Point2f GetBoundaryCenter(bool useAverage = false);
       public Point2f GetLampCentroid(bool useAverage = false);
   }
   ```

2. **정대 알고리즘** (C++ CHeadlampFC::SearchLampCenter 기반)
   ```
   1. 히스토그램 계산 (256 bin)
   2. 상위 1.3% 픽셀 임계값 계산 (PercentPixel)
   3. 임계값의 90%로 이진화 (Binary)
   4. Morphology 연산 (Open → Close, 5x5 커널)
   5. ConnectedComponentsWithStats로 레이블링
   6. 면적 기준 상위 2개 컴포넌트 선택 (좌/우 램프)
   7. 각 램프의 경계(BoundingBox), 중심, 무게중심 계산
   8. Moving Average 필터로 안정화
   ```

3. **AlignmentResult 클래스**
   ```csharp
   public class AlignmentResult
   {
       public bool Success { get; set; }
       public LampCenterInfo LeftLamp { get; set; }   // 좌측 램프 정보
       public LampCenterInfo RightLamp { get; set; }  // 우측 램프 정보
       public Point2f BoundaryCenter { get; set; }    // 경계 중심
       public Point2f Centroid { get; set; }          // 무게중심
   }
   ```

4. **LampCenterInfo 클래스**
   ```csharp
   public class LampCenterInfo
   {
       public Point2f Center { get; set; }     // 램프 중심
       public int Left, Top, Right, Bottom;    // 경계 좌표
       public double Area { get; set; }        // 면적
   }
   ```

5. **정대 모드 UI** (MainForm)
   ```csharp
   // 모드 선택 RadioButton
   rbModeAlignment  // 정대 모드
   rbModeMeasurement  // 측정 모드

   // 정대 상태 표시
   lblAlignStatus.Text = "정대 완료";
   lblAlignStatus.ForeColor = Color.Green;
   ```

6. **정대 → 측정 흐름**
   - 정대 모드에서 램프 중심 검출 후 "정대 완료" 버튼 클릭
   - 검출된 센터 좌표 저장 (`_alignedCenter`)
   - 측정 모드로 전환하여 Hot Point/Cross Point 분석

### 5단계: 이미지 처리 - 측정 (완료)

OpenCvSharp4를 사용한 헤드램프 분석 (C++ `CHeadlampPA` 기반):

1. **NuGet 패키지** (HeadLightInspection.ImageProcessing.csproj)
   ```xml
   <PackageReference Include="OpenCvSharp4" Version="4.10.0.20240615" />
   <PackageReference Include="OpenCvSharp4.Extensions" Version="4.10.0.20240615" />
   <PackageReference Include="OpenCvSharp4.runtime.win" Version="4.10.0.20240615" />
   ```

2. **HeadlampAnalyzer 클래스**
   ```csharp
   public class HeadlampAnalyzer : IDisposable
   {
       // 이미지 설정 (BGRA/BGR/Mono → Grayscale 자동 변환)
       public void SetImage(Bitmap bitmap);

       // Hot Point 검출 (가장 밝은 픽셀)
       public (OpenCvSharp.Point position, int value) FindHotPoint(int margin = 0);

       // Cutoff Line 검출 (RANSAC 알고리즘)
       public List<LineSegment> FindCutoffLines(double tolerance = 3.0, int maxLines = 2);

       // 두 직선의 교차점 계산
       public Point2f? FindCrossPoint(LineSegment line1, LineSegment line2);

       // 전체 분석 수행
       public AnalysisResult Analyze(BeamType beamType = BeamType.LowBeam);

       // 하향등 전용 분석 (알고리즘 선택 가능)
       public AnalysisResult AnalyzeLowBeam(LampPosition lampPosition, CutoffAlgorithm? algorithm = null);

       // 상향등 분석 (Hot Point만 사용)
       public AnalysisResult AnalyzeHighBeam();

       // 필터 파라미터 설정
       public void SetHotPointFilterParams(int windowLen, double threshold);
       public void SetCrossPointFilterParams(int windowLen, double threshold);
       public void SetLineFilterParams(int windowLen, double threshold);
       public void ResetFilters();

       // 차종별 파라미터 적용
       public void ApplyModelParameter(ModelParameter param);
   }
   ```

3. **Cutoff 알고리즘 (하향등 전용)** - C++ `nCutAlgorizm` 기반

   | 알고리즘 | 설명 | 사용 상황 |
   |---------|------|----------|
   | **None** | Hot Point + 고정 오프셋 | 오프셋 알고 있을 때 |
   | **Edge** | Cutoff Line 교점 + 오프셋 | 일반 하향등 (기본값) |
   | **Fog** | 1st Line 위의 점 (Hot Point X좌표) | 안개등 (교점 없음) |
   | **Combined** | 자동 오프셋 계산 + Fog 방식 | 오프셋 모를 때 |

   ```csharp
   // 알고리즘 enum (ModelParameter.cs)
   public enum CutoffAlgorithm
   {
       None = 0,      // Hot Point + 고정 오프셋
       Edge = 1,      // Cutoff Line 교점 + 오프셋
       Fog = 2,       // 1st Line Y값 계산 (안개등용)
       Combined = 3   // 자동 오프셋 + Fog 방식
   }

   // 사용 예시
   var result = analyzer.AnalyzeLowBeam(LampPosition.Left, CutoffAlgorithm.Edge);
   Point2f measurementPoint = result.MeasurementPoint;  // 최종 측정점
   ```

   **이전값 사용 (Edge 알고리즘만)**:
   - Edge 알고리즘은 Cutoff Line 교점 검출에 실패할 수 있음
   - 검출 실패 시 이전 프레임의 측정점을 사용 (`UsePreviousValue = true`)
   - 최대 `PreviousValueMaxCount` 프레임까지 이전값 사용
   - 다른 알고리즘(None, Fog, Combined)은 항상 값을 반환하므로 이전값 불필요

4. **Moving Average 필터** (실시간 안정화)
   - C++ `GetAvgHotPos()`, `GetAvgCrossPoint()` 알고리즘 기반
   - Hot Point, Cross Point, Cutoff Line 각각에 적용
   - 급격한 변화(threshold 초과) 시 필터 리셋
   ```csharp
   // 필터 초기화 (MainForm 생성자)
   _analyzer.SetHotPointFilterParams(windowLen: 5, threshold: 30.0);
   _analyzer.SetCrossPointFilterParams(windowLen: 5, threshold: 30.0);
   _analyzer.SetLineFilterParams(windowLen: 5, threshold: 20.0);

   // 빔 타입 변경 시 필터 리셋
   _analyzer.ResetFilters();
   ```

3. **분석 동작 방식**
   - 분석 버튼 클릭 시에만 분석 수행 (C++ 동작과 동일)
   - 분석 결과는 다음 분석 전까지 고정
   - 카메라 프레임에 기존 분석 결과를 오버레이 표시
   - 스레드 안전을 위해 이미지 접근 시 lock 사용

4. **그레이스케일 변환 (중요)**
   ```csharp
   // MinMaxLoc, Sobel 등은 단일 채널(Grayscale)에서만 동작
   int channels = _currentImage.Channels();
   if (channels == 4)  // BGRA
       Cv2.CvtColor(_currentImage, _grayImage, ColorConversionCodes.BGRA2GRAY);
   else if (channels == 3)  // BGR
       Cv2.CvtColor(_currentImage, _grayImage, ColorConversionCodes.BGR2GRAY);
   ```

### 5단계: 차종별 파라미터 시스템 (완료)

차종(모델)별로 다른 검사 파라미터를 관리하는 시스템. C++ `tbl_Spec` 테이블 기반.

1. **ModelParameter 클래스** (`Models/ModelParameter.cs`)
   ```csharp
   public class ModelParameter
   {
       public string ModelName { get; set; }           // 차종명
       public int HotPointFilterCount { get; set; }    // 필터 윈도우 크기
       public int HotPointThreshold { get; set; }      // 필터 임계값
       public KernelType HotPointAlgorithm { get; set; } // 알고리즘 (AVR5X5 등)

       // 하향등 Cutoff 알고리즘 (C++ nCutAlgorizm)
       public CutoffAlgorithm CutoffAlgorithmType { get; set; }  // None/Edge/Fog/Combined
       public bool UsePreviousValue { get; set; }                 // 검출 실패 시 이전값 사용
       public int PreviousValueMaxCount { get; set; }             // 이전값 최대 사용 횟수

       // 좌/우 헤드램프 오프셋 (C++ nOFFSET_XL/YL, nOFFSET_XR/YR)
       public int OffsetLeftX { get; set; }
       public int OffsetLeftY { get; set; }
       public int OffsetRightX { get; set; }
       public int OffsetRightY { get; set; }

       public double ExposureTime { get; set; }        // 노출 시간
       public HandlePosition HandlePosition { get; set; } // 좌/우핸들

       // RANSAC 파라미터
       public int RansacTryCount { get; set; }
       public double RansacTolerance { get; set; }

       // ROI (Region of Interest)
       public int RoiX, RoiY, RoiWidth, RoiHeight;
   }
   ```

### 6단계: 캘리브레이션 시스템 (완료)

픽셀 좌표를 각도(도/분)로 변환하는 캘리브레이션 시스템. C++ `Calibration.ini` 기반.

1. **CalibrationData 클래스** (`Models/CalibrationData.cs`)
   ```csharp
   public class CalibrationData
   {
       // High Beam 기준점 (픽셀)
       public int HighZeroX { get; set; }
       public int HighZeroY { get; set; }

       // 각도 캘리브레이션 포인트 (1도, 2도 위치)
       public int HighAngleL1X, HighAngleL1Y;  // 좌측 1도
       public int HighAngleR1X, HighAngleR1Y;  // 우측 1도
       public int HighAngleU1X, HighAngleU1Y;  // 상단 1도
       public int HighAngleD1X, HighAngleD1Y;  // 하단 1도

       // Low Beam 기준점
       public int LowZeroX, LowZeroY;

       // 광도 캘리브레이션 테이블 (cd 값)
       public int[] HighCD { get; set; }       // 픽셀값 배열
       public int[] LowCD { get; set; }
       public int[] HighCDKeys { get; set; }   // cd 값 배열 (10000~100000)
       public int[] LowCDKeys { get; set; }    // cd 값 배열 (2000~40000)

       // 각도 변환 메서드
       public (double angleX, double angleY) PixelToAngleHigh(int pixelX, int pixelY);
       public (double angleX, double angleY) PixelToAngleLow(int pixelX, int pixelY);
       public (int pixelX, int pixelY) AngleToPixelHigh(double angleX, double angleY);

       // 광도 변환 메서드 (C++ GetCalcLux 알고리즘)
       public int PixelToCandela(int pixelValue, bool isHighBeam = true);
       public int PixelToCandelaHigh(int pixelValue);
       public int PixelToCandelaLow(int pixelValue);

       // INI 파일 로드/저장 (C++ Calibration.ini 호환)
       public static CalibrationData LoadFromIni(string filePath);
       public void SaveToIni(string filePath);
   }
   ```

2. **광도(Candela) 측정 알고리즘** (C++ `GetCalcLux()` 기반)
   ```
   1. 제로값 보정: pixelValue -= zeroValue
   2. 캘리브레이션 테이블에서 해당 구간 찾기
   3. 선형 보간:
      - span = (keys[i] - keys[i-1]) / (values[i] - values[i-1])
      - candela = keys[i-1] + (pixelValue - values[i-1]) × span
   4. 범위 외 처리: 비례 계산으로 외삽
   ```

   **주의**: 광도 측정 시 Hot Point 값이 255(포화)면 측정 불가. 암흑 모드로 노출을 낮춰야 함.

### 6-1단계: 교정 UI (CalibrationForm) (완료)

카메라가 인식하는 이미지 데이터(픽셀 좌표, 밝기)를 실제 물리적인 측정 단위(각도, 칸델라)로 변환하기 위한 기준 정보를 설정하는 UI. C++ `ViewCalcHigh`, `ViewCalcLow` 기반.

1. **CalibrationForm 클래스** (`Forms/CalibrationForm.cs`)
   ```csharp
   public class CalibrationForm : Form
   {
       // 외부에서 실시간 측정값 업데이트
       public void UpdateMeasurement(Point hotPoint, int pixelValue);

       // 현재 교정 데이터 반환
       public CalibrationData GetCalibrationData();
   }
   ```

2. **폼 구조** (TabControl 기반)
   - **상향등 교정 (High Beam)** 탭
     - 광도 교정 (CD Calibration) 그룹
     - 각도 교정 (Angle Calibration) 그룹
     - 현재 교정 데이터 표시 그룹
   - **하향등 교정 (Low Beam)** 탭
     - 동일 구조

3. **각도 교정 (Angle Calibration)**

   픽셀 좌표를 실제 각도로 변환하기 위한 기준점 설정.

   | 버튼 | 설명 |
   |------|------|
   | **ZERO** | 기준점(0도) 설정. 마스터 램프 중심(Hot Point) 좌표를 기록 |
   | **LEFT1/LEFT2** | 좌측 1도/2도 위치의 핫포인트 좌표 기록 |
   | **RIGHT1/RIGHT2** | 우측 1도/2도 위치의 핫포인트 좌표 기록 |
   | **UP1/UP2** | 상단 1도/2도 위치의 핫포인트 좌표 기록 |
   | **DOWN1/DOWN2** | 하단 1도/2도 위치의 핫포인트 좌표 기록 |

   - **ZERO 버튼 클릭 시**: 현재 핫포인트 좌표를 기준점(0,0)으로 설정. 다른 각도 위치도 자동 계산.
   - **방향 버튼 클릭 시**: 현재 핫포인트 좌표를 해당 각도 위치로 저장.
   - 픽셀-각도 변환 비율 계산에 사용 (1도 이동 시 픽셀 몇 칸 이동인지).

4. **광도 교정 (CD Calibration)**

   픽셀 밝기값을 칸델라(cd)로 변환하기 위한 기준 테이블 설정.

   | 버튼 | 설명 |
   |------|------|
   | **ZERO** | 빛이 없을 때의 기준 밝기(노이즈 레벨) 설정 |
   | **10000~100000 cd** (High) | 해당 광도의 기준 픽셀값 기록 (10개 포인트) |
   | **2000~40000 cd** (Low) | 해당 광도의 기준 픽셀값 기록 (10개 포인트) |

   - **버튼 상태 순환**: 미사용(회색) → 기본값(파란색) → 변경됨(녹색) → 미사용
   - **측정 방법**: 공인 장비로 정확한 광도를 내는 기준 광원을 비추고 버튼 클릭.
   - 여러 기준점으로 픽셀-칸델라 변환 그래프 생성 (선형 보간).

5. **버튼 클릭 동작**
   ```csharp
   private void ClickHighCdButton(int index)
   {
       if (_highCdStatus[index] == 0)  // 미사용 → 파일에서 로드
       {
           _highCdStatus[index] = 1;
           var tempData = CalibrationData.LoadFromIni(_iniFilePath);
           _calibrationData.HighCD[index] = tempData.HighCD[index];
           _highCdButtons[index].BackColor = Color.LightSteelBlue;
       }
       else if (_highCdStatus[index] == 1)  // 기본 → 현재값으로 변경
       {
           _highCdStatus[index] = 2;
           _calibrationData.HighCD[index] = CurrentPixelValue;
           _highCdButtons[index].BackColor = Color.LightGreen;
       }
       else  // 변경 → 미사용
       {
           _highCdStatus[index] = 0;
           _calibrationData.HighCD[index] = 0;
           _highCdButtons[index].BackColor = Color.LightGray;
       }
   }
   ```

6. **MainForm 연동**
   ```csharp
   // 교정 폼 열기
   private void btnCalibration_Click(object sender, EventArgs e)
   {
       _calibrationForm = new CalibrationForm(_calibrationIniPath);
       _calibrationForm.Show();

       // 실시간 측정값 전달 타이머
       var calibTimer = new System.Windows.Forms.Timer { Interval = 200 };
       calibTimer.Tick += (s, args) =>
       {
           if (_lastResult != null && _lastResult.IsValid)
           {
               _calibrationForm.UpdateMeasurement(
                   new Point(_lastResult.HotPoint.X, _lastResult.HotPoint.Y),
                   _lastResult.HotPointValue);
           }
       };
       calibTimer.Start();
   }
   ```

7. **INI 파일 구조** (C++ Calibration.ini 호환)
   ```ini
   [HIGH_CD]
   ZERO=1000
   10000=75
   20000=747
   ...
   100000=0

   [HIGH_ANGLE]
   ZEROX=640
   ZEROY=480
   LEFT1_X=605
   LEFT1_Y=480
   ...

   [LOW_CD]
   ZERO=500
   2000=1000
   ...

   [LOW_ANGLE]
   ZEROX=640
   ZEROY=512
   ...
   ```

### 7단계: 합/불 판정 시스템 (완료)

헤드램프 각도 및 광도 기준 합격/불합격 판정. C++ `tbl_StdData` 기반.

1. **StandardData 클래스** (`Models/StandardData.cs`)
   ```csharp
   public class StandardData
   {
       public string ModelName { get; set; }

       // High Beam 판정 기준 (분 단위)
       public int HighLeftUp, HighLeftDown, HighLeftLeft, HighLeftRight;
       public int HighRightUp, HighRightDown, HighRightLeft, HighRightRight;
       public int HighCD2DS { get; set; }  // 2등식 광도 기준
       public int HighCD4DS { get; set; }  // 4등식 광도 기준

       // Low Beam 판정 기준
       public int LowHorizontalMin, LowHorizontalMax;  // 수평 각도 범위
       public int LowCutoffMin, LowCutoffMax;          // Cutoff 각도 범위
       public int LowCDMin, LowCDMax;                  // 광도 범위
   }
   ```

2. **JudgmentEngine 클래스** (`Models/JudgmentResult.cs`)
   ```csharp
   public class JudgmentEngine
   {
       public JudgmentEngine(CalibrationData calibration, StandardData standard);

       // High Beam 판정 (Hot Point 기준)
       // - 수평/수직 각도 판정
       // - 광도(cd) 판정: PixelToCandelaHigh()로 변환 후 HighCD4DS 기준 비교
       public JudgmentResult JudgeHighBeam(AnalysisResult analysis, HeadlampSide side);

       // Low Beam 판정 (Cross Point 또는 Hot Point 기준)
       // - 수평/수직 각도 판정
       // - 광도(cd) 판정: PixelToCandelaLow()로 변환 후 LowCDMin~LowCDMax 범위 비교
       public JudgmentResult JudgeLowBeam(AnalysisResult analysis, HeadlampSide side);

       // 허용 범위 원 반지름 계산 (화면 표시용)
       public (int radiusX, int radiusY) GetToleranceRadius(BeamType beamType, HeadlampSide side);
   }
   ```

3. **JudgmentResult 클래스**
   ```csharp
   public class JudgmentResult
   {
       public JudgmentStatus Status { get; set; }      // Pass/Fail/Warning/NotTested
       public HeadlampSide Side { get; set; }          // Left/Right
       public BeamType BeamType { get; set; }          // LowBeam/HighBeam

       public double HorizontalDeviation { get; set; } // 수평 편차 (분)
       public double VerticalDeviation { get; set; }   // 수직 편차 (분)
       public int Candela { get; set; }                // 광도 (cd)

       public JudgmentStatus HorizontalStatus { get; set; }
       public JudgmentStatus VerticalStatus { get; set; }
       public JudgmentStatus CandelaStatus { get; set; }

       public string Message { get; set; }
   }
   ```

### 8단계: 데이터 관리 및 UI 통합 (완료)

데이터 로드/저장 및 차종 선택 UI 구현. C++ `DBMng`, `ViewMain` 기반.

1. **DataManager 클래스** (`Data/DataManager.cs`)
   ```csharp
   public class DataManager
   {
       // 모델 파라미터 (C++ tbl_Model)
       public void LoadAllModelParameters();
       public ModelParameter GetModelParameter(string modelName);
       public void SaveModelParameter(ModelParameter param);

       // 판정 기준 (C++ tbl_StdData)
       public void LoadAllStandardData();
       public StandardData GetStandardData(string modelName);

       // 캘리브레이션 (C++ Calibration.ini)
       public void LoadCalibration();
       public CalibrationData Calibration { get; }

       // 전체 로드/저장
       public void LoadAll();
       public void SaveAll();

       // 모델 목록
       public List<string> ModelNames { get; }
   }
   ```

2. **데이터 파일 구조** (실행 폴더)
   ```
   Data/
   ├── ModelParameters.json   # 차종별 파라미터
   ├── StandardData.json      # 합/불 판정 기준
   └── Calibration.json       # 캘리브레이션 데이터 (JSON 버전)

   Calibration.ini            # 캘리브레이션 데이터 (C++ 호환 INI 버전)
   ```
   - C++ MDB 대신 JSON 파일 사용 (간단하고 편집 용이)
   - Calibration.ini는 C++ 프로그램과 호환성 유지
   - 첫 실행 시 기본값으로 자동 생성

3. **차종 선택 UI** (MainForm)
   ```csharp
   // 차종 선택 ComboBox (C++ m_CBO_MODEL)
   cmbModel.SelectedIndexChanged += cmbModel_SelectedIndexChanged;

   // 헤드램프 위치 (좌/우)
   cmbHeadlampSide.Items.AddRange(new[] { "좌측 (Left)", "우측 (Right)" });

   // 차종 변경 시 (C++ OnCbnSelchangeCboModel)
   private void cmbModel_SelectedIndexChanged(object sender, EventArgs e)
   {
       _currentModelParam = _dataManager.GetModelParameter(modelName);
       _currentStandardData = _dataManager.GetStandardData(modelName);
       _analyzer.ApplyModelParameter(_currentModelParam);
       _judgmentEngine = new JudgmentEngine(_dataManager.Calibration, _currentStandardData);
       _analyzer.ResetFilters();
   }
   ```

4. **판정 결과 표시** (C++ ViewResult 동일)
   ```csharp
   // OK/NG 대형 텍스트 (녹색/빨간색)
   lblJudgmentResult.Text = result.Status == JudgmentStatus.Pass ? "OK" : "NG";
   lblJudgmentResult.ForeColor = result.Status == JudgmentStatus.Pass ? Color.Green : Color.Red;

   // 수평/수직 편차 (분 단위)
   lblHorizontalValue.Text = $"{result.HorizontalDeviation:F1} '";
   lblVerticalValue.Text = $"{result.VerticalDeviation:F1} '";

   // 불합격 항목은 빨간색
   lblHorizontalValue.ForeColor = result.HorizontalStatus == JudgmentStatus.Pass ? Color.Black : Color.Red;
   ```

5. **8bpp 그레이스케일 표시**
   ```csharp
   // 8bpp 인덱스 이미지를 24bpp RGB로 변환 (오버레이 그리기 위해)
   private Bitmap ConvertTo24bpp(Bitmap source)
   {
       var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
       using (var g = Graphics.FromImage(result))
       {
           g.DrawImage(source, 0, 0, source.Width, source.Height);
       }
       return result;
   }
   ```

## C++ to C# 코드 매핑

### 정대/측정 클래스
| C++ | C# |
|-----|-----|
| `CHeadlampFC` | `HeadlampAligner` |
| `CHeadlampFC::SearchLampCenter()` | `HeadlampAligner.SearchLampCenter()` |
| `CHeadlampFC::GetLampCenterLeft()` | `HeadlampAligner.GetLampCenterLeft()` |
| `CHeadlampFC::GetBoundaryCenter()` | `HeadlampAligner.GetBoundaryCenter()` |
| `CHeadlampFC::GetLampCentroid()` | `HeadlampAligner.GetLampCentroid()` |
| `CHeadlampPA` | `HeadlampAnalyzer` |
| `CHeadlampPA::GetHotPos()` | `HeadlampAnalyzer.FindHotPoint()` |
| `CHeadlampPA::GetCrossPoint()` | `HeadlampAnalyzer.FindCrossPoint()` |
| `CIDSCamSet::SetExposure()` | `IDSCamera.SetExposureTime()` |
| `CIDSCamSet::SetGain()` | `IDSCamera.SetGain()` |
| `GetCalcLux()` | `CalibrationData.PixelToCandela()` |
| `is_Blacklevel()` | `IDSCamera.SetBlacklevelOffset()` |
| `is_SetAutoParameter()` | `IDSCamera.SetAutoBrightnessReference()`, `SetAutoExposureMax()`, `SetAutoGainMax()` |
| `CViewCalcHigh` | `CalibrationForm` (상향등 탭) |
| `CViewCalcLow` | `CalibrationForm` (하향등 탭) |
| `IniFileHelper` (Win32 API) | `IniFileHelper.cs` (P/Invoke) |

### 데이터 관리
| C++ | C# |
|-----|-----|
| `DBMng::FindModelParam()` | `DataManager.GetModelParameter()` |
| `DBMng::FindSTDData()` | `DataManager.GetStandardData()` |
| `tbl_Model` | `ModelParameter` |
| `tbl_StdData` | `StandardData` |
| `stCalibration` | `CalibrationData` |
| `Data.mdb` (Access DB) | `*.json` (JSON 파일) |

### UI 이벤트
| C++ | C# |
|-----|-----|
| `CViewMain::OnCbnSelchangeCboModel()` | `cmbModel_SelectedIndexChanged()` |
| `CMainFrame::LoadParamFromModel()` | `cmbModel_SelectedIndexChanged()` 내부 |
| `CViewResult::ShowReport()` | `UpdateJudgmentDisplay()` |
| `CViewResult::GetResColor()` | 조건부 `ForeColor` 설정 |
| 모드 전환 (정대/측정) | `rbMode_CheckedChanged()` |
| `CViewCalcHigh::OnBtnXXX()` | `CalibrationForm.ClickHighCdButton()`, `SetHighAngle()` 등 |
| `CViewCalcLow::OnBtnXXX()` | `CalibrationForm.ClickLowCdButton()`, `SetLowAngle()` 등 |

## C++ to C# 매핑 참조

| C++ (uEye) | C# (uEye .NET) |
|------------|----------------|
| `is_InitCamera(&hCam, NULL)` | `camera.Init(cameraId)` |
| `is_CaptureVideo(hCam, IS_DONT_WAIT)` | `camera.Acquisition.Capture()` |
| `is_StopLiveVideo(hCam, IS_FORCE_VIDEO_STOP)` | `camera.Acquisition.Stop()` |
| `is_Exposure(hCam, IS_EXPOSURE_CMD_SET, &val)` | `camera.Timing.Exposure.Set(val)` |
| `is_AllocImageMem` | `camera.Memory.Allocate(out memId)` |
| `is_SetImageMem` | `camera.Memory.SetActive(memId)` |
| `is_LockSeqBuf` | `camera.Memory.Lock(memId)` |
| `is_UnlockSeqBuf` | `camera.Memory.Unlock(memId)` |
| `is_ExitCamera` | `camera.Exit()` |

## 검사 시스템 개념

### 헤드램프 검사란?

자동차 생산 라인에서 헤드램프 광축(빛의 방향)이 규격에 맞는지 검사하는 시스템.

**검사 과정:**
1. 차량이 검사 위치에 정지
2. **정대 (Alignment)**: 램프 중심 검출하여 차량 센터 잡기
3. 헤드램프 점등 (Low Beam 또는 High Beam)
4. 카메라로 헤드램프 광 패턴 촬영
5. **측정 (Measurement)**: Hot Point, Cutoff Line, Cross Point 검출
6. 기준점 대비 편차 측정
7. 규격 범위 내면 합격, 벗어나면 조정 필요

**작업자 조정 방식:**
- 작업자가 화면을 보면서 헤드램프 조정 볼트를 드라이버로 조절
- Cross Point 또는 Hot Point가 화면 중앙(기준점)에 오도록 조정
- 기준점 범위 내에 들어오면 검사 통과

### 화면 표시 요소

**측정 모드 (구현 완료):**
1. ✅ **Hot Point** - 빨간 원으로 표시
2. ✅ **Cutoff Lines** - 녹색/노란색/마젠타 선으로 표시
3. ✅ **Cross Point** - 청록색 십자선으로 표시
4. ✅ **측정점 (Measurement Point)** - 주황색 사각형으로 최종 측정 기준점 표시
5. ✅ **합격/불합격 표시** - OK(녹색)/NG(빨간색) 대형 텍스트
6. ✅ **수평/수직 편차** - 분 단위로 표시, 불합격 시 빨간색
7. ✅ **광도(cd) 표시** - 측정된 광도값 표시
8. ✅ **차종 선택** - ComboBox로 모델 선택
9. ✅ **헤드램프 위치** - 좌측/우측 선택
10. ✅ **기준점 십자선** - 흰색 십자선으로 캘리브레이션 Zero 포인트 표시
11. ✅ **허용 범위 타원** - 녹색 타원으로 합격 범위 표시 (GetToleranceRadius 사용)
12. ✅ **알고리즘 선택** - ComboBox로 Cutoff 알고리즘 선택 (하향등만)
13. ✅ **이전값 사용 체크박스** - Edge 알고리즘 선택 시에만 활성화

**정대 모드 (구현 완료):**
1. ✅ **좌측 램프 영역** - 녹색 사각형 (BoundingBox)
2. ✅ **우측 램프 영역** - 청록색 사각형 (BoundingBox)
3. ✅ **경계 중심점** - 노란색 십자선 (BoundaryCenter)
4. ✅ **무게중심** - 마젠타 십자선 (Centroid)
5. ✅ **정대 상태** - 정대 완료/대기 라벨 표시
6. ✅ **모드 선택** - RadioButton (정대/측정, 기본값: 정대)

**카메라 제어 UI (구현 완료):**
1. ✅ **노출 시간** - TrackBar + NumericUpDown
2. ✅ **게인** - NumericUpDown (0~100)
3. ✅ **흑색레벨** - NumericUpDown (0~255)
4. ✅ **자동 노출/게인 체크박스** - 자동 모드 ON/OFF (해제 시 한계값 자동 최소화)
5. ✅ **암흑 모드 버튼** - 모든 밝기 설정 최소값으로 일괄 설정
6. ✅ **파라미터 파일 로드 버튼** - IDS Camera Manager .ini 파일 로드
7. ✅ **교정 설정 버튼** - CalibrationForm 열기

**교정 UI (CalibrationForm, 구현 완료):**
1. ✅ **상향등/하향등 탭** - TabControl로 High/Low Beam 분리
2. ✅ **각도 교정 버튼** - ZERO, LEFT1/2, RIGHT1/2, UP1/2, DOWN1/2
3. ✅ **광도 교정 버튼** - ZERO, 10K~100K cd (High), 2K~40K cd (Low)
4. ✅ **현재 측정값 표시** - 실시간 핫포인트 좌표, 픽셀값
5. ✅ **교정 데이터 표시** - 현재 로드된 교정값 테이블
6. ✅ **저장/초기화 버튼** - INI 파일 저장, 기존값 복원

## 문제 해결

### 카메라를 찾을 수 없음
1. IDS uEye Cockpit에서 카메라 인식 확인
2. 장치 관리자에서 드라이버 설치 상태 확인
3. 다른 프로그램에서 카메라 사용 중인지 확인
4. USB 연결 상태 확인

### 이미지가 너무 어둡거나 밝음
- `camera.Timing.Exposure.Set(ms)` 값 조정
- Gain 값 확인: `camera.Gain.Hardware.Scaled.SetMaster(gain)`

### 광도(Candela) 값이 항상 동일함 (255 포화)
- **원인**: 노출/게인이 높아서 Hot Point 픽셀값이 255로 포화
- **해결**: 암흑 모드 버튼 클릭 또는 자동 모드 체크박스 해제
  ```csharp
  // 방법 1: 암흑 모드 버튼 클릭 (권장)
  _camera.SetDarkMode();

  // 방법 2: 자동 모드 OFF + 수동 설정
  _camera.SetAutoExposureEnabled(false);
  _camera.SetAutoGainEnabled(false);
  _camera.SetAutoBrightnessReference(0);
  _camera.SetAutoExposureMax(minExposure);
  _camera.SetAutoGainMax(0);
  ```

### 자동 모드 OFF + 노출/게인 0으로 해도 암흑이 안 됨
- **원인**: 자동 모드만 OFF해도 **한계값(Limit)**이 높으면 카메라가 해당 값 사용
- **해결**: 자동 모드 OFF 시 한계값도 함께 최소로 설정 필요
  ```csharp
  // 반드시 함께 설정해야 함
  SetAutoExposureEnabled(false);
  SetAutoGainEnabled(false);
  SetAutoBrightnessReference(0);    // 밝기 기준값
  SetAutoExposureMax(minExposure);  // 노출 한계
  SetAutoGainMax(0);                // 게인 한계
  ```
- UI의 "자동 노출/게인" 체크박스 해제 시 자동으로 위 설정 적용됨

### 프레임 드롭 발생
- 메모리 버퍼 개수 증가
- USB 대역폭 확인

### MinMaxLoc/Sobel 오류 (cn == 1 오류)
- **원인**: BGRA(4채널) 또는 BGR(3채널) 이미지에서 단일 채널 함수 호출
- **해결**: SetImage()에서 Grayscale로 변환 후 사용
  ```csharp
  if (channels == 4)
      Cv2.CvtColor(_currentImage, _grayImage, ColorConversionCodes.BGRA2GRAY);
  ```

### 분석 결과가 사라짐 (오버레이 깜빡임)
- **원인**: 카메라가 계속 새 프레임을 보내서 이전 오버레이 덮어씀
- **해결**: 분석 결과를 별도 저장하고, 새 프레임에 기존 결과를 다시 그림
  ```csharp
  private AnalysisResult? _lastResult;  // 마지막 분석 결과 저장
  // ProcessReceivedImage에서 _lastResult로 오버레이 그리기
  ```

### 이미지 접근 시 스레드 오류
- **원인**: 카메라 스레드와 UI 스레드가 동시에 이미지 접근
- **해결**: lock으로 동기화
  ```csharp
  private readonly object _imageLock = new object();
  lock (_imageLock) { /* 이미지 접근 */ }
  ```

### 8bpp 그레이스케일 이미지에 오버레이 그리기 오류
- **원인**: `Graphics.FromImage()`는 8bpp 인덱스 이미지에서 동작하지 않음
- **해결**: 24bpp RGB로 변환 후 오버레이 그리기
  ```csharp
  private Bitmap ConvertTo24bpp(Bitmap source)
  {
      var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
      using (var g = Graphics.FromImage(result))
      {
          g.DrawImage(source, 0, 0, source.Width, source.Height);
      }
      return result;
  }
  ```

## 참조 문서

- `references/cpp-camera-code.md` - 기존 C++ 카메라 코드 분석
- `references/cpp-headlamp-analysis.md` - 헤드램프 분석 알고리즘
