using UnityEngine;
using System.Collections;

public class CreateArrowRandom : MonoBehaviour
{
    public GameObject Arrow;
    private SphereCollider sphereCollider;
    private int ArrowIndex;
    private Vector3 center;
    private float Radius;
         
    // Use this for initialization
    void Start()
    {
        ArrowIndex = 0;
        sphereCollider = GetComponent<SphereCollider>();
        Radius = sphereCollider.radius;
        center = sphereCollider.center;
        InvokeRepeating("CheckArrowNumbers",5f,1);
    }
	
	// Update is called once per frame
	void CheckArrowNumbers ()
    {
        GameObject gobj = GameObject.Find("Arrow(Clone)");
        if (null == gobj)
        {
            CreateArrowAuto();
        }
	}

    void CreateArrowAuto()
    {
        Vector3 tmpvec = Random.onUnitSphere * Radius;
        Instantiate(Arrow, this.transform.position + new Vector3(tmpvec.x,0, tmpvec.z) , Quaternion.AngleAxis(Random.Range(0, 180), Vector3.up));
    }
}
