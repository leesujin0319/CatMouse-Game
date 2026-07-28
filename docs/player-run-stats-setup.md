# 런 아이템 스탯 설정

## 1. 기본 스탯 정의

`Assets/_Project/Data/Run/Player/`에 `Create > CatMouse > Run > Player Base Stats Definition`으로 기본 스탯 에셋을 생성합니다.

- 공격력: 도토리 한 발의 피해량
- 초당 공격 횟수: 공격속도입니다. 기존 공격 간격이 아니라 초당 발사 횟수로 입력합니다.
- 공격 사거리, 도토리 속도, 자동 전진 속도, 상하 이동 속도를 기본값으로 설정합니다.

## 2. 플레이어 연결

`TestRunnerController`, `TestPlayerAutoAttack`가 붙은 플레이어 루트에 `PlayerRunStats`를 추가하고 기본 스탯 정의 에셋을 할당합니다.

그 다음 두 컴포넌트의 `Run Stats` 필드에 같은 `PlayerRunStats`를 할당합니다. 정의나 컴포넌트를 할당하지 않은 기존 씬은 이전 Inspector 수치를 계속 사용합니다.

## 3. 런 아이템 생성

`Assets/_Project/Data/Run/Items/`에 `Create > CatMouse > Run > Item Definition`으로 아이템 에셋을 생성합니다.

- `Maximum Stacks`: 같은 아이템의 최대 중첩 수
- `Flat`: 고정 수치 보정입니다. 예: 공격력 `2`
- `Percent`: 비율 보정입니다. 예: 공격속도 15%는 `0.15`

최종 계산식은 `(기본값 + Flat 합계) × (1 + Percent 합계)`입니다.

## 4. 픽업 프리팹 연결

아이템 프리팹에 Trigger `Collider2D`와 `RunStatItemPickup`을 추가하고, `Item Definition`에 생성한 아이템 에셋을 할당합니다.

플레이어가 닿으면 아이템은 즉시 활성화됩니다. 중첩 한도에 도달한 아이템은 획득되지 않고 월드에 남습니다.

## 현재 범위

이번 구현은 현재 연결 가능한 공격력, 공격속도, 공격 사거리, 도토리 속도, 자동 전진 속도, 상하 이동 속도만 처리합니다. 체력, 방어, 치즈 흡수 범위, 관통, 폭발, 추적처럼 아직 소비 시스템이 없는 효과는 해당 시스템을 추가할 때 별도 구현합니다.

## PlayerScene 자동공격 연결

`PlayerScene`의 `Player`에는 `PlayerRunStats`와 함께 `PlayerSceneAutoAttack`을 연결합니다.

- `PlayerSceneAutoAttack`: 현재 스탯의 공격력, 초당 공격 횟수, 공격 사거리, 투사체 속도로 가장 가까운 테스트 고양이를 자동 공격합니다.
- `PlayerSceneAcornProjectile`: 도토리 풀의 투사체입니다.
- `PlayerSceneTestEnemy`: 자동공격 확인용으로 좌측으로 접근하고, 처치 또는 화면 이탈 시 최초 위치로 돌아오는 테스트 고양이입니다.

아이템을 획득해 `PlayerRunStats`가 갱신되면 다음 발사부터 변경된 공격 스탯이 적용됩니다.

## PlayerScene 체력 연결

- `Player`에 `PlayerRunHealth`를 추가했습니다. PlayerScene 설정값은 최대 체력 `100`, 초당 감소량 `8`입니다.
- `PlayerRunHud > HealthBarPanel`의 `PlayerRunHealthBarView`가 상단 체력바와 `HP 현재 / 최대` 텍스트를 갱신합니다.
- `PlayerSceneAutoAttack` 투사체는 적을 처치했을 때 `PlayerSceneTestEnemy`의 `Health Restore On Defeat`만큼 플레이어 체력을 회복합니다.
- 현재 테스트 고양이 회복값은 `TestCat_01 = 1`, `TestCat_02 = 2`, `TestCat_03 = 3`이며 Inspector에서 적마다 조정할 수 있습니다.
