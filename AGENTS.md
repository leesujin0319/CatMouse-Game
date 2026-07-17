# CatMouse Game Project 협업 규칙

## 응답과 협업 방식

- 모든 응답은 한국어 존댓말로, 정중하고 실무적으로 작성합니다.
- 설명은 명확하고 직접적으로 작성하며, 불필요한 감탄·과한 친근함·이모티콘은 사용하지 않습니다.
- 설명은 목록과 짧은 문단으로 구조화합니다.
- 이 저장소는 다인이 함께 작업합니다. 현재 작업과 무관한 변경은 수정·되돌리기·스테이징하지 않습니다.
- 변경은 기능 또는 목적 단위로 좁게 유지하고, 패키지·설정·문서 변경을 코드 변경과 구분해 검토합니다.

## 저장소 구조

```text
docs/                   공유 기획·기술·제출 문서
docs-local/             개인 임시 문서, Git 제외
CatMouse Game Project/  Unity 프로젝트 루트
.codex/                 Codex 협업 설정과 프로젝트 스킬
```

- Unity는 반드시 `CatMouse Game Project/` 폴더를 프로젝트 루트로 열어야 합니다.
- 경로에 공백이 있으므로 셸 명령에서 Unity 프로젝트 경로는 항상 인용합니다.
- `docs/`는 팀과 공유·커밋하고, 초안·개인 메모·조사 원문은 `docs-local/`에만 둡니다.

## Unity 작업 범위

기본 탐색·수정 대상은 아래 경로로 한정합니다.

- `CatMouse Game Project/Assets/`: 게임 코드, 씬, 프리팹, 자체 제작 에셋
- `CatMouse Game Project/Packages/`: Unity 패키지 선언과 잠금 파일
- `CatMouse Game Project/ProjectSettings/`: 의도적으로 변경한 프로젝트 설정

다음은 생성물 또는 개인 환경이므로, 오류 진단을 위해 꼭 필요할 때만 읽고 기본적으로 탐색·수정·커밋하지 않습니다.

- `CatMouse Game Project/Library/`
- `CatMouse Game Project/Logs/`
- `CatMouse Game Project/Temp/`, `Obj/`, `Build/`, `Builds/`
- `CatMouse Game Project/UserSettings/`, `MemoryCaptures/`, `Recordings/`
- `.vs/`, `.idea/`, `docs-local/`, `graphify-out/`

## Unity 파일 안전 규칙

- Unity 에디터가 열린 상태에서는 프로젝트 루트, `Assets/`, `Packages/`, `ProjectSettings/`를 이동·이름 변경하지 않습니다.
- 에셋을 이동·이름 변경할 때는 대응하는 `.meta` 파일을 항상 함께 유지합니다.
- `Library/PackageCache`는 수정 대상이 아닙니다. 패키지 변경은 `Packages/manifest.json`에서 수행합니다.
- 패키지를 추가·삭제·업데이트하면 `Packages/manifest.json`과 `Packages/packages-lock.json`을 함께 검토·커밋합니다.
- `ProjectSettings/` 변경은 빌드·입력·렌더링에 영향을 줄 수 있습니다. Unity가 자동 변경한 파일은 별도로 확인하고, 의도하지 않은 변경은 다른 작업과 섞지 않습니다.
- 실제 플레이 검증은 `CatMouse Game Project/`를 Unity Hub 또는 Unity Editor로 연 뒤 수행합니다.
- Unity C# 구현은 `docs/technical-foundation/code-conventions-v0.1.md`와 `docs/technical-foundation/mvp-architecture-v0.1.md`를 함께 따릅니다.

## Git과 다인 협업 규칙

- `Assets/`, `Packages/`, `ProjectSettings/`, 관련 `.meta` 파일은 Git에 포함합니다.
- Unity 캐시·로그·빌드 산출물·개인 설정은 `.gitignore` 규칙을 유지하고 추적하지 않습니다.
- 다른 사람의 미완료 변경이 있는 작업 트리에서 전체 스테이징이나 일괄 포맷을 하지 않습니다.
- 파일 이동은 Unity 종료 후 수행하고, Git에서 이동으로 인식되는지 검토한 뒤 커밋합니다.
- 외부 에셋·오픈소스·AI 생성물은 출처와 라이선스를 `docs/`의 제출 문서에 기록합니다.
- 커밋 메시지·분리 기준·사전 검증은 `docs/conventions/commit-convention.md`를 따릅니다.

## Graphify

- 지식 그래프 산출물은 `graphify-out/`에 생성되며 Git에 포함하지 않습니다.
- 사용자가 `/graphify`를 입력하면 설치된 Graphify 스킬 또는 해당 지침을 먼저 따릅니다.
- `graphify-out/graph.json`이 있으면 코드베이스 질문에 먼저 `graphify query`·`path`·`explain`을 사용합니다.
- 코드 또는 코드 경로를 변경한 뒤에는 `graphify update .`를 실행해 AST 그래프를 갱신합니다.
- 문서·이미지·PDF 변경은 의미 추출이 필요할 때만 `/graphify --update`로 별도 갱신합니다.
