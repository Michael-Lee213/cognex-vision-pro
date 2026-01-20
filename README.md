# ■ 임플란트 앰플 자동화 조립 설비: 부시(Bush) 판별 시스템

<br>

## 1. Project Overview
임플란트 앰플(Root 세트) 자동화 조립 설비의 핵심 공정인 부시(Bush) 부품의 정밀 판별을 목적으로 합니다. 제품의 픽앤플레이스(Pick & Place)를 위한 좌표 추출과 조립 방향(상/하 및 앞/뒷면) 구분을 위한 비전 알고리즘을 구현하였으며, Cognex VisionPro 솔루션 도입을 위한 기술 검증(PoC) 및 사전 테스트 용도로 개발되었습니다. <br>

<제품이미지 출처 : Demillion>
<img width="770" height="522" alt="image" src="https://github.com/user-attachments/assets/d5c0cdcf-8a8a-4b8a-945f-5042cdfc15d6" />


<br>

## 2. Process & Algorithm Logic
본 시스템은 Cognex VisionPro를 활용하여 하드웨어 환경에 최적화된 파라미터를 미세 조정하고, 이를 `.vpp` 파일로 구성하여 C# 라이브러리 환경에서 호출하는 하이브리드 방식으로 설계되었습니다.

* **객체 인식 및 바운딩 박스 설정**: 카메라 시야 내에 임의로 놓인 부시 제품을 검출하고 분석을 위한 ROI를 동적으로 생성합니다.
* **좌표 및 각도 추출**: `CogPMAlignTool`을 활용하여 제품의 중심점(X, Y)과 회전 각도를 정밀하게 산출하며, 이는 로봇의 Picking 공정 데이터로 연동됩니다.
* **제품 분류 (상/하 및 앞/뒷면)**: 앰플 조립 시 조립 방향을 결정짓는 형상적 특징(Hole, 단차 등)을 분석하여 제품의 방향성을 판별합니다. <br>

<프로그램 이미지  출처 : 밀리세컨즈> <br>
<img width="459" height="131" alt="image" src="https://github.com/user-attachments/assets/23bbcf52-d7e6-4457-91ff-186c9cccb02b" />

<br>

## 3. Implementation Details
* **VisionPro Integration**: QuickBuild를 통해 미세 조정된 알고리즘을 `.vpp` 파일로 관리하여 유지보수성을 확보했습니다.
* **Dynamic Parameter Control**: Reflection 기법을 도입하여 VisionPro 버전 호환성을 확보하고, 런타임 시 툴의 Segmentation 파라미터를 동적으로 제어합니다.
* **Hybrid Inspection**: 고정밀 검출이 필요한 영역은 Blob 분석을, 고속 연산이 필요한 영역은 직접적인 픽셀 분석(`BrightnessAnalyzer`)을 사용하는 다중 알고리즘 체계를 구축했습니다.

<br>

## 4. Key Components
* **Program.cs**: 테스트 이미지 시퀀스 실행 및 전체 공정 제어 로직.
* **VisionProClassifier.cs**: 제품의 특징점 분석을 통한 상/하 판별 핵심 엔진.
* **CameraGrabber.cs**: 하드웨어 또는 시뮬레이션 환경에서의 이미지 획득 인터페이스.
* **train_PMAlign.vpp**: 최적화된 패턴 매칭 및 특징 추출 파라미터 세트.

<br>

## 5. Result Analysis (Screenshots)
이미지 분석을 통해 제품의 좌표 및 각도를 검출하고, 형상 분석을 거쳐 최종적으로 상단/하단을 분류합니다.

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
