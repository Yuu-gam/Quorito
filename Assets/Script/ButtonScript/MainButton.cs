using UnityEngine;
using UnityEngine.SceneManagement;

public class MainButton : MonoBehaviour
{
    [Header("Sprites")]
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    //클릭 시 Play 씬 로드
    [Header("Scales")]
    [SerializeField] private Vector3 normalScale = new Vector3(0.7f, 0.7f, 0.7f); //기본
    [SerializeField] private Vector3 pressedScale = new Vector3(0.65f, 0.65f, 0.65f); //눌렀을때 작아짐

    private bool isPressed = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResetToNormalState();
    }

    void OnMouseEnter()
    {
        if (isPressed) return;

        if (hoverSprite != null)
        {
            spriteRenderer.sprite = hoverSprite;
        }
    }

    void OnMouseExit()
    {
        if (isPressed) return;

        ResetToNormalState();
    }

    void OnMouseDown()
    {
        isPressed = true;
        transform.localScale = pressedScale;
    }

    void OnMouseUp()
    {
        isPressed = false;
        ResetToNormalState();
        SceneManager.LoadScene("GameMain");
    }

    private void ResetToNormalState()
    {
        spriteRenderer.sprite = normalSprite;
        transform.localScale = normalScale;
    }
}

//메인 버튼: prefab / 메인 버튼의 이미지를 변경하고 싶을 경우 (만일의 경우를 대비해 작성, 추후 삭제 요망)
//mainbut의 inspector에서 Sprite Renderer -> Sprite 클릭 후 원하는 이미지 선택