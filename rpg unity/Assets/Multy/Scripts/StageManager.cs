using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;

public class StageManager : MonoBehaviourPunCallbacks//, IPunObservable
{
    public int stage;
    public static bool active = false;
    public static float stageTime;

    public TMP_Text stageText;
    public TMP_Text timeText;
    public static float LimitTime;
    public PhotonView PV;
    public GameObject timeLimitGameOver;

    private Color failColor = new Color((94f / 255f), 0, 0);
    private Color succesColor = new Color(0, (94f / 255f), 0);
    void Start()
    {
        
    }

    void Update()
    {
        if (active)
        {
            stageTime += Time.deltaTime;
            if (LimitTime > 0 && stageTime >= LimitTime && PhotonNetwork.IsMasterClient)
                PV.RPC("TimeLimitGame", RpcTarget.All);
        }        
        timeText.text = string.Format("{0:00}:{1:00}:{2:00}", (int)stageTime / 3600, (int)stageTime / 60 % 60, (int)stageTime % 60);
    }
    [PunRPC]
    void TimeLimitGame()
    {
        active = false;        
        GameObject players = GameObject.Find("Player Group");
        GameObject monsters = GameObject.Find("Enemy Group");
        foreach (Transform p in players.transform)
        {
            p.GetComponent<MultyPlayer>().isDeath = true;
        }
        foreach (Transform monster in monsters.transform)
        {
            monster.GetComponent<MonsterControl>().attackable = false;
            if (monster.GetComponent<MonsterControl>().monsterSpec.monsterType.ToLower() == "boss")
                timeLimitGameOver.transform.GetChild(2).GetComponent<TMP_Text>().text = "소요 시간\n" + LimitTime.ToString() + "초\n" + "남은 체력\n" + monster.GetComponent<MonsterState>().health.value.ToString();
        }
        timeLimitGameOver.SetActive(true);
    }

    [PunRPC]
    void gameOver()
    {

    }
}
