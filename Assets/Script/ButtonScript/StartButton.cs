using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    //클릭 시 Play 씬 로드
    void OnMouseDown()
    {
        SceneManager.LoadScene("Play");
    }
}
