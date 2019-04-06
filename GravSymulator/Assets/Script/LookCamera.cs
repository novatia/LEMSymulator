using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{

    public float MinDist, CurrentDist, MaxDist, TranslateSpeed;

    private float LookHInput, LookVInput;

    public Transform Target;

    public void Update()
    {
        LookHInput = Input.GetAxis("LookH");
        LookVInput = Input.GetAxis("LookV");
        //CurrentDist += Input.GetAxis("Distance");
    }

    public void LateUpdate()
    {
        transform.RotateAround(Target.transform.position, Vector3.up, LookHInput * TranslateSpeed * Time.deltaTime*10);
        transform.RotateAround(Target.transform.position, Vector3.forward, LookVInput * TranslateSpeed * Time.deltaTime * 10);
    }
}
