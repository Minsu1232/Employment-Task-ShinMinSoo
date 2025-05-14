# 액션핏 Unity 개발자 기술과제

**지원자:** 신민수

---

## 🌟 주요 변경 사항

### 1. 코드 구조 개선

#### ✅ MVC 및 MVP 패턴 구현

- **Model**: 게임 데이터 구조를 추상화하고 인터페이스 기반으로 정리  
  - `IGameData`, `IPositionData`, `IColorableData`, `IGimmickData` 등의 인터페이스 도입  
  - 데이터 클래스들(`BlockData`, `BoardBlockData`, `WallData` 등)을 추상 계층화하고 상속 구조 개선

- **View**: 시각적 표현을 담당하는 클래스 분리  
  - `WallView`, `ParticleEffectView`, `VertexStencilEffectView` 등 구현  
  - 효과 렌더링과 시각적 피드백을 전담하는 클래스 구현

- **Controller**: 게임 로직 처리 클래스 세분화  
  - 기존 `BoardController`를 여러 서브 컨트롤러로 분할  
  - `BlockFactory`, `BlockDestroyManager`, `WallBuilder`, `BoardBuilder` 등으로 책임 분리  
  - `StageController`를 중앙 컨트롤러로 활용하여 다른 컨트롤러들 관리

- **Presenter**: 기믹 로직 처리 담당  
  - `IGimmickPresenter` 인터페이스 정의  
  - `GimmickPresenter` 추상 클래스를 통한 공통 기능 구현  
  - 각 기믹 유형별 구체 프레젠터 클래스(`IceGimmick`, `KeyGimmick` 등)  
  - Model과 View 사이의 중개자 역할 수행

---

### 2. 컴포넌트 분리

#### ✅ BlockDragHandler 분리

- `BlockInputHandler`, `BlockPhysicsHandler`, `BlockGridHandler`로 분리  
- 입력 처리, 물리 시뮬레이션, 그리드 정렬 로직을 명확히 구분

#### ✅ BoardController 분리

- **StageController**  
  - 게임의 중앙 관리자 역할로 변경  
  - 각 서브 컨트롤러를 초기화하고 관리  
  - 스테이지 전환 및 레벨 진행 관리

- **BoardBuilder**  
  - 보드 블록 생성 및 초기 설정  
  - 보드의 구조적 데이터 관리  
  - `CreateBoardAsync` 메서드로 보드 생성 로직 담당

- **BlockFactory**  
  - 플레이어 블록 생성 및 설정  
  - 블록 그룹 구성 및 초기화  
  - `CreatePlayingBlocksAsync` 메서드로 블록 생성 로직 분리

- **WallBuilder**  
  - 벽 오브젝트 생성 및 관리  
  - 벽 데이터 처리 및 좌표 시스템 관리  
  - `CreateCustomWalls` 메서드로 벽 생성 로직 분리

#### ✅ 게임 메커니즘 관련 컨트롤러

- **CheckBlockGroupManager**  
  - 체크 블록 그룹 관리 및 파괴 조건 확인  
  - 기존 `CheckCanDestroy` 메서드의 분리 및 개선  
  - 블록 그룹 간 관계 추적 및 관리

- **BlockDestroyManager**  
  - 블록 파괴 처리 및 효과 조정  
  - 파괴 애니메이션 실행 및 이벤트 발생  
  - `DestroyBlockWithEffect` 메서드로 효과 처리

#### ✅ 시각 효과 컨트롤러

- `VisualEffectManager`, `ParticleEffectManager`  
  - 게임 내 모든 시각 효과 관리  
  - 파티클 시스템 및 셰이더 효과 제어  
  - 스텐실 버퍼 효과 및 클리핑 처리

---

### 3. Visual Effect 최적화

#### ✅ 스텐실 버퍼 기반 셰이더 구현

- **버텍스 기반 스텐실 효과**  
  - `VertexStencilEffectView` 클래스 구현으로 실제 오브젝트 대신 버텍스 기반 스텐실 사용

- **셰이더 시스템**  
  - 벽 오브젝트가 먼저 렌더링되며 스텐실 버퍼에 값을 작성  
  - 블록 오브젝트가 나중에 렌더링되며 스텐실 버퍼의 값을 확인  
  - 스텐실 테스트를 통과한 경우에만 블록의 해당 부분이 렌더링

#### ✅ 성능 개선

- 기존 방식: 많은 수의 Quad 오브젝트 생성 → 성능 저하  
- 개선 방식: GPU에서 처리되는 스텐실 버퍼 사용 → 성능 향상

#### ✅ 시각적 품질 향상

- 더 정확한 클리핑 경계  
- 자연스러운 벽 통과 효과  
- 어느 각도에서도 올바른 가림 효과 제공

---

### 4. 이벤트 시스템 도입

#### ✅ 게임 이벤트 구조화

- **ScriptableObject 기반 이벤트**  
  - `GameEvent<T>` 템플릿 클래스를 통한 타입 안정성 보장  
  - 이벤트 리스너 인터페이스 구현으로 표준화된 이벤트 구독 패턴

- **이벤트 유형**  
  - `IntEvent`, `BlockEvent`, `CheckDestroyEvent` 등 다양한 데이터를 전달하는 이벤트 정의  
  - 컴포넌트 간 직접 참조 없이 통신 가능

---

### 5. ScriptableObject 기반 설정 시스템

#### ✅ 중앙화된 게임 설정

- `GameConfig` 클래스  
  - 모든 게임 설정을 하나의 ScriptableObject로 관리  
  - 인스펙터에서 직접 값을 편집하여 빠른 반복 개발 가능

#### ✅ 세분화된 설정 클래스

- `BlockConfig`, `BoardConfig`, `WallConfig`, `VisualConfig` 등으로 설정 분리  
- 각 컨트롤러가 필요한 설정만 참조하여 의존성 감소

#### ✅ 설정 주입 시스템

- `ConfigInjector` 클래스  
  - 생성된 게임 오브젝트에 설정 참조를 전달하는 헬퍼 클래스  
  - 의존성 주입 패턴의 간소화된 구현

---

### 6. 스테이지 에디터 구현

#### ✅ 직관적인 UI/UX

- 비개발자 친화적 인터페이스로 쉬운 스테이지 제작  
- 드래그 앤 드롭 방식의 간편한 요소 배치

#### ✅ 블록 및 기믹 관리

- 블록 배치 및 색상 설정 도구  
- 다양한 기믹(별, 잠금장치, 열쇠 등) 설정 패널  
- 확장 가능한 기믹 시스템으로 추후 신규 기능 쉽게 추가

#### ✅ 벽 및 레벨 구성

- 다양한 벽 유형과 방향 설정  
- 출구 위치 지정 기능  
- 3D 공간에서의 직관적인 배치 보조선

#### ✅ 테스트 및 검증

- 에디터 내 즉시 플레이 기능  
- 스테이지 유효성 자동 검사  
- 플레이 중 실시간 스테이지 수정 가능

#### ✅ 데이터 관리

- **저장 및 불러오기**  
  - ScriptableObject 형식으로 Unity 내 저장  
  - JSON 파일 내보내기/가져오기 지원  
  - 스테이지 복제 및 변형 기능

- **파일 포맷**  
  - 인간이 읽을 수 있는 JSON 구조로 외부 편집 가능

#### ✅ 구현 방식

- `Unity EditorWindow` 기반의 독립 에디터 창  
- 모듈식 설계로 향후 기능 확장 용이
