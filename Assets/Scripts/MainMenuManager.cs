using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private Coroutine TextFadeCoroutineSave; //경고문구 페이드아웃

    

    public GameObject mainMenuUi;//메인UI 부모
    public GameObject creditUi;//크레딧UI 부모
    public GameObject exitUi;//종료UI 부모
    public GameObject gameStartOptionUi;//게임 시작 옵션 UI 부모
    public GameObject grayScale;//배경 암전 오브젝트
    public TMP_InputField xInputUi;//게임시작옵션 x값 입력창
    public TMP_InputField yInputUi;//게임시작옵션 y값 입력창
    public TMP_InputField safeAreaInputUi;//게임시작옵션 안전지대 반지름 입력창
    public TMP_InputField bombInputUi;//게임시작옵션 폭탄개수 입력창
    public TMP_Text WorningUi;//경고문구 UI
    public InformationLoader infoLoader;//메인게임으로 정보 전송하는 오브젝트 로직

    public int xInput;//게임시작옵션 x값 toInt
    public int yInput;//게임시작옵션 y값 toInt
    public int bombInput;//게임시작옵션 폭탄개수 toInt
    public int safeAreaInput;//게임시작옵션 안전지대 반지름 toInt
    void Start()
    {
        infoLoader = FindObjectOfType<InformationLoader>();

        grayScale.SetActive(false);
        mainMenuUi.SetActive(true);
        creditUi.SetActive(false);
        exitUi.SetActive(false);
        gameStartOptionUi.SetActive(false);
    }
    
    void Update()
    {
        
    }

    public void CreditButton()//크레딧 코드
    {
        mainMenuUi.SetActive(false);
        creditUi.SetActive(true);
    }

    public void ReturnToMenu()//메뉴로 돌아가기
    {
        grayScale.SetActive(false);
        mainMenuUi.SetActive(true);
        creditUi.SetActive(false);
        exitUi.SetActive(false);
        gameStartOptionUi.SetActive(false);
    }

    public void ExitGame()//게임종료 코드
    {
        grayScale.SetActive(true);
        exitUi.SetActive(true);
    }

    public void RealExit()//진짜 종료
    {
        Application.Quit();
    }

    public void GameStartButton()//게임 시작 버튼
    {
        gameStartOptionUi.SetActive(true);
        grayScale.SetActive(true);
    }
    public void GameEnterButton()//사전정보 입력 완료 및 씬 이동 버튼
    {
        try
        {
            xInput = int.Parse(xInputUi.text);
            yInput = int.Parse(yInputUi.text);
            bombInput = int.Parse(bombInputUi.text);
            safeAreaInput = int.Parse(safeAreaInputUi.text);
        }
        catch(System.FormatException e)
        {
            WorningUi.text = "All of this field is essential item to fill.";
            TextFadeOut(WorningUi, 3f);

        }
        int safeAreaCount = ((safeAreaInput * 2) + 1)*((safeAreaInput * 2) + 1);
        int biggerOne = xInput < yInput? yInput: xInput;

        if (xInput < 1 || yInput < 1) // x나 y input이 0일때
        {
            WorningUi.text = "Game board cant be smaller then 1 tile.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(xInput > 50)//x가 최대치를 넘어설때
        {
            WorningUi.text = "DONT EVEN THINK ABOUT IT.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(yInput > 50)// y가 최대치를 넘어설때
        {
            WorningUi.text = "DONT EVEN THINK ABOUT IT.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(safeAreaInput < 0)// 안전지대를 -로 설정했을때
        {
            WorningUi.text = "Safe zone cant be smaller then 0.";
            TextFadeOut(WorningUi, 3f);
        }
        else if (safeAreaInput > biggerOne)//모든 타일을 안전지대로 덮을때
        {
            WorningUi.text = "Safe zone cant be bigger then game board.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(bombInput < 1)//폭탄이 0과 같거나 그보다 작을 때
        {
            WorningUi.text = "you cant make park in this game.";
            TextFadeOut(WorningUi, 3f);
        }
        else if((xInput*yInput)-safeAreaCount < bombInput)//폭탄이 들어갈 자리가 부족할 때
        {
            WorningUi.text = "bomb cant be many then number of tile.";
            TextFadeOut(WorningUi, 3f);
        }
        else//성공!
        {
            infoLoader.xInput = xInput;
            infoLoader.yInput = yInput;
            infoLoader.bombInput = bombInput;
            infoLoader.safeAreaInput = safeAreaInput;
            SceneManager.LoadScene("MainGameScene");
        }
    }



    public void TextFadeOut(TMP_Text FadeObject, float ActiveTime)//경고문구 
    {
        if (TextFadeCoroutineSave == null)
        {
            TextFadeCoroutineSave = StartCoroutine(TextFadeCoroutine(FadeObject, ActiveTime));
        }
        else
        {
            StopCoroutine(TextFadeCoroutineSave);
            TextFadeCoroutineSave = StartCoroutine(TextFadeCoroutine(FadeObject, ActiveTime));
        }
    }

    public IEnumerator TextFadeCoroutine(TMP_Text FadeObject, float ActiveTime)
    {
        FadeObject.color = new Color(FadeObject.color.r, FadeObject.color.g, FadeObject.color.b, 1);//투명도 1
        yield return new WaitForSeconds(ActiveTime);//activeTime만큼 기다렸다가

        while (FadeObject.color.a > 0.0f)
        {
            FadeObject.color = new Color(FadeObject.color.r, FadeObject.color.g, FadeObject.color.b, FadeObject.color.a - (Time.deltaTime / 2.0f));//서서히 fadeOut
            yield return null;
        }
    }
}
