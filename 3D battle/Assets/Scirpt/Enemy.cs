
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum Type {normal, charge, throwing, boss }

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public int score;
    public GameManager manager;

    public bool isChase;
    public bool isAttack;
    public bool isDie;

    public Type type;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coin;

    public MeshRenderer[] meshs;
    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public NavMeshAgent nav;
    public Animator anim;
    
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (type !=Type.boss)
        {
            Invoke("ChaseStart", 2.0f);
        }
    }

    void ChaseStart() 
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    private void Update()
    {
        if (nav.enabled && type != Type.boss)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    private void FixedUpdate()
    {
        FreezeVelocity();
        Targeting();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee") 
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Debug.Log("attack");
            StartCoroutine(OnDamage(reactVec));
        }
        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Debug.Log("ÃÑ¾ËÇÇ°Ý");
            StartCoroutine(OnDamage(reactVec));
        }
    }
    IEnumerator OnDamage(Vector3 vec) 
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.4f);
        if (curHealth > 0)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
            vec = vec.normalized;
            vec += Vector3.up;
            rigid.AddForce(vec * 5, ForceMode.Impulse);
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;
            gameObject.layer = 9;
            isDie = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");
            Player player = target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 1);
            Instantiate(coin[ranCoin], transform.position, Quaternion.identity);

            switch (type) 
            {
                case Type.normal:
                    manager.enemyCntA--;
                    break;
                case Type.charge:
                    manager.enemyCntB--;
                    break;
                case Type.throwing:
                    manager.enemyCntC--;
                    break;
                case Type.boss:
                    manager.enemyCntD--;
                    break;
            }

            Destroy(gameObject, 3);
        }
    }
    void Targeting()
    {
        if (type != Type.boss && !isDie)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (type)
            {
                case Type.normal:
                    targetRadius = 1.5f;
                    targetRange = 3.0f;
                    break;
                case Type.charge:
                    targetRadius = 1.0f;
                    targetRange = 10.0f;
                    break;
                case Type.throwing:
                    targetRadius = 0.5f;
                    targetRange = 20.0f;
                    break;
            }
            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius,
                transform.forward, targetRange, LayerMask.GetMask("Player"));
            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }
    IEnumerator Attack() 
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (type) 
        {
            case Type.normal:
                yield return new WaitForSeconds(0.7f);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(1.0f);
                meleeArea.enabled = false;
                yield return new WaitForSeconds(1.0f);
                break;
            case Type.charge: 
                yield return new WaitForSeconds(0.3f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                yield return new WaitForSeconds(2.0f);
                break;
            case Type.throwing:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 15.0f;
                yield return new WaitForSeconds(2.0f);
                break;
        }
        isAttack = false;
        isChase = true;
        anim.SetBool("isAttack", false);
    }
    void FreezeVelocity() 
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }
}
