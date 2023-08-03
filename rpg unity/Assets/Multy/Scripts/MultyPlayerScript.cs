using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MultyPlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public Animator AN;
    public PhotonView PV;
    public Text NickNameText;
    //public Image HealthImage;

    Vector3 curPos;

    private void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
