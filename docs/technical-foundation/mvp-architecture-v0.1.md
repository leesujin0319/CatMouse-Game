# 쥐생역전: 찍찍런 — MVP 기술 아키텍처 v0.1

> 적용 대상: Unity 6000.3.5f2, URP 2D, PC WebGL 우선. 이 문서는 동화풍 식료품 저장실 1개 스테이지의 플레이 가능한 사전 과제 빌드를 위한 최소 구조를 정의합니다.

## 1. 설계 원칙

1. **플레이가 우선입니다.** 자유로운 상하 이동, 자동 공격, 치즈 획득, 레벨업, 결과 화면이 하나의 안정적인 루프로 동작해야 합니다.
2. **데이터와 런타임 상태를 분리합니다.** 능력·적·스폰 설정은 `ScriptableObject` 정의로, 체력·쿨다운·스택은 런타임 인스턴스로 관리합니다.
3. **조립은 한 곳에서 합니다.** 시스템은 서로를 `Find`하지 않습니다. 전역 서비스는 부트스트랩에서, 씬 전용 시스템은 씬 조립 루트에서 연결합니다.
4. **의존성은 작은 계약으로 연결합니다.** UI·카메라·오디오는 게임플레이 상태를 직접 변경하지 않고, 명령 또는 타입이 있는 이벤트를 통해 요청합니다.
5. **WebGL을 기준으로 검증합니다.** 외부 서버·런타임 AI·유료 SDK 없이 실행되며, 반복 생성되는 오브젝트는 풀링합니다.

## 2. 확정 기술 선택

| 영역 | 현재 결정 | 근거 |
| --- | --- | --- |
| 입력 | Unity Input System | 프로젝트에 `com.unity.inputsystem` 1.17.0이 이미 포함되어 있습니다. |
| 씬 조립 | 수동 `Composition Root` | MVP 규모에서는 실행 순서가 보이고 디버깅이 단순합니다. |
| 이벤트 | 타입이 있는 로컬 Event Bus | 발신자와 수신자의 직접 참조를 줄이고 테스트가 쉽습니다. |
| 상태 | 플레이어·런 세션의 유한 상태 머신(FSM) | 행동 트리는 현재 적 3종 규모에 필요하지 않습니다. |
| 데이터 | `ScriptableObject` 정의 + 런타임 상태 | 능력·적 수치·스폰 웨이브 조정이 빠릅니다. |
| 반복 생성 | 프리팹 오브젝트 풀 | 도토리, 치즈, 적, 피격 이펙트의 GC·Instantiate 비용을 줄입니다. |
| UI | 화면 매니저 + 표시 전용 View | UI가 전투 규칙을 직접 소유하지 않게 합니다. |
| 카메라 | Cinemachine 3.1.7 | `CinemachineBrain`, 러너용 Virtual Camera, Impulse로 추적·블렌딩·피격 연출을 분리합니다. |

`com.unity.cinemachine` 3.1.7은 Unity 6용 정식 패키지로 설치합니다. `Addressables`와 `VContainer`는 아직 도입하지 않습니다. 다중 씬 콘텐츠·대용량 리소스 또는 수동 조립의 반복 비용처럼 도입 조건이 생겼을 때 팀 검토 후 `manifest.json`과 `packages-lock.json`을 함께 변경합니다.

## 3. 씬과 수명주기

```text
00_Bootstrap
  └─ GlobalBootstrapper
       ├─ GameConfig / Settings
       ├─ EventBus
       ├─ SaveGateway / MetaProgressionService
       └─ 다음 씬 로드

10_PantryRun
  └─ PantryRunCompositionRoot
       ├─ RunSession
       ├─ PlayerVerticalMovement / PlayerCombat
       ├─ EnemySpawner / EnemyRuntime
       ├─ CheeseCollector / LevelSystem / SkillRuntime
       ├─ PoolRegistry
       ├─ RunHudPresenter / LevelUpPresenter / ResultPresenter
       └─ CameraDirector / AudioPresenter
```

- `GlobalBootstrapper`는 앱 전체에서 한 번만 필요한 설정과 공용 서비스를 준비합니다. 게임플레이 오브젝트를 직접 생성하거나 전투 규칙을 소유하지 않습니다.
- `PantryRunCompositionRoot`는 씬 전용 시스템의 생성·초기화·해제 순서를 책임집니다. 각 시스템의 내부 세부 구현을 대신하지 않습니다.
- 빠른 프로토타입은 `10_PantryRun` 단일 씬으로 시작할 수 있습니다. 이때도 부트스트랩과 씬 조립의 책임을 코드 구조상 분리해, 이후 `00_Bootstrap` 씬으로 이동 가능한 상태를 유지합니다.

## 4. 코드와 에셋 배치 기준

