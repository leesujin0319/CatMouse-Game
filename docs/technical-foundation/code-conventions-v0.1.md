# 쥐생역전: 찍찍런 — Unity 코드 컨벤션 v0.1

> 이 문서는 [`mvp-architecture-v0.1.md`](mvp-architecture-v0.1.md)의 구현 규칙입니다. 새 C# 코드와 Unity 프리팹·데이터 변경은 두 문서를 함께 따릅니다.

## 1. 기본 원칙

1. 한 클래스는 하나의 게임 책임만 가집니다. `RunSession`이 UI·카메라·저장 세부 구현을 직접 소유하지 않습니다.
2. 게임 규칙은 런타임 시스템에, 수치와 선택지는 `ScriptableObject` 정의에, 표시는 View·Presenter에 둡니다.
3. `Find`, `FindObjectOfType`, 태그 검색으로 의존성을 숨기지 않습니다. Inspector 참조 또는 Composition Root 주입을 사용합니다.
4. Unity 생명주기 순서에 기대는 암묵적 초기화보다 명시적인 `Initialize`·`Dispose` 계약을 우선합니다.

## 2. 경로와 네임스페이스

```text
Assets/_Project/Scripts/
  Runtime/Bootstrap/ Core/ Run/ Player/ Enemy/ Skill/ Meta/ UI/ Presentation/
  Editor/
  Tests/EditMode/
  Tests/PlayMode/
```

- 런타임 네임스페이스는 `CatMouse.Game.<Feature>` 형식을 사용합니다. 예: `CatMouse.Game.Player`.
- 파일 이름과 public 최상위 타입 이름은 동일하게 합니다. 예: `PlayerVerticalMovement.cs` / `PlayerVerticalMovement`.
- 정의 데이터는 `*Definition`, 런타임 상태는 `*Runtime`, 화면 표시는 `*View` 또는 `*Presenter`, 테스트는 `*Tests` 접미사를 사용합니다.

## 3. C# 표기와 구조

- 타입·메서드·프로퍼티·이벤트는 `PascalCase`를 사용합니다.
- private 필드와 `[SerializeField] private` 필드는 `_camelCase`를 사용합니다.
- 지역 변수와 매개변수는 `camelCase`를 사용합니다.
- 상수는 `PascalCase`, 인터페이스는 `I` 접두사를 사용합니다.
- public 필드는 사용하지 않습니다. Inspector 노출은 `[SerializeField] private`와 읽기 전용 프로퍼티를 우선합니다.
- `var`는 우변으로 타입이 명확할 때만 사용하고, 게임 수치·컬렉션·비동기 핸들은 타입을 드러냅니다.
- 매직 넘버는 정의 데이터 또는 이름 있는 상수로 올립니다. 밸런스 수치를 코드에 흩어 두지 않습니다.

## 4. 게임플레이와 입력

- 입력은 Unity Input System Action을 통해서만 읽습니다. 특정 키·터치 API를 `Update`에서 직접 호출하지 않습니다.
- `PlayerVerticalMovement`는 입력을 연속 세로 속도로 변환하고, 플레이 영역 상·하 경계를 제한합니다. 고정 레인·스냅 이동을 구현하지 않습니다.
- 일반 입력 수집은 `Update`, `Rigidbody2D` 물리 이동은 `FixedUpdate`에서 처리합니다.
- `RunSession` 상태가 `Playing`이 아닐 때 플레이 이동·공격 명령은 실행되지 않아야 합니다.
- 메서드 이름은 의도를 드러냅니다. 검증 뒤 실행은 `Try...`, 이벤트 처리기는 `Handle...`, Unity 콜백은 `On...`을 사용합니다.

## 5. 데이터, 이벤트, 수명주기

- `AbilityDefinition`, `EnemyDefinition`, `SpawnPatternDefinition`, `RunBalanceDefinition`은 불변에 가깝게 다루며 런타임에서 원본 에셋을 수정하지 않습니다.
- 런타임 변화는 `AbilityRuntime`, `EnemyRuntime`, `MetaProgressionState`처럼 별도 객체에 둡니다.
- Event Bus에는 `PlayerVerticalMoved`, `EnemyDefeated`, `CheeseCollected`, `LevelUpStarted`, `RunEnded`처럼 타입이 있는 이벤트만 게시합니다.
- 이벤트 구독자는 비활성화·반납·Dispose 시점에 반드시 구독을 해제합니다. 풀 오브젝트가 중복 구독하지 않게 합니다.
- 실행 순서·보상 지급처럼 순서 보장이 필요한 규칙은 Event Bus 구독 순서에 의존하지 않습니다.

## 6. UI, 카메라, 저장

- UI는 상태를 표시하고 명령을 요청할 뿐 전투 수치·레벨·저장 데이터를 직접 수정하지 않습니다.
- Main Camera에는 `CinemachineBrain`을 하나만 둡니다. `CameraTarget` 추적, Virtual Camera 우선순위, Impulse는 `CameraDirector`가 관리합니다.
- 카메라 흔들림은 짧고 약하게 제한하며, 자유 세로 이동의 충돌·위험 판독을 가리지 않습니다.
- 저장은 `ISaveStore`를 통해서만 수행합니다. `MetaProgressionService`가 `RunResult`를 한 번 처리한 뒤 저장을 요청합니다.
- 저장 데이터에는 `schemaVersion`을 포함하고, 런 중 임시 체력·스킬 쿨다운·적 상태는 저장하지 않습니다.

## 7. 테스트와 리뷰

- EditMode 테스트는 정의 데이터, 능력 중첩, 보상·저장 변환, 상태 전이를 다룹니다.
- PlayMode 테스트는 세로 이동 경계, 풀 반납, 치즈 회수, 레벨업, 사망→결과→재시작을 다룹니다.
- 코드 리뷰에서는 책임 과다, 숨은 `Find`, Event Bus 구독 해제, 풀 초기화, `.meta` 누락, WebGL 호환성을 확인합니다.
- 패키지·ProjectSettings·대형 씬·공유 프리팹 변경은 기능 코드와 분리해 검토하기 쉬운 커밋으로 만듭니다.
