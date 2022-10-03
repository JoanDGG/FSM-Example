using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TankController : MonoBehaviour
{
    [Header("References")]
    public GameObject bullet;
    public GameObject turret;
    public GameObject bulletSpawnPoint;
    public ParticleSystem debris;
    public ParticleSystem smoke;
    public Text InfoText;

    [Header("Attributes")]
    public int maxHP = 10;
    public float rotSpeed = 150.0f;
    public float turretRotSpeed = 10.0f;
    public float maxForwardSpeed = 30.0f;
    public float maxBackwardSpeed = -30.0f;
    public float shootRate = 0.5f;

    private float HP, elapsedTime, curSpeed, targetSpeed;
    private ParticleSystem.EmissionModule debrisEmission;
    private ParticleSystem.EmissionModule smokeEmission;

    void OnEndGame() {
        // Don't allow any more control changes when the game ends
        smokeEmission.enabled = true;
        this.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        debrisEmission = debris.emission;
        smokeEmission = smoke.emission;
        smokeEmission.enabled = false;
        HP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateControl();
        UpdateWeapon();
        if (Input.GetKeyDown(KeyCode.R)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        InfoText.text = "Press R to restart\n" + HP + " / " + maxHP;
    }

    void UpdateControl() 
    {
        // Generate a plane that intersects the transform's position with an upwards normal.
        Plane playerPlane = new Plane(Vector3.up, transform.position);

        // Generate a ray from the cursor position
        Ray rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Determine the point where the cursor ray intersects the plane.

        // If the ray is parallel to the plane, Raycast will return false.
        if (playerPlane.Raycast(rayCast, out var hitDist)) 
        {
            // Get the point along the ray that hits the calculated distance.
            Vector3 rayHitPoint = rayCast.GetPoint(hitDist);

            Quaternion targetRotation = Quaternion.LookRotation(rayHitPoint - transform.position);
            turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed);
        }

        if (Input.GetKey(KeyCode.W)) 
            targetSpeed = maxForwardSpeed;
        else if (Input.GetKey(KeyCode.S))
            targetSpeed = maxBackwardSpeed;
        else
            targetSpeed = 0f;

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(0f, -rotSpeed * Time.deltaTime, 0f);
        else if (Input.GetKey(KeyCode.D))
            transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f);

        debrisEmission.enabled = (targetSpeed != 0);

        //Determine current speed
        curSpeed = Mathf.Lerp(curSpeed, targetSpeed, 7.0f * Time.deltaTime);
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    void UpdateWeapon() 
    {
        elapsedTime += Time.deltaTime;
        if (Input.GetMouseButtonDown(0)) 
        {
            if (elapsedTime >= shootRate) 
            {
                //Reset the time
                elapsedTime = 0.0f;

                GameObject bulletObject = Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
                bulletObject.GetComponent<BulletController>().parent = this.gameObject;
            }
        }
    }

    public void TakeDamage()
    {
        HP--;
        if(HP <= 0)
        {
            print("Game Over");
            OnEndGame();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<BulletController>())
        {
            if(collision.gameObject.GetComponent<BulletController>().parent != this.gameObject)
            {
                Destroy(collision.gameObject);
                TakeDamage();
            }
        }
    }
}
