# AI 생성 로비 에셋 기록

## 2026-07-31 식료품 저장실 로비 배경

- 생성 도구: Codex 내장 `imagegen`
- 최종 파일: `Assets/_Project/Art/Backgrounds/Lobby/PantryLobby_v1.png`
- 사용 씬: `LobbyScene`
- 사용 범위: 식료품 저장실 속 쥐의 은신처 로비 배경과 우측 세로 메뉴의 시각적 받침 영역입니다.

### 생성 순서

1. 따뜻한 동화풍 식료품 저장실의 재질·팔레트·메뉴 표지판 방향을 담은 컨셉시트를 생성했습니다.
2. 컨셉시트를 기준으로 16:9 로비 배경을 생성했습니다. 중앙 좌측에는 플레이어 대기 영역을, 우측에는 세로 메뉴 영역을 남겼습니다.

### 프롬프트·출처

- 주요 프롬프트 방향: 꿀빛 목재, 크림색 회벽, 청록 수납장, 치즈와 유리병, 우측으로 이어지는 저장실 통로, 텍스트·로고·외부 게임 캐릭터 없는 2D 게임 배경입니다.
- 외부 이미지 입력: 없습니다. 최종 배경은 이번 작업에서 생성한 컨셉시트만 참조했습니다.
- 라이선스 및 출처: AI 생성물이며, 외부 게임의 이미지·아이콘·UI를 포함하거나 재사용하지 않습니다.

### Unity 적용

- `Sprite (2D and UI)`, `Single`, Bilinear, Mipmap 비활성화로 가져옵니다.
- 이 파일은 정적인 로비 배경이며, 무한 횡스크롤 맵에 수평 심리스 타일로 사용하지 않습니다.

## 2026-07-31 로비 기능 아이콘

- 생성 도구: Codex 내장 `imagegen`
- 최종 파일:
  - `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Upgrade_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Equipment_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Settings_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Start_v1.png`
- 사용 씬: `LobbyScene`
- 사용 범위: 영구 강화, 장비 착용, 설정, 게임 시작 버튼의 기능 표식입니다.

### 생성·정리 순서

1. 식료품 저장실의 목재·황동·청록 색상과 다크 카라멜 외곽선을 기준으로 아이콘 컨셉시트를 생성했습니다.
2. 도토리 강화, 가방·새총 장비, 황동 톱니 설정, 목재 재생 표식의 고해상도 원본을 각각 생성했습니다.
3. 원본의 마젠타 단색 배경을 알파로 제거하고 실제 알파 경계로 크롭했습니다.
4. 각 결과를 `256×256 px` 투명 PNG로 축소하고 중앙 피벗과 `24 px` 균일 안전 여백을 적용했습니다.

### 프롬프트·출처

- 주요 프롬프트 방향: 64px에서도 읽히는 굵은 실루엣, 정면형 손그림 2D 게임 UI, 텍스트·로고·워터마크·그림자 없는 단일 아이콘입니다.
- 외부 이미지 입력: 없습니다. 현재 프로젝트의 식료품 저장실 색감과 자체 생성 컨셉시트만 기준으로 사용했습니다.
- 라이선스 및 출처: AI 생성물이며, 외부 게임의 아이콘·UI·캐릭터를 포함하거나 재사용하지 않습니다.

### Unity 적용

- `Sprite (2D and UI)`, `Single`, PPU 200, Bilinear, Mipmap 비활성화, Wrap Mode `Clamp`로 가져옵니다.
- UI에서는 `64×64 px` `Image`로 표시하며, 폰트 기호나 Unicode 글리프를 대체하지 않습니다.

## 2026-08-02 메타 성장·장비 아이콘

