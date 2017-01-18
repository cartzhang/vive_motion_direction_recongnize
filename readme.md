
本文由catzhang 编写，转载请注明出处，所有权利保留。

博客地址：
http://blog.csdn.net/cartzhang

github地址：
https://github.com/cartzhang


## 一、概要

使用Vive手柄，我们需要做一个简单的姿势识别，用来判断手柄的运动方向，然后我根据需要做了一个运动方向的识别，根据上下左右和各个夹角的方向，总共有八个方向。

功能：基本实现了手柄的八方向运动方向识别，也可以叫动作识别。识别精度和效率，可以根据参数来调整。
里面也附带了一个通过射线识别的方法，当然有缺点在后面也做了分析和说明，若有问题，还请不吝指教。

同样工程在后面会给出源码和unity包，还有图片路径地址。


## 二、 实现原理和实现过程

### 1 实现原理

原理很简单，就是根据路线，在一定时间内，记录路线数据，然后把数据映射到相机平面上，在根据取第一个点、中间点以及最后一个点，计算角度和斜率，在一定度数之内，都认为是某个方向的运行识别成功。

![image](H:\Unity\UnityProject\vive_motion_direction_recongnize\img\a.jpg)

### 2 实现过程

#### 1. 项目使用之前的vive消息解耦传递方法。

使用了Unity的5.4.0f3版本，也使用了之前的vive的Event项目的消息传递机制。
项目地址，把它放在了图说VR的工程中

https://github.com/cartzhang/ImgSayVRabc/tree/master/ViveEventDemo

希望有需要可以迁出和修改，提交，前几天也做了一点点的修改和提交。
当然，根据惯例，本项目的源码也会在项目后面分享出来，并给出到处的unity包，贴心吧。

#### 2. 项目说明

打开下载好的项目，打开DemoScene中SteamVR_motion_direction_recongnize这个场景。
可以看到检视板中，内容不算多。

