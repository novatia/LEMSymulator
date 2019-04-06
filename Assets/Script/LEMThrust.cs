using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TTControllerAssemblyState
{
    JET, THROTTLE
};



public class LEMThrust : MonoBehaviour
{
    const float RAD_TO_DEG = 57.2958f;

    public Vector3 V0;
    public float HypergolicPropeller = 100;

    [Header("RCS 1")]
    public GameObject RCS1ForwardDummy;
    public Light RCS1ForwardLight;
    public GameObject RCS1SideDummy;
    public Light RCS1SideLight;
    public GameObject RCS1UpDummy;
    public Light RCS1UpLight;
    public GameObject RCS1DownDummy;
    public Light RCS1DownLight;

    [Header("RCS 2")]
    public GameObject RCS2ForwardDummy;
    public Light RCS2ForwardLight;
    public GameObject RCS2SideDummy;
    public Light RCS2SideLight;
    public GameObject RCS2UpDummy;
    public Light RCS2UpLight;
    public GameObject RCS2DownDummy;
    public Light RCS2DownLight;

    [Header("RCS 3")]
    public GameObject RCS3SideDummy;
    public Light RCS3SideLight;
    public GameObject RCS3ForwardDummy;
    public Light RCS3ForwardLight;
    public GameObject RCS3UpDummy;
    public Light RCS3UpLight;
    public GameObject RCS3DownDummy;
    public Light RCS3DownLight;

    [Header("RCS 4")]
    public GameObject RCS4SideDummy;
    public Light RCS4SideLight;
    public GameObject RCS4ForwardDummy;
    public Light RCS4ForwardLight;
    public GameObject RCS4UpDummy;
    public Light RCS4UpLight;
    public GameObject RCS4DownDummy;
    public Light RCS4DownLight;


    private bool RCS1_forward_on;
    private bool RCS1_side_on;
    private bool RCS1_up_on;
    private bool RCS1_down_on;

    private bool RCS2_forward_on;
    private bool RCS2_side_on;
    private bool RCS2_up_on;
    private bool RCS2_down_on;

    private bool RCS3_side_on;
    private bool RCS3_forward_on;
    private bool RCS3_up_on;
    private bool RCS3_down_on;

    private bool RCS4_side_on;
    private bool RCS4_forward_on;
    private bool RCS4_up_on;
    private bool RCS4_down_on;


    //TELEMETRIES
    private float Altitude;
    private float Yaw;
    private float Roll;
    private float Pitch;


    public float GetAltitude()
    {
        return Altitude;
    }

    public float GetYaw()
    {
        return Yaw;
    }

    public float GetRoll()
    {
        return Roll;
    }

    public float GetPitch()
    {
        return Pitch;
    }

    public float GetVelocityX()
    {
        return rb.velocity.x;
    }
    public float GetVelocityY()
    {
        return -rb.velocity.y;
    }
    public float GetVelocityZ()
    {
        return rb.velocity.z;
    }


    private bool HustonWeHaveAProblem = false;

    [Header("Thrust/Translation Control")]  
    public float TTCFriction = 0;
    public TTControllerAssemblyState TTState = TTControllerAssemblyState.JET;


    [Header("RCS Forces")]
    public float RCSLateralForce  = 110.0f;

    [Header("Moon Forces")]
    public float MoonGravityForce = 1.62f;

