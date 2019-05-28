using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum TTControllerAssemblyState
{
    JET, THROTTLE
};



public class LEMThrust : MonoBehaviour
{
    //PHY CONSTANTS
    private const float RAD_TO_DEG = 57.2958f;
    private const float MOON_RADIUS_M = 1737400;
    private const double  G_CONSTANT = 6.673e-11;
    private const double MOON_M_KG = 7.34767309e22;

    [Header("Orbit insertion")]
    public Vector3 V0;

    [Header("APS")]
    public float APSPropellantMass; //kg

    [Header("DPS")]
    public float DPSThrustMinLevel;
    public float DPSThrustMaxLevel;
    public float DPSPropellantMass; //kg
    public float DPSPropellantBurnRate;
    public Transform DPSThruster;

    public AudioSource DPSAudio;
    public AudioSource RCSAudio;

    private bool DPSThruster_on;
    public float DPSForce;
    public float DPSEngineLevel;

    [Header("RCS")]
    public float RCSLateralForce;
    public float RCSPropellantMass ; //kg
    public float RCSPropellantBurnRate;

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
        return Mathf.Round(YawSpeed*1000.0f)/1000;
    }

    public float GetRollSpeed()
    {
        return Mathf.Round(RollSpeed * 1000.0f) / 1000;
    }

    public float GetPitchSpeed()
    {
        return Mathf.Round(PitchSpeed * 1000.0f) / 1000;
    }

    public float GetYaw()
    {
        return Mathf.Round(transform.eulerAngles.y * 10) / 10;
    }

    public float GetRoll()
    {
        return Mathf.Round(transform.eulerAngles.x * 10) / 10;
    }

    public float GetPitch()
    {
        return Mathf.Round((transform.eulerAngles.z-270)*10)/10;
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
      //  Debug.Log("DPS off");
        DPSThruster.localScale = Vector3.zero;
        DPSAudio.enabled = false;
    }

    private void DPSOn()
    {
      //  Debug.Log("DPS on");
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
        float HUSTON = 150.0f;

        if (( (Mathf.Abs( YawSpeed ) >= HUSTON || Mathf.Abs(PitchSpeed ) >= HUSTON || Mathf.Abs(RollSpeed ) >= HUSTON)
            ||
            DPSPropellantMass <= 0 ||
            RCSPropellantMass <= 0 ||
            APSPropellantMass <= 0)
            && !HustonWeHaveAProblem
            
            )
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

        bool TSelector = Input.GetButton("TSelector");

        float RCS2H = Input.GetAxis("RCS2H");
        float RCS2V = Input.GetAxis("RCS2V");

        float RCS1V = Input.GetAxis("RCS1V");
        float RCS1H = Input.GetAxis("RCS1H");

        

        //RCS1 OFF
        RCS1_up_on = false;
        RCS1_down_on = false;
        RCS1_forward_on = false;
        RCS1_side_on = false;

        RCS1DownLight.enabled = false;
        RCS1UpLight.enabled = false;
        RCS1ForwardLight.enabled = false;
        RCS1SideLight.enabled = false;

        //RCS2 OFF
        RCS2_up_on = false;
        RCS2_down_on = false;
        RCS2_forward_on = false;
        RCS2_side_on = false;

        RCS2DownLight.enabled = false;
        RCS2UpLight.enabled = false;
        RCS2ForwardLight.enabled = false;
        RCS2SideLight.enabled = false;

        //RCS3 OFF
        RCS3_up_on = false;
        RCS3_down_on = false;
        RCS3_forward_on = false;
        RCS3_side_on = false;

        RCS3DownLight.enabled = false;
        RCS3UpLight.enabled = false;
        RCS3ForwardLight.enabled = false;
        RCS3SideLight.enabled = false;

        //RCS4 OFF
        RCS4_up_on = false;
        RCS4_down_on = false;
        RCS4_forward_on = false;
        RCS4_side_on = false;

        RCS4DownLight.enabled = false;
        RCS4UpLight.enabled = false;
        RCS4ForwardLight.enabled = false;
        RCS4SideLight.enabled = false;

        if (RCSPropellantMass <= 0)
        {
            return;
        }

        //PITCH  +
        if (RCS1V > 0.1f)
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
        if (RCS1V < -0.1f)
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

        //YAW +
        if (RCS1H > 0.1f)
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

        //YAW -
        if (RCS1H< -0.1f)
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

        if (TSelector)
        {
            //FORWARD  1S,2F
            if (RCS2H > 0.15f)
            {
                RCS1_forward_on = false;
                RCS1_side_on = true;
                RCS1ForwardLight.enabled = false;
                RCS1SideLight.enabled = true;

                RCS2_forward_on = true;
                RCS2_side_on = false;
                RCS2ForwardLight.enabled = true;
                RCS2SideLight.enabled = false;

                RCS3_forward_on = false;
                RCS3_side_on = false;
                RCS3ForwardLight.enabled = false;
                RCS3SideLight.enabled = false;

                RCS4_forward_on = false;
                RCS4_side_on = false;
                RCS4ForwardLight.enabled = false;
                RCS4SideLight.enabled = false;
            }

            //SIDE 4F,3S
            if (RCS2H < -0.15f)
            {
                //FRONT
                RCS1_forward_on = false;
                RCS1_side_on = false;
                RCS1ForwardLight.enabled = false;
                RCS1SideLight.enabled = false;

                RCS2_forward_on = false;
                RCS2_side_on = false;
                RCS2ForwardLight.enabled = false;
                RCS2SideLight.enabled = false;

                //BACK
                RCS3_forward_on = false;
                RCS3_side_on = true;
                RCS3ForwardLight.enabled = false;
                RCS3SideLight.enabled = true;

                RCS4_forward_on = true;
                RCS4_side_on = false;
                RCS4ForwardLight.enabled = true;
                RCS4SideLight.enabled = false;
            }

            //FRONT  3FS,2S
            if (RCS2V > 0.15f)
            {
                RCS1_forward_on = false;
                RCS1_side_on = false;
                RCS1ForwardLight.enabled = false;
                RCS1SideLight.enabled = false;

                RCS2_forward_on = false;
                RCS2_side_on = true;
                RCS2ForwardLight.enabled = false;
                RCS2SideLight.enabled = true;

                RCS3_forward_on = true;
                RCS3_side_on = false;
                RCS3ForwardLight.enabled = true;
                RCS3SideLight.enabled = false;

                RCS4_forward_on = false;
                RCS4_side_on = false;
                RCS4ForwardLight.enabled = false;
                RCS4SideLight.enabled = false;
            }

            //BACK 4S,1F
            if (RCS2V < -0.15f)
            {
                //FRONT
                RCS1_forward_on = true;
                RCS1_side_on = false;
                RCS1ForwardLight.enabled = true;
                RCS1SideLight.enabled = false;

                RCS2_forward_on = false;
                RCS2_side_on = false;
                RCS2ForwardLight.enabled = false;
                RCS2SideLight.enabled = false;

                //BACK
                RCS3_forward_on = false;
                RCS3_side_on = false;
                RCS3ForwardLight.enabled = false;
                RCS3SideLight.enabled = false;

                RCS4_forward_on = false;
                RCS4_side_on = true;
                RCS4ForwardLight.enabled = false;
                RCS4SideLight.enabled = true;
            }
        }
        else {

            //ROLL -
            if (RCS2H < -0.15f)
            {
                //FRONT
                RCS1_forward_on = false;
                RCS1_side_on = true;
                RCS1ForwardLight.enabled = false;
                RCS1SideLight.enabled = true;

                RCS2_forward_on = false;
                RCS2_side_on = true;
                RCS2ForwardLight.enabled = false;
                RCS2SideLight.enabled = true;

                //BACK
                RCS3_forward_on = false;
                RCS3_side_on = true;
                RCS3ForwardLight.enabled = false;
                RCS3SideLight.enabled = true;

                RCS4_forward_on = false;
                RCS4_side_on = true;
                RCS4ForwardLight.enabled = false;
                RCS4SideLight.enabled = true;
            }

            //ROLL +
            if (RCS2H > 0.15f)
            {
                RCS1_forward_on = true;
                RCS1_side_on = false;
                RCS1ForwardLight.enabled = true;
                RCS1SideLight.enabled = false;

                RCS2_forward_on = true;
                RCS2_side_on = false;
                RCS2ForwardLight.enabled = true;
                RCS2SideLight.enabled = false;

                RCS3_forward_on = true;
                RCS3_side_on = false;
                RCS3ForwardLight.enabled = true;
                RCS3SideLight.enabled = false;

                RCS4_forward_on = true;
                RCS4_side_on = false;
                RCS4ForwardLight.enabled = true;
                RCS4SideLight.enabled = false;
            }

            //UP
            if (RCS2V > 0.15f)
            {
                RCS1_up_on = true;
                RCS1_down_on = false;
                RCS1UpLight.enabled = true;
                RCS1DownLight.enabled = false;

                RCS2_up_on = true;
                RCS2_down_on = false;
                RCS2UpLight.enabled = true;
                RCS2DownLight.enabled = false;

                RCS3_up_on = true;
                RCS3_down_on = false;
                RCS3UpLight.enabled = true;
                RCS3DownLight.enabled = false;

                RCS4_up_on = true;
                RCS4_down_on = false;
                RCS4UpLight.enabled = true;
                RCS4DownLight.enabled = false;

            }

            //DOWN
            if (RCS2V < -0.15f)
            {
                RCS1_up_on = false;
                RCS1_down_on = true;
                RCS1UpLight.enabled = false;
                RCS1DownLight.enabled = true;

                RCS2_up_on = false;
                RCS2_down_on = true;
                RCS2UpLight.enabled = false;
                RCS2DownLight.enabled = true;

                RCS3_up_on = false;
                RCS3_down_on = true;
                RCS3UpLight.enabled = false;
                RCS3DownLight.enabled = true;

                RCS4_up_on = false;
                RCS4_down_on = true;
                RCS4UpLight.enabled = false;
                RCS4DownLight.enabled = true;
            }
        }
    }

    private float GetMoonSurfaceDistance() {
        Vector3 MoonDistance = Moon.transform.position - transform.position;
        Altitude = (MoonDistance.magnitude - MOON_RADIUS_M ) / 1000;

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
        Vector3 moonGravity = Vector3.Normalize(  Moon.transform.position - transform.position) * MoonGravityForce;
        rb.AddForce( moonGravity, ForceMode.Acceleration);
      
        //double f = G_CONSTANT * MoonMass / (MOON_RADIUS_M+Altitude*1000);
        //double F = Math.Sqrt(f)/10000;
        //double v = Math.Sqrt((MOON_RADIUS_M + Altitude * 1000) * F);
        //rb.AddForceAtPosition(Moon.transform.position, Moon.transform.position, ForceMode.Impulse);
    }

    private void RCSApplyThrust()
    {

        float RCSForceK = RCSLateralForce ;


        if (RCS1_forward_on)
        {
            Debug.Log("RCS1L");
            rb.AddForceAtPosition(-RCS1ForwardDummy.transform.forward * RCSForceK, RCS1ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS1_side_on)
        {
            Debug.Log("RCS1R");
            rb.AddForceAtPosition(-RCS1SideDummy.transform.forward * RCSForceK, RCS1SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS1_up_on)
        {
            Debug.Log("RCS1U");
            rb.AddForceAtPosition(-RCS1UpDummy.transform.forward * RCSForceK, RCS1UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS1_down_on)
        {
            Debug.Log("RCS1D");
            rb.AddForceAtPosition(-RCS1DownDummy.transform.forward * RCSForceK, RCS1DownDummy.transform.position, ForceMode.Impulse);
        }


     
        if (RCS2_forward_on)
        {
            Debug.Log("RCS2L");
            rb.AddForceAtPosition(-RCS2ForwardDummy.transform.forward * RCSForceK, RCS2ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS2_side_on)
        {
            Debug.Log("RCS2R");
            rb.AddForceAtPosition(-RCS2SideDummy.transform.forward * RCSForceK, RCS2SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS2_up_on)
        {
            Debug.Log("RCS2U");
            rb.AddForceAtPosition(-RCS2UpDummy.transform.forward * RCSForceK, RCS2UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS2_down_on)
        {
            Debug.Log("RCS2D");
            rb.AddForceAtPosition(-RCS2DownDummy.transform.forward * RCSForceK, RCS2DownDummy.transform.position, ForceMode.Impulse);
        }







        if (RCS3_forward_on)
        {
            Debug.Log("RCS3L");
            rb.AddForceAtPosition(-RCS3ForwardDummy.transform.forward * RCSForceK, RCS3ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS3_side_on)
        {
            Debug.Log("RCS3R");
            rb.AddForceAtPosition(-RCS3SideDummy.transform.forward * RCSForceK, RCS3SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS3_up_on)
        {
            Debug.Log("RCS3U");
            rb.AddForceAtPosition(-RCS3UpDummy.transform.forward * RCSForceK, RCS3UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS3_down_on)
        {
            Debug.Log("RCS3D");
            rb.AddForceAtPosition(-RCS3DownDummy.transform.forward * RCSForceK, RCS3DownDummy.transform.position, ForceMode.Impulse);
        }





        if (RCS4_forward_on)
        {
            Debug.Log("RCS4L");
            rb.AddForceAtPosition(-RCS4ForwardDummy.transform.forward * RCSForceK, RCS4ForwardDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS4_side_on)
        {
            Debug.Log("RCS4R");
            rb.AddForceAtPosition(-RCS4SideDummy.transform.forward * RCSForceK, RCS4SideDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS4_up_on)
        {
            Debug.Log("RCS4U");
            rb.AddForceAtPosition(-RCS4UpDummy.transform.forward * RCSForceK, RCS4UpDummy.transform.position, ForceMode.Impulse);
        }

        if (RCS4_down_on)
        {
            Debug.Log("RCS4D");
            rb.AddForceAtPosition(-RCS4DownDummy.transform.forward * RCSForceK, RCS4DownDummy.transform.position,ForceMode.Impulse);
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

        if (RCSPropellantMass <= 0.0f)
            RCSPropellantMass = 0.0f;


        if (RCS1_forward_on || RCS1_side_on || RCS1_up_on || RCS1_down_on ||
            RCS2_forward_on || RCS2_side_on || RCS2_up_on || RCS2_down_on ||
            RCS3_forward_on || RCS3_side_on || RCS3_up_on || RCS3_down_on ||
            RCS4_forward_on || RCS4_side_on || RCS4_up_on || RCS4_down_on)
        {
            if (!RCSAudio.enabled)
                RCSAudio.enabled = true;
        }
        else {
            RCSAudio.enabled = false;
        }
    }

    private void DPSApplyThrust() {

        if (DPSPropellantMass <= 0.0f) {
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
        GUI.Label(new Rect(10, 30, 250, 20), "Yaw  Speed [Deg/s]: "  + GetYawSpeed());
        GUI.Label(new Rect(10, 50, 250, 20), "Roll Speed [Deg/s]: " + GetRollSpeed());
        GUI.Label(new Rect(10, 70, 250, 20), "Roll Speed [Deg/s]: " + GetPitchSpeed());
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