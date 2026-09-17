using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Story;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 이야기를 틀고 본 것으로 저장하는 공용 순서다 (36일차).
    /// 지도 · 허브 도착, 허브 일기장 · 장소 입장이 같이 쓴다.
    /// </summary>
    public static class StoryPlayback
    {
        /// <summary>대사 창 정렬 순서다. 업적 알림 · 창들보다 위다.</summary>
        public const int SortOrder = 120;

        /// <summary>조건을 채웠는데 아직 안 본 장면을 모두 튼다. 없으면 바로 끝낸다.</summary>
        public static bool PlayPending(
            DialogueOverlay overlay,
            Action onFinished)
        {
            GameSession session = GameSession.Instance;

            if (session == null ||
                session.Save == null ||
                overlay == null)
            {
                onFinished?.Invoke();

                return false;
            }

            List<StoryScene> pending = StoryLogic.GetPending(session.Save);

            if (pending.Count == 0)
            {
                onFinished?.Invoke();

                return false;
            }

            Play(overlay, pending, false, onFinished);

            return true;
        }

        /// <summary>장면들을 틀고, 시작할 때마다 본 것으로 남기고, 끝나면 저장한다.</summary>
        public static void Play(
            DialogueOverlay overlay,
            IList<StoryScene> scenes,
            bool pauseGame,
            Action onFinished)
        {
            bool changed = false;

            overlay.Play(
                scenes,
                pauseGame,
                scene =>
                {
                    GameSession session = GameSession.Instance;

                    if (session != null &&
                        StoryLogic.MarkSeen(session.Save, scene.Id))
                    {
                        changed = true;
                    }
                },
                () =>
                {
                    if (changed)
                    {
                        GameSession.Instance?.WriteSave();
                    }

                    onFinished?.Invoke();
                });
        }
    }
}
