using UnityEngine;
using UnityEngine.SceneManagement;

public class MainButton : MonoBehaviour
{
    //버튼 클릭시 게임 메인화면 씬 불러옴
    void OnMouseDown()
    {
        SceneManager.LoadScene("GameMain");
    }
}

//메인 버튼: prefab / 메인 버튼의 이미지를 변경하고 싶을 경우 (만일의 경우를 대비해 작성, 추후 삭제 요망)
//mainbut의 inspector에서 Sprite Renderer -> Sprite 클릭 후 원하는 이미지 선택