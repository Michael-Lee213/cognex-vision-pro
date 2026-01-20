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
* VPP 기반 패턴 데이터 추출: 사전에 학습된 train_PMAlign.vpp 파일을 로드하여 테스트 이미지 내 제품의 특징을 탐색합니다. 학습된 패턴을 바탕으로 제품의 정확한 위치(X, Y)와 회전 각도를 실시간으로 따와서 후속 분석의 기준점으로 활용합니다.
* 지능형 툴 로딩 (Dynamic Extraction): 비전 설정 파일(.vpp) 내부의 저장 방식(단독 저장, 그룹화 저장 등)에 상관없이, 실행 시점에 파일 내부를 스스로 탐색하여 필요한 툴을 찾아내는 리플렉션(Reflection) 로직을 구현했습니다. 이를 통해 파일 내부 구조가 변경되어도 코드 수정 없이 대응이 가능합니다.
* Adaptive Feature Analysis (유동적 ROI): 패턴 매칭으로 찾은 좌표에 맞춰 검사 영역(ROI)을 동적으로 이동시켜 배치합니다. 이후 CogBlobTool을 사용하여 중앙 홀의 면적을 계산하거나 각인 상태를 분석하여 제품의 상태를 최종 판정합니다.
* DLL 직접 제어 및 하이브리드 검사: Cognex 핵심 라이브러리(DLL)를 직접 참조하여 툴 파라미터를 제어합니다. VisionPro 엔진의 분석 결과와 BrightnessAnalyzer를 통한 직접적인 픽셀 데이터 접근 방식을 결합하여 판독 신뢰성과 처리 속도를 동시에 확보했습니다.
<br>

## 4. Key Components
* **Program.cs**: 메인 컨트롤러. 이미지 루프 처리 및 전반적인 비즈니스 로직 제어.
* **VisionProClassifier.cs**: Blob 분석 기반의 상/하 판별 및 중앙 홀 면적 계산 엔진.
* **CameraGrabber.cs**: VPP 내 AcqFifoTool을 이용한 이미지 획득 인터페이스.
* **train_PMAlign.vpp**: 최적화된 패턴 정보 및 VisionPro 툴 파라미터 저장 데이터.

<br>

## 5. Result Analysis (Screenshots)
이미지 분석을 통해 제품의 좌표 및 각도를 검출하고, 중앙 홀의 존재 여부와 각인 상태를 확인하여 최종 결과를 도출합니다.

<전체 계층구조>
<br><br> 
<img width="283" height="588" alt="image" src="https://github.com/user-attachments/assets/1957e13c-5057-44cb-926b-09bc8b077e4c" /> <br>

<결과 이미지>
<br><br> 
<img width="382" height="194" alt="image" src="https://github.com/user-attachments/assets/3dd69004-84e4-410a-bf55-e8ca713192fd" />


---

### Contact & Maintenance
* **Developer**: Michael Lee
* **Field**: Vision Algorithm Design / Factory Automation System Integration