- 생성 도구: Codex 내장 `imagegen`
- 최종 파일:
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_AttackDamage_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_AttackSpeed_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_ForwardSpeed_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_HardenedAcorn_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_WindupSlingshot_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_LongTailScope_v1.png`
- 사용 씬: `LobbyScene`
- 사용 범위: 영구 강화 3종과 장착 장비 3종의 목록 아이콘입니다. 각 아이콘은 대응하는 `RunItemDefinition`의 `Icon` 속성으로 연결합니다.

### 생성·정리 순서

1. 식료품 저장실 로비의 꿀빛 목재·청록·카라멜 외곽선을 기준으로, 강화와 장비를 구분할 수 있는 3×2 아이콘 컨셉시트를 생성했습니다.
2. 텍스트·로고·외부 게임 이미지가 없는 마젠타 단색 배경 아틀라스로 다시 생성했습니다.
3. 단색 배경을 알파로 제거한 뒤, 3×2 셀에서 아이콘을 분리하고 실제 알파 경계를 기준으로 정리했습니다.
4. 각 아이콘은 최대 `208×208 px`의 그림 영역과 사방 `24 px` 안전 여백을 갖는 `256×256 px` 투명 PNG로 맞췄습니다. Sprite와 UI `Image`의 피벗은 중앙 `(0.5, 0.5)`입니다.

### 프롬프트·출처

- 주요 프롬프트 방향: 굵고 읽기 쉬운 손그림 2D 캐주얼 게임 아이콘, 강화 도토리·앞발·운동화·방어 도토리·태엽 새총·청록 조준경, 다크 카라멜 외곽선, 텍스트·숫자·로고·워터마크·외부 캐릭터 없는 단일 오브젝트입니다.
- 외부 이미지 입력: 현재 프로젝트에서 생성한 로비 아이콘 컨셉시트만 스타일 기준으로 사용했습니다.
- 라이선스 및 출처: AI 생성물이며, 외부 게임의 아이콘·UI·캐릭터를 포함하거나 재사용하지 않습니다.

### Unity 적용

- `Sprite (2D and UI)`, `Single`, PPU 200, Bilinear, Mipmap 비활성화, Wrap Mode `Clamp`로 가져옵니다.
- `LobbySceneInstaller`가 아이콘을 `RunItemDefinition`과 로비 목록 `Image`에 연결합니다. 런타임 `LobbySceneController`는 정의 데이터의 아이콘·이름·설명을 화면에 다시 반영합니다.

## 2026-08-03 로비 드로어·메타 슬롯 UI v4

- 생성 도구: Codex 내장 `imagegen`
- 최종 파일:
  - `Assets/_Project/Art/UI/Lobby/Panels/LobbyPanel_SlideDrawer_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Panels/LobbyPanel_MetaSlot_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Panels/LobbyPanel_MetaSlotSelected_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Panels/LobbyPanel_CurrencyChip_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Buttons/LobbyButton_Primary_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Buttons/LobbyButton_Secondary_v1.png`
- 사용 씬: `LobbyScene`
- 사용 범위: 우측 메타 드로어, 강화·장비 슬롯, 선택 장비 표시, 주·보조 행동 버튼, 치즈 표시입니다.

### 생성·정리 순서

1. 식료품 저장실의 크림·꿀빛 목재·청록·주황 강조색을 기준으로, 로비 전체와 드로어·슬롯·버튼을 함께 검토하는 컨셉시트를 생성했습니다.
2. 컨셉 방향을 기준으로 드로어, 중립 슬롯, 선택 슬롯, 주 버튼, 보조 버튼, 치즈 칩을 한 장의 마젠타 단색 배경 소스 시트로 생성했습니다.
3. 마젠타 배경을 알파로 제거하고 6개 컴포넌트를 각각 별도 PNG로 분리했습니다.
4. 각 PNG는 실제 알파 경계 기준으로 여백을 제거하고 중앙 피벗으로 가져왔습니다. 문구·숫자·아이콘은 이미지에 넣지 않았습니다.

### 프롬프트·출처

- 주요 프롬프트 방향: 정면형 2D 캐주얼 모바일 UI, 크림색 매트 표면, 단일 카라멜 외곽선, 청록 선택 상태, 주황 주 행동, 넓고 비어 있는 중심부, 과한 목재 명패·리벳·다중 테두리·강한 광택 없음입니다.
- 외부 이미지 입력: 없습니다. 현재 프로젝트의 식료품 저장실 컨셉과 자체 생성 컨셉시트만 기준으로 사용했습니다.
- 라이선스 및 출처: AI 생성물이며, 외부 게임의 UI·이미지·문구를 재사용하지 않습니다.

### Unity 적용

- `Sprite (2D and UI)`, `Single`, PPU 200, Bilinear, Mipmap 비활성화, Wrap Mode `Clamp`로 가져옵니다.
- 9-Slice 보호 영역은 드로어 사방 32px, 슬롯 사방 28px, 버튼 사방 32px, 치즈 칩 사방 20px입니다.
- 선택 장비 슬롯은 `LobbyPanel_MetaSlotSelected_v1`만 사용하고, 이름·설명·아이콘·버튼 라벨은 `TextMeshProUGUI`와 별도 `Image`로 관리합니다.

## 2026-08-03 슬롯 장비 아이콘 v1

- 생성 도구: Codex 내장 `imagegen`
- 최종 파일:
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_PantryCap_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_AcornArmor_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_WindupShoes_v1.png`
- 사용 씬: `LobbyScene`
- 사용 범위: 장비 탭의 `모자`, `갑옷`, `신발` 슬롯과 해당 `RunItemDefinition` 아이콘입니다.