```text
CatMouse Game Project/Assets/
  _Project/
    Art/                 # 팀 제작 원본·임포트 설정과 .meta 포함
    Audio/
    Prefabs/             # Player, Enemy, Pickup, UI, VFX
    Scenes/              # 00_Bootstrap, 10_PantryRun
    ScriptableObjects/   # Ability, Enemy, Spawn, Run 설정
    Scripts/
      Runtime/
        Bootstrap/ Core/ Run/ Player/ Enemy/ Skill/ Meta/ UI/ Presentation/
      Editor/
    Tests/
      EditMode/ PlayMode/
```

- 기능 폴더 안에서 정의(`*Definition`), 런타임 상태(`*Runtime`), 화면 표현(`*View` 또는 `*Presenter`)을 구분합니다.
- 프리팹의 참조는 Inspector에 명시하고, 런타임 시스템은 조립 루트에서 필요한 계약만 받습니다.
- 에셋의 이동·이름 변경에는 반드시 대응하는 `.meta` 파일을 함께 포함합니다.

## 5. 게임플레이 계약

### 핵심 소유자

| 시스템 | 단일 책임 | 외부에 제공하는 결과 |
| --- | --- | --- |
| `RunSession` | 런 시작·일시정지·종료와 거리·처치·치즈·레벨 통계 | 읽기 전용 Run 상태, 결과 스냅샷 |
| `MetaProgressionService` | 영구 치즈 잔고·강화 단계·해금 상태의 적용과 저장 요청 | 메타 상태 스냅샷, 강화 적용 결과 |
| `PlayerVerticalMovement` | 플레이 영역 경계 안의 연속 상하 이동과 피격 불가 시간 | 현재 세로 위치, 이동 상태 이벤트 |
| `PlayerCombat` | 가장 가까운 적 자동 조준·도토리 발사 | 발사 요청, 피해 결과 |
| `EnemySpawner` | 거리·웨이브 기준 적과 치즈 배치 | 스폰·디스폰 이벤트 |
| `CheeseCollector` | 치즈 회수 감지와 경험치 전달 | 회수 수량 이벤트 |
| `LevelSystem` | 경험치, 레벨업 정지, 능력 선택 적용 | 레벨업 후보, 선택 결과 |
| `SkillRuntime` | 패시브 스택·액티브 쿨다운·효과 실행 | 수정자와 스킬 사용 결과 |
| `PoolRegistry` | 반복 프리팹의 생성·대여·반납 | 풀 인스턴스 |

### 이벤트 최소 집합

이벤트는 데이터 전달 또는 관찰 통지에만 사용합니다. 순서 보장이 필요한 규칙은 이벤트 구독 순서에 의존하지 않고 `RunSession` 또는 호출 계약에서 명시합니다.

```text
RunStarted, RunPaused, RunResumed, RunEnded
PlayerVerticalMoved, PlayerDamaged
EnemySpawned, EnemyDefeated
CheeseCollected, LevelUpStarted, AbilityChosen
ActiveSkillRequested, ActiveSkillUsed
```

- 이벤트 이름과 payload는 기능 담당자가 임의로 바꾸지 않습니다. 변경 전 해당 문서와 사용처를 함께 검토합니다.
- UI는 `RunSession`의 읽기 전용 스냅샷과 이벤트를 구독해 표시만 갱신합니다.
- 사운드·화면 흔들림·VFX는 전투 시스템 내부가 아니라 표현 계층에서 이벤트를 구독해 재생합니다.

## 6. 입력, 상태, UI

### 입력

- Action Map은 `Player`, `UI`로 분리합니다.
- `Player`: `MoveUp`, `MoveDown`, `ActiveSkill`, `Pause`.
- 키보드와 모바일 스와이프는 같은 게임 명령으로 변환합니다. `Update`에서 직접 특정 키를 읽지 않습니다.
- 레벨업·일시정지·결과 화면이 열린 동안은 `UI` 맵을 활성화하거나 플레이 명령을 차단합니다.

### 상태

- 런 세션은 `Boot → Playing ↔ Paused → LevelUpChoice → Playing → Result` 상태를 가집니다.
- 플레이어는 입력이 허용된 동안 세로 속도를 연속적으로 갱신합니다. 별도 이동 단계 상태 대신 `MovementEnabled ↔ MovementLocked`만 관리합니다.
- 적은 기본적으로 `Spawn → Approach → Attack 또는 Defeated → Despawn`만 사용합니다. 복잡한 판단이 실제로 필요해질 때만 행동 트리를 검토합니다.

### UI와 카메라

