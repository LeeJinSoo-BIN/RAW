using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    // Start is called before the first frame update
    private CharacterState myCharacterState;
    public GameObject myCharacter;
    public Slider uiHealth;
    private int maxHealth;
    private int currentHealth;
    private Slider characterHealth;
    public TMP_Text maxHealthText;
    public TMP_Text currentHealthText;
    public void setUp()
    {
        myCharacterState = myCharacter.GetComponentInChildren<CharacterState>();
        Debug.Log(myCharacterState == null);
        Debug.Log(myCharacterState.health);
        characterHealth = myCharacterState.health;
        uiHealth.maxValue = characterHealth.maxValue;
        StartCoroutine(update_health());
    }
    private void Update()
    {
        
    }
    IEnumerator update_health()
    {
        while (true)
        {
            uiHealth.value = characterHealth.value;
            currentHealth = (int)characterHealth.value;
            maxHealth = (int)characterHealth.maxValue;
            maxHealthText.text = maxHealth.ToString();
            currentHealthText.text = currentHealth.ToString();
            yield return null;
        }
    }
}

