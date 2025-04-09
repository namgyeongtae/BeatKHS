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
# 채보 편집 노트 배치의 오류
- 현상 : 편집 창의 스크롤을 어느 정도 한 경우 클릭한 지점이 아닌 엉뚱한 곳에 노트가 배치되는 이슈 발견
- 원인 : 스크롤 뷰 내에서 클릭 지점인 mousePosition 에 Horizontal Scroll View가 넘어간 거리 만큼 더한 것이 오류를 발생
- 해결 방식
  마우스 클릭 후 드래그 할 시 실시간으로 mousePostion 의 좌표와 scrollPosition의 좌표를 로그로 찍어 관찰해보았습니다.
  관찰 결과 : 스크롤 뷰 내에서 클릭을 할 시 mousePostion의 좌표 성분값이 비정상적으로 큰 것이 확인 되었고 이를 로그에 나온 scrollPosition을 뺀 결과의 좌표값이 실제 마우스가 위치한 스크린 좌표와 같다는 것을 파악하였습니다.
             즉, 이미 scrollPosition 까지 포함된 mousePosition에 중복으로 한번 더 scrollPosition을 더하려고 한 것이 문제이므로 이를 더하지 않는 것으로 해결책은 아주 간단하였다.

  # 새로 알게 된 사실
  Unity IMGUI (Immediate Mode GUI) 시스템에서 스크롤뷰 관련 중요 포인트:

  1. EditorGUILayout.BeginScrollView()로 생성된 스크롤뷰 내에서:
     - 일반 클릭의 경우 (OnClick)
        - 마우스 이벤트의 좌표(e.mousePosition)는 Unity가 자동으로 처리
        - 스크롤뷰의 로컬 좌표계로 자동 변환
        - 따라서 추가로 scrollPosition을 더하면 안 됨
     - 드래그의 경우 (OnDrag)
        - 마우스 이벤트의 좌표가 스크롤 로컬좌표계가 아닌 절대적인 좌표

   <img width="467" alt="image" src="https://github.com/user-attachments/assets/33302e01-61d4-4047-9df9-715cb27cf7dd" />
    위 이미지는 마우스를 수직 드래그 하였을 때의 상황인데 마우스 커서의 좌표와 스크롤이 넘어간 거리(Scroll Position)을 실시간으로 로그에 출력해서 보여주는 이미지입니다.
    [Drag]를 보면 1084로 x좌표가 나와 있는 걸 보고 바로 [Click]쪽을 확인해보니 드래그 전 딱 클릭을 했을 때는 2028로 더 크게 나와 있음을 확인할 수 있었습니다.
    
    2028 (Click Mouse Position) - 944.xx(ScrollPosition.x) = 약 1084(Mouse Screen Position) 
    이런 식이 성립되는 걸 발견하게 되었고 이를 통해 스크롤 뷰 내 클릭 시 mousePosition은 유니티 내부에서 scrollPosition 까지 더해서 반환해 준다는 사실을 배우게 되었습니다.
## 회고...


---
