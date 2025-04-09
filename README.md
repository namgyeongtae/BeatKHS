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

---

## 회고...


---
