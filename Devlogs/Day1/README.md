# Project θ 개발 일지 — 1일차

## 개발 목표

Project θ의 본격적인 기능 구현에 앞서 Unity 프로젝트 기반을 구축하고, 이후 2.5D 사이드뷰 프로토타입을 안정적으로 확장할 수 있는 기본 구조를 준비했다.

## 개발 환경

- Unity 6 `6000.3.21f1`
- Universal Render Pipeline `17.0.4`
- Input System `1.11.2`
- Physics 2D
- PC / Steam 우선 개발
- 2D Sprite 캐릭터 기반
- 2.5D 사이드뷰 구조

## 구현 내용

### 프로젝트 기본 구조

- Unity 프로젝트 및 Git 저장소 기반 구성
- Unity 자동 생성 파일을 제외하기 위한 `.gitignore` 구성
- Git 텍스트 파일 줄바꿈 관리를 위한 `.gitattributes` 구성
- `Assets/_Project` 중심의 전용 프로젝트 폴더 구조 구성
- Runtime Assembly Definition 구성

### 렌더링 기반

- URP 기반 프로젝트 설정
- 2D Renderer용 설정 구조 구성
- 2D 캐릭터의 앞뒤 표현을 위한 Sorting Layer 기반 마련
- Y 좌표를 이용한 Depth Sorting 구조 추가
- Orthographic Camera 기반 테스트 환경 구성

### 씬 구조

- `Boot` 씬 구성
- `Test` 씬 구성
- Boot 실행 후 Test 씬으로 진입하는 기본 흐름 구성
- Build Settings에 Boot / Test 씬 등록

### 입력 및 2D 시스템 기반

- Unity Input System 패키지 적용
- Physics 2D 모듈 적용
- 플레이어 및 NPC 기능을 확장할 수 있도록 스크립트 영역 분리
- Player / NPC / Hypnosis / Companion / Stage / UI / Core 구조 구성

### 프로토타입 기반 코드

- 플레이어 사이드뷰 컨트롤러 기반
- 카메라 추적 기반
- NPC 프로토타입 기반
- 최면 게이지 및 최면 대상 처리 기반
- 동행자 처리 기반
- 충동 게이지 기반
- 정기 회수 및 스테이지 목표 기반
- 프로토타입 HUD 기반
- Test 씬에서 임시 Sprite를 이용해 기능을 확인할 수 있는 런타임 부트스트랩 구성

## 프로젝트 구조

```text
Assets/
└─ _Project/
   ├─ Editor/
   ├─ Scenes/
   ├─ Scripts/
   │  ├─ Companion/
   │  ├─ Core/
   │  ├─ Hypnosis/
   │  ├─ NPC/
   │  ├─ Player/
   │  ├─ Stage/
   │  └─ UI/
   ├─ Settings/
   └─ Tests/

Packages/
ProjectSettings/
```

## 확인 결과

최신 `main` 커밋 `28beb3e`를 기준으로 프로젝트 기반 파일을 확인했다.

- Unity 버전이 Unity 6 계열로 설정되어 있음
- URP 패키지가 등록되어 있음
- Input System 패키지가 등록되어 있음
- Physics 2D 모듈이 등록되어 있음
- Runtime asmdef가 Input System을 참조하도록 구성되어 있음
- BootLoader가 Test 씬으로 전환하도록 구성되어 있음
- Boot / Test 씬이 Build Settings에 등록되어 있음
- `_Project` 중심의 코드 및 설정 구조가 구성되어 있음

원격 저장소에는 별도의 CI 빌드 검증이 연결되어 있지 않으므로 실제 Unity Editor 컴파일 및 플레이 모드 오류 여부는 로컬 실행을 통해 계속 확인한다.

## 1일차 완료 상태

1일차의 목표였던 프로젝트 기반 구축을 완료했다.

이후 개발은 현재 구조를 유지하면서 실제 플레이어 2.5D 이동, 대시 및 입력 처리를 우선 구현하고 테스트 씬에서 기능을 검증하는 방향으로 진행한다.
