using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    public Button button;
    public static GameObject door;
    public static string answer;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(ButtonOnClick);
    }

	void ButtonOnClick()
	{
        var option = button.GetComponentInChildren<Text>().text;
        //Debug.Log("[OnClick] " + option);

        if (option == AnswerButton.answer)
        {
            //Debug.Log(door);
            Animator anim = AnswerButton.door.transform.parent.GetComponent<Animator>();
            anim.SetBool("opened", true);
        }

        GameObject.Find("Player").GetComponent<FirstPersonController>().enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        transform.parent.gameObject.SetActive(false);
	}
}
