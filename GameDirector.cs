using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    [Header("UI 元件綁定")]
    public Text statusText;
    public GameObject restartButton;

    void Start()
    {
        if (statusText != null)
        {
            statusText.text = "按空白鍵或點擊螢幕開始飛行";
        }
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }
    }

    public void OnStartFly(int totalObstacles)
    {
        if (statusText != null)
        {
            statusText.text = "飛行中！已通過障礙物：0 / " + totalObstacles;
        }
    }

    public void OnPassObstacle(int passedCount, int totalObstacles)
    {
        if (statusText != null)
        {
            statusText.text = "順利通過障礙物！進度：" + passedCount + " / " + totalObstacles;
        }
    }

    public void OnCrash()
    {
        if (statusText != null)
        {
            statusText.text = "撞到障礙物！挑戰失敗";
        }
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }

    public void OnWin(int totalObstacles)
    {
        if (statusText != null)
        {
            statusText.text = "恭喜！順利飛過全部 " + totalObstacles + " 個障礙物！";
        }
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}