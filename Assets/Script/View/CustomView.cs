using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI UIDText;
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] Button backButton;
    [SerializeField] Button usernameChangeButton;

    public Button BackButton => backButton;
    public Button UsernameChangeButton => usernameChangeButton;
    public TMP_InputField UsernameInputField => usernameInputField;

    public void Init(string uid, string username)
    {
        SetUIDText(uid);
        SetUsername(username);

        gameObject.SetActive(true);
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }

    public void SetUIDText(string str)
    {
        UIDText.text = str;
    }

    public void SetUsername(string str)
    {
        usernameInputField.placeholder.GetComponent<TextMeshProUGUI>().text = str;
    }
}
