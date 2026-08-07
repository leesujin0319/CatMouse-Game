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
