# C++ 헤드램프 분석 코드 분석

원본 파일: `E:\HLT\KI-HLT\CHeadlampPA.h`, `CImageProcess.h`

## 분석 알고리즘 개요

### 1. Hot Point 검출
이미지에서 가장 밝은 점(Hot Point)을 찾는 알고리즘

```cpp
// Pyramid 이미지를 사용하여 검색 속도 향상
Mat m_ImgPyramid[MAX_LV_PYRAMID];  // 최대 5레벨

int32_t SearchHotPos(kernel_type type, uint32_t margin = 0);
Point2i GetHotPos(int32_t idx = 0);
Point2i GetAvgHotPos(void);  // 평균 Hot Position
```

### 2. Cutoff Line 검출 (RANSAC)
헤드램프 빔 패턴의 경계선 검출

```cpp
#define RANSAC_TRY_NUM (200)        // RANSAC 시도 횟수
#define RANSAC_SMALL_SAMPLES (6)    // 최소 샘플 수

int32_t SearchCutoffLines(kernel_type type, double tol = 3.0, int32_t maxiter = 10);

// 라인 정보 구조체
typedef struct _line_segment {
    double a, b, c;        // 직선 방정식: ax + by + c = 0
    Point2i left, right;   // 선분 시작/끝점
    int32_t samples_in;    // 인라이어 샘플 수
    double avg_dist;       // 평균 거리
} line_segment;

line_segment m_1st_line;  // 첫 번째 라인
line_segment m_2nd_line;  // 두 번째 라인
line_segment m_3rd_line;  // 세 번째 라인
```

### 3. Cross Point 검출
두 Cutoff Line의 교차점

```cpp
Point2f m_cross_point[2];      // 교차점 (최대 2개)
int32_t m_cross_point_num;

Point2f GetCrossPoint(int32_t idx = 0);
Point2f GetAvgCrossPoint(void);  // 평균 Cross Point
```

### 4. Moving Average 필터 (안정화)
Hot Point, Cross Point의 급격한 변화를 완화하는 필터

```cpp
// Hot Point 필터링
int32_t m_hpAvgSampleNum;        // 누적 샘플 수
int32_t m_hpAvgWindowLen;        // 윈도우 길이 (기본값: 50)
double m_hpThAvg;                // 거리 임계값 (기본값: 30픽셀)
Point2f m_avgHotPos;             // 필터링된 평균 위치

Point2i GetAvgHotPos(void) {
    if (m_hpAvgSampleNum < m_hpAvgWindowLen)
        m_hpAvgSampleNum++;

    if (m_hpAvgSampleNum == 1) {
        m_avgHotPos = current;
    } else {
        double dist = sqrt(pow(m_avgHotPos.x - current.x, 2) +
                          pow(m_avgHotPos.y - current.y, 2));
        if (dist <= m_hpThAvg) {
            // 이동 평균 업데이트
            m_avgHotPos.x = (m_avgHotPos.x * (m_hpAvgSampleNum-1) + current.x) / m_hpAvgSampleNum;
            m_avgHotPos.y = (m_avgHotPos.y * (m_hpAvgSampleNum-1) + current.y) / m_hpAvgSampleNum;
        } else {
            // 급격한 점프 → 필터 리셋
            m_avgHotPos = current;
            m_hpAvgSampleNum = 1;
        }
    }
    return m_avgHotPos;
}

// Cross Point 필터링 (동일 알고리즘)
int32_t m_cpAvgSampleNum;
int32_t m_cpAvgWindowLen;
double m_cpThAvg;
Point2f m_avgCrossPoint;
```

### 4. Edge 검출
수평 방향 에지 검출

```cpp
typedef struct _edge_info {
    Point2i pos[EDGE_POS_NUM];  // 에지 위치 (최대 1280개)
    int32_t diff[EDGE_POS_NUM]; // 그래디언트 값
    int32_t max_diff;           // 최대 그래디언트
    int32_t num;                // 에지 개수
} edge_info;
```

## 커널 타입

