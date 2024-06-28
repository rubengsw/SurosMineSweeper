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
    private Coroutine TextFadeCoroutineSave; //����� ���̵�ƿ�

    

    public GameObject mainMenuUi;//����UI �θ�
    public GameObject creditUi;//ũ����UI �θ�
    public GameObject exitUi;//����UI �θ�
    public GameObject gameStartOptionUi;//���� ���� �ɼ� UI �θ�
    public GameObject grayScale;//��� ���� ������Ʈ
    public TMP_InputField xInputUi;//���ӽ��ۿɼ� x�� �Է�â
    public TMP_InputField yInputUi;//���ӽ��ۿɼ� y�� �Է�â
    public TMP_InputField safeAreaInputUi;//���ӽ��ۿɼ� �������� ������ �Է�â
    public TMP_InputField bombInputUi;//���ӽ��ۿɼ� ��ź���� �Է�â
    public TMP_Text WorningUi;//����� UI
    public InformationLoader infoLoader;//���ΰ������� ���� �����ϴ� ������Ʈ ����

    public int xInput;//���ӽ��ۿɼ� x�� toInt
    public int yInput;//���ӽ��ۿɼ� y�� toInt
    public int bombInput;//���ӽ��ۿɼ� ��ź���� toInt
    public int safeAreaInput;//���ӽ��ۿɼ� �������� ������ toInt
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

    public void CreditButton()//ũ���� �ڵ�
    {
        mainMenuUi.SetActive(false);
        creditUi.SetActive(true);
    }

    public void ReturnToMenu()//�޴��� ���ư���
    {
        grayScale.SetActive(false);
        mainMenuUi.SetActive(true);
        creditUi.SetActive(false);
        exitUi.SetActive(false);
        gameStartOptionUi.SetActive(false);
    }

    public void ExitGame()//�������� �ڵ�
    {
        grayScale.SetActive(true);
        exitUi.SetActive(true);
    }

    public void RealExit()//��¥ ����
    {
        Application.Quit();
    }

    public void GameStartButton()//���� ���� ��ư
    {
        gameStartOptionUi.SetActive(true);
        grayScale.SetActive(true);
    }
    public void GameEnterButton()//�������� �Է� �Ϸ� �� �� �̵� ��ư
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

        if (xInput < 1 || yInput < 1) // x�� y input�� 0�϶�
        {
            WorningUi.text = "Game board cant be smaller then 1 tile.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(xInput > 50)//x�� �ִ�ġ�� �Ѿ��
        {
            WorningUi.text = "DONT EVEN THINK ABOUT IT.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(yInput > 50)// y�� �ִ�ġ�� �Ѿ��
        {
            WorningUi.text = "DONT EVEN THINK ABOUT IT.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(safeAreaInput < 0)// �������븦 -�� ����������
        {
            WorningUi.text = "Safe zone cant be smaller then 0.";
            TextFadeOut(WorningUi, 3f);
        }
        else if (safeAreaInput > biggerOne)//��� Ÿ���� ��������� ������
        {
            WorningUi.text = "Safe zone cant be bigger then game board.";
            TextFadeOut(WorningUi, 3f);
        }
        else if(bombInput < 1)//��ź�� 0�� ���ų� �׺��� ���� ��
        {
            WorningUi.text = "you cant make park in this game.";
            TextFadeOut(WorningUi, 3f);
        }
        else if((xInput*yInput)-safeAreaCount < bombInput)//��ź�� �� �ڸ��� ������ ��
        {
            WorningUi.text = "bomb cant be many then number of tile.";
            TextFadeOut(WorningUi, 3f);
        }
        else//����!
        {
            infoLoader.xInput = xInput;
            infoLoader.yInput = yInput;
            infoLoader.bombInput = bombInput;
            infoLoader.safeAreaInput = safeAreaInput;
            SceneManager.LoadScene("MainGameScene");
        }
    }



    public void TextFadeOut(TMP_Text FadeObject, float ActiveTime)//����� 
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
        FadeObject.color = new Color(FadeObject.color.r, FadeObject.color.g, FadeObject.color.b, 1);//���� 1
        yield return new WaitForSeconds(ActiveTime);//activeTime��ŭ ��ٷȴٰ�

        while (FadeObject.color.a > 0.0f)
        {
            FadeObject.color = new Color(FadeObject.color.r, FadeObject.color.g, FadeObject.color.b, FadeObject.color.a - (Time.deltaTime / 2.0f));//������ fadeOut
            yield return null;
        }
    }
}
