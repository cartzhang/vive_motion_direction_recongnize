using UnityEngine;
using System.Collections;

public class CoordinateTest : MonoBehaviour
{
    //场景的相机，拖放进来
    public Camera camera;
    //场景的物体
    private GameObject obj;

    void Start()
    {
        //初始化
        obj = GameObject.Find("Cube");
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {

            print("世界坐标" + obj.transform.position);

            //print("屏幕坐标" + Input.mousePosition);

            //print("Plane 世界坐标→屏幕坐标" + camera.WorldToScreenPoint(obj.transform.position));

            //print("鼠标屏幕坐标→视口坐标" + camera.ScreenToViewportPoint(Input.mousePosition));

            //print("对象的坐标→视口坐标" + camera.ScreenToViewportPoint(obj.transform.position)); //这里若是三维世界坐标，结果就是不合法的，只有Z值（0，0，z）。
            Vector3 pos = camera.WorldToViewportPoint(obj.transform.position);
            print("Plane 世界坐标→视口坐标" + pos);   // 视口坐标系
            float angle = Vector2.Angle(new Vector2(pos.x - 0.5f, pos.y - 0.5f), Vector2.right);
            print("current vector angle is  " + angle);
        }

    }
}
