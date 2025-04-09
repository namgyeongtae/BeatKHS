**Unity 기반 2D 리듬게임 프로젝트**
---

## 기술스택
- 엔진 : Unity 2022.3.10f1
- 언어 : C#
- 오디오 미들웨어 : FMOD
- 백엔드 : Firebase (Authentication, Realtime Database)

---
## 주요 기능
- UI 바인딩 시스템 프레임워크 구축
  
- FMOD 기반 오디오 재생 처리
  - DSP Clock 기반 판정 타이밍 계산
 
- 플레이 시스템 구현
  - Perfect / Good / Bad / Miss 판정
  - 점수, 콤보 계산 및 판정 UI 이펙트 연동
  - 키 입력 처리 및 판정 타이밍 비교
 
- 채보 제작용 에디터 툴 개발
  - 음악에 맞는 노트 배치 에디터 툴 제작
  - 클릭 기반 노트 생성/삭제
  - 채보 데이터 저장 및 Json 데이터 파싱
 
- 채보 데이터에 따른 노트 생성
  - 데이터를 통해 숏노트/롱노트 식별 후 노트 오브젝트 생성
  - 오브젝트 풀링을 통한 노트 생성 관리
 
- Firebase 연동 기능 구현
  - Firebase Authentication으로 회원가입/로그인 기능 구현
  - Firebase Realtime Database에 유저의 곡별 점수, 콤보, 판정 등급 저장
  - 로그인된 유저 기준으로 데이터 로드 및 UI에 반영

---

## 프리뷰

<img width="926" alt="image" src="https://github.com/user-attachments/assets/b8268058-dd35-406c-bd9b-9e19870be4bb" />

<img width="926" alt="image" src="https://github.com/user-attachments/assets/de43b578-7125-4e54-8a73-08ae57fd949c" />

---

## 채보 편집 툴
음악의 채보 제작을 위한 Unity Editor 툴 개발
- 타임라인 기반 UI
- 음악 재생과 동시에 편집
- 마우스 입력을 통한 숏노트/롱노트 배치
- 채보 데이터 저장 및 Json 파싱

  <img width="833" alt="image" src="https://github.com/user-attachments/assets/cfaa4306-0b38-4a4b-b8f0-bb6859ad14a9" />

K-shoot mania의 채보 편집 툴을 참고로 제작하였습니다.

---

## 트러블 슈팅
<details>
<summary>채보 편집 노트 배치의 오류</summary>

### 현상  
편집 창의 스크롤을 어느 정도 한 경우 클릭한 지점이 아닌 엉뚱한 곳에 노트가 배치되는 이슈 발견

### 원인  
스크롤 뷰 내에서 클릭 지점인 `mousePosition`에 `Horizontal Scroll View`가 넘어간 거리만큼 더한 것이 오류를 발생

### 해결 방식  
- 마우스 클릭 후 드래그할 시 실시간으로 `mousePosition`의 좌표와 `scrollPosition`의 좌표를 로그로 찍어 관찰  
- 관찰 결과: 스크롤 뷰 내에서 클릭 시 `mousePosition`의 좌표 성분값이 비정상적으로 큰 것이 확인됨  
- 로그에 나온 `scrollPosition`을 뺀 좌표값이 실제 마우스가 위치한 스크린 좌표와 같다는 것을 파악  
- 즉, 이미 `scrollPosition`이 포함된 `mousePosition`에 중복으로 한 번 더 `scrollPosition`을 더하려고 한 것이 문제  
- 따라서 **`scrollPosition`을 더하지 않도록 수정**하여 간단하게 해결

---

### 새로 알게 된 사실  

Unity IMGUI (Immediate Mode GUI) 시스템에서 스크롤뷰 관련 중요 포인트:

1. `EditorGUILayout.BeginScrollView()`로 생성된 스크롤뷰 내에서:
   - **OnClick**:
     - 마우스 이벤트의 좌표(`e.mousePosition`)는 Unity가 자동 처리
     - 스크롤뷰의 로컬 좌표계로 자동 변환됨
     - → **추가로 `scrollPosition`을 더하면 안 됨**
   - **OnDrag**:
     - 마우스 이벤트 좌표는 절대 좌표 기준  
     - → 이 경우엔 `scrollPosition`을 더하거나 고려해야 할 수 있음

---

<img width="467" alt="image" src="https://github.com/user-attachments/assets/33302e01-61d4-4047-9df9-715cb27cf7dd" />

위 이미지는 마우스를 수직 드래그 하였을 때의 로그 예시입니다.

- [Drag] → 1084 (x 좌표)
- [Click] → 2028 (x 좌표)
- `2028 (Click Mouse Position) - 944.xx(ScrollPosition.x) ≒ 1084` 성립  
→ 즉, **스크롤 뷰 내에서 클릭 시 `mousePosition`에 `scrollPosition`이 이미 포함되어 있다**는 사실을 알게 되었습니다.
</details>

## 회고...


---