### 생성·정리 순서

1. 현재 로비 화면을 스타일 참조로 사용해 모자·갑옷·신발의 단일 아이콘 컨셉시트를 생성했습니다.
2. 마젠타 단색 배경을 알파로 제거했습니다.
3. 세 장비를 개별 `512×512 px` 투명 PNG로 분리하고, 실제 알파 경계를 기준으로 중앙 배치했습니다.
4. Unity에서 `Sprite (2D and UI)`, `Single`, 중앙 피벗, Bilinear, Mipmap 비활성화로 가져왔습니다.

### 프롬프트·출처

- 주요 프롬프트 방향: 치즈 핀이 달린 베이지 탐험 모자, 도토리 껍질과 가죽으로 만든 갑옷, 황동 태엽 키가 달린 갈색 신발입니다. 무기·조준경·새총·캐릭터·문자·로고는 포함하지 않습니다.
- 외부 이미지 입력: 현재 프로젝트에서 생성한 로비 화면만 스타일 기준으로 사용했습니다.
- 라이선스 및 출처: AI 생성물이며, 외부 게임의 이미지·아이콘·UI를 포함하거나 재사용하지 않습니다.

## 2026-08-03 추가 슬롯 장비 아이콘 v1

- 생성 도구: Codex 내장 `imagegen`
- 최종 파일:
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_ScoutCap_v1.png`
  - `Assets/_Project/Art/UI/Lobby/Icons/Progression/LobbyMetaIcon_SpringBoots_v1.png`
- 사용 씬: `LobbyScene`
- 사용 범위: 모자 슬롯의 `나침반 모자`, 신발 슬롯의 `통통 태엽 장화` 아이콘입니다.

### 생성·정리 순서

1. 기존 식료품 저장실 로비와 슬롯 장비 아이콘 v1을 승인된 스타일 기준으로 사용했습니다.
2. 마젠타 단색 배경에서 나침반·치즈 핀이 달린 모자와 황동 스프링 장화를 각각 생성했습니다.
3. 크로마 키를 알파로 제거하고 실제 알파 경계를 기준으로 자른 뒤, 사방 최소 24px 안전 여백을 둔 `512×512 px` 투명 PNG로 정리했습니다.

### 프롬프트·출처

- 주요 프롬프트 방향: 꿀빛 가죽, 청록 포인트, 황동 부품, 다크 카라멜 외곽선을 갖는 동화풍 2D 모바일 게임 장비 아이콘입니다. 문자·로고·워터마크·캐릭터·무기·바닥 그림자는 포함하지 않았습니다.
- 외부 이미지 입력: 없습니다. 기존 프로젝트의 식료품 저장실 로비와 자체 생성 장비 아이콘을 스타일 기준으로 삼았습니다.
- 라이선스 및 출처: AI 생성물이며, 외부 게임의 이미지·아이콘·UI를 포함하거나 재사용하지 않습니다.

### Unity 적용

- `Sprite (2D and UI)`, `Single`, PPU 100, Bilinear, Mipmap 비활성화, Wrap Mode `Clamp`, 중앙 피벗으로 가져옵니다.
