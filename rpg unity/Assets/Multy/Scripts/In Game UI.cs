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
    public Slider uiBossHealth;
    public Slider bossHealth;
    public TMP_Text bossMaxHealthText;
    public TMP_Text bossCurrentHealthText;
    private bool bossConnected;
    public MonsterState bossState;

    public GameObject BossStateUI;
    public GameObject BossSpawnButton;

    public GameObject CharacterProfile;
    
    public GameObject skillKeyUI;
    public Dictionary<string, string> skillNameToKey = new Dictionary<string, string>();
    private string skillThumbnailPath = "Character/skills/thumbnails";
    private List<string> quickSlotKeys = new List<string> { "1", "2", "3", "4" };
    public Dictionary<string, string> quickSlotItems = new Dictionary<string, string>();

    public Dictionary<string, int> carringItems;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            string now_input_key = Input.inputString;
            useQuickSlot(now_input_key);
        }
    }


    public void setUp()
    {
        myCharacterState = myCharacter.GetComponentInChildren<CharacterState>();
        makeProfile();
        characterHealth = myCharacterState.health;
        characterMana = myCharacterState.mana;        
        quickSlotItems.Clear();
        for(int k = 0; k < quickSlotKeys.Count; k++)
        {
            quickSlotItems.Add(quickSlotKeys[k], "");
        }

        setKeyMap();

        if (GameObject.Find("Enemy Group").transform.childCount > 0)
            BossSpawnButton.SetActive(false);
        
        StartCoroutine(update_health());
    }

    public void BossSetUp()
    {
        Boss = GameObject.Find("Enemy Group").transform.GetChild(0).gameObject;
        bossState = Boss.GetComponentInChildren<MonsterState>();
        bossHealth = bossState.health;
        uiBossHealth.maxValue = bossHealth.maxValue;
        bossConnected = true;
        BossStateUI.SetActive(true);
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
    public void CoolDown(string key, float coolingTime)
    {
        StartCoroutine(CoolDownCoroutine(key, coolingTime));
    }
    IEnumerator CoolDownCoroutine(string skill_name, float coolingTime)
    {
        string key = skillNameToKey[skill_name];
        Image skill_cool = skillKeyUI.transform.Find(key.ToLower()).GetChild(1).GetComponent<Image>();
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
        myCharacterHead.transform.parent = CharacterProfile.transform;
        myCharacterHead.transform.localPosition = Vector3.zero;
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
                currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(Path.Combine(skillThumbnailPath, skillNames[k]));
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = keys[k];
                currentSlot.GetChild(3).GetComponent<TMP_Text>().text = GameManager.Instance.skillInfoDict[skillNames[k]].consumeMana.ToString();
                StartCoroutine(CoolDownCoroutine(skillNames[k], 0f));
            }
        }
        setQuickSlot("1", "red potion small");
        setQuickSlot("2", "blue potion small");
    }
    
    void useQuickSlot(string key)
    {
        if (carringItems.ContainsKey(quickSlotItems[key]))
        {
            if (carringItems[quickSlotItems[key]] > 0)
            {
                carringItems[quickSlotItems[key]]--;
            }
            consumePotion(quickSlotItems[key]);
            myCharacter.GetComponent<MultyPlayer>().updateInventory();
            updateThisQuickSlot(key);
        }
    }
    public void updateAllQuickSlot(bool updateSprite = false)
    {
        for(int k = 0; k < quickSlotKeys.Count; k++)
        {
            Transform currentSlot = skillKeyUI.transform.Find(quickSlotKeys[k].ToLower());
            if(updateSprite)
                currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(GameManager.Instance.itemInfoDict[quickSlotItems[quickSlotKeys[k]]].spriteDirectory);
            if (carringItems.ContainsKey(quickSlotItems[quickSlotKeys[k]]))
            {
                Debug.Log(quickSlotItems[quickSlotKeys[k]] + " " + carringItems[quickSlotItems[quickSlotKeys[k]]]);
                currentSlot.GetChild(0).GetComponent<Image>().color = Color.white;
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = carringItems[quickSlotItems[quickSlotKeys[k]]].ToString();
            }
            else
            {
                Debug.Log(quickSlotItems[quickSlotKeys[k]] + " 0");
                currentSlot.GetChild(0).GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "0";
            }
        }
    }
    void updateThisQuickSlot(string key, bool updateSprtie = false)
    {
        Transform currentSlot = skillKeyUI.transform.Find(key);
        if(updateSprtie)
            currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(GameManager.Instance.itemInfoDict[quickSlotItems[key]].spriteDirectory);        
        if (carringItems.ContainsKey(quickSlotItems[key]))
        {
            Debug.Log(quickSlotItems[key] + " " + carringItems[quickSlotItems[key]]);
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.white;
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = carringItems[quickSlotItems[key]].ToString();
        }
        else
        {
            Debug.Log(quickSlotItems[key] + " 0");
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.gray;
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "0";
        }
    }
    
    void setQuickSlot(string key, string itemName)
    {
        quickSlotItems[key] = itemName;
        updateThisQuickSlot(key, true);
    }


    void consumePotion(string itemName)
    {
        myCharacterState.ProcessSkill(1, GameManager.Instance.itemInfoDict[itemName].recoveryHealth);
        myCharacterState.ProcessSkill(5, GameManager.Instance.itemInfoDict[itemName].recoveryMana);

    }


}


