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
    public void setUp()
    {
        myCharacterState = myCharacter.GetComponentInChildren<CharacterState>();
        characterHealth = myCharacterState.health;
        uiHealth.maxValue = characterHealth.maxValue;
        StartCoroutine(update_health());
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

