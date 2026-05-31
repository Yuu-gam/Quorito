using UnityEngine;

public class EndButton : MonoBehaviour
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

        //유니티 에디터 내에서 종료
        UnityEditor.EditorApplication.isPlaying = false;
        //실제 프로그램 종료
        Application.Quit();
    }

    private void ResetToNormalState()
    {
        spriteRenderer.sprite = normalSprite;
        transform.localScale = normalScale;
    }
}