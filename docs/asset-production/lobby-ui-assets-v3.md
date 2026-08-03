# 로비 UI 에셋 v3 기록

## 목적

식료품 저장실 메타 로비의 세로 메뉴와 게임 시작 버튼을 동일한 UI 키트로 통일합니다. 이 에셋은 인게임 HUD가 아니라 강화·장비·설정·게임 시작을 위한 로비 UI에만 사용합니다.

## 생성 에셋

| 파일 | 용도 | Unity 적용 |
| --- | --- | --- |
| `Assets/_Project/Art/UI/Lobby/Buttons/LobbyNavButton_Base_v3.png` | 우측 세로 메뉴 원본 | 생성 원본, 투명 여백 포함 |
| `Assets/_Project/Art/UI/Lobby/Buttons/LobbyStartButton_Base_v3.png` | 게임 시작 버튼 원본 | 생성 원본, 투명 여백 포함 |
| `Assets/_Project/Art/UI/Lobby/Buttons/LobbyNavButton_Cropped_v1.png` | 우측 세로 메뉴 배경 | 단일 Sprite, 나인슬라이스 |
| `Assets/_Project/Art/UI/Lobby/Buttons/LobbyStartButton_Cropped_v1.png` | 게임 시작 버튼 배경 | 단일 Sprite, 나인슬라이스 |
| `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Upgrade_v1.png` | 영구 강화 도토리 아이콘 | 단일 Sprite, 64×64 UI |
| `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Equipment_v1.png` | 장비 가방·새총 아이콘 | 단일 Sprite, 64×64 UI |
| `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Settings_v1.png` | 설정 톱니 아이콘 | 단일 Sprite, 64×64 UI |
| `Assets/_Project/Art/UI/Lobby/Icons/LobbyIcon_Start_v1.png` | 게임 시작 목재 재생 아이콘 | 단일 Sprite, 64×64 UI |

## 생성 방식

- 도구: Codex 내장 `imagegen`
- 기준 시안: 제목이 없는 메타 로비 컨셉의 양피지 버튼, 허니 골드 시작 버튼, 원형 아이콘 배지
- 참조 에셋: `PlayerMouse_v1.png`, `PantryLobby_Background_v2.png`
- 공통 지시: 정면형 2D UI, 다크 카라멜 외곽선, 얇은 목재 테두리, 늘릴 수 있는 평면 중심부, 텍스트·로고·아이콘 미포함

## 투명 처리와 임포트

- 생성 원본의 크로마 키 배경은 프로젝트의 imagegen 제공 도구로 투명 PNG로 변환했습니다.
- 메뉴·시작 버튼은 원본 이미지의 실제 알파 경계로 잘라낸 `*_Cropped_v1.png`만 Sprite로 등록해 투명 여백이 레이아웃 크기에 영향을 주지 않게 했습니다.
- 버튼 Sprite는 Bilinear, Mipmap 비활성화, Clamp, Pixels Per Unit 200을 사용합니다.
- 메뉴·시작 버튼의 나인슬라이스 경계는 좌우 `96 px`, 상하 `56 px`입니다. 둥근 모서리를 고정하고 가운데 평면만 늘립니다.
- 잘라낸 Sprite의 피벗은 중앙 `(0.5, 0.5)`으로 고정합니다.

## 아이콘 제작·적용 규칙

- UI 아이콘은 Unicode 글리프나 폰트 기호를 사용하지 않고, 기능별 개별 이미지 리소스로 제작합니다.
- 원본은 큰 캔버스에서 제작한 뒤 단색 배경을 알파로 제거하고, 실제 알파 경계로 크롭합니다.
- 최종 파일은 `256×256 px` 투명 PNG입니다. 사용 중 잘리지 않도록 `24 px`의 균일한 안전 여백만 남기고, UI에서는 `64×64 px`로 표시합니다.
- 모든 아이콘은 중앙 피벗 `(0.5, 0.5)`, 단일 Sprite, Bilinear, Mipmap 비활성화, Clamp로 임포트합니다.

## 출처와 라이선스

- 본 문서의 세 PNG는 현재 프로젝트를 위해 AI로 생성한 오리지널 에셋입니다.
- 타 게임의 이미지, 로고, 캐릭터, UI 캡처를 직접 사용하지 않았습니다.
- 공모전 제출용 AI 활용 기술 문서에는 Codex와 imagegen 사용, 프롬프트 목적, 에셋 경로를 함께 기재합니다.
