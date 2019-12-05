using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadController : MonoBehaviour
{
    public static bool AI;
    public static string msg;
    public bool tailAccelSpeed;
    public float accelerationStep;
    public float linearSpeed;
    public float angularSpeed;

    private LineRenderer line;
    Vector3 eulerRotation;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        eulerRotation = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.state == GameManager.GameState.Play)
        {
            if (!AI)
            {
                switch (GameManager.control)
                {
                    case GameManager.Control.idle:
                        eulerRotation += Vector3.zero;
                        break;
                    case GameManager.Control.Up:
                        eulerRotation.x += angularSpeed;
                        break;
                    case GameManager.Control.Down:
                        eulerRotation.x -= angularSpeed;
                        break;
                    case GameManager.Control.Left:
                        eulerRotation.y -= angularSpeed;
                        break;
                    case GameManager.Control.Right:
                        eulerRotation.y += angularSpeed;
                        break;
                }
                transform.eulerAngles = eulerRotation;
                transform.position += transform.forward * linearSpeed * Time.deltaTime;
                GetComponent<FollowArray>().tailCounterText.transform.localRotation = Quaternion.Euler(-eulerRotation);
            }
            else
            {
                Vector3 direction = (GameManager.FindNearestFood(transform.position) - transform.position).normalized;
                transform.position += direction * linearSpeed * Time.deltaTime;
            }
            if (tailAccelSpeed) SetSpeed();
            line.SetPosition(0, transform.position);
            line.SetPosition(1, GameManager.FindNearestFood(transform.position));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Food")
        {
            if (GetComponent<FollowArray>().useGameObject)
                GetComponent<FollowArray>().AddTail();
            else
                GetComponent<FollowArray>().AddEntityTail();

            GameManager.DeleteFood(collision.transform);
        }
        else if(collision.transform.tag == "Tail")
        {
            msg = collision.transform.name;
            GameManager.eatTail = true;
        }
    }

    void SetSpeed()
    {
        float length = GetComponent<FollowArray>().length;
        linearSpeed = accelerationStep * Mathf.Log10(length * length + accelerationStep);
        FollowArray.moveSpeed = linearSpeed;
        Camera.main.GetComponent<TrackingFollow>().speed = linearSpeed;
    }
}
