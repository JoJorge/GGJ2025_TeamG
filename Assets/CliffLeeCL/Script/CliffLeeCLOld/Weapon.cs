using UnityEngine;
using System.Collections;
using System;

public class Weapon : MonoBehaviour {
    public GameObject bulletPrefab;
    public float targetScale = 0.3f, shootForce = 10.0f, attackRate = 0.5f, destroyTime = 5.0f;

    GameObject currentBullet;
    Rigidbody bulletRigidbody;
    //SelfHiding bulletSelfHiding;
    float attackTimer = 0.0f;
    bool isGameOver = false;

    void Start() {
       // EventManager.instance.onGameOver += OnGameOver;
    }

    void OnDisable()
    {
        //EventManager.instance.onGameOver -= OnGameOver;
    }
	
	// Update is called once per frame
	void Update () {
        if (!isGameOver)
        {
            attackTimer += Time.deltaTime;

            if (currentBullet)
            {
                float scale = Mathf.Lerp(0.0f, targetScale, attackTimer / attackRate);

                currentBullet.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                currentBullet = (GameObject)Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                currentBullet.transform.parent = transform;
                currentBullet.transform.localScale = Vector3.zero;
                bulletRigidbody = currentBullet.GetComponent<Rigidbody>();
                //bulletSelfHiding = currentBullet.GetComponent<SelfHiding>();
            }

            if (Input.GetButton("Fire1"))
            {
                if (attackTimer > attackRate)
                {
                    Shoot();
                    attackTimer = 0.0f;
                }
            }
        }
    }

    private void Shoot()
    {
        Ray screenToCenter = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit hit;

        currentBullet.transform.parent = null;
        bulletRigidbody.isKinematic = false;
        if (Physics.Raycast(screenToCenter, out hit, 1000.0f))
        {
            bulletRigidbody.AddForce(shootForce * (hit.point - transform.position).normalized, ForceMode.Impulse);
        }
        else
        {
            bulletRigidbody.AddForce(shootForce * screenToCenter.direction, ForceMode.Impulse);
        }
        //StartCoroutine(bulletSelfHiding.HideAfterTime(destroyTime));
        currentBullet = null;
        //AudioManager.instance.PlaySound(AudioManager.AudioName.PlayerAttack);
    }

    void OnGameOver()
    {
        isGameOver = true;
    }
}
