# Unity 자동화 도구 사용 기준

## 목적

Unity Editor 자동화에 `unity-cli`와 Unity MCP를 함께 사용하되, 작업 성격에 맞는 도구를 선택하고 같은 대상에 대한 충돌을 방지합니다.

## 도구 선택

| 작업 | 우선 도구 | 기준 |
| --- | --- | --- |
| Editor 상태·콘솔 조회 | `unity-cli` | 짧은 명령으로 즉시 확인 가능 |
| 플레이·정지·일시정지 | `unity-cli` | 결과가 명확한 단일 작업 |
| 에셋 새로고침·컴파일 요청 | `unity-cli` | 파일 수정 후 빠른 반영 |
| EditMode·PlayMode 테스트 | `unity-cli` | 명령과 결과가 정형화됨 |
| 스크린샷·프로파일러 조회 | `unity-cli` | 단일 캡처나 수치 확인 |
| 씬·Prefab·`.asset` 리시리얼라이즈 | `unity-cli` | 텍스트 수정 후 Unity 직렬화 검증 |
| 짧은 Unity API 조회·수정 | `unity-cli exec` | 대상과 범위가 명확한 일회성 작업 |
| 씬 계층·컴포넌트 구조 탐색 | Unity MCP | 대상을 구조화된 데이터로 식별 |
| 다수 GameObject·Prefab·UI 조작 | Unity MCP | 세밀한 속성 변경과 배치 실행 |
| 카메라·패키지·에셋 관리 | Unity MCP | 전용 도구와 스키마 활용 |
| 반복적인 Game View·Scene View 검증 | Unity MCP | 인라인 이미지와 조회 결과를 연속 비교 |

CLI 한두 번으로 끝나는 작업은 `unity-cli`를 먼저 사용합니다. CLI 사용을 위해 긴 C# 코드를 임시로 작성해야 하거나 대상 식별이 모호하면 Unity MCP를 사용합니다.

일반 C#·JSON·Markdown 파일은 로컬 파일 도구로 필요한 부분만 수정합니다. CLI와 MCP는 Unity 반영, Editor 상태 확인, 직렬화, 컴파일, 테스트와 시각 검증에 사용합니다.

## `unity-cli` 작업 절차

1. `unity-cli status`로 실행 중인 Editor와 프로젝트 경로를 확인합니다.
2. 여러 Unity Editor가 실행 중이면 모든 명령에 `--project "<Unity 프로젝트 절대 경로>"`를 지정합니다.
3. 필요한 명령을 실행하고 컴파일 또는 도메인 리로드 완료를 기다립니다.
4. `console --type error --stacktrace user`로 오류를 확인합니다.
5. 씬·Prefab·`.asset` YAML을 텍스트로 수정했다면 변경한 대상만 `reserialize`하고 다시 검증합니다.

Windows에서 `--project` 값은 `unity-cli status`의 `Project:`에 표시되는 것과 같이 `/` 구분자를 사용합니다.

### CLI 사용 제한

- `exec`는 Editor 메인 스레드에서 실제 C#을 실행하므로 요청에 필요한 최소 범위로 제한합니다.
- 복잡한 비동기 처리나 넓은 범위의 변경에는 `exec`를 사용하지 않습니다.
- `--allow-async`, 강제 새로고침, 전체 프로젝트 리시리얼라이즈는 필요성과 영향 범위를 확인한 경우에만 사용합니다.
- `unity-cli` 실행 파일과 `%USERPROFILE%/.unity-cli/` 인스턴스 정보는 개발자 로컬 도구이므로 저장소에 포함하지 않습니다.

## Unity MCP 작업 절차

1. Editor 상태와 활성 Unity 인스턴스를 확인합니다.
2. 리소스와 조회 도구로 대상 씬·GameObject·컴포넌트를 식별합니다.
3. 독립적인 여러 작업은 가능한 경우 배치로 실행합니다.
4. 스크립트 변경 후 컴파일과 도메인 리로드 완료를 확인합니다.
5. 콘솔 오류와 실제 Game View 또는 Scene View를 검증합니다.

## 도구 전환과 장애 대응

- CLI와 MCP로 같은 대상에 대한 변경 명령을 동시에 실행하지 않습니다.
- 도구를 전환하기 전에 앞선 명령의 컴파일·저장·도메인 리로드가 끝났는지 확인합니다.
- CLI가 연결되지 않으면 Connector 패키지, 프로젝트 경로, Editor 상태를 확인한 뒤 MCP를 사용합니다.
- MCP가 연결되지 않아도 CLI로 안전하게 완료 가능한 작업은 CLI로 계속 진행합니다.
- 어느 도구를 사용하든 변경 후 콘솔 오류와 실제 결과를 확인해야 작업이 완료된 것으로 봅니다.
