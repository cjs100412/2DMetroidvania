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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.GameStart);
        }
    }

    public void OnControlKeyButtonEnter()
    {
        if (ControlKeyMenu.activeSelf == true)
        {
            ControlKeyMenu.SetActive(false);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.UIExit);
            }
        }
        else
        {
            ControlKeyMenu.SetActive(true);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.UIPopup);
            }
        }
    }

    public void OnControlCancelButtonEnter()
    {
        ControlKeyMenu.SetActive(false);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.UIExit);
        }
    }

    public void OnOptionKeyButtonEnter()
    {
        if (OptionKeyMenu.activeSelf == true)
        {
            OptionKeyMenu.SetActive(false);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.UIExit);
            }
        }
        else
        {
            OptionKeyMenu.SetActive(true);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.UIPopup);
            }
        }
    }

    public void OnOptionCancelButtonEnter()
    {
        OptionKeyMenu.SetActive(false);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.UIExit);
        }
    }

    public void OnExitKeyButtonEnter()
    {
        if (ExitKeyMenu.activeSelf == true)
        {
            ExitKeyMenu.SetActive(false);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.UIExit);
            }
        }
        else
        {
            ExitKeyMenu.SetActive(true);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.UIPopup);
            }
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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.UIExit);
        }
    }
}