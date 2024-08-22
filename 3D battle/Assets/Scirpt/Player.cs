using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{

    float hAxis;
    float vAxis;
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapon;

    public GameManager manager;
    public Camera followCamera;

    GameObject nearObject;
    public Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float aDelay;

    public int HP;
    public int coin;
    public int score;
    public int ammo;

    public int maxHP;
    public int maxCoin;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool aDown;
    bool rDown;

   
    bool isSwap;
    bool isJump;
    bool isDodge;
    bool isAttackReady;
    bool isBorder;
    bool isDamage;
    bool isShop;
    bool isDead;
    bool isReload;
    bool isFireReady;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rid;
    Animator anim;
    MeshRenderer[] meshs;

    void Awake()
    {
        rid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        isDamage = false;

        PlayerPrefs.SetInt("MaxScore", 112500);
    }
   

    // Update is called once per frame
    void Update()
    {
        GetInput();
        move();
        turn();
        Jump();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interaction();
        StopToWall();
    }
    void GetInput() 
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        aDown = Input.GetButtonDown("Fire1");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        rDown = Input.GetButton("Reload");
    }

    void move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }
        if (isDead) 
        {
            moveVec = Vector3.zero;
        }
        if (!isBorder) 
        {
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        }
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    void turn()
    {   //#키보드 회전
        transform.LookAt(transform.position + moveVec);

        //# 마우스회전
        if (aDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }
    void Jump() 
    {
        if (jDown && moveVec == Vector3.zero &&isJump ==false && !isDodge && !isShop && !isDead) 
        {
            rid.AddForce(Vector3.up *10, ForceMode.Impulse);
            isJump = true;
            anim.SetBool("isJump", isJump);
            anim.SetTrigger("doJump"); 
            isJump = true;

          
        }
    }
    void Attack() 
    {
        if (equipWeapon == null) return;
        aDelay += Time.deltaTime;
        isAttackReady = equipWeapon.rate < aDelay;
        if (aDown && isAttackReady && !isDodge && !isSwap && !isJump && !isShop && !isDead) 
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            aDelay = 0;
        }
    }

    void Reload() 
    {
        if (equipWeapon == null || equipWeapon.type == Weapon.Type.Melee || ammo <= 0)
            return;

        if (rDown && !isJump && !isDodge && !isSwap && !isReload) 
        {
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 1.0f);
        }
        
    }

    void ReloadOut() 
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }
    void Dodge() 
    {
        if(jDown && moveVec != Vector3.zero &&!isJump && !isDodge && !isShop &&!isDead) 
        {
            dodgeVec = moveVec;
            speed *= 2.0f;
            anim.SetTrigger("isDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor")) 
        {
            isJump = false;
            anim.SetBool("isJump", isJump);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Shop") 
        {
            nearObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
        else if(other.tag == "Shop") 
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            nearObject = null;
            isShop = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin >= maxCoin)
                    {
                        coin = maxCoin;
                    }
                    break;
                case Item.Type.Heart:
                    HP += item.value;
                    if (HP >= maxHP)
                    {
                        HP = maxHP;
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                HP -= enemyBullet.damage;
                
                StartCoroutine(OnDamage());
            }
            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }
    IEnumerator OnDamage()
    {
        isDamage = true;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        if (HP <= 0 && !isDead)
            OnDie();
        yield return new WaitForSeconds(0.5f);
        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

    }
    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    void Interaction()
    {
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapon[weaponIndex] = true;

                Destroy(nearObject);
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
            
        }
    }
    void Swap() 
    {
        if (sDown1 && (!hasWeapon[0] || equipWeaponIndex == 0)) 
        {
            return;
        }
        if (sDown2 && (!hasWeapon[1] || equipWeaponIndex ==1))
        {
            return;
        }
        if (sDown3 && (!hasWeapon[2] || equipWeaponIndex == 2))
        {
            return;
        }

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;
        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge) 
        {
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);
            anim.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut", 0.3f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }
    void StopToWall() 
    {
        Debug.DrawRay(transform.position, transform.forward*5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5,LayerMask.GetMask("Wall"));
    }

}
