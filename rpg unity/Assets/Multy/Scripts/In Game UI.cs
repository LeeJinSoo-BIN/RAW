using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject myCharacter;
    private CharacterState myCharacterState;

    public Slider uiHealth;
    private int maxHealth;
    private int currentHealth;
    private Slider characterHealth;
    public TMP_Text maxHealthText;
    public TMP_Text currentHealthText;

    public Slider uiMana;
    private int maxMana;
    private int currentMana;
    private Slider characterMana;
    public TMP_Text maxManaText;
    public TMP_Text currentManaText;

    public GameObject Boss;
    public Slider uiBossHealth;
    public Slider bossHealth;
    private int bossMaxHealth;
    private int bossCurrentHealth;
    public TMP_Text bossMaxHealthText;
    public TMP_Text bossCurrentHealthText;
    private bool bossConnected;
    public MonsterState bossState;

    public GameObject BossStateUI;
    public GameObject BossSpawnButton;

    public Dictionary<string, float> coolDown;
    public GameObject skillKeyUI;
    public void setUp()
    {
        myCharacterState = myCharacter.GetComponentInChildren<CharacterState>();

        characterHealth = myCharacterState.health;
        uiHealth.maxValue = characterHealth.maxValue;
        characterMana = myCharacterState.mana;
        uiMana.maxValue = characterMana.maxValue;

        if(GameObject.Find("Enemy Group").transform.childCount > 0)
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
            uiMana.value = characterMana.value;
            currentHealth = (int)characterHealth.value;
            currentMana = (int)characterMana.value;
            maxHealth = (int)characterHealth.maxValue;
            maxMana = (int)characterMana.maxValue;
            maxHealthText.text = maxHealth.ToString();
            maxManaText.text = maxMana.ToString();
            currentHealthText.text = currentHealth.ToString();
            currentManaText.text = currentMana.ToString();

            if (bossConnected)
            {
                uiBossHealth.value = bossHealth.value;
                bossCurrentHealth = (int)bossHealth.value;
                bossMaxHealth = (int)bossHealth.maxValue;
                bossMaxHealthText.text = bossMaxHealth.ToString();
                bossCurrentHealthText.text = bossCurrentHealth.ToString();
                
            }
            yield return null;


        }
    }
    public void CoolDown(string key, float coolingTime)
    {
        StartCoroutine(CoolDownCoroutine(key, coolingTime));
    }
    IEnumerator CoolDownCoroutine(string key, float coolingTime)
    {
        skillKeyUI.transform.Find(key.ToLower()).GetChild(2).GetComponent<Image>().fillAmount = 100;
        float _time = coolingTime;
        while (_time >= 0)
        {
            _time -= Time.deltaTime;
            skillKeyUI.transform.Find(key.ToLower()).GetChild(2).GetComponent<Image>().fillAmount = _time / coolingTime;
            
            yield return null;
        }
        
    }
}

