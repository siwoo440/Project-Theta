using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTheta.Core
{
    public sealed class BootLoader : MonoBehaviour
    {
        [SerializeField] private string _prototypeSceneName = "Test"; // 대상 씬

        private void Start()
        {
            SceneManager.LoadScene(_prototypeSceneName); // 테스트 씬 전환
        }
    }
}
