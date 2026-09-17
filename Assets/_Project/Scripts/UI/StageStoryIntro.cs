using UnityEngine;
using ProjectTheta.Stage;
using ProjectTheta.Story;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 장소에 처음 들어갈 때의 이야기다 (36일차).
    ///
    /// 로딩 · 시작 카드 선택이 끝나고 판이 흐를 때 대사 창을 띄우고 게임을 멈춘다.
    /// 끝나면 장소 규칙 카드(35일차)가 이어서 뜬다(<see cref="DialogueOverlay.Busy"/>로 기다린다).
    /// </summary>
    public sealed class StageStoryIntro : MonoBehaviour
    {
        /// <summary>장소 규칙 카드(270)보다 위다.</summary>
        private const int SortOrder = 275;

        private StageSessionController _stage;
        private RunUpgradeChoicePanel _cards;
        private DialogueOverlay _overlay;
        private StoryScene _scene;

        public void Configure(
            StageSessionController stage,
            StoryScene scene)
        {
            _stage = stage;
            _scene = scene;

            if (_scene == null)
            {
                enabled = false;

                return;
            }

            Canvas canvas =
                UiFactory.CreateCanvas(
                    "StageStoryCanvas",
                    SortOrder,
                    transform);

            _overlay = DialogueOverlay.Create(canvas.transform, SortOrder);

            // 규칙 카드가 먼저 뜨지 않게 곧바로 자리를 잡는다.
            _overlay.Reserve();
        }

        private void Update()
        {
            if (_scene == null)
            {
                return;
            }

            if (_cards == null)
            {
                _cards = FindFirstObjectByType<RunUpgradeChoicePanel>();
            }

            bool ready =
                _stage != null &&
                _stage.IsRunning &&
                PauseMenuLogic.CanOpen(
                    true,
                    _cards != null && _cards.IsOpen,
                    Boss.EndingSequence.IsPlaying,
                    LoadingScreen.IsLoading) &&
                UiEscapeStack.IsEmpty;

            if (!ready)
            {
                return;
            }

            StoryScene scene = _scene;
            _scene = null;

            StoryPlayback.Play(
                _overlay,
                new[] { scene },
                true,
                null);
        }
    }
}