- HUD, 레벨업, 결과는 독립 View로 분리하고 화면 전환은 `UIFlow` 또는 `ScreenManager`가 담당합니다.
- 화면 버튼은 `ActiveSkillRequested`, `PauseRequested` 같은 명령을 보내며 전투 수치나 레벨을 직접 수정하지 않습니다.
- Main Camera에는 `CinemachineBrain`을 하나만 둡니다. 러너용 Virtual Camera는 플레이어 본체가 아닌 `CameraTarget`을 Follow해 자유로운 세로 이동을 부드럽게 추적하고, 화면 밖으로 나가지 않도록 경계를 제한합니다.
- 기본 블렌드는 가독성을 우선하는 짧은 Ease In/Out으로 시작합니다. 보스 진입·결과 화면 등 별도 구도는 우선순위가 높은 Virtual Camera로 전환합니다.
- 피격·보스 패턴에는 `CinemachineImpulse`를 사용하되, 짧고 약하게 제한합니다. 지속적인 흔들림이나 세로 동선 판독을 방해하는 Noise는 사용하지 않습니다.

## 7. 데이터·풀링·성능

- `AbilityDefinition`, `EnemyDefinition`, `SpawnPatternDefinition`, `RunBalanceDefinition`은 `ScriptableObject`로 저장하고 런타임에 원본 에셋을 수정하지 않습니다.
- 도토리, 치즈, 적, 피해 숫자, 간단한 VFX는 시작 시 필요한 수만큼 풀을 준비하고 `Spawn`/`Despawn`으로 재사용합니다.
- 풀 오브젝트는 반납 시 구독·코루틴·파티클·상태를 초기화합니다. 풀에 남은 객체가 이벤트를 중복 구독하지 않도록 합니다.
- WebGL 검증 시 Console 오류 0개, 반복 실행 후 증가하지 않는 오브젝트 수, 레벨업·재시작 후 정상 풀 회수를 확인합니다.
- Addressables는 다중 스테이지 또는 큰 에셋 묶음이 실제로 생길 때 도입합니다. 도입 시 원본 에셋 로딩 수명과 인스턴스 풀 수명을 분리해 관리합니다.

## 8. 메타 성장과 저장

- 영구 성장은 장기 로드맵의 확정 시스템입니다. 이번 기반 단계에서는 `MetaProgressionService`, `SaveGateway`, 데이터 모델의 경계를 먼저 만들고, 상점·강화 화면은 코어 러닝 루프가 안정된 뒤 구현합니다.
- `MetaProgressionState`는 `schemaVersion`, 영구 치즈 잔고, 강화 단계, 해금 상태만 보관합니다. 런 중 임시 능력·체력·쿨다운은 저장하지 않습니다.
- 런 종료 시 `RunSession`은 변경할 수 없는 `RunResult`를 만들고, `MetaProgressionService`가 보상 지급을 한 번만 적용한 뒤 저장을 요청합니다. 재시작·중복 종료로 보상이 두 번 지급되지 않게 `RunId` 또는 완료 플래그를 사용합니다.
- 저장 구현은 `ISaveStore` 계약 뒤에 둡니다. 플랫폼별 저장 방식은 교체 가능해야 하며, 저장 실패 시에도 이번 판의 결과 화면과 재시작은 막지 않습니다.
- 메타 강화는 기본 공격력, 최대 체력, 공격 속도, 치즈 획득량, 액티브 스킬 강화로 시작합니다. 각 강화의 실제 전투 수정자는 `RunSession` 시작 시점에만 적용합니다.

## 9. 다인 협업과 검증

- 기능 하나는 가능한 한 코드·프리팹·정의 데이터·테스트를 함께 변경합니다. 다른 사람이 수정 중인 씬·프리팹·ProjectSettings를 광범위하게 다시 저장하지 않습니다.
- 공유 프리팹 변경 전에는 소유자 또는 작업 범위를 팀에 알립니다. 동일 프리팹의 병렬 편집을 피하고, 충돌 시 YAML을 임의 병합하지 말고 Unity에서 확인합니다.
- 패키지와 ProjectSettings 변경은 독립 커밋 후보로 분리합니다. `Library/`, `Logs/`, `UserSettings/`는 포함하지 않습니다.
- 최소 검증: EditMode에서 능력 수치·레벨업 후보·상태 전이를 확인하고, PlayMode에서 세로 경계 안의 자유 이동·도토리 풀·치즈 회수·레벨업·사망→결과→재시작을 점검합니다.

## 10. 도입 순서

1. `RunSession`, 자유로운 상하 이동, 자동 공격, 적·치즈 풀을 구현해 30초 플레이 루프를 만듭니다.
2. 레벨업 선택과 `ScriptableObject` 능력 8개, 액티브 스킬 2개를 연결합니다.
3. Cinemachine 러너 카메라·Impulse, 식료품 저장실 패럴랙스, HUD·레벨업·결과 화면, 고양이 3종과 엘리트를 연결합니다.
4. `MetaProgressionState`와 `SaveGateway`를 연결하고, 결과 보상·영구 강화·저장 실패 복구를 검증합니다.
5. 보스·사운드·WebGL 성능 점검을 추가합니다.
6. 수동 조립 또는 리소스 관리가 실제로 부담이 되는 지점을 측정한 뒤에만 VContainer·Addressables 도입을 결정합니다.
