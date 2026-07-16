# Unity 기술 파운데이션

## 목적

이 폴더는 `쥐생역전: 찍찍런`의 Unity 구현 원칙을 팀이 같은 기준으로 사용하기 위한 공용 기술 문서입니다. 원문 레퍼런스는 보존하되, 현재 프로젝트의 실제 구현 판단은 `mvp-architecture-v0.1.md`를 우선합니다.

## 구성

| 경로 | 역할 |
| --- | --- |
| `mvp-architecture-v0.1.md` | NAN 2026 사전 과제용 2D 러너 MVP의 확정 기술 기준 |
| `reference/` | 2026-07-16에 제공된 Unity 기술 설계 원문 10종 |

## 원문 출처와 취급

- 제공 파일: `Unity-Technical-Design-Content-20260716.zip`
- 제공 위치: `\\SoftRocket\\KeroStorage\\8_Etc\\Unity-Technical-Design-Content-20260716.zip`
- 반영일: 2026-07-16
- 원문은 구조와 설계 원칙을 검토하는 공동 레퍼런스입니다. 코드·에셋·외부 서비스 의존성을 자동으로 도입하는 지시가 아닙니다.
- 이 저장소를 공개하기 전에는 원문 10종을 저장소에 재배포해도 되는지 제공자 또는 권리자에게 확인합니다. 확인이 끝나지 않았으면 원문은 비공개 저장소에서만 공유하고, 공개 저장소에는 이 문서와 프로젝트 고유 설계 문서만 남깁니다.

## 레퍼런스 목록

| 주제 | 원문 경로 | MVP 적용 |
| --- | --- | --- |
| 전역 시작 | `reference/global-bootstrapper-architecture-guide/course.mdx` | 적용 |
| 씬 조립 | `reference/scene-di-architecture-guide/course.mdx` | 적용, 수동 조립부터 시작 |
| 이벤트 | `reference/eventbus-architecture-guide/course.mdx` | 적용 |
| 입력 | `reference/input-system-key-binding-architecture-guide/course.mdx` | 적용 |
| 스킬·버프 | `reference/skill-system-architecture-guide/course.mdx` | 적용 |
| 상태 | `reference/state-machine-behavior-tree-architecture-guide/course.mdx` | FSM 적용, 행동 트리는 보류 |
| UI | `reference/ui-architecture-manager-architecture-guide/course.mdx` | 적용 |
| 카메라 | `reference/camera-cinemachine-architecture-guide/course.mdx` | Cinemachine 3.1.7 적용 |
| 풀링·리소스 | `reference/addressables-pooling-architecture-guide/course.mdx` | 로컬 풀링 적용, Addressables는 보류 |
| DI 컨테이너 | `reference/vcontainer-architecture-guide/course.mdx` | 수동 조립 유지, VContainer는 보류 |
