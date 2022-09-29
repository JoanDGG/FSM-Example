using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public enum FSMStates
    {
        Patrol, Chase, Aim, Shoot, Evade
    }

    [SerializeField]
    public FSMStates currentState = FSMStates.Patrol;
    private int health = 100;
    private Vector3 destPos;

    public GameObject bullet;
    public ParticleSystem debris;
    public Transform playerTransform;
    public GameObject bulletSpawnPoint;
    public GameObject turret;
    public List<GameObject> pointList;
    public float tankSpeed = 2f;
    public float rotSpeed = 150.0f;
    public float turretRotSpeed = 10.0f;
    private Quaternion targetRotation;
    public float maxForwardSpeed = 30.0f;
    public float maxBackwardSpeed = -30.0f;
    public float shootRate = 0.5f;
    public float evadeTime = 1.5f;
    private float elapsedTime;
    private float elapsedTimeEvade;
    public float patrolRadius = 10f;
    public float chaseRadius = 25f;
    public float AttackRadius = 20f;
    private int index = -1;
    private float curSpeed;
    private ParticleSystem.EmissionModule debrisEmission;

    // Start is called before the first frame update
    void Start()
    {
        debrisEmission = debris.emission;
        FindNextPoint();
    }

    private void FindNextPoint() 
    {
        print("Finding next point");
        index = (index+1)%pointList.Count; //Random.Range(0, pointList.Count);
        destPos = pointList[index].transform.position;
    }

    void Update()
    {
        switch(currentState)
        {
            case FSMStates.Patrol:
                UpdatePatrol();
                break;
                
            case FSMStates.Chase:
                UpdateChase();
                break;
                
            case FSMStates.Aim:
                UpdateAim();
                break;

            case FSMStates.Shoot:
                UpdateShoot();
                break;

            case FSMStates.Evade:
                UpdateEvade();
                break;
        }

        debrisEmission.enabled = (curSpeed != 0);
    }

    void UpdatePatrol()
    {
        curSpeed = tankSpeed;
        //Find another random patrol point if the current point is reached
        if (Vector3.Distance(transform.position, destPos) <= patrolRadius) 
        {
            print("Reached the destination point -- calculating the next point");
            FindNextPoint();
        }
        //Check the distance with player tank, when the distance is near, transition to chase state
        else if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRadius) 
        {
            print("Switch to Chase state");
            currentState = FSMStates.Chase;
        }

        //Rotate to the target point
        targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        //Turret rotation
        turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed);
    }

    void UpdateChase()
    {
        curSpeed = tankSpeed;
        if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRadius)
        {
            targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
            if (Vector3.Distance(transform.position, playerTransform.position) <= AttackRadius)
            {
                print("Switch to Aim state");
                currentState = FSMStates.Aim;
            }
            else
            {
                print("Switch to Patrol state");
                currentState = FSMStates.Patrol;
            }
        }  
        
    }

    void UpdateAim()
    {
        curSpeed = tankSpeed;
        //Turret rotation
        targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
        turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed * 2.0f);
        print("Switch to Shoot state");
        currentState = FSMStates.Shoot;
    }

    void UpdateShoot()
    {
        curSpeed = 0f;
        //Turret rotation
        targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
        turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed * 2.0f);

        // Shoot
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= shootRate) 
        {
            //Reset the time
            elapsedTime = 0.0f;
            Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
        }

        if (Vector3.Distance(transform.position, playerTransform.position) > AttackRadius)
        {
            print("Switch to Chase state");
            currentState = FSMStates.Chase;
        }
    }

    void UpdateEvade()
    {
        curSpeed = tankSpeed;
        //Turret rotation
        targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
        turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed * 2.0f);
        
        // Shoot
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= shootRate) 
        {
            //Reset the time
            elapsedTime = 0.0f;
            Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
        }

        // Move away from player
        targetRotation = Quaternion.LookRotation(transform.position - playerTransform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        elapsedTimeEvade += Time.deltaTime;
        if (elapsedTimeEvade >= evadeTime) 
        {
            //Reset the time
            elapsedTimeEvade = 0.0f;
            print("Switch to Chase state");
            currentState = FSMStates.Chase;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<BulletController>())
        {
            Destroy(collision.gameObject);
            print("Switch to Evade state");
            currentState = FSMStates.Evade;
        }
    }
}