    public GameObject Moon;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = V0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRCSCrontrol();
        GetTelemeties();
        HustonWeHaveAProblemCheck();
    }

    private void HustonWeHaveAProblemCheck() {

        if (( Mathf.Abs( Yaw ) >= 180 || Mathf.Abs(Pitch ) >= 180 || Mathf.Abs(Roll ) >= 180 ) && !HustonWeHaveAProblem)
        {
            HustonWeHaveAProblem = true;
            GetComponent<AudioSource>().Play();
        }
    }

    private void UpdateRCSCrontrol() {


        RCS1_up_on = false;
        RCS1_down_on = false;
        RCS1_forward_on = false;
        RCS1_side_on = false;

        RCS2_up_on = false;
        RCS2_down_on = false;
        RCS2_forward_on = false;
        RCS2_side_on = false;

        RCS3_up_on = false;
        RCS3_down_on = false;
        RCS3_forward_on = false;
        RCS3_side_on = false;

        RCS4_up_on = false;
        RCS4_down_on = false;
        RCS4_forward_on = false;
        RCS4_side_on = false;

        RCS1DownLight.enabled = false;
        RCS1UpLight.enabled = false;
        RCS1ForwardLight.enabled = false;
        RCS1SideLight.enabled = false;

        RCS2DownLight.enabled = false;
        RCS2UpLight.enabled = false;
        RCS2ForwardLight.enabled = false;
        RCS2SideLight.enabled = false;

        RCS3DownLight.enabled = false;
        RCS3UpLight.enabled = false;
        RCS3ForwardLight.enabled = false;
        RCS3SideLight.enabled = false;

        RCS4DownLight.enabled = false;
        RCS4UpLight.enabled = false;
        RCS4ForwardLight.enabled = false;
        RCS4SideLight.enabled = false;



        //PITCH  +
        if (Input.GetAxis("RCS1V") > 0.1f)
        {
            RCS1_up_on = true;
            RCS1_down_on = false;
            RCS1UpLight.enabled = true;
            RCS1DownLight.enabled = false;

            RCS2_up_on = true;
            RCS2_down_on = false;
            RCS2UpLight.enabled = true;
            RCS2DownLight.enabled = false;





            RCS3_up_on = false;
            RCS3_down_on = true;
            RCS3UpLight.enabled = false;
            RCS3DownLight.enabled = true;

            RCS4_up_on = false;
            RCS4_down_on = true;
            RCS4UpLight.enabled = false;
            RCS4DownLight.enabled = true;
        }

        //PITCH  -
        if (Input.GetAxis("RCS1V") < -0.1f)
        {

            //FRONT
            RCS1_up_on = false;
            RCS1_down_on = true;
            RCS1UpLight.enabled = false;
            RCS1DownLight.enabled = true;

            RCS2_up_on = false;
            RCS2_down_on = true;
            RCS2UpLight.enabled = false;
            RCS2DownLight.enabled = true;




            //BACK
            RCS3_up_on = true;
            RCS3_down_on = false;
            RCS3UpLight.enabled = true;
            RCS3DownLight.enabled = false;

            RCS4_up_on = true;
            RCS4_down_on = false;
            RCS4UpLight.enabled = true;
            RCS4DownLight.enabled = false;
        }


        //ROLL +
        if (Input.GetAxis("RCS1H") > 0.1f)
        {
            //FRONT
            RCS1_up_on = true;
            RCS1_down_on = false;
            RCS1UpLight.enabled = true;
            RCS1DownLight.enabled = false;

            RCS4_up_on = true;
            RCS4_down_on = false;
            RCS4UpLight.enabled = true;
            RCS4DownLight.enabled = false;


            //BACK
            RCS2_up_on = false;
            RCS2_down_on = true;
            RCS2UpLight.enabled = false;
            RCS2DownLight.enabled = true;

            RCS3_up_on = false;
            RCS3_down_on = true;
            RCS3UpLight.enabled = false;
            RCS3DownLight.enabled = true;

        }

        //ROLL -
        if (Input.GetAxis("RCS1H") < -0.1f)
        {
            //FRONT
            RCS1_up_on = false;
            RCS1_down_on = true;
            RCS1UpLight.enabled = false;
            RCS1DownLight.enabled = true;

            RCS4_up_on = false;
            RCS4_down_on = true;
            RCS4UpLight.enabled = false;
            RCS4DownLight.enabled = true;


            //BACK
            RCS2_up_on = true;
            RCS2_down_on = false;
            RCS2UpLight.enabled = true;
            RCS2DownLight.enabled = false;

            RCS3_up_on = true;
            RCS3_down_on = false;
            RCS3UpLight.enabled = true;
            RCS3DownLight.enabled = false;

        }






        /*
        if (Input.GetAxis("RCS2V") > 0.1f)
        {
            RCS2_up_on = true;
            RCS2_down_on = false;
            RCS2UpLight.enabled = true;
            RCS2DownLight.enabled = false;

            RCS4_up_on = false;
            RCS4_down_on = true;
            RCS4UpLight.enabled = false;
            RCS4DownLight.enabled = true;
        }

        if (Input.GetAxis("RCS2V") < -0.1f)
        {
            RCS2_up_on = false;
            RCS2_down_on = true;
            RCS2UpLight.enabled = false;
            RCS2DownLight.enabled = true;

            RCS4_up_on = true;
            RCS4_down_on = false;
            RCS4UpLight.enabled = true;
            RCS4DownLight.enabled = false;
        }






        if (Input.GetAxis("RCS2H") > 0.1f)
        {
            RCS2_side_on = false;
            RCS2_forward_on = true;
            RCS2SideLight.enabled = false;
            RCS2ForwardLight.enabled = true;

            RCS4_side_on = true;
            RCS4_forward_on = false;
            RCS4SideLight.enabled = true;
            RCS4ForwardLight.enabled = false;
        }

        if (Input.GetAxis("RCS2H") < -0.1f)
        {
            RCS2_side_on = true;
            RCS2_forward_on = false;
            RCS2SideLight.enabled = true;
            RCS2ForwardLight.enabled = false;
           
            RCS4_side_on = false;
            RCS4_forward_on = true;
            RCS4SideLight.enabled = false;
            RCS4ForwardLight.enabled = true;
        }
        */
    }


    private void FixedUpdate()
    {
        RCSApplyThrust();
        ApplyGravity();
    }

    private void GetTelemeties() {
        Vector3 MoonDistance = Moon.transform.position - transform.position;
        Altitude = MoonDistance.magnitude;

        Yaw     = rb.angularVelocity.z * RAD_TO_DEG;
        Roll    = rb.angularVelocity.x * RAD_TO_DEG;
        Pitch   = rb.angularVelocity.y * RAD_TO_DEG;
        Debug.Log( Altitude+" ,"+ Yaw+" ,"+ Roll+ " ,"+ Pitch );
    }

    private void ApplyGravity()
    {
        Vector3 moonGravity = new Vector3(0.0f, -MoonGravityForce, 0.0f);

        rb.AddForce( moonGravity , ForceMode.Acceleration);
    }

    private void RCSApplyThrust()
    {

        if (RCS1_forward_on)
        {
            Debug.Log("RCS1L");
            rb.AddForceAtPosition(RCS1ForwardDummy.transform.up * RCSLateralForce, RCS1ForwardDummy.transform.position);
        }

        if (RCS1_side_on)
        {
            Debug.Log("RCS1R");
            rb.AddForceAtPosition(RCS1SideDummy.transform.up * RCSLateralForce, RCS1SideDummy.transform.position);
        }

        if (RCS1_up_on)
        {
            Debug.Log("RCS1U");
            rb.AddForceAtPosition(RCS1UpDummy.transform.up * RCSLateralForce, RCS1UpDummy.transform.position);
        }

        if (RCS1_down_on)
        {
            Debug.Log("RCS1D");
            rb.AddForceAtPosition(RCS1DownDummy.transform.up * RCSLateralForce, RCS1DownDummy.transform.position);
        }






        if (RCS2_forward_on)
        {
            Debug.Log("RCS2L");
            rb.AddForceAtPosition(RCS2ForwardDummy.transform.up * RCSLateralForce, RCS2ForwardDummy.transform.position);
        }

        if (RCS2_side_on)
        {
            Debug.Log("RCS2R");
            rb.AddForceAtPosition(RCS2SideDummy.transform.up * RCSLateralForce, RCS2SideDummy.transform.position);
        }

        if (RCS2_up_on)
        {
            Debug.Log("RCS2U");
            rb.AddForceAtPosition(RCS2UpDummy.transform.up * RCSLateralForce, RCS2UpDummy.transform.position);
        }

        if (RCS2_down_on)
        {
            Debug.Log("RCS2D");
            rb.AddForceAtPosition(RCS2DownDummy.transform.up * RCSLateralForce, RCS2DownDummy.transform.position);
        }







        if (RCS3_forward_on)
        {
            Debug.Log("RCS3L");
            rb.AddForceAtPosition(RCS3ForwardDummy.transform.up * RCSLateralForce, RCS3ForwardDummy.transform.position);
        }

        if (RCS3_side_on)
        {
            Debug.Log("RCS3R");
            rb.AddForceAtPosition(RCS3SideDummy.transform.up * RCSLateralForce, RCS3SideDummy.transform.position);
        }

        if (RCS3_up_on)
        {
            Debug.Log("RCS3U");
            rb.AddForceAtPosition(RCS3UpDummy.transform.up * RCSLateralForce, RCS3UpDummy.transform.position);
        }

        if (RCS3_down_on)
        {
            Debug.Log("RCS3D");
            rb.AddForceAtPosition(RCS3DownDummy.transform.up * RCSLateralForce, RCS3DownDummy.transform.position);
        }





        if (RCS4_forward_on)
        {
            Debug.Log("RCS4L");
            rb.AddForceAtPosition(RCS4ForwardDummy.transform.up * RCSLateralForce, RCS4ForwardDummy.transform.position);
        }

        if (RCS4_side_on)
        {
            Debug.Log("RCS4R");
            rb.AddForceAtPosition(RCS4SideDummy.transform.up * RCSLateralForce, RCS4SideDummy.transform.position);
        }

        if (RCS4_up_on)
        {
            Debug.Log("RCS4U");
            rb.AddForceAtPosition(RCS4UpDummy.transform.up * RCSLateralForce, RCS4UpDummy.transform.position);
        }

        if (RCS4_down_on)
        {
            Debug.Log("RCS4D");
            rb.AddForceAtPosition(RCS4DownDummy.transform.up * RCSLateralForce, RCS4DownDummy.transform.position);
        }
    }


    public void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 250, 20), "Altitude [m]: " + GetAltitude());
        GUI.Label(new Rect(10, 30, 250, 20), "Yaw  [Deg/s]: "  + GetYaw());
        GUI.Label(new Rect(10, 50, 250, 20), "Roll [Deg/s]: " + GetRoll());
        GUI.Label(new Rect(10, 70, 250, 20), "Roll [Deg/s]: " + GetPitch());
        GUI.Label(new Rect(10, 90, 250, 20), "Velocity X[Deg/s]: " + GetVelocityX());
        GUI.Label(new Rect(10, 110, 250, 20), "Velocity Y[Deg/s]: " + GetVelocityY());
        GUI.Label(new Rect(10, 130, 250, 20), "Velocity Z[Deg/s]: " + GetVelocityZ());
    }
}