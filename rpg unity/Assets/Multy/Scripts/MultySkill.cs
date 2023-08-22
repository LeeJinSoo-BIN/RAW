using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MultySkill : MonoBehaviourPunCallbacks
{
    void Start() => Destroy(gameObject, 3.5f);
}
