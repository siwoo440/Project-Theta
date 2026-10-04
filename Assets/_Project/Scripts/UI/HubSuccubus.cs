using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.Story;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 허브 방 안의 서큐버스다 (36일차).
    ///
    /// 허브에 들어올 때마다 <see cref="HubSuccubusLogic.Spots"/> 중 한 자리에 무작위로 선다(바로 전 자리는 피함).
    /// 누르면 머리 오른쪽 위 말풍선에 대사가 한 글자씩 나오고, 잠시 뒤 사라진다.
    /// 오른쪽 끝 자리에서는 말풍선이 머리 왼쪽 위로 뒤집힌다.
    ///
    /// 그림: Resources/Characters/Riella/Room_Stand · Room_Sit → Idle → 기존 Succubus → 임시 실루엣 순서로 찾는다.
    /// </summary>
    public sealed class HubSuccubus : MonoBehaviour
    {
        private static int _lastSpot = -1;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _lastSpot = -1;
        }

        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();
        private readonly System.Random _random = new System.Random(Environment.TickCount);

        private RectTransform _body;
        private RectTransform _bubble;
        private Text _bubbleText;
        private Func<bool> _blocked;
        private Func<SaveData> _save;
        private Action<LobbyDialogueDefinition> _spoken; // 대사 확인 알림

        private HubSuccubusSpot _spot;
        private float _height;
        private float _elapsed;
        private string _line = string.Empty;
        private float _lineElapsed;
        private int _lastVisible;
        private float _bubbleRemaining;
        private Action _onFinished; // 대사 완료 알림

        /// <summary>지금 서 있는 자리 이름이다(확인용).</summary>
        public string SpotName =>
            _spot.Name;

        public bool IsSpeaking => // 대사 재생 여부
            _bubble != null && // 말풍선 존재 확인
            _bubble.gameObject.activeSelf; // 말풍선 활성 확인

        public void Build(
            Transform parent,
            Func<SaveData> save,
            Func<bool> blocked,
            Action<LobbyDialogueDefinition> spoken) // 대사 확인 알림
        {
            _save = save;
            _blocked = blocked;
            _spoken = spoken; // 확인 알림 저장

            int index = HubSuccubusLogic.PickSpot(_random, _lastSpot);
            _lastSpot = index;
            _spot = HubSuccubusLogic.Spots[index];
            _height = HubSuccubusLogic.GetHeight(_spot.Pose);

            // 발끝 기준으로 세운다.
            RectTransform root = UiFactory.CreateRect(parent, "Succubus");
            UiFactory.Place(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(_spot.X, _spot.Y), new Vector2(_height * 0.6f, _height));

            // 발밑 그림자
            Image shadow = UiDecor.CreateGlow(root, "Shadow", new Color(0f, 0f, 0f, 0.45f), new Vector2(_height * 0.55f, 26f));
            UiFactory.Place(shadow.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(_height * 0.55f, 26f));

            _body = UiFactory.CreateRect(root, "Body");
            UiFactory.Place(_body, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(_height * 0.6f, _height));

            BuildArt();

            // 몸 전체가 누르는 자리다.
            UiButton hit = UiFactory.CreateButton(root, "Hit", string.Empty);
            UiFactory.Stretch(hit.Background.rectTransform);
            hit.Background.color = new Color(1f, 1f, 1f, 0f);
            hit.Button.transition = Selectable.Transition.None;
            hit.Button.onClick.AddListener(Talk);

            BuildBubble(root);
        }

        private void BuildArt()
        {
            Sprite art = LoadFirstSprite( // 리엘라 허브 이미지 조회
                CharacterArtCatalog.GetHubPaths(_spot.Pose == HubSuccubusPose.Sit)); // 자세별 후보 경로

            if (art != null)
            {
                Image image = UiFactory.CreateImage(_body, "Art", Color.white);
                image.sprite = art;
                image.preserveAspect = true;
                UiFactory.Stretch(image.rectTransform);

                return;
            }

            BuildSilhouette();
        }

        /// <summary>그림이 없을 때 쓰는 임시 실루엣이다. 로딩 화면 실루엣을 방 크기로 키운 모양이다.</summary>
        private void BuildSilhouette()
        {
            RectTransform figure = UiFactory.CreateRect(_body, "Silhouette");
            UiFactory.Place(figure, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(100f, 100f));

            float scale = _height / 100f;
            figure.localScale = new Vector3(scale, scale, 1f);

            Color body = new Color(0.62f, 0.28f, 0.86f);
            Color wing = new Color(0.36f, 0.16f, 0.52f, 0.9f);
            Color glow = new Color(1.00f, 0.45f, 0.85f);
            bool sit = _spot.Pose == HubSuccubusPose.Sit;

            Piece(figure, "Aura", new Vector2(0f, 50f), new Vector2(60f, 90f), new Color(glow.r, glow.g, glow.b, 0.14f), 0f);
            Piece(figure, "WingLeft", new Vector2(-26f, 62f), new Vector2(34f, 22f), wing, 25f);
            Piece(figure, "WingRight", new Vector2(26f, 62f), new Vector2(34f, 22f), wing, -25f);
            Piece(figure, "Body", new Vector2(0f, sit ? 38f : 44f), new Vector2(22f, sit ? 32f : 44f), body, 0f);

            if (sit)
            {
                Piece(figure, "Legs", new Vector2(8f, 18f), new Vector2(28f, 10f), body, 0f);
            }
            else
            {
                Piece(figure, "LegLeft", new Vector2(-5f, 12f), new Vector2(7f, 24f), body, 0f);
                Piece(figure, "LegRight", new Vector2(5f, 12f), new Vector2(7f, 24f), body, 0f);
            }

            Piece(figure, "Head", new Vector2(0f, 76f), new Vector2(20f, 20f), body, 0f);
            Piece(figure, "HornLeft", new Vector2(-8f, 90f), new Vector2(5f, 12f), glow, 20f);
            Piece(figure, "HornRight", new Vector2(8f, 90f), new Vector2(5f, 12f), glow, -20f);
            Piece(figure, "Tail", new Vector2(16f, 26f), new Vector2(4f, 24f), body, 40f);
            Piece(figure, "TailTip", new Vector2(24f, 16f), new Vector2(9f, 9f), glow, 45f);
        }

        private static void Piece(
            RectTransform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color,
            float rotation)
        {
            Image image = UiFactory.CreateImage(parent, name, color);
            UiFactory.Place(image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), position, size);
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        /// <summary>머리 오른쪽 위(또는 왼쪽 위)에 붙는 말풍선이다.</summary>
        private void BuildBubble(
            RectTransform root)
        {
            float headX = _height * 0.12f;
            bool flip = HubSuccubusLogic.ShouldFlip(_spot.X, headX, HubSuccubusLogic.BubbleWidth);

            _bubble = UiFactory.CreateRect(root, "Bubble");
            UiFactory.Place(
                _bubble,
                new Vector2(0.5f, 0f),
                new Vector2(flip ? 1f : 0f, 0f),
                new Vector2(flip ? -headX : headX, _height * 0.92f + 18f),
                new Vector2(HubSuccubusLogic.BubbleWidth, HubSuccubusLogic.BubbleMinHeight));

            Color fill = new Color(0.98f, 0.95f, 1f, 0.96f);
            Color edge = new Color(0.62f, 0.38f, 0.86f, 1f);

            // 꼬리: 말풍선 아래 모서리에서 머리 쪽으로 내려오는 작은 마름모
            Image tailEdge = UiFactory.CreateImage(_bubble, "TailEdge", edge);
            UiFactory.Place(tailEdge.rectTransform, new Vector2(flip ? 1f : 0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(flip ? -28f : 28f, -2f), new Vector2(24f, 24f));
            tailEdge.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            RectTransform box = UiFactory.CreatePanel(_bubble, "Box", fill, edge, 3f);
            UiFactory.Stretch(box);

            Image tail = UiFactory.CreateImage(_bubble, "Tail", fill);
            UiFactory.Place(tail.rectTransform, new Vector2(flip ? 1f : 0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(flip ? -28f : 28f, 1f), new Vector2(19f, 19f));
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            _bubbleText = UiFactory.CreateText(_bubble, "Text", string.Empty, UiTheme.FontBody + 1, new Color(0.18f, 0.10f, 0.26f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.Stretch(_bubbleText.rectTransform, HubSuccubusLogic.BubblePadding);
            _bubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _bubbleText.lineSpacing = 1.15f;
            _bubbleText.raycastTarget = false;

            _bubble.gameObject.SetActive(false);
        }

        private void Talk()
        {
            if (_blocked != null &&
                _blocked())
            {
                return;
            }

            if (!LobbyDialogueLogic.CanReplaceSpeech(IsSpeaking, _onFinished != null)) // 중요 완료 대사 확인
            { // 교체 차단 시작
                return; // 상호작용 종료
            } // 교체 차단 끝

            LobbyDialogueDefinition dialogue = LobbyDialogueLogic.Select( // 일반 상호작용 대사 선택
                LobbyDialogueTrigger.Idle, // 방치형 일반 대사
                _save?.Invoke(), // 현재 저장 자료
                StageResultSummary.Empty, // 직전 결과 없음
                _random, // 난수 도구
                (float)_random.NextDouble()); // 희귀 확률 값

            if (dialogue == null) // 선택 실패 확인
            { // 실패 시작
                return; // 상호작용 종료
            } // 실패 끝

            _spoken?.Invoke(dialogue); // 대사 확인 알림 호출
            Speak(dialogue, null); // 선택 대사 재생
        }

        public void Speak( // 지정 로비 대사 재생
            LobbyDialogueDefinition dialogue, // 재생 대사
            Action onFinished) // 완료 알림
        { // 재생 시작
            if (dialogue == null || _bubble == null || _bubbleText == null) // 재생 환경 확인
            { // 환경 없음 시작
                onFinished?.Invoke(); // 완료 알림 호출

                return; // 재생 종료
            } // 환경 없음 끝

            if (!LobbyDialogueLogic.CanReplaceSpeech(IsSpeaking, _onFinished != null)) // 중요 완료 대사 확인
            { // 교체 차단 시작
                return; // 기존 대사 유지
            } // 교체 차단 끝

            _line = dialogue.Text; // 대사 본문 저장
            _onFinished = onFinished; // 완료 알림 저장

            // 다 나온 글 기준으로 말풍선 높이를 먼저 맞춘다.
            _bubbleText.text = _line;
            float height =
                Mathf.Max(
                    HubSuccubusLogic.BubbleMinHeight,
                    _bubbleText.preferredHeight + HubSuccubusLogic.BubblePadding * 2f);

            _bubble.sizeDelta = new Vector2(HubSuccubusLogic.BubbleWidth, height);
            _bubbleText.text = string.Empty;

            _lineElapsed = 0f;
            _lastVisible = 0;
            _bubbleRemaining = HubSuccubusLogic.GetBubbleSeconds(_line) + StoryLogic.GetLineSeconds(_line.Length);
            _bubble.gameObject.SetActive(true);

            GameAudio.Play(GameSfx.Bubble, 0.8f);
        } // 재생 끝

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            _elapsed += delta;

            // 숨 쉬듯 살짝 오르내린다.
            if (_body != null)
            {
                _body.anchoredPosition = new Vector2(0f, Mathf.Sin(_elapsed * 1.6f) * 4f);
            }

            if (_bubble == null ||
                !_bubble.gameObject.activeSelf)
            {
                return;
            }

            _lineElapsed += delta;

            int visible = StoryLogic.GetVisibleCharacters(_lineElapsed, _line.Length);

            if (MusicLogic.ShouldBlip(_lastVisible, visible, _line))
            {
                GameAudio.PlayPitched(GameSfx.DialogueBlip, 0.25f, MusicLogic.GetBlipPitch(StoryCatalog.Me));
            }

            _lastVisible = visible;
            _bubbleText.text = _line.Substring(0, visible);

            // 시간이 지나면 말풍선을 닫는다.
            _bubbleRemaining -= delta;

            if (_bubbleRemaining <= 0f)
            {
                _bubble.gameObject.SetActive(false);
                Action finished = _onFinished; // 완료 알림 보관
                _onFinished = null; // 완료 알림 초기화
                finished?.Invoke(); // 완료 알림 호출
            }
        }

        private Sprite LoadFirstSprite( // 첫 유효 허브 이미지 조회
            string[] paths) // 후보 경로 목록
        { // 조회 시작
            if (paths == null) // 후보 목록 확인
            { // 목록 없음 시작
                return null; // 이미지 없음 반환
            } // 목록 없음 끝

            for (int i = 0; i < paths.Length; i++) // 후보 경로 순회
            { // 순회 시작
                string path = paths[i]; // 현재 경로 조회
                Texture2D texture = Resources.Load<Texture2D>(path); // 텍스처 조회

                if (texture == null) // 텍스처 없음 확인
                { // 없음 시작
                    continue; // 다음 후보 이동
                } // 없음 끝

                Sprite sprite = Sprite.Create( // 허브 스프라이트 생성
                    texture, // 원본 텍스처
                    new Rect(0f, 0f, texture.width, texture.height), // 전체 영역
                    new Vector2(0.5f, 0f), // 발 기준점
                    100f); // 픽셀 단위

                sprite.name = path.Replace('/', '_') + "_Hub"; // 스프라이트 이름 설정
                _created.Add(sprite); // 정리 목록 추가

                return sprite; // 첫 이미지 반환
            } // 순회 끝

            return null; // 이미지 없음 반환
        } // 조회 끝

        private void OnDestroy()
        {
            foreach (UnityEngine.Object created in _created)
            {
                if (created != null)
                {
                    Destroy(created);
                }
            }

            _created.Clear();
        }
    }
}