![image](https://github.com/cartzhang/vive_motion_direction_recongnize/blob/master/img/00.png)


看看工程中大致的内容，包括样例场景，预制体对象，脚本和steamVR原插件中的内容。
当然SteamVR中代码已经做了部分修改，这个之前Event消息解耦使用所添加的一点点代码。

![image](https://github.com/cartzhang/vive_motion_direction_recongnize/blob/master/img/01.png)

若你需要了解可以参考：

http://blog.csdn.net/cartzhang/article/details/53915229

https://github.com/cartzhang/ImgSayVRabc/tree/master/ViveEventDemo

有不足之处还请不吝指教。

#### 3. 对相机渲染层的设置

因为这里有Vive的头盔中项目camera(eye)，需要把层设置为不渲染UI。

![image](https://github.com/cartzhang/vive_motion_direction_recongnize/blob/master/img/02.png)

然后也同样对UI项目做了处理，让他只渲染UI。

![image](https://github.com/cartzhang/vive_motion_direction_recongnize/blob/master/img/03.png)


#### 4. 手柄运动方向识别

主要的脚本为：GestureJudge.cs。
我在测试场景中，只使用了右侧手柄来处理，这里没有在left左侧手柄挂载代码，若有需要，可以自己添加，是没有问题的。

![image](https://github.com/cartzhang/vive_motion_direction_recongnize/blob/master/img/04.png)

可以通过调节参数来识别效果。

**
挥动超过距离：手柄在识别过程中的路线，必须大于一定长度才算数。这里设置的为0.8f.

数据跟踪的最短距离: 在同一个位置，间隔太小的手柄位置，不作为参考数据。手柄运动之间间隔大于这个值，才有效的位置数据。

检测周期：数值越大识别越容易。但是过大，就会提高误识别率。默认为0.15秒。测试结果到0.3s也是可以的，这个值作为参数吧。数据过了这个时间，就重新清零检测。

检测角度：我们本来检测的是八个方向，在把个方向一定角度内都算是某个方向的，这就是冗余，因为没有冗余，必须直直的路线才可以的。
**


#### 5. 手柄路径标识

写了个小代码来标识手柄的运动路线。
就是在手柄的位置，每间隔一段距离就放置一个红色小球来表示。

代码：


```

/// <summary>
/// 标记手柄路经。
/// @cartzhang
/// </summary>
public class CotrollerPathMark : MonoBehaviour
{
    public GameObject spehere;
    public float StepDistance = 0.1f;
    private Vector3 currentPos;    
	// Use this for initialization
	void Start ()
    {
        currentPos = this.transform.position;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if ( Vector3.Distance(currentPos, this.transform.position) > StepDistance)
        {
            GameObject Tmpobj = Instantiate(spehere, currentPos,Quaternion.identity) as GameObject;
            currentPos = transform.position;
            Destroy(Tmpobj, 0.3f);
        }
	
	}
}
```



#### 6. 结果

![image](https://github.com/cartzhang/vive_motion_direction_recongnize/blob/master/img/05.png)

在键盘上按下S键后，就可以挥动手柄来测试结果了。结果会实时在头盔中文字提示。

基本在正常速度下，都可以识别出来。

识别出来的结果还是通知来实现。若需要在根据结果来操作，只需要简单的订阅消息就可以了。是不是很简单。

## 三、识别代码解析

#### 1 代码


```
using UnityEngine;
using System.Collections;
using SLQJ;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// 主要功能：
/// 1.实现8中不同方向上的姿势识别
/// 2.实现输入矢量来判断是否完成对应路线。 
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

        CheckGestureByRay();
        CheckGetsturebyCoordinate();
    }

    void GestureRecongnize(MessageObject obj)
    {
        object[] objArray = (object[])obj.MsgValue;
        StartCoroutine(RecongnizeGetsture(Vector3.zero,(float)objArray[1]));
    }

    /// <summary>
    /// 射线检测
    /// </summary>
    /// <param name="getstureVec"></param>
    /// <param name="TimeToDectect"></param>
    /// <returns></returns>    
    IEnumerator RecongnizeGetsture(Vector3 getstureVec, float TimeToDectect)
    {
        Debug.Log("start recongnize");
        bOutputResult = false;
        gestureType = GestureType.None;
        bStartCheck = true;
        currentStepTime = stepTime;
        while (TimeToDectect > 0)
        {
            if (bOutputResult || gestureType != GestureType.None)
            {
                Debug.Log("jump out while");
				#if !UNITY_EDITOR
                //for test
                TimeToDectect = 0;
				#endif
            }
            yield return null;
            TimeToDectect -= Time.deltaTime;            
        }
        bStartCheck = false;
        Debug.Log("begin notify");
        NotificationManager.Instance.Notify(
               NotificationType.Gesture_Recongnize_Result.ToString(), bOutputResult);
        NotificationManager.Instance.Notify(
               NotificationType.Gesture_Recongnize_Result.ToString(), gestureType);
    }

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
        gestureType = GestureType.None;
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

    void StartUnderEditorTest()
    {
        NotificationManager.Instance.Subscribe(
              NotificationType.Gesture_Recongnize_Result.ToString(), GetRecongnizeResult);
    }
    
    void UpdateUnderEditor()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            NotificationManager.Instance.Notify(
                NotificationType.Gesture_Recongnize.ToString(),new Vector3(5,0,0),1000.0f);
            //Debug.Log("start to check gesture");
        }
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

```

识别的代码并算多，但也不少。我把类写成了两个部分，一部分就是常规的算法实现过程，另一部分就是辅助操作的代码，包括按下按键S开始识别运动方向，实时绘制射线，消息的通知和订阅等，结果的显示。

#### 2  方向枚举类
首先给八个方向定义了一个枚举类。


```

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
```


这里说明，我实现了两种方向判断的，一种是用射线来判断的，对应函数CheckGestureByRay（），而另一种方法，是根据开头文章说的，根据路线方向通过矢量判断。

#### 3 射线判断方法及其优缺点

射线判断方法，使用了先判断射线射中的对象，通过层过滤和标签过滤来判断是否通过A点，然后在通过B点，这里就返回了。

优点是，几乎可以识别相机平面的所有方向，不在意朝向和位置，只要碰撞盒对就没有问题。

缺点就是，需设置层和标签。还有就是移动速度也不能过高，因为他会来不及计算或穿透过碰撞体，也可能会发生的。

说明，这个方法在的demo场景中并没有展示。有需要的可以找我或自己也可以根据代码稍微设置下tag和Layer，几乎无难度。

#### 4 路径跟踪计算及其优缺点

路径跟踪计算，根据记录来判断有限长度序列中的点，判断与X轴

优点是，可以屏蔽转向中，不会误识别成功。计算量其实也不大，效率也可以。可以根据需要随时调节参数，得到想要的结果。

缺点：算法需要转换为相机矩阵下的平面，因为手柄在移动过程中，相机也在移动，造成数据点在不同坐标平面下，可能会造成误识别。若每个点都计算在当前下，在整体计算过程中，就会得到记录的坐标点大家都不在一个坐标下生成的尴尬情况，这样更不能保证是否精确了。（希望大家有好的方法或想法，还请不吝指教。）


还有就是在代码中

```

 	/// <summary>
    /// 姿势判断。
    /// </summary>
    private void CalcuolateDirection()

```

里面使用了很多人不太乐意的goto。其实，我大部分或说绝大部分时间都不希望这样用的，但是这里觉得可以用一些。



## 源码地址

源码地址：

https://github.com/cartzhang/vive_motion_direction_recongnize

整个工程包下载地址：

https://github.com/cartzhang/vive_motion_direction_recongnize/raw/master/MotionDirectionRecongnizeTest/vive_motion_direction_recongnize_cartzhang.unitypackage


图片地址：

https://github.com/cartzhang/vive_motion_direction_recongnize/tree/master/img


## 参考

[1] https://github.com/cartzhang/ImgSayVRabc/tree/master/ViveEventDemo

[2] http://blog.csdn.net/cartzhang/article/details/53915229


