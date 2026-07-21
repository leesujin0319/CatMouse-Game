# 문서 작성 규칙

## `docs/`

팀이 함께 검토하고 Git에 커밋하는 공통 문서만 둡니다.

- 게임 기획서
- 기술 설계서
- 기술 파운데이션 원문과 프로젝트 적용 기준
- AI 활용 기술 문서
- 제출용 문서의 Markdown 원본
- 팀 역할·협업 규칙

개인 임시 메모, AI 초안, 조사 원문, 실험 기록은 저장소 밖에서 관리합니다. 공통 검토가 필요한 내용만 정리해 `docs/`에 반영합니다. 원본 추출물, 외부 에셋, 비공개 정보는 넣지 않되, 팀 공동 기준으로 제공되었고 공유 권한을 확인한 기술 레퍼런스는 `docs/technical-foundation/reference/`에 보존할 수 있습니다.

## 기술 파운데이션

- [`technical-foundation/README.md`](technical-foundation/README.md): 원문 출처, 문서 구성, 공개 전 확인 사항
- [`technical-foundation/mvp-architecture-v0.1.md`](technical-foundation/mvp-architecture-v0.1.md): 현재 Unity 프로젝트에서 바로 따를 MVP 설계 기준
- [`technical-foundation/code-conventions-v0.1.md`](technical-foundation/code-conventions-v0.1.md): Unity C# 구현·데이터·이벤트·카메라·저장 코드 규칙
- `technical-foundation/reference/`: 제공된 Unity 기술 설계 원문 10종. 원문은 참고용이며, 실제 구현 결정은 MVP 아키텍처 문서가 우선합니다.

## 협업 컨벤션

- [`conventions/commit-convention.md`](conventions/commit-convention.md): 한국어 중심 커밋 메시지, 커밋 분리, 사전 검증 규칙
