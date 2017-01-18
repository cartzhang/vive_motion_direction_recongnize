using UnityEngine;
using System.Collections;
using Valve.VR;

public class GetControllerInfor : MonoBehaviour
{
    public VRControllerState_t controllerState;
    public TrackedDevicePose_t DevicePoseState;
    private ETrackingUniverseOrigin trackingUniverOrig = ETrackingUniverseOrigin.TrackingUniverseStanding;
    private uint controllerIndex;
    public Vector3 velocity;
    public Vector3 angleVecity;

    private Rigidbody rigibody;

    private void Start()
    {
        controllerIndex = (uint)GetComponent<SteamVR_TrackedObject>().index;
        velocity = angleVecity = Vector3.zero;
    }

    void ShowPosState()
    {
        var system = OpenVR.System;
        if (system != null && system.GetControllerStateWithPose(trackingUniverOrig,controllerIndex, ref controllerState, ref DevicePoseState))
        {  
            if (DevicePoseState.bPoseIsValid && DevicePoseState.bDeviceIsConnected)
            {
                velocity = new Vector3(DevicePoseState.vVelocity.v0, DevicePoseState.vVelocity.v1, -DevicePoseState.vVelocity.v2);
                angleVecity = new Vector3(-DevicePoseState.vAngularVelocity.v0, -DevicePoseState.vAngularVelocity.v1, DevicePoseState.vAngularVelocity.v2);
                Debug.Log("current GamePoseState velocity is " + velocity + " 角速度" + angleVecity);
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        ShowPosState();
    }
}
