using NUnit.Framework; // 테스트 도구 참조
using UnityEditor; // 빌드 씬 설정 참조
using UnityEditor.SceneManagement; // 미리보기 씬 도구 참조
using UnityEngine; // 오디오 리스너 참조
using UnityEngine.SceneManagement; // 씬 구조 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class AudioListenerSceneTests // 오디오 리스너 씬 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void EnabledBuildScenes_HaveExactlyOneEnabledAudioListener() // 활성 빌드 씬 리스너 수 검증
        { // 테스트 시작
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes; // 빌드 씬 목록 조회

            for (int i = 0; i < buildScenes.Length; i++) // 빌드 씬 순회
            { // 순회 시작
                if (!buildScenes[i].enabled) // 비활성 씬 확인
                { // 건너뛰기 시작
                    continue; // 다음 씬 이동
                } // 건너뛰기 끝

                string scenePath = buildScenes[i].path; // 현재 씬 경로 조회
                Scene previewScene = EditorSceneManager.OpenPreviewScene(scenePath); // 미리보기 씬 열기

                try // 검사 보호 시작
                { // 보호 시작
                    int enabledListenerCount = CountEnabledListeners(previewScene); // 활성 리스너 수 계산

                    Assert.AreEqual( // 단일 리스너 확인
                        1, // 예상 리스너 수
                        enabledListenerCount, // 실제 리스너 수
                        $"{scenePath} 씬의 활성 AudioListener 수"); // 실패 메시지
                } // 보호 끝
                finally // 정리 시작
                { // 정리 블록 시작
                    EditorSceneManager.ClosePreviewScene(previewScene); // 미리보기 씬 닫기
                } // 정리 블록 끝
            } // 순회 끝
        } // 테스트 끝

        private static int CountEnabledListeners(Scene scene) // 활성 리스너 수 계산
        { // 계산 시작
            int count = 0; // 리스너 수 초기화
            GameObject[] rootObjects = scene.GetRootGameObjects(); // 씬 루트 조회

            for (int i = 0; i < rootObjects.Length; i++) // 씬 루트 순회
            { // 루트 순회 시작
                AudioListener[] listeners = rootObjects[i].GetComponentsInChildren<AudioListener>(true); // 하위 리스너 조회

                for (int j = 0; j < listeners.Length; j++) // 리스너 순회
                { // 리스너 순회 시작
                    if (listeners[j].enabled && listeners[j].gameObject.activeInHierarchy) // 활성 상태 확인
                    { // 활성 리스너 처리 시작
                        count++; // 활성 리스너 수 증가
                    } // 활성 리스너 처리 끝
                } // 리스너 순회 끝
            } // 루트 순회 끝

            return count; // 계산 결과 반환
        } // 계산 끝
    } // 클래스 끝
} // 공간 끝
