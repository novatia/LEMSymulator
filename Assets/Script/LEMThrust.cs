using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TTControllerAssemblyState
{
    JET, THROTTLE
};



public class LEMThrust : MonoBehaviour
{
    //PHY CONSTANTS
    private float HeightError = 5478.586f;
    private const float RAD_TO_DEG = 57.2958f;
    private const float MOON_RADIUS_M = 1737400;

    [Header("Orbit insertion")]
    public Vector3 V0;

    [Header("APS")]
    public float APSPropellantMass = 2352; //kg

    [Header("DPS")]
    public float DPSThrustMinLevel = 10;
    public float DPSThrustMaxLevel = 60;
    public float DPSPropellantMass = 8200; //kg
    public float DPSPropellantBurnRate = 120.0f;
    public Transform DPSThruster;
    public AudioSource DPSAudio;
    private bool DPSThruster_on;
    public float DPSForce = 45040.0f;
    public float DPSEngineLevel = 60;

    [Header("RCS")]
    public float RCSLateralForce = 110.0f;
    public float RCSPropellantMass = 287; //kg
    public float RCSPropellantBurnRate = 120;

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

    [Header("Thrust/Translation Control")]
    public float TTCFriction = 0;
    public TTControllerAssemblyState TTState = TTControllerAssemblyState.JET;

    [Header("Moon")]
    public float MoonGravityForce = 1.62f;
    public GameObject Moon;

    //TELEMETRIES
    private float Altitude;

    private float YawSpeed;
    private float RollSpeed;
    private float PitchSpeed;

    //RIGID BODY CONFIG
    private Rigidbody rb;
    private float StuffMass;

    private bool HustonWeHaveAProblem = false;




    private void FixedUpdate()
    {
        RCSApplyThrust();
        DPSApplyThrust();
        UpdateRBCfg();
        ApplyGravity();
    }



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = V0;
        Time.fixedDeltaTime = 0.005f;

        //Time.timeScale = 30;

        StuffMass = rb.mass - DPSPropellantMass - RCSPropellantMass - APSPropellantMass;

