using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager instance = null; // Singleton
    private void Awake() // Singleton
    {
        if (instance == null) instance = this;
        else
        {
            if (instance != this) Destroy(this.gameObject);
        }
    }

    public GameObject MainCamera;
    public GameObject HouseButton;
    bool OnHouse; // 카메라가 하우스에 있는지 체크 Boolean
    public GameObject ScoreButton;
    bool OnScore; // 카메라가 스코어보드에 있는지 체크 Boolean
    public GameObject NextButton;
    bool NowNextButton;
    public GameObject ScoreInfo;
    public GameObject SliderPower;
    bool NowSliderPower; //현재 파워 슬라이더가 보이는 상태인지 아닌지
    public GameObject SliderTorque;
    bool NowSliderTorque; //현재 토크 슬라이더가 보이는 상태인지 아닌지
    public GameObject GuideLine;
    public GameObject MiniMap;
    LineRenderer GLRenderer;
    Vector3 GLEndVector;
    public GameObject ReplayButton;
    bool NowReplayButton;
    bool ReplayMode;
    public GameObject NowSweeper;
    public GameObject RedSweeper1;
    public GameObject BlueSweeper1;

    GameObject[,] ScoreGO = new GameObject[2, 10];
    public GameObject[,] LeftStones = new GameObject[2, 8];
    public GameObject NowRound;

    // Use in UI ScoreBoard 
    string[] UIrounds = new string[10] { "Total", "1", "2", "3", "4", "5", "6", "7", "8", "EE" };
    string[] UIRoundteamnames = new string[2] { "RedTeamScoreRound", "BlueTeamScoreRound" };
    string[] UILeftteamnames = new string[2] { "RedLeft", "BlueLeft" };

    void Start()
    {
        HouseButton.GetComponentInChildren<Text>().text = "SEE HOUSE(Q)";
        OnHouse = false;
        ScoreButton.GetComponentInChildren<Text>().text = "SCORE(W)";
        OnScore = false;
        NextButton.SetActive(false);
        SliderTorque.SetActive(false);
        GLRenderer = GuideLine.GetComponent<LineRenderer>();
        GLRenderer.SetPositions(new[] { new Vector3(0, -2065f, 0), new Vector3(0, -1905f, 0) });
        ReplayButton.GetComponentInChildren<Text>().text = "Replay\n(S)";
        ReplayButton.SetActive(false);
        ReplayMode = false;
        NowSweeper = null;
        RedSweeper1.SetActive(false);
        BlueSweeper1.SetActive(false);

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                ScoreGO[i, j] = GameObject.Find(UIRoundteamnames[i] + UIrounds[j]);
                ScoreGO[i, j].GetComponent<Text>().text = "0";
            }
        }
        ScoreInfo.SetActive(false);
        for (int i = 0; i < 2; i++)
            for(int j = 0; j < 8; j++)
                LeftStones[i, j] = GameObject.Find(UILeftteamnames[i] + (j + 1).ToString());
        NowRound.SetActive(true);
    }

    void Update()
    {
        GLEndVector = GLRenderer.GetPosition(1);
        if (Input.GetKey(KeyCode.LeftArrow) && GLEndVector.x > -25f)
            GLRenderer.SetPosition(1, GLEndVector + new Vector3(-0.8f, 0, 0));
        if (Input.GetKey(KeyCode.RightArrow) && GLEndVector.x < 25f)
            GLRenderer.SetPosition(1, GLEndVector + new Vector3(0.8f, 0, 0));

        if (NowSweeper != null)
            NowSweeper.transform.position = new Vector2(GameManager.instance.CurStone.transform.position.x + 50f, GameManager.instance.CurStone.transform.position.y);
    }

    public void CameraChangeHouse() // 스톤 <--> 하우스
    {
        if (!OnHouse) // 스톤 -> 하우스
        {
            MainCamera.GetComponent<CameraController>().MoveCameraToHouse();
            HouseButton.GetComponentInChildren<Text>().text = "SEE STONE(Q)";

            NowNextButton = NextButton.activeSelf;
            NowReplayButton = ReplayButton.activeSelf;
            ScoreButton.SetActive(false);
            OnHouse = true;
        }
        else // 하우스 -> 스톤
        {
            MainCamera.GetComponent<CameraController>().MoveCameraToStone();
            HouseButton.GetComponentInChildren<Text>().text = "SEE HOUSE(Q)";

            NextButton.SetActive(NowNextButton);
            ReplayButton.SetActive(NowReplayButton);
            ScoreButton.SetActive(true);
            OnHouse = false;
        }
    }

    public void CameraChangeScore() // 스톤 <--> 점수판
    {
        if (!OnScore) // 스톤 -> 점수판
        {
            MainCamera.GetComponent<CameraController>().MoveCameraToScore();
            ScoreButton.SetActive(true);
            ScoreButton.GetComponentInChildren<Text>().text = "BACK(W)";
            OnScore = true;
            // 버튼 누르기 전 상태 저장
            NowSliderPower = SliderPower.activeSelf; 
            NowSliderTorque = SliderTorque.activeSelf;

            HouseButton.SetActive(false);
            MiniMap.SetActive(false);
            SliderPower.SetActive(false);
            SliderTorque.SetActive(false);
            GameManager.instance.CurStone.GetComponent<StoneController>().enabled = false; // 스코어를 볼때 스톤움직임 비활성화
            ScoreInfo.SetActive(true);
        }
        else // 점수판 -> 스톤
        {
            MainCamera.GetComponent<CameraController>().MoveCameraToStone();
            ScoreButton.GetComponentInChildren<Text>().text = "SCORE(W)";
            OnScore = false;

            HouseButton.SetActive(true);
            MiniMap.SetActive(true);
            SliderPower.SetActive(NowSliderPower);
            SliderTorque.SetActive(NowSliderTorque);
            GameManager.instance.CurStone.GetComponent<StoneController>().enabled = true;
            ScoreInfo.SetActive(false);
            if (GameManager.instance.StoneNum == 17) GameManager.instance.StoneNum = 18; // 라운드끝났을땐 초기화
        }
    }

    
    public void ProceedGame() // NextButton클릭시 동작
    {
        GameManager.instance.NextStoneNum();
        GameManager.instance.StopDelivery();
        MainCamera.GetComponent<CameraController>().SetNowStone();
        NextButton.SetActive(false);
        ReplayButton.SetActive(false);
        SliderPower.SetActive(true);
    } 

    // GuideLine                                     
    public void GuideLineInit() { GLRenderer.SetPosition(1, new Vector3(0, -1905f, 0)); }
    public Vector3 GetGuideLineVector() { return GLRenderer.GetPosition(1) - GLRenderer.GetPosition(0); }
    public Vector3 GetGLEndVector() { return GLEndVector; }
    public void SetGuideLine(Vector2 LastGuideLine) { GLRenderer.SetPosition(1, LastGuideLine); }

    public void WatchReplay() // ReplayButton클릭시 동작
    {
        if (!ReplayMode)
        {
            ReplayMode = true;
            ReplayButton.GetComponentInChildren<Text>().text = "Back\n(S)";
            NextButton.SetActive(false);
            GameManager.instance.DoReplay();
        }
        else
        {
            ReplayMode = false;
            ReplayButton.GetComponentInChildren<Text>().text = "Replay\n(S)";
            ReplayButton.SetActive(false);
            SliderPower.GetComponent<Slider>().value = 0;
            SliderTorque.GetComponent<Image>().fillAmount = 0;
            GameManager.instance.DoReplay();
        }
    }
    
    public void WriteScore(int curRound, int winTeamNum, int score) // ScoreInfo UI에 점수입력
    {
        ScoreInfo.SetActive(true);
        ScoreGO[winTeamNum, curRound].GetComponent<Text>().text = score.ToString();
        ScoreInfo.SetActive(false);
    }

    // Sweeping
    public void SweeperOn()
    {
        switch (GameManager.instance.GetNowTeam())
        {
            case 0:
                NowSweeper = RedSweeper1;
                break;
            case 1:
                NowSweeper = BlueSweeper1;
                break;
        }
        NowSweeper.SetActive(true);
    }
    public void SweeperOff() 
    { 
        NowSweeper.SetActive(false);
        NowSweeper = null;
    }
    public void SweepingLeft() { if(NowSweeper.activeSelf) NowSweeper.GetComponent<Animator>().SetBool("Broom", true); }
    public void SweepingRight() { if (NowSweeper.activeSelf) NowSweeper.GetComponent<Animator>().SetBool("Broom", false); }
}
