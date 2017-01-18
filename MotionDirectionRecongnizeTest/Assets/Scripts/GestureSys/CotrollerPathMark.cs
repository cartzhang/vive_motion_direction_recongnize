using UnityEngine;
using System.Collections;
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