        DPSOff();
    }




    //TELEMETRIES
    public float GetAltitude()
    {
        return Altitude;
    }

    public float GetYawSpeed()
    {
        return YawSpeed;
    }

    public float GetRollSpeed()
    {
        return RollSpeed;
    }

    public float GetPitchSpeed()
    {
        return PitchSpeed;
    }

    public float GetYaw()
    {
        return transform.eulerAngles.y;
    }

    public float GetRoll()
    {
        return transform.eulerAngles.x;
    }

    public float GetPitch()
    {
        return transform.eulerAngles.z;
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
    
    private void DPSOff() {
        Debug.Log("DPS off");
        DPSThruster.localScale = Vector3.zero;
        DPSAudio.enabled = false;
    }

    private void DPSOn()
    {
        Debug.Log("DPS on");
        DPSThruster.localScale= new Vector3(0.106383f, 0.106383f, 0.106383f);
        DPSAudio.enabled = true;
        rb.AddForce( transform.up * DPSForce *255* DPSThrustMaxLevel / 100 , ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDPSControl();
        UpdateRCSControl();

        GetTelemeties();
        HustonWeHaveAProblemCheck();
    }

    private void UpdateRBCfg()
    {
        rb.mass = rb.mass + DPSPropellantMass + RCSPropellantMass + APSPropellantMass;
    }

    private void HustonWeHaveAProblemCheck() {

        if (( Mathf.Abs( YawSpeed ) >= 180 || Mathf.Abs(PitchSpeed ) >= 180 || Mathf.Abs(RollSpeed ) >= 180 ) && !HustonWeHaveAProblem)
        {
            HustonWeHaveAProblem = true;
            GetComponent<AudioSource>().Play();
        }
    }

    private void UpdateDPSControl()
    {
        if (Input.GetButtonDown("DPS"))
        {
            DPSThruster_on = !DPSThruster_on;
        }
    }

    private void UpdateRCSControl()
    {

      


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


        if (RCSPropellantMass <= 0)
        {
            return;
        }

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

  

    private float GetMoonSurfaceDistance() {
        Vector3 MoonDistance = Moon.transform.position - transform.position;
        Altitude = (MoonDistance.magnitude - MOON_RADIUS_M - HeightError) / 1000;

        return Altitude;
    }

    private void GetTelemeties() {
        Altitude = GetMoonSurfaceDistance();
        YawSpeed      = rb.angularVelocity.z * RAD_TO_DEG;
        RollSpeed     = rb.angularVelocity.x * RAD_TO_DEG;
        PitchSpeed    = rb.angularVelocity.y * RAD_TO_DEG;
       // Debug.Log( Altitude+" ,"+ Yaw+" ,"+ Roll+ " ,"+ Pitch );
    }

    private void ApplyGravity()
    {
        Vector3 moonGravity = Vector3.Normalize(Moon.transform.position - transform.position)* MoonGravityForce;
        //Vector3 moonGravity = new Vector3(0.0f, -MoonGravityForce, 0.0f);

        rb.AddForce( moonGravity , ForceMode.Acceleration);
    }

    private void RCSApplyThrust()
    {

        float RCSForceK = RCSLateralForce * 255 ;


        if (RCS1_forward_on)
        {
            Debug.Log("RCS1L");
            rb.AddForceAtPosition(RCS1ForwardDummy.transform.up * RCSForceK, RCS1ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS1_side_on)
        {
            Debug.Log("RCS1R");
            rb.AddForceAtPosition(RCS1SideDummy.transform.up * RCSForceK, RCS1SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS1_up_on)
        {
            Debug.Log("RCS1U");
            rb.AddForceAtPosition(RCS1UpDummy.transform.up * RCSForceK, RCS1UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS1_down_on)
        {
            Debug.Log("RCS1D");
            rb.AddForceAtPosition(RCS1DownDummy.transform.up * RCSForceK, RCS1DownDummy.transform.position, ForceMode.Impulse);
        }






        if (RCS2_forward_on)
        {
            Debug.Log("RCS2L");
            rb.AddForceAtPosition(RCS2ForwardDummy.transform.up * RCSForceK, RCS2ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS2_side_on)
        {
            Debug.Log("RCS2R");
            rb.AddForceAtPosition(RCS2SideDummy.transform.up * RCSForceK, RCS2SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS2_up_on)
        {
            Debug.Log("RCS2U");
            rb.AddForceAtPosition(RCS2UpDummy.transform.up * RCSForceK, RCS2UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS2_down_on)
        {
            Debug.Log("RCS2D");
            rb.AddForceAtPosition(RCS2DownDummy.transform.up * RCSForceK, RCS2DownDummy.transform.position, ForceMode.Impulse);
        }







        if (RCS3_forward_on)
        {
            Debug.Log("RCS3L");
            rb.AddForceAtPosition(RCS3ForwardDummy.transform.up * RCSForceK, RCS3ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS3_side_on)
        {
            Debug.Log("RCS3R");
            rb.AddForceAtPosition(RCS3SideDummy.transform.up * RCSForceK, RCS3SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS3_up_on)
        {
            Debug.Log("RCS3U");
            rb.AddForceAtPosition(RCS3UpDummy.transform.up * RCSForceK, RCS3UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS3_down_on)
        {
            Debug.Log("RCS3D");
            rb.AddForceAtPosition(RCS3DownDummy.transform.up * RCSForceK, RCS3DownDummy.transform.position, ForceMode.Impulse);
        }





        if (RCS4_forward_on)
        {
            Debug.Log("RCS4L");
            rb.AddForceAtPosition(RCS4ForwardDummy.transform.up * RCSForceK, RCS4ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS4_side_on)
        {
            Debug.Log("RCS4R");
            rb.AddForceAtPosition(RCS4SideDummy.transform.up * RCSForceK, RCS4SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS4_up_on)
        {
            Debug.Log("RCS4U");
            rb.AddForceAtPosition(RCS4UpDummy.transform.up * RCSForceK, RCS4UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS4_down_on)
        {
            Debug.Log("RCS4D");
            rb.AddForceAtPosition(RCS4DownDummy.transform.up * RCSForceK, RCS4DownDummy.transform.position,ForceMode.Impulse);
        }



        if (RCS1_up_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }


        if (RCS2_up_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS3_up_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS4_up_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS1_up_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }


        if (RCS1_down_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS2_down_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS3_down_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS4_down_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }



        if (RCS1_side_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS2_side_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS3_side_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS4_side_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }



        if (RCS1_forward_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS2_forward_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS3_forward_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }

        if (RCS4_forward_on)
        {
            RCSPropellantMass -= Time.deltaTime * RCSPropellantBurnRate;
        }



    }

    private void DPSApplyThrust() {

        if (DPSPropellantMass <= 0) {
            DPSOff();
            DPSPropellantMass = 0;
            return;
        }

        if (DPSThruster_on)
        {
            DPSOn();
        }
        else
        {
            DPSOff();
        }

        if (DPSThruster_on)
        {
            DPSPropellantMass -= Time.deltaTime * DPSPropellantBurnRate*DPSEngineLevel/100;
        }
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 250, 20), "Altitude [km]: " + GetAltitude());
        GUI.Label(new Rect(10, 30, 250, 20), "Yaw  Speed [Deg/s]: "  + GetYaw());
        GUI.Label(new Rect(10, 50, 250, 20), "Roll Speed [Deg/s]: " + GetRoll());
        GUI.Label(new Rect(10, 70, 250, 20), "Roll Speed [Deg/s]: " + GetPitch());
        GUI.Label(new Rect(10, 90, 250, 20), "Speed X[m/s]: " + GetVelocityX());
        GUI.Label(new Rect(10, 110, 250, 20), "Speed Y[m/s]: " + GetVelocityY());
        GUI.Label(new Rect(10, 130, 250, 20), "Speed Z[m/s]: " + GetVelocityZ());


        GUI.Label(new Rect(10, 150, 250, 20), "RCP Propeller [kg]: " + RCSPropellantMass);
        GUI.Label(new Rect(10, 170, 250, 20), "APS Propeller [kg]: " + APSPropellantMass);
        GUI.Label(new Rect(10, 190, 250, 20), "DPS Propeller [kg]: " + DPSPropellantMass);

        GUI.Label(new Rect(10, 210, 250, 20), "Yaw  [Deg]: " + GetYaw());
        GUI.Label(new Rect(10, 230, 250, 20), "Roll [Deg]: " + GetRoll());
        GUI.Label(new Rect(10, 250, 250, 20), "Pitch [Deg]: " + GetPitch());


    }
}