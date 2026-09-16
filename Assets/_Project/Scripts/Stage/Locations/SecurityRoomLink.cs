using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Disruptors;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 쇼핑몰 보안실 직원이다 (25일차, 부록 C.3 [5]).
    /// 이 직원이 플레이어에게 넘어오는 순간, 같은 층 CCTV가 30초 꺼진다 (한 번만).
    /// </summary>
    [RequireComponent(typeof(HypnosisTarget))]
    public sealed class SecurityRoomLink : MonoBehaviour
    {
        private readonly List<DisruptorBase> _buffer =
            new List<DisruptorBase>();

        private HypnosisTarget _target;
        private bool _used;

        private void Awake()
        {
            _target = GetComponent<HypnosisTarget>();
        }

        private void Update()
        {
            if (_used ||
                _target == null ||
                _target.Owner != NpcOwner.Player)
            {
                return;
            }

            _used = true;

            int floor =
                FloorSpace.FloorAt(transform.position.y);

            DisruptorBase.CopyActive(_buffer);

            int count = 0;

            for (int i = 0;
                 i < _buffer.Count;
                 i++)
            {
                DisruptorBase body = _buffer[i];

                if (body == null ||
                    body.Floor != floor ||
                    body.Profile == null ||
                    body.Profile.Kind != DisruptorKind.SecurityCamera)
                {
                    continue;
                }

                body.Stun(MallOfficeValues.CameraOffSeconds);
                count++;
            }

            GameVfx.FloatText(
                $"보안실 장악 · CCTV {count}대 {MallOfficeValues.CameraOffSeconds:0}초 정지",
                (Vector2)transform.position + new Vector2(0f, 2.4f),
                UiTheme.Positive,
                UiTheme.FontBody,
                1.4f);
        }
    }
}
