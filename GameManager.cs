using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Control { Up, Down, Left, Right, idle };
    public enum GameState { Start, Play, Win, GameOver };

    [Header("Pawn Setup")]
    public Transform foodPrefab;
    public float spawnFoodInterval;
    public float spawnRange = 50f;
    public int startSpawnAmount = 100;
    public GameObject head;

    [Header("Control Setup")]
    public bool AIControl;
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public static Control control;

    private static List<Transform> foods;
    private float spawnTimer;
    public static GameState state;
    public static bool eatTail;

    [TextArea]
    public string DbgMsg;

    private void Awake()
    {
        foods = new List<Transform>();
    }
    // Start is called before the first frame update
    void Start()
    {
        state = GameState.Start;
        control = Control.idle;
        eatTail = false;
    }

    private void OnGUI()
    {



        switch (state)
        {
            case GameState.Start:
                head.SetActive(false);
                GUIStyle startLabel = GUI.skin.GetStyle("Label");
                startLabel.fontSize = 50;
                GUI.Label(new Rect(Screen.width / 2f-100f, Screen.height/2f, 800f, 100f), "3D Snake", startLabel);
                if(GUI.Button(new Rect(Screen.width / 2f-100f, Screen.height / 2f+80f, 200f, 40f), "Start"))
                {
                    for (int i = 0; i < startSpawnAmount; i++)
                    {
                        SpawnFood();
                    }
                    state = GameState.Play;
                }
                break;
            case GameState.Play:
                head.SetActive(true);
                GUIStyle foodCountLabel = GUI.skin.GetStyle("Label");
                foodCountLabel.fontSize = 15;
                GUI.Label(new Rect(Screen.width / 2f, Screen.height * 7f / 8f, 200f, 40f), "Food Available : " + foods.Count, foodCountLabel);
                break;
            case GameState.Win:
                GUIStyle winLabel = GUI.skin.GetStyle("Label");
                winLabel.fontSize = 50;
                GUI.Label(new Rect(Screen.width / 2f - 100f, Screen.height / 2f, 800f, 100f), "WINNER !!!", winLabel);
                head.SetActive(false);
                break;
            case GameState.GameOver:
                int foodLeft = foods.Count;
                GUIStyle gameoverLabel = GUI.skin.GetStyle("Label");
                gameoverLabel.fontSize = 50;
                GUI.Label(new Rect(Screen.width / 2f - 100f, Screen.height / 2f, 800f, 100f), "GAME OVER", gameoverLabel);
                GUIStyle foodleftLabel = GUI.skin.GetStyle("Label");
                foodleftLabel.fontSize = 40;
                GUI.Label(new Rect(Screen.width / 2f-75f, Screen.height / 2f+200f, 800f, 100f), "Food left : "+ foodLeft, gameoverLabel);

                head.SetActive(false);
                break;
        }


    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case GameState.Start:

                break;
            case GameState.Play:

                spawnTimer += Time.deltaTime;
                if (spawnTimer > spawnFoodInterval)
                {
                    SpawnFood();
                    spawnTimer = 0f;
                }
                RotateFoods();
                HeadController.AI = AIControl;
                control = GetControl(); spawnTimer += Time.deltaTime;
                if (spawnTimer > spawnFoodInterval)
                {
                    SpawnFood();
                    spawnTimer = 0f;
                }
                RotateFoods();
                HeadController.AI = AIControl;
                control = GetControl();

                if (eatTail)
                    DbgMsg = "Eat tail " + HeadController.msg;

                if (eatTail)
                    state = GameState.GameOver;
                else if (foods.Count == 0)
                    state = GameState.Win;
                break;
            case GameState.GameOver:
                break;
        }
    }

    private Control GetControl()
    {
        Control pressed = Control.idle;

        if (Input.GetKey(upKey))
        {
            pressed = Control.Up;
        }
        if (Input.GetKey(downKey))
        {
            pressed = Control.Down;
        }
        if (Input.GetKey(leftKey))
        {
            pressed = Control.Left;
        }
        if (Input.GetKey(rightKey))
        {
            pressed = Control.Right;
        }
        return pressed;
    }

    void SpawnFood()
    {
        Transform food = Instantiate(foodPrefab, new Vector3(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange)),
            Quaternion.identity
            );
        foods.Add(food);
    }

    void RotateFoods()
    {
        foreach(Transform food in foods)
        {
            food.Rotate(new Vector3(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) * 45f * Time.deltaTime);
        }
    }

    public static void DeleteFood(Transform food)
    {
        foods.Remove(food);
        food.gameObject.SetActive(false);
        Destroy(food.gameObject, 1f);
    }

    public static Vector3 FindNearestFood(Vector3 position)
    {
        Vector3 nearest = new Vector3(500f, 500f, 500f);
        for(int i=0; i < foods.Count; i++)
        {
            if(Vector3.Distance(foods[i].position, position) < Vector3.Distance(nearest, position))
            {
                nearest = foods[i].position;
            }
        }
        return nearest;
    }
}