```cpp
enum kernel_type {
    AVR3X3 = 0,  // 3x3 평균
    AVR5X5,      // 5x5 평균
    AVR7X7,      // 7x7 평균
    AVR9X9,      // 9x9 평균
    GAU3X3,      // 3x3 가우시안
    GAU5X5,      // 5x5 가우시안
    NONE
};
```

## 빔 타입

```cpp
enum beam_type {
    LOWBEAM = 0,   // 하향등
    HIGHBEAM       // 상향등
};

enum lamp_type {
    TWOLAMP = 0,   // 2등식
    FOURLAMP       // 4등식
};
```

## 이미지 처리 (CImageProcess)

### 주요 함수

```cpp
void CropImage(Mat src, Mat &dst, int nWidth, int nHeight);
void GetHotPoint(Mat src, Point &pt, int &nValue);
void myThreshold(const Mat& src, Mat& dst, int th);

// 시각화
void DrawCM(Mat &dst);           // cm 눈금 표시
void DrawDegree(Mat &dst);       // 각도 표시
void DrawHotZone(Mat &dst, Point pt);
void DrawCutOff(Mat& src, Point2f pf0);
void DrawCrossLine(Mat& src, CHeadlampPA* pHlpa);
```

## 차종별 파라미터 (tbl_Spec)

```cpp
// stParam 구조체 - 차종별 검사 파라미터
typedef struct _stParam {
    char modelName[64];           // 차종명
    int32_t hotPointFilterCount;  // 필터 윈도우 크기
    int32_t hotPointThreshold;    // 필터 임계값
    kernel_type hotPointAlgorithm;// 알고리즘 (AVR5X5 등)

    int32_t offsetLeftX, offsetLeftY;   // 좌측 헤드램프 오프셋
    int32_t offsetRightX, offsetRightY; // 우측 헤드램프 오프셋

    double exposureTime;          // 노출 시간
    int32_t handlePosition;       // 핸들 위치 (0:좌핸들, 1:우핸들)

    int32_t ransacTryCount;       // RANSAC 시도 횟수 (기본: 200)
    double ransacTolerance;       // RANSAC 허용 오차 (기본: 3.0)
} stParam;
```

## 캘리브레이션 (Calibration.ini)

```cpp
// stCalibration 구조체 - 픽셀 ↔ 각도 변환용
typedef struct _stCalibration {
    // High Beam 기준점
    int32_t highZeroX, highZeroY;

    // 각도 캘리브레이션 포인트 (1도, 2도 위치)
    int32_t highAngleL1X, highAngleL1Y;  // 좌측 1도
    int32_t highAngleL2X, highAngleL2Y;  // 좌측 2도
    int32_t highAngleR1X, highAngleR1Y;  // 우측 1도
    int32_t highAngleU1X, highAngleU1Y;  // 상단 1도
    int32_t highAngleD1X, highAngleD1Y;  // 하단 1도

    // Low Beam 기준점
    int32_t lowZeroX, lowZeroY;

    // 광도 캘리브레이션
    int32_t highCD[10];  // High Beam 광도
    int32_t lowCD[10];   // Low Beam 광도
} stCalibration;

// 픽셀 → 각도 변환
double pixelsPerDegree = highZeroX - highAngleL1X;  // 1도당 픽셀 수
double angleX = (pixelX - highZeroX) / pixelsPerDegree;  // 도 단위
double angleMinutes = angleX * 60;  // 분 단위
```

## 합/불 판정 기준 (tbl_StdData)

