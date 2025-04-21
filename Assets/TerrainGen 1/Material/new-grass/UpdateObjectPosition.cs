using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UpdateObjectPosition : MonoBehaviour
{
    public Transform Obj;
    //sdds

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_ObjPosition", Obj.position);
    }
}
