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
    
    public GameObject skillKeyUI;
    public Dictionary<string, string> skillNameToKey = new Dictionary<string, string>();
    public void setUp()
    {
        myCharacterState = myCharacter.GetComponentInChildren<CharacterState>();

        characterHealth = myCharacterState.health;
        characterMana = myCharacterState.mana;

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

