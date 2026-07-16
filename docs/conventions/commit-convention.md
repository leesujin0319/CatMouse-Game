# Git 커밋 컨벤션

## 1. 목적

이 규칙은 다인 Unity 프로젝트에서 변경 의도와 영향 범위를 빠르게 파악하고, 되돌리기·체리픽·릴리스 검토를 안전하게 하기 위한 기준입니다. 모든 커밋은 하나의 검토 가능한 목적만 담습니다.

## 2. 메시지 형식

```text
<type>(<scope>): <한국어 제목>

<선택: 한국어 본문>

<선택: Summary: English summary>
```

- `scope`는 선택이며 영문 소문자 또는 kebab-case를 사용합니다.
- 제목은 **한국어를 주 언어**로 쓰고, 마침표 없이 50자 안팎으로 작성합니다.
- Unity·Cinemachine·WebGL·API처럼 고유명사와 코드 식별자는 영문을 유지할 수 있습니다.
- 본문은 변경 이유와 호환성·검증 결과를 한국어 bullet로 남깁니다.
- 영문 `Summary:`는 외부 협업 또는 릴리스 노트가 필요할 때만 한 줄로 추가합니다.

## 3. Type

| Type | 사용 시점 | 예시 |
| --- | --- | --- |
| `feat` | 플레이 기능 추가 | `feat(player): 자유 상하 이동 추가` |
| `fix` | 오류·회귀 수정 | `fix(save): 중복 보상 지급 방지` |
| `docs` | 기획·기술·협업 문서 | `docs: 기술 파운데이션과 규칙 정리` |
| `refactor` | 동작을 바꾸지 않는 구조 개선 | `refactor(run): 런 세션 책임 분리` |
| `test` | 테스트 추가·수정 | `test(skill): 능력 중첩 검증 추가` |
| `build` | 패키지·빌드 설정 | `build(camera): Cinemachine 패키지 추가` |
| `asset` | 자체 제작 에셋·임포트 설정 | `asset(pantry): 식료품 저장실 타일 추가` |
| `chore` | 프로젝트 구조·도구·정리 | `chore(project): Unity 프로젝트 구조 정리` |
| `ci` | 자동화·배포 파이프라인 | `ci(webgl): Pages 빌드 검사 추가` |

## 4. Scope 권장어

`project`, `run`, `player`, `enemy`, `skill`, `meta`, `save`, `ui`, `camera`, `audio`, `pantry`, `webgl`, `docs`를 우선 사용합니다. 새 Scope가 필요하면 기능의 책임 이름을 따라 짧고 일관되게 정합니다.

## 5. 커밋 분리 기준

- 게임 기능 코드·프리팹·정의 데이터·해당 테스트는 같은 커밋에 둡니다.
- `Assets` 이동은 대응 `.meta` 파일을 반드시 같은 커밋에 포함합니다.
- 패키지 변경은 `Packages/manifest.json`과 `Packages/packages-lock.json`을 함께 커밋합니다.
- `ProjectSettings` 변경은 코드·에셋 대량 변경과 분리합니다.
- `Library`, `Logs`, `Temp`, `UserSettings`, 개인 메모는 커밋하지 않습니다.
- 다른 사람이 작업 중인 씬·프리팹의 자동 저장 변경은 별도 커밋으로 만들지 말고 원인을 먼저 확인합니다.

## 6. 예시

```text
chore(project): Unity 프로젝트와 협업 기반 정리

- Unity 프로젝트 루트를 CatMouse Game Project로 분리
- 공용 기획·기술 문서와 Graphify 협업 설정 추가
- Cinemachine 패키지 선언 및 잠금 정보 반영

Summary: Establish the Unity project and collaboration baseline.
```

```text
feat(player): 자유 상하 이동과 화면 경계 적용

- Input System 입력을 연속 세로 속도로 변환
- PlayerVerticalMovement에서 플레이 영역 경계 제한
- PlayMode 이동 경계 테스트 추가
```

## 7. 커밋 전 확인

1. `git status --short`로 다른 작업자의 변경이 섞이지 않았는지 확인합니다.
2. `git diff --cached --check`로 공백·충돌 표식 오류를 확인합니다.
3. Unity 변경은 필요한 EditMode 또는 PlayMode 테스트와 Console 오류를 확인합니다.
4. WebGL에 영향이 있으면 최소 한 번의 WebGL 빌드 또는 관련 검증을 기록합니다.
5. 푸시 전 커밋 제목과 본문이 이 문서의 형식을 따르는지 확인합니다.
