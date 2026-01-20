# ■ 임플란트 앰플 자동화 조립 설비: 부시(Bush) 판별 시스템

<br>

## 1. Project Overview
임플란트 앰플(Root 세트) 자동화 조립 설비의 핵심 공정인 부시(Bush) 부품의 정밀 판별을 목적으로 합니다. 제품의 픽앤플레이스(Pick & Place)를 위한 좌표 추출과 조립 방향(상/하 및 앞/뒷면) 구분을 위한 비전 알고리즘을 구현하였으며, Cognex VisionPro 솔루션 도입을 위한 기술 검증(PoC) 및 사전 테스트 용도로 개발되었습니다.

<br>

<p align="center">
  <img src="https://github.com/user-attachments/assets/d5c0cdcf-8a8a-4b8a-945f-5042cdfc15d6" width="80%" alt="제품이미지">
  <br>
  <sub>이미지 출처 : Demillion</sub>
</p>

<br>

## 2. Process & Algorithm Logic
본 시스템은 Cognex VisionPro의 DLL 참조를 통해 VPP 파일 내의 알고리즘 객체를 직접 제어하며, 하드웨어 최적화 파라미터를 기반으로 테스트 이미지에 대한 정밀 판별을 수행합니다.

* **객체 인식 및 동적 ROI 설정**: `CogPMAlignTool`을 통해 카메라 시야 내 부시 제품을 검출하고, 인식된 좌표를 중심으로 분석용 ROI를 실시간으로 생성 및 추종합니다.
* **좌표 및 각도 추출**: 제품의 중심점(X, Y)과 회전 각도를 산출하여 픽킹(Picking) 공정 시 로봇 가이드를 위한 정밀 좌표 데이터를 생성합니다.
* **상/하단 판별 (Central Hole Logic)**: 
  - **상단 장착 제품**: 제품 중앙에 관통된 구멍(Hole)이 존재하는 특성을 활용합니다. Blob 알고리즘이 해당 영역의 픽셀 면적(Area)을 계산하여 임계값 이상의 면적이 검출되면 '상단'으로 판별합니다.
  - **하단 장착 제품**: 제품 중앙이 막혀 있는 구조로, 픽셀 계산 시 유효한 면적이 검출되지 않는 경우 '하단'으로 분류합니다.
* **앞/뒷면 구분 (Marking/OCR Analysis)**: 제품 표면에 각인된 정보를 OCR(광학 문자 인식) 및 특징점 분석 알고리즘으로 확인하여 제품의 앞/뒷면 방향성을 최종 확정합니다.

<br>

<p align="center">
  <img src="https://github.com/user-attachments/assets/23bbcf52-d7e6-4457-91ff-186c9cccb02b" width="80%" alt="프로그램 이미지">
  <br>
  <sub>이미지 출처 : 밀리세컨드</sub>
</p>

<br>

## 3. Implementation Details
* **DLL-Based Logic Integration**: `Cognex.VisionPro.PMAlign.dll` 등 핵심 라이브러리를 참조하여, `.vpp` 파일 내 알고리즘 객체를 C#에서 실시간 런타임 객체로 변환하여 제어합니다.
* **Dynamic Tool Extraction via Reflection**: `CogSerializer`를 통해 로드된 객체 계층(ToolGroup, Job 등)에 관계없이 리플렉션을 사용하여 필요한 비전 툴을 동적으로 추출함으로써 유지보수성을 극대화했습니다.
* **Hybrid Inspection Architecture**: 
  - **VisionPro Engine**: `CogBlobTool`을 이용한 고정밀 형상 및 면적 분석 수행.
  - **Direct Pixel Access**: `BrightnessAnalyzer`를 통해 비트맵 픽셀 데이터에 직접 접근하여 처리 속도를 최적화하는 하이브리드 검사 체계를 구축했습니다.
* **Advanced Error Handling**: Segmentation 설정 시 버전별로 상이한 `ConnectivityMode`(EightConnected/Grey8Connected)를 리플렉션으로 체크하는 방어적 프로그래밍을 적용했습니다.

<br>

## 4. Key Components
* **Program.cs**: 메인 컨트롤러. 이미지 루프 처리 및 전반적인 비즈니스 로직 제어.
* **VisionProClassifier.cs**: Blob 분석 기반의 상/하 판별 및 중앙 홀 면적 계산 엔진.
* **CameraGrabber.cs**: VPP 내 AcqFifoTool을 이용한 이미지 획득 인터페이스.
* **train_PMAlign.vpp**: 최적화된 패턴 정보 및 VisionPro 툴 파라미터 저장 데이터.

<br>

## 5. Result Analysis (Screenshots)
이미지 분석을 통해 제품의 좌표 및 각도를 검출하고, 중앙 홀의 존재 여부와 각인 상태를 확인하여 최종 결과를 도출합니다.

<br>

<p align="center">
  <img src="./docs/pma_sample.png" width="45%" title="Object Detection" alt="PMA Result">
  <img src="./docs/blob_sample.png" width="45%" title="Classification" alt="Blob Result">
</p>

<br>

---

### Contact & Maintenance
* **Developer**: Michael Lee
* **Field**: Vision Algorithm Design / Factory Automation System Integration
