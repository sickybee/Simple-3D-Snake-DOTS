using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public class FollowArray : MonoBehaviour
{
    [Tooltip("Set it before start")]
    public bool useGameObject;

    public Transform prefab;
    public int length = 3;
    public static float minDistance = 1f;
    public static float moveSpeed = 1f;
    public TextMesh tailCounterText;
    private List<Transform> tails;

    private Mesh prefabMesh;
    private Material prefabMaterial;

    private EntityManager _manager;
    private EntityArchetype tailType;
    private  NativeList<Entity> _tails;


    private void Awake()
    {
        if (useGameObject)
        {
            tails = new List<Transform>();
        }
        else
        {
            prefabMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            prefabMaterial = prefab.GetComponent<MeshRenderer>().sharedMaterial;
            _manager = World.Active.EntityManager;
            _tails = new NativeList<Entity>(Allocator.Persistent);

        }
    }

    private void OnEnable()
    {
        if (!useGameObject)
        {
            tailType = _manager.CreateArchetype(
                    typeof(TailComponent),
                    typeof(RenderMesh),
                    typeof(Translation),
                    typeof(MoveSpeed),
                    typeof(Direction),
                    typeof(StartFollow),
                    typeof(LocalToWorld)
                );
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (useGameObject)
        {
            for (int i = 0; i < length; i++)
            {
                if (i == 0)
                {
                    Transform tail = Instantiate(prefab, -transform.forward, Quaternion.identity);
                    tails.Add(tail);
                }
                else
                {
                    Transform tail = Instantiate(prefab, -tails[i - 1].forward * i, Quaternion.identity);
                    tails.Add(tail);
                }
            }
        }
        else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.state == GameManager.GameState.Play)
        {
            if (useGameObject)
            {
                for (int i = 0; i < length; i++)
                {
                    if (i == 0)
                        Follow(transform, tails[i]);
                    else
                        Follow(tails[i - 1], tails[i]);
                }
                tailCounterText.text = tails.Count.ToString();
            }
            else
            {
                for (int i = 0; i < _tails.Length; i++)
                {
                    _manager.SetComponentData(_tails[i], new MoveSpeed { value = moveSpeed });

                    if (i == 0)
                    {
                        FollowEntity(transform, _tails[i]);
                    }
                    else
                    {
                        FollowEntity(_tails[i - 1], _tails[i]);
                    }
                }
                tailCounterText.text = _tails.Length.ToString();
            }
        }
    }

    private void OnDisable()
    {

    }

    private void OnApplicationQuit()
    {
        if (!useGameObject)
        {
            _tails.Dispose();
        }
    }

void Follow(Transform leader, Transform follower)
    {
        if(Vector3.Distance(leader.position, follower.position) > minDistance)
        {
            Vector3 direction = (leader.position-follower.position).normalized;
            follower.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }

    void FollowEntity(Entity leader, Entity follower)
    {
        if (math.distance(_manager.GetComponentData<Translation>(leader).Value, _manager.GetComponentData<Translation>(follower).Value) > minDistance)
        {
            _manager.SetComponentData(follower, new StartFollow { value = true });
            _manager.SetComponentData(follower, new Direction {
                value = math.normalizesafe(
                        _manager.GetComponentData<Translation>(leader).Value - _manager.GetComponentData<Translation>(follower).Value
                    )
            });
        }
    }

    void FollowEntity(Transform leader, Entity follower)
    {
        if (math.distance(leader.position, _manager.GetComponentData<Translation>(follower).Value) > minDistance)
        { 
            _manager.SetComponentData(follower, new StartFollow { value = true });
            _manager.SetComponentData(follower, new Direction
            {
                value = math.normalizesafe(
                        (float3)leader.position - _manager.GetComponentData<Translation>(follower).Value
                    )
            });
        }
    }


    public void AddTail()
    {
        if (tails.Count > 0)
        {
            int lastTailIndex = tails.Count - 1;
            Transform tail = Instantiate(prefab, tails[lastTailIndex].position, Quaternion.identity);
            tail.transform.name = "Tail " + tails.Count;
            tails.Add(tail);
        }
        else
        {
            Transform tail = Instantiate(prefab, transform.position + transform.forward, Quaternion.identity);
            tail.name = "Tail " + tails.Count;
            tail.GetComponent<Collider>().enabled = false;
            tails.Add(tail);
        }
        length++;
        if (tails.Count > 10)
        {
            tails[1].GetComponent<Collider>().enabled = false;
            tails[2].GetComponent<Collider>().enabled = false;
            tails[3].GetComponent<Collider>().enabled = false;
            tails[4].GetComponent<Collider>().enabled = false;

        }
    }

    public void AddEntityTail()
    {
        Entity tail = _manager.CreateEntity(tailType);
        _manager.SetComponentData(tail, new TailComponent { index = _tails.Length });
        if (_tails.Length > 0)
        {
            float3 position = _manager.GetComponentData<Translation>(_tails[_tails.Length - 1]).Value;
            _manager.SetComponentData(tail, new MoveSpeed { addition = _tails.Length });
            _manager.SetComponentData(tail, new Translation { Value = position });
            _manager.SetSharedComponentData(tail, new RenderMesh { mesh = prefabMesh, material = prefabMaterial });
            _tails.Add(tail);
        }
        else
        {
            float3 position = transform.position + transform.forward;
            _manager.SetComponentData(tail, new MoveSpeed { value = GetComponent<HeadController>().linearSpeed });
            _manager.SetComponentData(tail, new Translation { Value = position });
            _manager.SetComponentData(tail, new Direction { value = transform.position });
            _manager.SetSharedComponentData(tail, new RenderMesh { mesh = prefabMesh, material = prefabMaterial });
            _tails.Add(tail);
        }
        length++;
    }
}

public struct Direction : IComponentData
{
    public float3 value;
}

public struct MoveSpeed : IComponentData
{
    public float value;
    public float addition;
}

public struct StartFollow : IComponentData
{
    public bool value;
}

public struct TailComponent : IComponentData
{
    public int index;
}

public class TailMoveSystem : JobComponentSystem
{
    [BurstCompile]
    struct TailMoveJob : IJobForEach<Translation, Direction, MoveSpeed, StartFollow>
    {
        public float deltaTime;

        public void Execute(
                ref Translation c0,
                [ReadOnly]ref Direction c1,
                [ReadOnly]ref MoveSpeed c2,
                [ReadOnly] ref StartFollow c3
            )
        {

            if (c3.value)
                c0.Value += c1.value * (c2.value + c2.addition) * deltaTime;
            else
            {
                c0.Value += float3.zero;
            }


        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new TailMoveJob { deltaTime = Time.deltaTime }.Schedule(this, inputDeps);
    }
}


    /*
    public class SetPositionList : JobComponentSystem
    {
        [BurstCompile]
        struct SetPositionJob : IJobForEach<Translation, MoveSpeed>
        {
            public NativeList<float3> position;
            public float moveSpeed;
            public void Execute([ReadOnly] ref Translation c0, ref MoveSpeed c1)
            {
                c1.value = moveSpeed;
            float3 currentPosition = c0.Value;
                position.Add(currentPosition);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new SetPositionJob { position = FollowArray.tailPosition, moveSpeed = FollowArray.moveSpeed }.Schedule(this, inputDeps);
        }
    }
    */