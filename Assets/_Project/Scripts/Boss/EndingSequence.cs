using System; // 완료 콜백 참조
using UnityEngine; // 유니티 기능 참조
using UnityEngine.UI; // 화면 UI 참조
#if ENABLE_INPUT_SYSTEM // 입력 시스템 사용 조건
using UnityEngine.InputSystem; // 새 입력 시스템 참조
#endif // 입력 시스템 조건 끝
using ProjectTheta.UI.Framework; // 공용 UI 참조

namespace ProjectTheta.Boss // 보스 공간
{ // 공간 시작
    public sealed class EndingSequence : MonoBehaviour // 엔딩 연출 제어기
    { // 클래스 시작
        private const int SortOrder = 250; // 화면 정렬 순서
        private static bool _playing; // 전체 재생 상태
        private Canvas _canvas; // 엔딩 화면
        private Image _backdrop; // 암전 배경
        private Text _title; // 엔딩 제목
        private Text[] _lines = new Text[0]; // 엔딩 문장 UI
        private float _elapsed; // 연출 경과 시간
        private bool _started; // 연출 시작 여부
        private bool _awaitingChoice; // 선택 대기 여부
        private EndingId _endingId; // 현재 엔딩
        private Action<EndingId> _onCompleted; // 완료 콜백

        public static bool IsPlaying => _playing; // 전체 재생 상태 반환

        [RuntimeInitializeOnLoadMethod( // 재생 초기화 표시
            RuntimeInitializeLoadType.SubsystemRegistration)] // 하위 시스템 재등록 시점
        private static void ResetOnPlayModeEnter() // 재생 진입 초기화
        { // 초기화 시작
            _playing = false; // 재생 상태 해제
        } // 초기화 끝

        public bool Play(EndingDecision decision, Action<EndingId> onCompleted) // 엔딩 분기 재생
        { // 재생 시작
            if (_started || decision == EndingDecision.None) // 재생 가능 여부 확인
            { // 거부 시작
                return false; // 재생 실패
            } // 거부 끝

            _started = true; // 시작 상태 설정
            _playing = true; // 전체 재생 상태 설정
            _onCompleted = onCompleted; // 완료 콜백 저장

            if (decision == EndingDecision.ChoiceAOrB) // 선택 분기 확인
            { // 선택 시작
                _awaitingChoice = true; // 선택 대기 설정
                BuildChoice(); // 선택 화면 생성
                return true; // 선택 재생 성공
            } // 선택 끝

            BeginEnding(EndingBranchLogic.GetDefaultEnding(decision)); // 확정 엔딩 시작
            return true; // 재생 성공
        } // 재생 끝

        private void OnDestroy() // 객체 제거 처리
        { // 제거 시작
            if (_started) // 시작 여부 확인
            { // 상태 정리 시작
                _playing = false; // 전체 재생 상태 해제
            } // 상태 정리 끝
        } // 제거 끝

        private void Update() // 프레임 갱신
        { // 갱신 시작
            if (!_started || !_playing || _awaitingChoice) // 연출 진행 가능 여부 확인
            { // 대기 시작
                return; // 갱신 중단
            } // 대기 끝

            _elapsed += Time.unscaledDeltaTime; // 비정규화 시간 누적

            if (ReadSkipPressed()) // 건너뛰기 입력 확인
            { // 건너뛰기 시작
                _elapsed = EndingLogic.Skip(_elapsed); // 연출 시간 이동
            } // 건너뛰기 끝

            Apply(); // 화면 상태 적용

            if (EndingLogic.IsFinished(_elapsed)) // 연출 완료 확인
            { // 완료 시작
                Complete(); // 완료 처리
            } // 완료 끝
        } // 갱신 끝

        private void BeginEnding(EndingId endingId) // 개별 엔딩 시작
        { // 시작 처리
            _awaitingChoice = false; // 선택 대기 해제
            _endingId = endingId; // 현재 엔딩 저장
            _elapsed = 0f; // 경과 시간 초기화
            DestroyCanvas(); // 기존 선택 화면 제거
            BuildEnding(); // 엔딩 화면 생성
            Apply(); // 초기 화면 적용
        } // 시작 처리 끝

        private void Complete() // 엔딩 완료 처리
        { // 완료 처리 시작
            _playing = false; // 전체 재생 상태 해제
            DestroyCanvas(); // 엔딩 화면 제거
            Action<EndingId> completed = _onCompleted; // 콜백 임시 저장
            _onCompleted = null; // 콜백 해제
            completed?.Invoke(_endingId); // 선택 엔딩 전달
        } // 완료 처리 끝

        private void Apply() // 연출 화면 적용
        { // 적용 시작
            if (_backdrop == null) // 배경 확인
            { // 중단 시작
                return; // 적용 중단
            } // 중단 끝

            Color dark = _backdrop.color; // 현재 배경색 조회
            dark.a = EndingLogic.GetBackdropAlpha(_elapsed); // 배경 투명도 계산
            _backdrop.color = dark; // 배경색 적용
            SetAlpha(_title, EndingLogic.GetTextAlpha(_elapsed, -1)); // 제목 투명도 적용

            for (int i = 0; i < _lines.Length; i++) // 문장 UI 순회
            { // 순회 시작
                SetAlpha(_lines[i], EndingLogic.GetTextAlpha(_elapsed, i)); // 문장 투명도 적용
            } // 순회 끝
        } // 적용 끝

