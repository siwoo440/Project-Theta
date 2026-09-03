using UnityEngine;
using ProjectTheta.Hypnosis;

namespace ProjectTheta.Stage
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class EssenceCollector : MonoBehaviour
    {
        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>(); // 콜라이더 참조
            trigger.isTrigger = true; // 트리거 설정
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HypnosisTarget target = other.GetComponent<HypnosisTarget>(); // 최면 대상 탐색
            if (target == null || !target.IsFollowing || target.Owner == null) // 회수 가능 확인
            {
                return; // 회수 중단
            }

            HypnosisCaster owner = target.Owner; // 소유자 저장
            int essence = target.Collect(); // 대상 회수
            owner.RemoveFollower(target); // 동행 제거
            StageGoalManager.Instance?.AddEssence(essence); // 정기 반영
            Destroy(target.gameObject); // 대상 제거
        }
    }
}
