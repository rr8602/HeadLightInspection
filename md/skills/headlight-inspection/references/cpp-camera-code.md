# C++ 카메라 코드 분석

원본 파일: `E:\HLT\KI-HLT\CIDSCamSet.cpp`

## 클래스 구조

### CIDSCamSet
uEye SDK를 사용한 IDS 카메라 제어 클래스

```cpp
class CIDSCamSet
{
    HIDS m_hCam;              // 카메라 핸들
    SENSORINFO m_sInfo;       // 센서 정보
    INT m_nBitsPerPixel;      // 픽셀당 비트 수
    IS_SIZE_2D m_imageSize;   // 이미지 크기

    // 시퀀스 버퍼 (10개)
    INT m_lSeqMemId[SEQ_BUFFERS];
    char* m_pcSeqImgMem[SEQ_BUFFERS];

    // 더블 버퍼링
    char* m_Oimgbuf;  // 홀수 프레임
    char* m_Eimgbuf;  // 짝수 프레임
};
```

## 주요 함수

### OpenCamera(int id)
```cpp
bool CIDSCamSet::OpenCamera(int id)
{
    m_hCam = (HIDS)id;
    m_Ret = is_InitCamera(&m_hCam, NULL);

    // 센서/카메라 정보 획득
    is_GetSensorInfo(m_hCam, &m_sInfo);
    is_GetCameraInfo(m_hCam, &m_CamInfo);

    // EEPROM에서 파라미터 로드
    is_ParameterSet(m_hCam, IS_PARAMETERSET_CMD_LOAD_EEPROM, NULL, NULL);

    // 컬러 모드에 따른 BitsPerPixel 설정
    // IS_CM_MONO8 -> 8bit (그레이스케일, 권장)
    // IS_CM_BGR8_PACKED -> 24bit (컬러)
    is_SetColorMode(m_hCam, IS_CM_MONO8);  // Mono8 모드 설정
    m_nBitsPerPixel = 8;

    // AOI(관심영역) 크기 설정
    is_AOI(m_hCam, IS_AOI_IMAGE_GET_SIZE, &m_imageSize, sizeof(m_imageSize));

    // 시퀀스 버퍼 할당
    SeqBuilt();

    // 노출 범위 획득
    is_Exposure(m_hCam, IS_EXPOSURE_CMD_GET_EXPOSURE_RANGE_MIN, &m_dblMin, ...);
    is_Exposure(m_hCam, IS_EXPOSURE_CMD_GET_EXPOSURE_RANGE_MAX, &m_dblMax, ...);
}
```

### Mono8 모드 (C# 구현)
```csharp
// uEye .NET에서 Mono8 모드 설정
Status pixelStatus = camera.PixelFormat.Set(uEye.Defines.ColorMode.Mono8);
if (pixelStatus == Status.Success)
{
    BitsPerPixel = 8;
    IsMonoMode = true;

    // 8비트 그레이스케일 Bitmap 생성
    var bitmap = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
    ColorPalette palette = bitmap.Palette;
    for (int i = 0; i < 256; i++)
        palette.Entries[i] = Color.FromArgb(i, i, i);
    bitmap.Palette = palette;
}
```

### StartVideo()
```cpp
bool CIDSCamSet::StartVideo()
{
    SetEvent();  // 이벤트 핸들러 설정
    m_Ret = is_CaptureVideo(m_hCam, IS_DONT_WAIT);
}
```

### ThreadProcEvent() - 프레임 수신 스레드
```cpp
void CIDSCamSet::ThreadProcEvent()
{
    do {
        dwRet = WaitForSingleObject(m_hEv, INFINITE);
        if (m_boRunThread)
        {
            // 최신 버퍼 찾기
            is_GetActSeqBuf(m_hCam, &nNum, &pcMem, &pcMemLast);

            // 버퍼 잠금
            is_LockSeqBuf(m_hCam, m_nSeqNumId[i], m_pcSeqImgMem[i]);

            // 홀수/짝수 프레임 더블 버퍼링
            if (m_FrameNum % 2)
                memcpy(m_Eimgbuf, m_pcSeqImgMem[i], m_dwSingleBufferSize);
            else
                memcpy(m_Oimgbuf, m_pcSeqImgMem[i], m_dwSingleBufferSize);

            m_FrameNum++;
            is_UnlockSeqBuf(...);
        }
    } while (m_boRunThread);
}
```

### SetExposure(double dExposure)
```cpp
void CIDSCamSet::SetExposure(double dExposure)
{
    // Low Beam 기본값: 1.2ms
    // High Beam 기본값: 6.0ms
    if (ma->m_nHorL_Beam == 0)  // Low
        if (dExposure == 0) dExposure = 1.2;
    else  // High
        if (dExposure == 0) dExposure = 6.0f;

    is_Exposure(m_hCam, IS_EXPOSURE_CMD_SET_EXPOSURE, &dExposure, sizeof(dExposure));
}
```

## C# 변환 시 주의사항

1. **이벤트 기반 → 스레드 기반**: IDS peak는 WaitForFinishedBuffer() 사용
2. **시퀀스 버퍼**: peak에서는 AllocAndAnnounceBuffer() + QueueBuffer()
3. **노출 단위**: uEye는 ms, IDS peak는 us (마이크로초)
4. **파라미터 저장**: EEPROM 대신 UserSet 사용
