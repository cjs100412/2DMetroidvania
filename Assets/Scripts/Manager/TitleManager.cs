using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    //컨트롤 버튼을 통해서 오픈해줄 메뉴
    public GameObject ControlKeyMenu;
    //종료 안내용 메뉴
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

    //에디터 쪽 환경에서의 종료와
    //빌드 쪽 환경에서의 종료를 상황에 따라 처리합니다.
    public void OnExitButtonEnter()
    {
#if UNITY_EDITOR // 유니티 에디터 쪽에서의 작업
        UnityEditor.EditorApplication.isPlaying = false;
        //누르면 바로 꺼지는 기능(모바일, 빌드용)
#else
        Application.Quit(); // 현재 비활성화되는 코드가 바로 적용
#endif
    }
    public void OnExitCancelButtonEnter()
    {
        ExitKeyMenu.SetActive(false);
    }
}