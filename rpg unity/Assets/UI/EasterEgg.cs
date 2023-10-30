using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EasterEgg : MonoBehaviour
{
    // Start is called before the first frame update
    public string commandInput = "";
    string command = "UUDDLRLRBA";
    float timer = 0f;
    public GameObject konamiCommand;

    public GameObject acceptPromotionButton;
    public GameObject rejectPromotionButton;
    public GameObject conversationPanel;
    public TMP_Text masterMent;
    public GameObject myCharacter;
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1f)
        {
            if (commandInput.Length > 0)
                commandInput = commandInput.Remove(0, 1);
            timer = 0f;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            commandInput += "U";
        }
        else if(Input.GetKeyUp(KeyCode.DownArrow))
        {
            commandInput += "D";
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            commandInput += "L";
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            commandInput += "R";
        }
        else if(Input.GetKeyUp(KeyCode.A))
        {
            commandInput += "A";
        }
        else if(Input.GetKeyUp(KeyCode.B))
        {
            commandInput += "B";
        }
        if(commandInput.Length > 10)
        {
            commandInput = commandInput.Remove(0, 1);
        }
        if(commandInput == command)
        {
            konamiCommand.SetActive(true);
            commandInput = "";
        }
    }
    
    public void CloseEasterEgg()
    {
        konamiCommand.SetActive(false);
    }

    public void actiaveCheat()
    {
        DataBase.Instance.usingCheat = true;
        myCharacter.GetComponent<MultyPlayer>().characterState.setUp();
        konamiCommand.SetActive(false);
    }

    public void closePromotion()
    {
        if(!DataBase.Instance.isPromotioned)
        {
            acceptPromotionButton.SetActive(true);
            rejectPromotionButton.SetActive(true);
            masterMent.text = "전직할 생각이 생겼나?";
        }
        conversationPanel.SetActive(false);
    }
    public void rejectPromotion()
    {
        acceptPromotionButton.SetActive(false);
        rejectPromotionButton.SetActive(false);
        masterMent.text = "이럴수가!";
        
    }
    public void acceptPromotion()
    {
        if (DataBase.Instance.selectedCharacterSpec.roll == "궁수")
        {
            DataBase.Instance.selectedCharacterSpec.roll = "보우 마스터";
        }
        else if (DataBase.Instance.selectedCharacterSpec.roll == "전사")
        {
            DataBase.Instance.selectedCharacterSpec.roll = "소드 마스터";
        }
        else if (DataBase.Instance.selectedCharacterSpec.roll == "마법사")
        {
            DataBase.Instance.selectedCharacterSpec.roll = "매직 마스터";
        }
        foreach (string skill in myCharacter.GetComponent<MultyPlayer>().characterState.characterSpec.skillLevel.SD_Keys)
        {
            DataBase.Instance.selectedCharacterSpec.skillLevel[skill] += 4;
        }
        acceptPromotionButton.SetActive(false);
        rejectPromotionButton.SetActive(false);
        masterMent.text = "스킬창(K)을 열어 스킬 레벨과 파티창(P)의 직업을 확인해보게!";
        myCharacter.GetComponent<MultyPlayer>().characterState.setUp();
        UIManager.Instance.ResetSkillPanel();
        UIManager.Instance.UpdateSkillPanel();
        DataBase.Instance.isPromotioned = true;
    }
}
