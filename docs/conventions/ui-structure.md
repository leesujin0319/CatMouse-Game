# UI 에셋·프리팹 구조 규칙

## 목적

로비 UI는 배경에 버튼이나 문구를 합성하지 않습니다. 배경, 버튼, 패널, 텍스트를 분리하여 한 요소를 수정해도 다른 UI에 영향을 주지 않도록 관리합니다.

## 경로

```text
CatMouse Game Project/Assets/_Project/
├─ Art/UI/
│  ├─ Fonts/                         # 프로젝트 공용 원본 폰트와 TMP SDF Font Asset
│  └─ Lobby/
│     ├─ Backgrounds/                # 문구·버튼이 없는 배경 전용 이미지
│     ├─ Buttons/                    # 버튼 1종당 PNG 1개
│     ├─ Icons/                      # 기능 아이콘 1종당 투명 PNG 1개
│     └─ Panels/                     # 드로어·슬롯·칩 등 패널 1종당 PNG 1개
└─ Prefabs/UI/Lobby/
   ├─ Buttons/                       # 실제 클릭·문구를 포함하는 버튼 프리팹
   └─ Panels/                        # 모달 단위 프리팹
```

- `LobbyScene`은 UI 배치와 화면 전환만 담당합니다.
- 버튼·패널의 시각 요소와 동작은 각각의 프리팹에서 관리합니다.
- 새 UI는 공통 프리팹을 임의 복제하지 않고, 변경 대상 버튼 또는 패널 프리팹만 수정합니다.

## 이미지 규칙

- 파일명은 `LobbyButton_<기능>_v<번호>.png`, `LobbyIcon_<기능>_v<번호>.png`, `LobbyPanel_<기능>_v<번호>.png` 형식을 사용합니다.
- 버튼 이미지는 텍스트를 포함하지 않습니다. 문구는 `TextMeshProUGUI` 컴포넌트로 별도 관리합니다.
- 기능 아이콘은 Unicode 글리프·폰트 기호를 사용하지 않고, 기능별 이미지 Sprite로 제작합니다.
- 기본 버튼과 슬롯 테두리는 단일 얇은 선만 사용합니다. 리벳, 돌출 장식, 다중 금속 프레임, 강한 광택은 사용하지 않습니다.
- 이미지 원본은 최종 표시 크기보다 크게 제작하고, 단색 배경을 알파로 제거한 뒤 실제 알파 경계로 크롭합니다.
- PNG 캔버스는 시각 요소 외부의 불필요한 투명 여백 없이 타이트하게 자릅니다. 그림자처럼 의도된 외곽 여백만 남길 수 있으며, 해당 경우 문서에 이유를 기록합니다.
- 현재 로비 아이콘은 고해상도 원본을 정리한 `256×256 px` 투명 PNG를 사용합니다. `64×64 px` UI 표시에서 실루엣이 잘리지 않도록 사방 `24 px`의 균일한 안전 여백만 남기고, Sprite와 RectTransform 피벗은 모두 중앙 `(0.5, 0.5)`으로 맞춥니다.
- Unity Import는 `Sprite (2D and UI)`, `Single`, Bilinear, Mipmap 비활성화, Wrap Mode `Clamp`를 기본으로 합니다.
- 크기 변경이 필요한 버튼·패널은 9-Slice를 사용합니다. 현재 v4 드로어는 사방 32px, 슬롯과 선택 슬롯은 사방 28px, 주·보조 버튼은 사방 32px, 치즈 칩은 사방 20px을 보호 영역으로 사용합니다. 모든 로비 UI 원본은 200 PPU 기준으로 적용합니다.
- 배경은 배경 경로에만 두며 버튼·패널 프레임 또는 UI 문구를 베이크하지 않습니다.

## 한글 텍스트 규칙

- 런타임 UI 한글은 `Art/UI/Fonts/TMP/`의 동적 SDF Font Asset을 사용합니다. 원본은 `Art/UI/Fonts/NEXONLv1GothicRegular.ttf`입니다.
- 메뉴·게임 시작 같은 주요 행동 문구에는 `NEXONKartGothicExtraBold.ttf`를, 모달 제목·일반 버튼에는 `NEXONLv1GothicBold.ttf`를, 설명·보조 정보에는 Regular를 사용합니다. Unity의 합성 Bold는 사용하지 않습니다.
- `LobbySceneController`는 실행 시 Bold·Display TMP 에셋을 `NEXONLv1GothicBold.ttf` 기반의 동적 TMP 폰트로 교체합니다. 이 처리는 TMP Atlas Texture 참조가 비어 있거나 한글 글리프가 누락된 개인 Editor 환경에서도 로비 문구를 동일하게 표시하기 위한 것입니다.
- 밝은 배경의 짙은 글자는 밝은 외곽선, 어두운 배경의 밝은 글자는 짙은 외곽선을 적용하여 배경 변화에서도 판독성을 유지합니다.
- 기준 해상도는 1920×1080이며, 캔버스 스케일러는 `Scale With Screen Size`를 사용합니다.

## 검수 기준

1. 1920×1080 Game View에서 문구가 잘리지 않고 읽혀야 합니다.
2. 버튼·패널 이미지에 검은 사각형, 흰 테두리, 불필요한 투명 여백이 없어야 합니다.
3. 버튼 클릭 영역은 보이는 이미지와 일치해야 합니다.
4. 변경 후 `LobbyScene`에서 기본 화면, 강화 드로어, 장비 드로어의 선택 상태, 설정 드로어를 모두 확인합니다.
