using UnityEngine;
using ProjectTheta.Capture;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Player;

namespace ProjectTheta.Stage
{
    public sealed class StageEndController : MonoBehaviour
    {
        private StageSessionController _stage;
        private PlayerSideViewController _movement;
        private HypnosisCaster _hypnosis;
        private PlayerCaptureController _capture;
        private bool _applied;

        public void Configure(
            StageSessionController stage,
            PlayerSideViewController movement,
            HypnosisCaster hypnosis,
            PlayerCaptureController capture)
        {
            _stage = stage;
            _movement = movement;
            _hypnosis = hypnosis;
            _capture = capture;
        }

        private void Update()
        {
            if (_applied ||
                _stage == null ||
                _stage.IsRunning)
            {
                return;
            }

            ApplyStageEnd();
        }

        private void ApplyStageEnd()
        {
            _applied = true;

            _capture?.ForceEndCapture(
                false);

            _movement?.SetInputLocked(
                true);

            if (_hypnosis != null)
            {
                _hypnosis.enabled =
                    false;
            }

            RecoveryPoint[] recoveryPoints =
                FindObjectsByType<RecoveryPoint>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < recoveryPoints.Length;
                 i++)
            {
                if (recoveryPoints[i] != null)
                {
                    recoveryPoints[i].enabled =
                        false;
                }
            }

            ImpulseMeter[] impulses =
                FindObjectsByType<ImpulseMeter>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < impulses.Length;
                 i++)
            {
                if (impulses[i] != null)
                {
                    impulses[i].enabled =
                        false;
                }
            }

            FollowerController[] followers =
                FindObjectsByType<FollowerController>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < followers.Length;
                 i++)
            {
                if (followers[i] != null)
                {
                    followers[i].SetExternalControl(
                        true);

                    followers[i].enabled =
                        false;
                }
            }

            NpcAgent[] agents =
                FindObjectsByType<NpcAgent>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < agents.Length;
                 i++)
            {
                if (agents[i] != null)
                {
                    agents[i].enabled =
                        false;
                }
            }

            FollowerEssenceProducer[] producers =
                FindObjectsByType<FollowerEssenceProducer>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < producers.Length;
                 i++)
            {
                if (producers[i] != null)
                {
                    producers[i].enabled =
                        false;
                }
            }

            Rigidbody2D[] bodies =
                FindObjectsByType<Rigidbody2D>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < bodies.Length;
                 i++)
            {
                if (bodies[i] != null)
                {
                    bodies[i].linearVelocity =
                        Vector2.zero;
                }
            }
        }
    }
}
