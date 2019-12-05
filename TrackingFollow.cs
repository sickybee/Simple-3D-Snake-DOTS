using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingFollow : MonoBehaviour
{
    public Transform target;
    public float minDistance;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) > minDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }

        transform.LookAt(target.parent);
    }
}
