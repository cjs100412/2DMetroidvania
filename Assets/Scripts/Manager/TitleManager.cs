using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    //��Ʈ�� ��ư�� ���ؼ� �������� �޴�
    public GameObject ControlKeyMenu;
    //���� �ȳ��� �޴�
    public GameObject ExitKeyMenu;
    public GameObject OptionKeyMenu;

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGM.Title);
        }
    }

    public void OnStartButtonEnter()
    {
        SceneManager.LoadScene("Bootstrap");
    }

    public void OnControlKeyButtonEnter()
    {
        if (ControlKeyMenu.activeSelf == true)
        {
            ControlKeyMenu.SetActive(false);
        }
        else
        {
            ControlKeyMenu.SetActive(true);

        }
    }

    public void OnControlCancelButtonEnter()
    {
        ControlKeyMenu.SetActive(false);
    }

    public void OnOptionKeyButtonEnter()
    {
        if (OptionKeyMenu.activeSelf == true)
        {
            OptionKeyMenu.SetActive(false);
        }
        else
        {
            OptionKeyMenu.SetActive(true);
        }
    }

    public void OnOptionCancelButtonEnter()
    {
        OptionKeyMenu.SetActive(false);
    }

    public void OnExitKeyButtonEnter()
    {
        if (ExitKeyMenu.activeSelf == true)
        {
            ExitKeyMenu.SetActive(false);
        }
        else
        {
            ExitKeyMenu.SetActive(true);
        }
    }   

    //������ �� ȯ�濡���� �����
    //���� �� ȯ�濡���� ���Ḧ ��Ȳ�� ���� ó���մϴ�.
    public void OnExitButtonEnter()
    {
#if UNITY_EDITOR // ����Ƽ ������ �ʿ����� �۾�
        UnityEditor.EditorApplication.isPlaying = false;
        //������ �ٷ� ������ ���(�����, �����)
#else
        Application.Quit(); // ���� ��Ȱ��ȭ�Ǵ� �ڵ尡 �ٷ� ����
#endif
    }
    public void OnExitCancelButtonEnter()
    {
        ExitKeyMenu.SetActive(false);
    }
}