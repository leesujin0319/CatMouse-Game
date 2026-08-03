# 메타 성장·장비·로컬 저장 계약

## 목적

로비의 영구 강화와 장비 선택을 인게임 런 스탯에 전달합니다. 표시 문구·효과·비용·장비 목록은 코드가 아닌 데이터 자산에서 관리하며, 로비 UI는 저장소에 직접 접근하지 않습니다.

## 데이터 원본

| 구분 | 자산 | 관리 내용 |
| --- | --- | --- |
| 메타 카탈로그 | `Assets/_Project/Data/Meta/MetaProgressionCatalog.asset` | 강화·장비 ID, 장착 슬롯, 표시 순서, 최대 레벨, 강화 비용 |
| 런 아이템 정의 | `Assets/_Project/Data/Meta/Items/*.asset` | 이름, 설명, 아이콘, 런에서 적용할 스탯 효과 |
| 로컬 진행 상태 | PlayerPrefs `CatMouse.MetaProgression.v1` | 코인, 강화 레벨, 장착 ID, 볼륨, 보상 지급 이력 |

- 새 강화나 장비를 추가할 때는 먼저 `RunItemDefinition`을 만들고 카탈로그에 연결합니다.
- 강화 비용은 `기본 비용 + (현재 레벨 × 증가 비용)`으로 카탈로그에서 계산합니다.
- `LobbySceneInstaller`는 자산이 없는 최초 1회에만 기본 카탈로그와 아이템을 만듭니다. 이후에는 기존 자산 값을 덮어쓰지 않습니다.

## ID와 저장 구조

강화와 장비는 순번이나 enum 값이 아닌 고정 문자열 ID로 저장합니다.

## 현재 프로토타입 성장 내용

| 구분 | 항목 | 효과 | 성장 또는 장착 규칙 |
| --- | --- | --- | --- |
| 영구 강화 | 도토리 단련 | 공격력 +1 | 최대 5레벨 |
| 영구 강화 | 앞발 훈련 | 공격 속도 +15% | 최대 5레벨 |
| 영구 강화 | 질주 훈련 | 전진 속도 +10% | 최대 5레벨 |
| 영구 강화 | 새총 튜닝 | 투사체 속도 +15% | 최대 5레벨 |
| 영구 강화 | 꼬리 조준 훈련 | 공격 범위 +15% | 최대 5레벨 |
| 모자 슬롯 | 탐험 모자 | 투사체 속도 +40% | 모자 슬롯 1개만 장착 |
| 모자 슬롯 | 나침반 모자 | 공격 범위 +30% | 모자 슬롯 1개만 장착 |
| 갑옷 슬롯 | 도토리 갑옷 | 공격력 +1 | 갑옷 슬롯 1개만 장착 |
| 갑옷 슬롯 | 철갑 도토리 갑옷 | 공격력 +2 | 갑옷 슬롯 1개만 장착 |
| 신발 슬롯 | 태엽 신발 | 전진 속도 +12% | 신발 슬롯 1개만 장착 |
| 신발 슬롯 | 통통 태엽 장화 | 상하 이동 속도 +25% | 신발 슬롯 1개만 장착 |

장비 효과는 영구 강화와 같은 `PlayerRunStats`의 영구 아이템 스택으로 전달합니다. 따라서 인게임 담당자는 장비 슬롯을 직접 읽지 않고, 시작 시 적용된 스탯만 사용합니다.

```text
upgradeLevels
  - attack_damage: 2
  - attack_speed: 1

equippedEquipmentBySlot
  - Hat: pantry_cap
  - Armor: acorn_armor
  - Shoes: windup_shoes
```

- 장비는 무기 교체가 아닙니다. `Hat`, `Armor`, `Shoes` 슬롯에 각각 하나씩 동시 장착하며, 각 장비는 외형 레이어와 런 스탯 특성을 함께 정의합니다.
- 1차 장비군은 슬롯마다 두 종류입니다. `탐험 모자`·`나침반 모자`, `도토리 갑옷`·`철갑 도토리 갑옷`, `태엽 신발`·`통통 태엽 장화` 중 각 슬롯에는 하나만 장착합니다. 실제 외형 스프라이트는 장비 ID와 슬롯을 기준으로 캐릭터 표현 계층에서 연결합니다.
- 카탈로그에서 ID를 제거하거나 변경하면 기존 로컬 저장값은 적용되지 않습니다. 출시 이후 ID는 변경하지 않습니다.
- 기존 스키마 2·3 저장값은 첫 로드 시 기존 무기형 장비 ID를 해당 슬롯 장비 ID로 한 번 이전합니다.
- 스키마 4부터는 같은 슬롯 안에 여러 장비를 코드 수정 없이 카탈로그에 추가할 수 있습니다.

