using System.Collections;
using System.Collections.Generic;
using easyInputs;
using Photon.Pun;
using UnityEngine;

public class Gun : MonoBehaviourPun
{
    [Header("MADE BY FROGGY | Modified BY SONORIAL")]
    public Rigidbody Bullet;
    public float Force;
    public Transform BulletTransform;
    public EasyHand Hand;
    public float TimeInScene;
    public AudioSource Shootsound;
    public PhotonView MyView;

    private void Update()
    {
        if (MyView.IsMine && EasyInputs.GetTriggerButtonDown(Hand))
        {
            MyView.RPC("Shoot", RpcTarget.All);
        }
    }

    [PunRPC]
    void Shoot()
    {
        Shootsound.Play();
        Rigidbody bulletInstance = Instantiate(Bullet, BulletTransform.position, BulletTransform.rotation);
        bulletInstance.AddForce(transform.forward * Force);
        StartCoroutine(TImeInscene(bulletInstance.gameObject));
    }

    IEnumerator TImeInscene(GameObject bullet)
    {
        yield return new WaitForSeconds(TimeInScene);
        Destroy(bullet);
    }
}
