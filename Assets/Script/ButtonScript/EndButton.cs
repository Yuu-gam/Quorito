using UnityEngine;
using UnityEngine.SceneManagement;

public class EndButton : MonoBehaviour
{
    //클릭 시 게임 종료
    void OnMouseDown()
    {
        //유니티 에디터 내에서 종료
        UnityEditor.EditorApplication.isPlaying = false;
        //실제 프로그램 종료
        Application.Quit();
    }
}
