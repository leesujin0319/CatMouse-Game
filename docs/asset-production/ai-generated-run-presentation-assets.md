# AI 생성 런 프레젠테이션 에셋 기록

## GroundShadow

- 생성일: 2026-08-06
- 생성 도구: Codex 내장 `imagegen`
- 최종 파일: `Assets/_Project/Art/Presentation/Effects/GroundShadow.png`
- 사용처: `GameScene`의 플레이어와 적 발밑 그림자

### 생성 및 후처리

- 생성 의도: 캐릭터의 하단 피벗과 바닥 접지감을 보여 주는 공용 타원 그림자입니다.
- 프롬프트 요약: 단일 검은 타원형 접지 그림자를 균일한 `#00ff00` 크로마 키 배경 위에 배치하고, 텍스트·소품·바닥 표현을 제외하도록 지시했습니다.
- 후처리: `remove_chroma_key.py`로 배경을 알파 채널로 변환한 뒤, 녹색 잔여색을 제거하고 투명 여백을 포함한 384×96 PNG로 정리했습니다.

### Unity 적용

- Import: `Sprite (2D and UI)`, `Single`, PPU 192, Bilinear, Alpha Is Transparency
- 배치: 플레이어와 적 루트의 `GroundShadow` 자식 오브젝트
- 정렬: `Characters` 소팅 레이어에서 캐릭터보다 한 단계 낮은 Y축 소팅 오더
- 표현: 원본 이미지의 알파와 SpriteRenderer 알파 0.24를 함께 사용합니다.

### 출처 및 권리 확인

- AI 생성물이며 외부 게임의 이미지, 캐릭터, 로고, UI 또는 에셋을 입력으로 사용하거나 재사용하지 않았습니다.
- 런타임에는 생성 도구나 외부 서비스에 연결하지 않으며, 최종 PNG만 게임 빌드에 포함합니다.

## PantryCellar 배경 타일과 AcornProjectile

- 생성일: 2026-08-08
- 생성 도구: Codex 내장 `imagegen`
- 최종 파일:
  - `Assets/_Project/Art/Backgrounds/Gameplay/PantryCellar_BackWall_Tile_v1.png`
  - `Assets/_Project/Art/Backgrounds/Gameplay/PantryCellar_CombatFloor_Tile_v1.png`
  - `Assets/_Project/Art/Backgrounds/Gameplay/PantryCellar_ForegroundWall_Tile_v1.png`
  - `Assets/_Project/Art/Sprites/Projectiles/AcornProjectile_v1.png`
- 사용처: `Dev_PantryBackground`와 `GameScene`의 원경 벽·전투 바닥·하단 근경, 플레이어의 도토리 기본 공격

### 생성 및 후처리

- 생성 의도: 수채화 외곽선의 플레이어·고양이 원화와 어울리는 식료품 저장실 전투 공간을 구성합니다. 중앙 전투 영역은 비우고, 벽·바닥·하단 근경을 분리해 수평 반복 배치합니다.
- 프롬프트 요약: 따뜻한 나무 보·석고 벽·보존식품 선반, 짙은 청록 석재 바닥, 갈색 석재 근경을 각각 가로형 타일로 생성했습니다. 도토리는 단일 발사체가 식별되도록 별도 생성했습니다.
- 후처리: 벽과 바닥은 1536×512로 정규화했습니다. 근경과 도토리는 `#00ff00` 크로마 키를 알파 채널로 변환하고, 투명 여백을 크롭했습니다.
- 반복 검증: 타일을 가로로 세 장 연결한 미리보기로 접합부의 색상·구조 단절 여부를 확인했습니다.

### Unity 적용

- 배경 Import: `Sprite (2D and UI)`, `Single`, PPU 96, Bilinear, Wrap Mode Repeat
- 도토리 Import: `Sprite (2D and UI)`, `Single`, PPU 768, Bilinear, Alpha Is Transparency
- 배치: `DevPantryVisualSliceInstaller`가 기존 `EndlessPantryBackground`의 세 레이어에 연결합니다.
- 정렬: 원경·바닥·캐릭터·근경의 기존 소팅 레이어 규칙을 유지하고, 그리드 오버레이는 개발용으로만 비활성화합니다.

### 출처 및 권리 확인

- AI 생성물이며 외부 게임의 이미지, 캐릭터, 로고, UI 또는 에셋을 입력으로 사용하거나 재사용하지 않았습니다.
- 런타임에는 생성 도구나 외부 서비스에 연결하지 않으며, 최종 PNG와 프로젝트 내 설치 코드만 게임 빌드에 포함합니다.

## 보상 픽업과 전투 피드백

- 생성일: 2026-08-08
- 생성 도구: Codex 내장 `imagegen`
- 컨셉 시트: 치즈·도토리 문양 코인·획득 반짝임·도토리 명중 효과를 한 장의 수채화 스타일 시트로 생성한 뒤, 같은 색·외곽선·광원 기준으로 개별 원화를 제작했습니다.
- 최종 파일:
  - `Assets/_Project/Art/Sprites/Pickups/CheesePickup_v1.png`
  - `Assets/_Project/Art/Sprites/Pickups/AcornCoinPickup_v1.png`
  - `Assets/_Project/Art/Presentation/Effects/PickupSparkle_v1.png`
  - `Assets/_Project/Art/Presentation/Effects/AcornImpact_v1.png`

### 생성 및 후처리

- 치즈는 경험치·체력 회복 보상으로, 코인은 영구 성장 재화로 즉시 구분되도록 서로 다른 실루엣과 문양을 사용했습니다.
- 생성 원화는 균일한 `#00ff00` 크로마 키 배경 위에서 크게 제작했습니다.
- `remove_chroma_key.py`로 알파 채널을 생성하고, 투명 여백을 크롭한 뒤 치즈·코인·획득 효과는 최대 256px, 명중 효과는 최대 320px로 축소했습니다.
- 최종 PNG의 네 모서리 알파가 0인지 확인해, Unity에서 사각형 배경이 남지 않도록 검증했습니다.

### Unity 적용

- Import: `Sprite (2D and UI)`, `Single`, Bilinear, Alpha Is Transparency, Wrap Mode Clamp
- PPU: 치즈·코인·획득 반짝임·명중 효과 모두 512
- 치즈·코인: 기존 `PrototypeCheeseDropPool`, `RunCoinCollector`의 풀·콜라이더·보상 수치는 유지하고 스프라이트만 교체합니다.
- 효과: `RunVisualEffectPool`이 획득 반짝임과 명중 효과를 재사용합니다. 치즈·코인 수집 및 도토리 명중 시에만 짧게 재생되며, 풀 용량을 초과하면 새 오브젝트를 만들지 않고 해당 효과를 생략합니다.
- 설치: `DevPantryVisualSliceInstaller`가 `Dev_PantryBackground`와 `GameScene`에 스프라이트와 효과 풀 참조를 연결합니다.

### 출처 및 권리 확인

- AI 생성물이며 외부 게임의 이미지, 캐릭터, 로고, UI 또는 에셋을 입력으로 사용하거나 재사용하지 않았습니다.
- 런타임에는 생성 도구나 외부 서비스에 연결하지 않으며, 최종 PNG와 프로젝트 내 표현 코드만 게임 빌드에 포함합니다.