```cpp
// stStdData 구조체 - 합격/불합격 기준
typedef struct _stStdData {
    char modelName[64];

    // High Beam 각도 허용 범위 (분 단위)
    int32_t highLeftUp, highLeftDown;    // 좌측 상/하한
    int32_t highLeftLeft, highLeftRight; // 좌측 좌/우한
    int32_t highRightUp, highRightDown;
    int32_t highRightLeft, highRightRight;

    // High Beam 광도 기준 (cd)
    int32_t highCD2DS;  // 2등식 기준 (기본: 15000cd)
    int32_t highCD4DS;  // 4등식 기준 (기본: 12000cd)

    // Low Beam 기준
    int32_t lowHorizontalMin, lowHorizontalMax;  // 수평 각도 범위 (분)
    int32_t lowCutoffMin, lowCutoffMax;          // Cutoff 각도 범위 (분)
    int32_t lowCDMin, lowCDMax;                  // 광도 범위 (cd)
} stStdData;

// 판정 로직
if (horizontalDev >= -limitLeft && horizontalDev <= limitRight &&
    verticalDev >= -limitUp && verticalDev <= limitDown) {
    result = PASS;
} else {
    result = FAIL;
}
```

## 데이터 관리 (DBMng)

```cpp
// C++ 데이터베이스 관리
class DBMng {
    // 모델 파라미터 조회
    BOOL FindModelParam(CTbl_Model *pData, CString strModel);
    BOOL FindModelParam(CArray<CTbl_Model, CTbl_Model&> *pDataArr);

    // 표준 데이터 조회
    BOOL FindSTDData(Ctbl_StdDAta *pData, CString Model_NM);

    // 결과 저장
    void InsertResult(...);
};
```

**C# 구현**: `DataManager` 클래스 (JSON 파일 사용)
- `GetModelParameter()` - 차종별 파라미터 조회
- `GetStandardData()` - 판정 기준 조회
- `LoadCalibration()` - 캘리브레이션 로드

## 차종 선택 UI (ViewMain)

```cpp
// C++ 차종 선택
CComboBox m_CBO_MODEL;

void CViewMain::OnCbnSelchangeCboModel()
{
    CString strModel;
    m_CBO_MODEL.GetLBText(m_CBO_MODEL.GetCurSel(), strModel);
    ma->LoadParamFromModel(strModel);
}

void CMainFrame::LoadParamFromModel(CString strModel)
{
    CTbl_Model cData;
    m_Database.FindModelParam(&cData, strModel);

    // 파라미터 적용
    m_stParam.nHot_Count = atoi(cData.M_HP_CNT);
    m_stParam.nHot_Th = atoi(cData.M_HP_THE);
    // ...

    // 필터 파라미터 적용
    m_Cam_In.m_hlPA.SetAvgHotPosParm(m_stParam.nHot_Count, m_stParam.nHot_Th);
}
```

**C# 구현**: `cmbModel_SelectedIndexChanged()` 이벤트

## 판정 결과 표시 (ViewResult)

```cpp
// C++ 결과 표시
COLORREF CViewResult::GetResColor(UINT nCtlColor)
{
    BOOL bRES;
    m_mapHighRes.Lookup(nCtlColor, bRES);
    if (!bRES) return RED;  // NG = 빨간색
    return RGB(0, 0, 0);    // OK = 검정색
}
```

**C# 구현**: `UpdateJudgmentDisplay()` 메서드
- OK: 녹색, NG: 빨간색

## C# 변환 구현 완료

1. **Hot Point 검출** ✓
2. **이미지 표시 및 시각화** ✓
3. **Edge 검출** ✓
4. **RANSAC Line Fitting** ✓
5. **Cross Point 계산** ✓
6. **Moving Average 필터** ✓
7. **차종별 파라미터** - ModelParameter ✓
8. **캘리브레이션** - CalibrationData ✓
9. **합/불 판정** - JudgmentEngine ✓
10. **데이터 관리** - DataManager (JSON) ✓
11. **차종 선택 UI** - cmbModel ✓
12. **판정 결과 표시** - OK/NG 색상 ✓
13. **Mono8 모드** - 흑백 출력 ✓

## OpenCvSharp 매핑

| C++ OpenCV | C# OpenCvSharp |
|------------|----------------|
| `Mat` | `Mat` |
| `Point2i` | `Point` |
| `Point2f` | `Point2f` |
| `Rect` | `Rect` |
| `pyrDown()` | `Cv2.PyrDown()` |
| `threshold()` | `Cv2.Threshold()` |
| `Sobel()` | `Cv2.Sobel()` |
