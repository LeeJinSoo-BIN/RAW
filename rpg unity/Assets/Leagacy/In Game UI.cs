using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Rendering;

public class InGameUI : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject myCharacter;
    private CharacterState myCharacterState;

    public Slider uiHealth;    
    private Slider characterHealth;
    public TMP_Text maxHealthText;
    public TMP_Text currentHealthText;

    public Slider uiMana;
    private Slider characterMana;
    public TMP_Text maxManaText;
    public TMP_Text currentManaText;

    public GameObject Boss;
    public MonsterState bossState;
    public Slider bossHealth;

    public GameObject BossStateUI;
    public Slider uiBossHealth;    
    public TMP_Text bossMaxHealthText;
    public TMP_Text bossCurrentHealthText;
    private bool bossConnected;    

    
    public GameObject StageUI;

    public GameObject CharacterProfile;
    
    public GameObject skillKeyUI;
    public Dictionary<string, string> skillNameToKey = new Dictionary<string, string>();
    
    private List<string> quickSlotKeys = new List<string> { "1", "2", "3", "4" };
    public Dictionary<string, string> keyToItemName = new Dictionary<string, string>();
    
    //public Dictionary<string, qucikInventoryInfo> quickInventory;

    public RectTransform ChatBox;
    public RectTransform ChatExpandButtonIcon;
    public Button ChatExpandButton;

    private void Start()
    {
        BossStateUI.SetActive(false);        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            string now_input_key = Input.inputString;
         //   useQuickSlot(now_input_key);
        }
    }


    public void setUp()
    {
        myCharacterState = myCharacter.GetComponentInChildren<CharacterState>();
        makeProfile();
        characterHealth = myCharacterState.health;
        characterMana = myCharacterState.mana;
        keyToItemName.Clear();
        for(int k = 0; k < quickSlotKeys.Count; k++)
        {
            keyToItemName.Add(quickSlotKeys[k], "");
        }
        setKeyMap();
        StartCoroutine(update_health());
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            BossStateUI.SetActive(true);
            Debug.Log("set boss ui true");
            StageUI.SetActive(true);
        }
        else if (DataBase.Instance.currentMapType == "village")
        {
            BossStateUI.SetActive(false);
            StageUI.SetActive(false);
        }
    }

    public void BossSetUp()
    {
        Boss = GameObject.Find("Enemy Group").transform.GetChild(0).gameObject;
        bossState = Boss.GetComponentInChildren<MonsterState>();
        bossHealth = bossState.health;
        uiBossHealth.maxValue = bossHealth.maxValue;
        bossConnected = true;
        BossStateUI.SetActive(true);
        Debug.Log("set boss ui true");
    }
    
    IEnumerator update_health()
    {
        while (true)
        {
            uiHealth.value = characterHealth.value;
            uiHealth.maxValue = characterHealth.maxValue;
            currentHealthText.text = ((int)uiHealth.value).ToString();
            maxHealthText.text = ((int)uiHealth.maxValue).ToString();

            uiMana.value = characterMana.value;
            uiMana.maxValue = characterMana.maxValue;
            currentManaText.text = ((int)uiMana.value).ToString();
            maxManaText.text = ((int)uiMana.maxValue).ToString();

            if (bossConnected)
            {
                uiBossHealth.value = bossHealth.value;
                uiBossHealth.maxValue = bossHealth.maxValue;                
                bossMaxHealthText.text = ((int)uiBossHealth.maxValue).ToString();
                bossCurrentHealthText.text = ((int)uiBossHealth.value).ToString();                
            }
            yield return null;
        }
    }
    public void CoolDown(string skillName, float coolingTime)
    {
        StartCoroutine(CoolDownCoroutine(skillName, coolingTime));
    }
    IEnumerator CoolDownCoroutine(string skill_name, float coolingTime)
    {
        string key = skillNameToKey[skill_name];
        Transform currentKeyUI = skillKeyUI.transform.Find(key.ToLower());
        if(currentKeyUI == null) yield break;
        Image skill_cool = currentKeyUI.GetChild(1).GetComponent<Image>();
        skill_cool.fillAmount = 100;
        float _time = coolingTime;
        if (coolingTime == 0)        
            skill_cool.fillAmount = 0;
        while (_time >= 0 && coolingTime > 0)
        {
            _time -= Time.deltaTime;
            skill_cool.fillAmount = _time / coolingTime;            
            yield return null;
        }
        
    }
    void makeNewHead(GameObject head)
    {
        SpriteRenderer spriteRenderer = head.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null)
        {            
            head.transform.AddComponent<Image>();
            Image imageComponent = head.GetComponent<Image>();
            imageComponent.color = spriteRenderer.color;
            imageComponent.sprite = spriteRenderer.sprite;
            if(imageComponent.sprite == null)
                imageComponent.gameObject.SetActive(false);
            Destroy(spriteRenderer);
        }
        for (int k = 0; k < head.transform.childCount; k++)
        {
            makeNewHead(head.transform.GetChild(k).gameObject);
        }
    }
    public void makeProfile()
    {
        GameObject myCharacterHead = Instantiate(myCharacter.transform.Find("Root").GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject);
        makeNewHead(myCharacterHead);
        foreach(Transform child in CharacterProfile.transform)
            Destroy(child.gameObject);
        myCharacterHead.transform.parent = CharacterProfile.transform;
        myCharacterHead.transform.localPosition = new Vector3(0f, 1f, 0f);
        CharacterProfile.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = "Lv. " + myCharacterState.characterSpec.characterLevel.ToString();
    }

    public void setKeyMap()
    {
        List<string> keys = skillNameToKey.Values.ToList();
        List<string> skillNames = skillNameToKey.Keys.ToList();
        for(int k = 0; k < skillNameToKey.Count; k++)
        {
            string key = keys[k].ToLower();
            if (key == "q" || key == "w" || key == "e" || key == "r")
            {
                Transform currentSlot = skillKeyUI.transform.Find(key);
                //currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(Path.Combine(DataBase.Instance.skillThumbnailPath, skillNames[k]));
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = keys[k];
                currentSlot.GetChild(3).GetComponent<TMP_Text>().text = DataBase.Instance.skillInfoDict[skillNames[k]].consumeMana.ToString();
                StartCoroutine(CoolDownCoroutine(skillNames[k], 0f));
            }
        }
        //setQuickSlot("1", "red potion small");
        //setQuickSlot("2", "blue potion small");
    }
    /*
    void useQuickSlot(string key)
    {
        if (quickInventory.ContainsKey(keyToItemName[key]))
        {
            if (quickInventory[keyToItemName[key]].count > 0)
            {
                quickInventory[keyToItemName[key]].count--;
            }
            consumePotion(keyToItemName[key]);
            //myCharacter.GetComponent<MultyPlayer>().updateInventory();
            updateThisQuickSlot(key);
        }
        *//*
        for (int k = 0; k < inventory.Count; k++)
        {
            if(inventory[k].itemName == keyToItemName[key])
            {
                if (inventory[k].count > 0)
                {
                    inventory[k].count--;
                    consumePotion(keyToItemName[key]);
                    myCharacter.GetComponent<MultyPlayer>().updateInventory();
                    updateThisQuickSlot(key);
                }
            }
        }*//*
    }
    public void updateAllQuickSlot(bool updateSprite = false)
    {
        for(int k = 0; k < quickSlotKeys.Count; k++)
        {
            Transform currentSlot = skillKeyUI.transform.Find(quickSlotKeys[k].ToLower());
            if(updateSprite)
                currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[keyToItemName[quickSlotKeys[k]]].spriteDirectory);            
            
            if (quickInventory.ContainsKey(keyToItemName[quickSlotKeys[k]]))
            {                
                currentSlot.GetChild(0).GetComponent<Image>().color = Color.white;
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = quickInventory[keyToItemName[quickSlotKeys[k]]].count.ToString();
            }
            else
            {
                currentSlot.GetChild(0).GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "0";
            }
        }
    }
    void updateThisQuickSlot(string key, bool updateSprtie = false)
    {
        Transform currentSlot = skillKeyUI.transform.Find(key);
        if(updateSprtie)
            currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[keyToItemName[key]].spriteDirectory);        
        if (quickInventory.ContainsKey(keyToItemName[key]))
        {
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.white;
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = quickInventory[keyToItemName[key]].count.ToString();
        }
        else
        {            
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.gray;
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "0";
        }
    }
  
    void setQuickSlot(string key, string itemName)
    {
        keyToItemName[key] = itemName;
        updateThisQuickSlot(key, true);
    }
    */

    void consumePotion(string itemName)
    {
        myCharacterState.ProcessSkill(1, DataBase.Instance.itemInfoDict[itemName].recoveryHealth);
        myCharacterState.ProcessSkill(5, DataBase.Instance.itemInfoDict[itemName].recoveryMana);
    }

    public void ClickExpandChatLog()
    {
        Debug.Log("clicked");
        if (ChatBox.sizeDelta.y == 120)
            ChatBox.sizeDelta = new Vector2(ChatBox.sizeDelta.x, 500);
        else
            ChatBox.sizeDelta = new Vector2(ChatBox.sizeDelta.x, 120);
        ChatExpandButtonIcon.localScale = new Vector3(ChatExpandButtonIcon.localScale.x, -ChatExpandButtonIcon.localScale.y, 1);        
    }

}


