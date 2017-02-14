#define  NotUse_Gestrure_ByRay
#define  NoTest_Gesture_Code
using UnityEngine;
using System.Collections;
using SLQJ;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// 主要功能：
/// 1.实现8中不同方向上的姿势识别
/// 注意：每次只能识别一个姿势，需要等识别完毕，才能下一个。
/// @cartzhang
/// </summary>

public enum GestureType:int
{
    None = 0,
    Left_Right,
    LeftDown_RightUp,
    Down_Up,
    RightDown_LeftUp,
    Right_Left,
    RightUp_LeftDown,
    Up_Down,
    LeftUp_RightDown
}

public partial class GuestureJudge : MonoBehaviour
{
    public Text showState;
    private bool isStartRecongnize = false;
    [Header("挥动需要超过的距离 default 0.8")]
    public float RecongnizeMinStepDistance = 0.8f;
    [Header("数据跟踪的最短距离 default 0.08")]
    public float addListMinStepDist = 0.08f;
    [Header("检测周期时间,default 0.15")]
    public float stepTime = 0.15f;
    [Header("检测角度的最小冗余")]
    [Range(5,15)]
    public float MaxAnlgeToConfirm = 15f; // 8个方向都有15度的间隔

    int arrowLayer = 10;
    int layerMask;
    int step = 0;
    float currentStepTime;
    ///开始检测标志
    private bool bStartCheck = false;
    //检测结果标志
    private bool bOutputResult = false;
    private GestureType gestureType = GestureType.None;
    private Camera mainCamera;

    void Start()
    {
        arrowLayer = 10;
        layerMask = 1 << arrowLayer;
        currentStepTime = stepTime;
        isStartRecongnize = false;
        bOutputResult = false;
        gestureType = GestureType.None;
        NotificationManager.Instance.Subscribe(NotificationType.Gesture_Recongnize.ToString(), GestureRecongnize);
        StartUnderEditorTest();
        mainCamera = Camera.main;
    }

    void Update()
    {
        UpdateUnderEditor();
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, transform.forward, Color.red);
#endif
#if !NotUse_Gestrure_ByRay
        CheckGestureByRay();
#endif
        CheckGetsturebyCoordinate();
    }

    void GestureRecongnize(MessageObject obj)
    {
        object[] objArray = (object[])obj.MsgValue;
        StartCoroutine(RecongnizeGetsture((GestureType)objArray[0], (float)objArray[1]));
    }

    /// <summary>
    /// 射线检测
    /// </summary>
    /// <param name="getstureVec"></param>
    /// <param name="TimeToDectect"></param>
    /// <returns></returns>    
    IEnumerator RecongnizeGetsture(GestureType reconGetstureType, float TimeToDectect)
    {
        Debug.Log("start recongnize");
        bOutputResult = false;
        gestureType = GestureType.None;
        bStartCheck = true;
        currentStepTime = stepTime;
        while (TimeToDectect > 0)
        {
            if (bOutputResult || gestureType == reconGetstureType)
            {
                Debug.Log("jump out while loop");
                TimeToDectect = 0;
            }
            yield return null;
            TimeToDectect -= Time.deltaTime;            
        }
        bStartCheck = false;
        Debug.Log("begin notify");
#if !NotUse_Gestrure_ByRay
        NotificationManager.Instance.Notify(
               NotificationType.Gesture_Recongnize_Result.ToString(), bOutputResult);
#endif
        NotificationManager.Instance.Notify(
               NotificationType.Gesture_Recongnize_Result.ToString(), gestureType);
    }

#if !NotUse_Gestrure_ByRay

    /// <summary>
    /// 射线来检测碰撞体标签
    /// </summary>
    private void CheckGestureByRay()
    {
        if (!bStartCheck)
        {
            return;
        }
        currentStepTime -= Time.deltaTime;
        if (currentStepTime <= 0)
        {
            currentStepTime = stepTime;
            step = 0;
        }

        // 动作判断
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 6f, layerMask))
        {
            if (hit.collider.tag == "ArrowA")
            {
                Debug.Log("collison A");
                step = 1;
            }
            if (hit.collider.tag == "ArrowB" && step == 1)
            {
                Debug.Log("collison B");
                step = 2;
                bOutputResult = true;
                bStartCheck = false;
                //Destroy(hit.transform.parent.gameObject, 0);
            }
        }
    }