        private static void SetAlpha(Text text, float alpha) // 글자 투명도 적용
        { // 적용 시작
            if (text == null) // 글자 확인
            { // 중단 시작
                return; // 적용 중단
            } // 중단 끝

            Color color = text.color; // 현재 글자색 조회
            color.a = alpha; // 투명도 변경
            text.color = color; // 글자색 적용
        } // 적용 끝

        private static bool ReadSkipPressed() // 건너뛰기 입력 조회
        { // 조회 시작
#if ENABLE_INPUT_SYSTEM // 새 입력 시스템 조건
            return (Mouse.current != null && // 마우스 존재 확인
                    Mouse.current.leftButton.wasPressedThisFrame) || // 왼쪽 클릭 확인
                   (Keyboard.current != null && // 키보드 존재 확인
                    (Keyboard.current.spaceKey.wasPressedThisFrame || // 스페이스 확인
                     Keyboard.current.enterKey.wasPressedThisFrame)); // 엔터 확인
#else // 구 입력 시스템 조건
            return Input.GetMouseButtonDown(0); // 왼쪽 클릭 반환
#endif // 입력 시스템 조건 끝
        } // 조회 끝

        private void BuildChoice() // 엔딩 선택 화면 생성
        { // 생성 시작
            _canvas = UiFactory.CreateCanvas("EndingChoiceCanvas", SortOrder, transform); // 선택 화면 생성
            _backdrop = UiFactory.CreateImage(_canvas.transform, "Backdrop", new Color(0.02f, 0.01f, 0.05f, 0.94f)); // 암전 배경 생성
            _backdrop.raycastTarget = true; // 입력 차단 설정
            UiFactory.Stretch(_backdrop.rectTransform); // 배경 화면 채움
            Text prompt = UiFactory.CreateText(_canvas.transform, "Prompt", "결전 뒤의 길을 선택하세요", UiTheme.FontTitle, UiTheme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold); // 선택 안내 생성
            UiFactory.Place(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(900f, 90f)); // 안내 위치 지정
            UiButton endingA = UiFactory.CreateButton(_canvas.transform, "EndingA", "각자의 길을 지킨다", UiTheme.FontHeading); // 엔딩 A 버튼 생성
            UiFactory.Place(endingA.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250f, 0f), new Vector2(420f, 86f)); // 엔딩 A 버튼 배치
            endingA.Button.onClick.AddListener(() => BeginEnding(EndingId.A)); // 엔딩 A 선택 연결
            UiButton endingB = UiFactory.CreateButton(_canvas.transform, "EndingB", "리엘라와 함께 걷는다", UiTheme.FontHeading, true); // 엔딩 B 버튼 생성
            UiFactory.Place(endingB.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250f, 0f), new Vector2(420f, 86f)); // 엔딩 B 버튼 배치
            endingB.Button.onClick.AddListener(() => BeginEnding(EndingId.B)); // 엔딩 B 선택 연결
        } // 생성 끝

        private void BuildEnding() // 엔딩 화면 생성
        { // 생성 시작
            _canvas = UiFactory.CreateCanvas("EndingCanvas", SortOrder, transform); // 엔딩 화면 생성
            _backdrop = UiFactory.CreateImage(_canvas.transform, "Backdrop", new Color(0.02f, 0.01f, 0.05f, 0f)); // 암전 배경 생성
            _backdrop.raycastTarget = true; // 입력 차단 설정
            UiFactory.Stretch(_backdrop.rectTransform); // 배경 화면 채움
            _title = UiFactory.CreateText(_canvas.transform, "Title", EndingLogic.GetTitle(_endingId), UiTheme.FontTitle + 20, UiTheme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold); // 엔딩 제목 생성
            UiFactory.Place(_title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(900f, 90f)); // 제목 위치 지정
            string[] content = EndingLogic.GetLines(_endingId); // 엔딩별 문장 조회
            _lines = new Text[content.Length]; // 문장 UI 배열 생성

            for (int i = 0; i < _lines.Length; i++) // 문장 순회
            { // 순회 시작
                _lines[i] = UiFactory.CreateText(_canvas.transform, $"Line_{i}", content[i], UiTheme.FontHeading, i == _lines.Length - 1 ? UiTheme.AccentSoft : UiTheme.TextPrimary, TextAnchor.MiddleCenter); // 문장 UI 생성
                UiFactory.Place(_lines[i].rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f - i * 60f), new Vector2(1400f, 44f)); // 문장 위치 지정
            } // 순회 끝

            Text hint = UiFactory.CreateText(_canvas.transform, "Hint", "클릭하면 넘어갑니다", UiTheme.FontSmall, new Color(1f, 1f, 1f, 0.35f), TextAnchor.MiddleCenter); // 건너뛰기 안내 생성
            UiFactory.Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(400f, 24f)); // 안내 위치 지정
        } // 생성 끝

        private void DestroyCanvas() // 현재 화면 제거
        { // 제거 시작
            if (_canvas != null) // 화면 확인
            { // 화면 제거 시작
                Destroy(_canvas.gameObject); // 화면 객체 제거
            } // 화면 제거 끝

            _canvas = null; // 화면 참조 해제
            _backdrop = null; // 배경 참조 해제
            _title = null; // 제목 참조 해제
            _lines = new Text[0]; // 문장 배열 초기화
        } // 제거 끝
    } // 클래스 끝
} // 공간 끝
