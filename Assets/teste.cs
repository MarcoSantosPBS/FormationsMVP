using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class teste : MonoBehaviour
{
    public Transform debugAlvo;
    public Transform debugPivo;
    public Transform destination;
    private Vector3 pos;


    private void Start()
    {
        pos = new Vector3(-1 * 1.5f, 0, 0);
        debugAlvo.position = debugPivo.position + debugPivo.rotation * pos;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Quaternion formationRotation = Quaternion.LookRotation(destination.position - debugPivo.position);
            Vector3 worldOffset = formationRotation * pos;
            debugAlvo.position = debugPivo.position + worldOffset;
        }
        

    }

}
