﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    public int State { get; private set; }
    public float Distance { get; private set; }
    public bool Landed { get; private set; }
    public bool OnInrun { get; private set; }
    public bool oldMode;

    [Header("Colliders")]

    public Collider distCollider;

    [Space]

    [Header("Parameters")]
    public float jumpSpeed;
    public float jumperAngle = 0f;

    public float brakeForce;
    [Space]

    [Header("Flight")]
    public double angle;
    public double drag = 0.001d;
    public double lift = 0.001d;
    public float rotCoef;
    public float smoothCoef = 0.01f;
    public float sensCoef = 0.01f;
    [Space]

    [Header("Wind")]
    public Vector2 windDir;
    public float windForce;
    public float dirChange;
    public float forceChange;

    public GameObject modelObject;

    bool button0, button1;

    public ManagerScript managerScript;

    void OnTriggerEnter(Collider other)
    {
        //    print(distCollider.transform.position);
        if (other.tag == "Inrun")
        {
            OnInrun = true;
        }
        if (!Landed && other.tag == "LandingArea")
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
            {
                Crash();
            }
            Landed = true;
            Debug.Log(managerScript.Distance(distCollider.transform.position));
        }
        if (other.tag == "Outrun")
        {
            Brake();
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Inrun")
        {
            OnInrun = false;
        }
    }


    void Start()
    {
        State = 0;
        Landed = false;

        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        //Wind
        //windDir.Set(0.0f, 0.0f);
        //windForce = 0.0f;

        //preprocessing
        {
            for (int i = 1; i < tab.Length / 4; i++)
            {
                tab[i, 2] = Mathf.Sqrt((tab[i - 1, 0] - tab[i, 0]) * (tab[i - 1, 0] - tab[i, 0]) + (tab[i - 1, 1] - tab[i, 1]) * (tab[i - 1, 1] - tab[i, 1]));
                tab[i, 3] = tab[i - 1, 3] + tab[i, 2];
                //print("[" + i + "]" + tab[i, 2] + ", " + tab[i, 3]);
            }
        }
        ResetValues();

    }

    public void ResetValues()
    {
        State = 0;
        Landed = false;
        rb.isKinematic = true;
        modelObject.GetComponent<Transform>().localPosition = new Vector3();
        jumperAngle = 1;
        animator.SetBool("JumperCrash", false);
        button0 = button1 = false;
    }

    
    void Update()
    {
        animator.SetInteger("JumperState", State);
        if (State == 0 && Input.GetKeyDown(KeyCode.Space))
        {
            Gate();
        }
        else if (State == 1 && Input.GetMouseButtonDown(0))
        {
            Jump();
        }
        else if ((State == 2 || State == 3) && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))) 
        {
            button0 |= Input.GetMouseButtonDown(0);
            button1 |= Input.GetMouseButtonDown(1);
            Land();
        }
        if (State == 2)
        {
            //double pitchMoment = 3.29363 - 0.11567 * angle - 0.00333928 * angle * angle + 0.0000573605f * angle * angle * angle;
            //double pitchMoment = 0.5f;
            if (oldMode)
            {
                jumperAngle += Time.deltaTime*Input.GetAxis("Mouse Y")*2;
                jumperAngle = Mathf.Clamp(jumperAngle, -1, 1);
            }
            else
            {
                jumperAngle += Time.deltaTime*Input.GetAxis("Mouse Y")*2;
                jumperAngle = Mathf.Clamp(jumperAngle, -1, 1);
                // jumperAngle -= jumperAngle * jumperAngle * Mathf.Sign(jumperAngle) * smoothCoef;
                // jumperAngle += Input.GetAxis("Moues Y") * sensCoef;
                // jumperAngle = Mathf.Clamp(jumperAngle, -1, 1);
            }

            //rb.AddTorque(0.0f, 0.0f, (float)pitchMoment);
            if(oldMode)
            {
                
            }
            else
            {
                Vector3 torque = new Vector3(0.0f, 0.0f, jumperAngle * rotCoef/* * 70.0f*/);
                rb.AddRelativeTorque(torque);
            }
            
            animator.SetFloat("JumperAngle", jumperAngle);
            // Debug.Log("angle: " + angle + " jumperAngle: " + jumperAngle);
        }
        // if ((Landed && State != 3 && !land) || (State == 2 && (angle < -10.0f || angle > 80.0f) && animator.GetCurrentAnimatorStateInfo(0).IsName("Flight")))
        // {
        //     // Crash();
        // }
    }

    void FixedUpdate()
    {
        //ToDo

        //wind
        //windDir += Random.insideUnitCircle * dirChange;
        //windDir.Normalize();
        //windForce += Random.Range(-1.0f, 1.0f) * forceChange;
        //windForce = Mathf.Clamp(windForce, 0.0f, 4.0f);

        Vector3 vel = rb.velocity + rb.velocity.normalized * windForce;
        // Debug.Log(vel);
        Vector3 liftVec = new Vector3(-vel.normalized.y, vel.normalized.x, 0.0f);
        double tmp = rb.rotation.eulerAngles.z;
        if (tmp > 180) tmp -= 360;
        
        angle = -Mathf.Atan(rb.velocity.normalized.y / rb.velocity.normalized.x) * 180 / Mathf.PI + tmp;
        if(oldMode)
        {
            angle = -Mathf.Atan(rb.velocity.normalized.y / rb.velocity.normalized.x) * 180 / Mathf.PI + jumperAngle*10;
        }
        if (-15.0f <= angle && angle <= 50)
        {
            lift = 0.000933d + 0.00023314d * angle - 0.00000008201d * angle * angle - 0.0000001233d * angle * angle * angle + 0.00000000169d * angle * angle * angle * angle;
            drag = 0.001822d + 0.000096017d * angle + 0.00000222578d * angle * angle - 0.00000018944d * angle * angle * angle + 0.00000000352d * angle * angle * angle * angle;
        }


        //Debug.Log("angle: " + angle + " drag: " + drag + " lift: " + lift);
        if (State == 2)
        {
            rb.AddForce(-vel.normalized * (float)drag * vel.sqrMagnitude/* * rb.mass*/);
            rb.AddForce(liftVec * (float)lift * vel.sqrMagnitude/* * rb.mass*/);
            //rb.AddForceAtPosition(-vel.normalized * (float)drag * vel.sqrMagnitude, rb.transform.position);
            //rb.AddForceAtPosition(liftVec * (float)lift * vel.sqrMagnitude, rb.transform.position);
        }
        if (State == 4)
        {
            Vector3 brakeVec = Vector3.left * brakeForce;
            rb.AddForce(brakeVec);
        }
    }

    public void Gate()
    {
        State = 1;

        rb.isKinematic = false;
    }

    public void Jump()
    {
        State = 2;

        Debug.Log(OnInrun);
        if (OnInrun)
        {
            Vector3 jumpDirection = rb.velocity.normalized;
            jumpDirection = new Vector3(-jumpDirection.y, jumpDirection.x, 0);
            rb.velocity += jumpSpeed * jumpDirection;
            //rb.AddTorque(0.0f, 0.0f, 10f);
        }

    }

    /*public void Flight(float val)
    {

    }
    */

    public void DistanceMeasurement(Vector3 position)
    {
        //Distance = position.magnitude/1.005f;
        int it = 0;
        Debug.Log("distance: " + Distance);

        //Debug.Log(it + " " + position.x + " " + tab[it, 0]);
        while (it < tab.Length / 4 && position.x > tab[it, 0])
        {
            //Debug.Log(it + " " + position.x + " " + tab[it, 0]);
            it++;
        }
        it--;
        Vector2 last = new Vector2(position.x - tab[it, 0], position.y - tab[it, 1]);
        //Debug.Log(tab[it, 3] + " " + last.magnitude);
        Distance = tab[it, 3] + last.magnitude;
        //Debug.Log(Distance);
    }
    
    public void Land()
    {
        State = 3;
        animator.SetFloat("Landing", 1);
        if(button0 && button1) animator.SetFloat("Landing", 0);   
    }

    public void Crash()
    {
        //Na plecy i na brzuch
        //State = ;
        animator.SetBool("JumperCrash", true);  
    }

    public void Brake()
    {
        //ToDo
        State = 4;
    }
}