## 화면과 런 전달 흐름

```text
MetaProgressionCatalog
  → LobbySceneController
  → MetaProgressionService
  → PlayerPrefsSaveStore

MetaProgressionCatalog + 저장 상태
  → MetaProgressionApplier
  → PlayerRunStats persistent stack
  → 자동 공격·이동·투사체 스탯
```

- `LobbySceneController`는 강화 목록을 카탈로그 순서대로 표시하고, 장비는 `모자·갑옷·신발` 탭에서 해당 슬롯의 목록만 표시한 뒤 ID로 장착을 요청합니다.
- `MetaProgressionService`는 비용 검증, 코인 차감, 장착 저장, 볼륨 저장만 담당합니다. 기존 로컬 저장의 `cheese` 필드명은 데이터 호환을 위해 유지합니다.
- `MetaProgressionApplier`는 `GameScene` 시작 시 카탈로그를 기준으로 저장된 강화 스택과 슬롯별 장비를 모두 `PlayerRunStats`에 적용합니다.
- 인게임 코드가 장비 외형, 전용 연출 등 추가 정보를 필요로 하면 `MetaProgressionService.GetLoadout()`만 사용합니다. 이 값은 강화의 영구 스택(`PersistentItemStacks`)과 장착 장비 목록(`EquippedItems`)을 함께 제공하며, 로비 UI나 PlayerPrefs를 직접 참조하지 않습니다.
- 런 중 카드·임시 아이템은 기존 `TryAcquire` 흐름을 사용하며, `ClearPersistentItems()` 이후에도 메타 효과는 다시 적용할 수 있습니다.

## 로비 갱신 계약

- 강화 구매, 장비 장착, 런 보상 지급, 볼륨 변경은 저장 완료 후 `MetaProgressionService.ProgressionChanged`를 발생시킵니다.
- `LobbySceneController`는 이 이벤트를 구독하므로 코인 잔액, 강화 레벨·비용·활성 상태, 장착 상태가 같은 데이터 원본을 기준으로 즉시 갱신됩니다.
- 현재 장비는 무료 장착 방식입니다. 장비 가격·소유·잠금 데이터가 카탈로그에 정의되기 전에는 장비 구매 기능을 추가하지 않습니다.
- 장비는 `Hat`, `Armor`, `Shoes` 세 슬롯으로 구분하며, 저장 상태는 슬롯별 장비 ID를 하나만 유지합니다. 같은 슬롯의 장비를 장착하면 이전 장비를 교체하며, 장착 중인 장비의 버튼은 `장착 해제`로 표시합니다.

## 런 종료 보상 계약

종료 담당자는 결과를 확정한 뒤 한 번만 아래 계약을 호출합니다.

```csharp
var result = new RunResult(runId, coinReward);
bool rewarded = MetaProgressionService.TryApplyRunResult(result);
```

- `runId`는 비어 있지 않은 고유 문자열이어야 합니다.
- 같은 `runId`는 최근 32개 이력 안에서 중복 지급하지 않습니다.
- `coinReward`는 1 이상이어야 합니다.
- `PlayerRunGameOverController`는 `RunCoinCollector.CollectedCoinCount`를 게임 종료 시 위 계약으로 한 번만 지급합니다. 생성된 `runId`와 지급 이력으로 중복 종료 보상을 막습니다.

## 검수 기준

1. 카탈로그 자산에서 비용 또는 최대 레벨을 바꾸면 코드 변경 없이 로비 표시와 구매 검증이 함께 바뀝니다.
2. 모자·갑옷·신발을 각각 장착하고 로비를 재실행하면 슬롯별 장착 ID와 선택 상태가 복원됩니다.
3. `GameScene` 시작 시 장착 장비 3개와 강화 스택이 `PlayerRunStats`에 적용됩니다.
