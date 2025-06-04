using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause UI Panel")]
    [Tooltip("ESC ������ �� ���� UI Panel")]
    public GameObject pausePanel;

    bool isPaused = false;

    void Start()
    {
        // ������ �� �ݵ�� ���α�
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        // ESC Ű ���� ������ ���
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void OnResumeButton()
    {
        ResumeGame();
    }

    void PauseGame()
    {
        // �ð� ���߱�
        Time.timeScale = 0f;
        // ���� ������Ʈ�� ���߰� ������ (�⺻ FixedDeltaTime ������ �ڵ� ó����)
        // Audio�� �Բ� ���߱�
        //AudioListener.pause = true;
        // UI ���̱�
        if (pausePanel != null)
            pausePanel.SetActive(true);

        isPaused = true;
    }

    void ResumeGame()
    {
        // �ð� �帣��
        Time.timeScale = 1f;
        // Audio�� ���
        //AudioListener.pause = false;
        // UI �����
        if (pausePanel != null)
            pausePanel.SetActive(false);

        isPaused = false;
    }

    public void OnTitleButton()
    {
        pausePanel.SetActive(false);
        SceneManager.LoadScene("TitleScene");
    }
    public void OnExitButtonEnter()
    {
#if UNITY_EDITOR // ����Ƽ ������ �ʿ����� �۾�
        UnityEditor.EditorApplication.isPlaying = false;
        //������ �ٷ� ������ ���(�����, �����)
#else
        Application.Quit(); // ���� ��Ȱ��ȭ�Ǵ� �ڵ尡 �ٷ� ����
#endif
    }
}