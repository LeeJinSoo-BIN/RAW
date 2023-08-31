using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class StageManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public int stage;
    public static bool active = false;
    public float stageTime;

    public TMP_Text stageText;
    public TMP_Text timeText;

    void Start()
    {
        
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && active)
        {
            stageTime += Time.deltaTime;
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", (int)stageTime / 3600, (int)stageTime / 60 % 60, (int)stageTime % 60);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(stageTime);
        }
        else
        {
            stageTime = (float)stream.ReceiveNext();
        }
    }
}
