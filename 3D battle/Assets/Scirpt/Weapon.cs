using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type {Melee, Range };
    public Type type;
    public int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;


    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos;
    public GameObject bulletPrefab;
    public Transform casePos;
    public GameObject casePrefab;

    public void Use() 
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if (type == Type.Range && curAmmo > 0) 
        {

            curAmmo--;
            StartCoroutine("Shot");
        }
    }
    IEnumerator Swing() 
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;
       
        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot() 
    {
        GameObject instantBullet = Instantiate(bulletPrefab, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 40;

        GameObject instantCase = Instantiate(casePrefab, casePos.position, casePos.rotation);
        Rigidbody caseRigid = instantBullet.GetComponent<Rigidbody>();
        Vector3 caseVec = casePos.forward * (Random.Range(-3, -1)) + Vector3.up *Random.Range(2,3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);

        yield return null; ;
    }
}
