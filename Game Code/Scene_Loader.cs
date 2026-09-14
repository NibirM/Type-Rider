using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string gameSceneName = "GameScene";

    public void LoadGameScene()
    {
        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError(
                "SceneLoader: Cannot load scene '" + gameSceneName + "'. " +
                "Check that (1) the name is spelled exactly right, case-sensitive, " +
                "and (2) both this tutorial scene AND the game scene are added " +
                "in File > Build Settings > Scenes In Build.",
                this
            );
        }
    }
}