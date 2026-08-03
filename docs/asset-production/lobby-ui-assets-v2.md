# 로비 UI 에셋 출처 및 적용 기록 v2

## 적용 대상

- 씬: `Assets/_Project/Scenes/Lobby/LobbyScene.unity`
- 목적: 배경에 합성된 UI를 제거하고 배경·버튼·패널·한글 폰트를 개별 관리 가능한 구조로 전환
- 생성 도구: Codex 내장 `imagegen`

## AI 생성 이미지

| 구분 | 프로젝트 파일 | 사용 위치 |
| --- | --- | --- |
| 배경 | `Art/UI/Lobby/Backgrounds/PantryLobby_Background_v2.png` | 로비 배경 |
| 설정 버튼 | `Art/UI/Lobby/Buttons/LobbyButton_Settings_v2.png` | 설정 메뉴 |
| 장비 버튼 | `Art/UI/Lobby/Buttons/LobbyButton_Equipment_v2.png` | 장비 착용 메뉴 |
| 강화 버튼 | `Art/UI/Lobby/Buttons/LobbyButton_Upgrade_v2.png` | 영구 강화 메뉴 |
| 시작 버튼 | `Art/UI/Lobby/Buttons/LobbyButton_RunStart_v2.png` | 런 시작 메뉴 |
| 모달 패널 | `Art/UI/Lobby/Panels/LobbyModalPanel_v1.png` | 강화·장비·설정 패널 |
| 목록 행 | `Art/UI/Lobby/Panels/LobbyListRow_v1.png` | 강화·장비 목록 행 |

- 이미지에는 버튼 문구, 외부 게임 로고, 타사 UI를 포함하지 않았습니다.
- 버튼·모달 패널·목록 행은 Unity 9-Slice로 사용하며, 문구·수치·상태는 Unity UI 텍스트로 별도 표시합니다.
- 버튼 v2는 단일 얇은 테두리와 평평한 중앙면을 사용하며, 리벳·돌출 장식·다중 프레임을 사용하지 않습니다.
- 버튼 v1은 검토용 이전 시안으로 사용을 중단했으며 프로젝트에서 제거했습니다.
- `PantryLobby_v1.png`은 이전 시안이며 현재 `LobbyScene`에서 참조하지 않습니다.

## 한글 폰트

- 파일: `Assets/_Project/Art/UI/Fonts/NotoSansKR-VF.ttf`
- 글꼴: Noto Sans KR
- 출처: [Noto Fonts GitHub](https://github.com/notofonts/noto-fonts)
- 라이선스: [SIL Open Font License 1.1](https://github.com/notofonts/noto-fonts/blob/main/LICENSE)
- 사용 방식: 프로젝트 런타임 UI 문구 렌더링. 배포 시 OFL 라이선스 고지와 저작권 표기를 유지합니다.