#endif

    #region  Test by use coordinate

    /// <summary>
    /// 每隔一帧采样数据，然后在时间间隔内
    /// </summary>
    /// <param name="getstureVec"></param>
    /// <param name="TimeToDectect"></param>
    /// <returns></returns>
    private bool bInitialOnce = false;
    private int linkListMaxLength;
    private List<Vector3> recordPosList;
    private List<Vector3> recordWorldToViewPosList;
    private Vector3[] SamplePos = new Vector3[3];
    private int UpdateLenToCheck = 5;
    private int iNewAddCount = 0;
    private void CheckGetsturebyCoordinate()
    {
        if (!bStartCheck)
        {
            return;
        }
        // 1. 初始化数据表，并把数据转换到相机视口坐标系上。
        Vector3 currentRecordPos = transform.position;
        Vector3 currentWVPPos = mainCamera.WorldToViewportPoint(currentRecordPos);        
        if (!bInitialOnce)
        {
            bInitialOnce = true;
            linkListMaxLength = (int)(RecongnizeMinStepDistance / addListMinStepDist);
            linkListMaxLength = linkListMaxLength < 5 ? 5 : linkListMaxLength; // 最小存5个数据
            recordPosList = new List<Vector3>(linkListMaxLength);
            recordWorldToViewPosList = new List<Vector3>(linkListMaxLength);
            recordPosList.Add(currentRecordPos);
            recordWorldToViewPosList.Add(currentWVPPos);
            UpdateLenToCheck = (int)(linkListMaxLength * 0.4f);
            iNewAddCount = 0;
        }

        //2. 数据加入，最小距离判断是否符合加入条件
        if (Vector3.Distance(recordPosList[recordPosList.Count - 1], currentRecordPos) > addListMinStepDist)
        {
            recordPosList.Add(currentRecordPos);
            recordWorldToViewPosList.Add(currentWVPPos);
            iNewAddCount++;
        }
        //3. list 的数据的添加和刷新，每次新入多少数据，重新开启检测。
        int currentListLen = recordPosList.Count;
        if (currentListLen >= linkListMaxLength)
        {   
            // 刷新iNewAddCount个数据，再做检测。
            if (iNewAddCount > UpdateLenToCheck)
            {
                iNewAddCount = 0;
                // 4. 移动行程换算角度来做判断
                if (Vector3.Distance(recordPosList[0], recordPosList[currentListLen - 1]) > RecongnizeMinStepDistance * 0.3f)
                {
                    int middleIndex = currentListLen >> 1;
                    SamplePos[0] = (middleIndex <= 3) ? recordWorldToViewPosList[0]:
                        (recordWorldToViewPosList[0] + recordWorldToViewPosList[1] + recordWorldToViewPosList[2])/3;
                    SamplePos[1] = (middleIndex <= 3) ? recordWorldToViewPosList[middleIndex]: 
                        (recordWorldToViewPosList[middleIndex-1] + recordWorldToViewPosList[middleIndex] + recordWorldToViewPosList[middleIndex+1]) / 3;
                    SamplePos[2] = (middleIndex <= 3) ? recordWorldToViewPosList[currentListLen - 1]:
                        (recordWorldToViewPosList[currentListLen - 1] + recordWorldToViewPosList[currentListLen - 2]) / 2;
                    CalcuolateDirection();
                    showState.text = gestureType.ToString();
                    if (gestureType != GestureType.None)
                    {
                        Debug.Log("current recongnize gesture is " + gestureType.ToString());
                    }
                }
            }
            // 移除首位，并开始判断
            recordPosList.RemoveAt(0);
            recordWorldToViewPosList.RemoveAt(0);
            //Debug.Log("current recongnize gesture is " + gestureType.ToString());
        }
    }
    /// <summary>
    /// 姿势判断。
    /// </summary>
    private void CalcuolateDirection()
    {   
        
        float anlge1 = GetAngleWithX(SamplePos[1] - SamplePos[0]);
        float anlge2 = GetAngleWithX(SamplePos[2] - SamplePos[1]);
        float stepAngle = 45;   // 8平分角度间隔
        int step = 0;
        //1. X轴0度夹角附近
        if ((anlge1 < MaxAnlgeToConfirm  + stepAngle * step || (anlge1 > stepAngle * 8 - MaxAnlgeToConfirm && anlge1 < 8 * stepAngle)) &&
            // 
            (anlge2 < MaxAnlgeToConfirm + stepAngle * step || (anlge2 > stepAngle * 8 - MaxAnlgeToConfirm && anlge2 < 8 * stepAngle))
           )
        {
            gestureType = GestureType.Left_Right;
            goto CDEND;
        }
        
        //2. X轴逆时针方向计算
        for (step = 1; step < 8; step++)
        {
            if ((anlge1 > stepAngle * step - MaxAnlgeToConfirm && anlge1 < stepAngle * step + MaxAnlgeToConfirm) &&
                (anlge2 > stepAngle * step - MaxAnlgeToConfirm && anlge2 < stepAngle * step + MaxAnlgeToConfirm))
            {
                gestureType = (GestureType)(step + 1);
                break;
            }
        }
    CDEND: return;
    }

    /// <summary>
    /// 计算与X轴夹角
    /// </summary>
    /// <param name="pos3D"></param>
    /// <returns></returns>
    private float GetAngleWithX(Vector3 pos3D)
    {
        float angleOutput = Vector2.Angle(new Vector2(pos3D.x, pos3D.y), Vector2.right);
        if (pos3D.y <= 0)
        {
            angleOutput = 360 - angleOutput;
        }
        if (angleOutput <= 0)
        {
            return 0;
        }
        return angleOutput;
    }
    #endregion
}


/// <summary>
/// 动作识别的调用和结果返回。
/// </summary>
public partial class GuestureJudge
{
    private GestureType needGestureType = GestureType.Left_Right;
    void StartUnderEditorTest()
    {
#if !NoTest_Gesture_Code
        NotificationManager.Instance.Subscribe(
              NotificationType.Gesture_Recongnize_Result.ToString(), GetRecongnizeResult);
#endif
    }
    
    void UpdateUnderEditor()
    {
        #if !NoTest_Gesture_Code
        // 1000.0 senconds is too long for recongnizition,so u can change it as you need.
        if (Input.GetKeyDown(KeyCode.S))
        {
            NotificationManager.Instance.Notify(
                NotificationType.Gesture_Recongnize.ToString(),needGestureType,1000.0f);            
            needGestureType = (GestureType)((int)needGestureType + 1);
        }
        #endif
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 100f));
    }

    void GetRecongnizeResult(MessageObject obj)
    {
        Debug.Log("recongnize output is " + obj.MsgValue);
    }
#endif
    }
