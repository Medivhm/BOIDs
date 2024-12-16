using System;
using System.Collections.Generic;
using System.Linq;
using BOIDs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;

public class RunnerJob : MonoBehaviour
{
    public int number;

    public GameObject prefab;
    public Transform parent;
    public float checkRadius;
    public float awayRadius;
    public float maxSpeed;

    //public List<Boid> boids;
    //public Dictionary<Boid, Transform> boidTrans;
    BoidSetting setting;

    JobHandle boidHandle;
    TransformAccessArray transformArray;
    public NativeArray<float2> positions;
    public NativeArray<float2> velocitys;
    public NativeArray<float3> colors;
    bool dirty = true;

    List<float2> positionsTemp;
    List<float2> velocitysTemp;
    List<float3> colorsTemp;
    List<Transform> boidTrans;

    private void Awake()
    {
        setting = new BoidSetting()
        {
            maxSpeed = maxSpeed,
            findRadius = checkRadius,
            awayRadius = awayRadius,
            outBorder = new float2(1910, 1070),
        };
        positionsTemp = new List<float2>();
        velocitysTemp = new List<float2>();
        colorsTemp = new List<float3>();
        boidTrans = new List<Transform>();
    }

    private void Update()
    {
        Tick();
        number = boidTrans.Count;
    }

    private void Tick()
    {
        OnLeftMouseDownTick();
        //BoidsTick();
        JobTick();
    }

    private void JobTick()
    {
        if (!boidHandle.IsCompleted)
        {
            return;
        }
        else
        {
            boidHandle.Complete();
        }

        if (dirty)
        {
            SetJobAndArray();
            dirty = false;
        }


        for(int i = 0; i < boidTrans.Count(); i++)
        {
            boidTrans[i].GetComponentInChildren<Image>().color = new Color(colors[i].x, colors[i].y, colors[i].z);
        }
        var boidJob = new BoidJob()
        {
            dt = Time.deltaTime,
            setting = setting,
            positions = positions,
            velocitys = velocitys,
            colors = colors,
        };
        boidHandle = boidJob.Schedule(transformArray);
    }

    private void SetJobAndArray()
    {
        // 备份数据
        for (int i = 0; i < positions.Length; i++)
        {
            positionsTemp[i] = positions[i];
        }
        for (int i = 0; i < velocitys.Length; i++)
        {
            velocitysTemp[i] = velocitys[i];
        }
        for (int i = 0; i < colors.Length; i++)
        {
            colorsTemp[i] = colors[i];
        }

        Dispose();
        positions = new NativeArray<float2>(boidTrans.Count, Allocator.Persistent);
        velocitys = new NativeArray<float2>(boidTrans.Count, Allocator.Persistent);
        colors = new NativeArray<float3>(boidTrans.Count, Allocator.Persistent);
        transformArray = new TransformAccessArray(boidTrans.ToArray());

        for (int i = 0; i < positionsTemp.Count; i++)
        {
            positions[i] = positionsTemp[i];
        }
        for (int i = 0; i < velocitysTemp.Count; i++)
        {
            velocitys[i] = velocitysTemp[i];
        }
        for (int i = 0; i < colorsTemp.Count; i++)
        {
            colors[i] = colorsTemp[i];
        }
    }

    private void Dispose()
    {
        boidHandle.Complete();
        if (positions.IsCreated)
        {
            positions.Dispose();
        }
        if(velocitys.IsCreated)
        {
            velocitys.Dispose();
        }
        if (colors.IsCreated)
        {
            colors.Dispose();
        }
        if (transformArray.isCreated)
        {
            transformArray.Dispose();
        }
    }

    //private void BoidsTick()
    //{
    //    for(int i = 0; i < boids.Count;i++)
    //    {
    //        boids[i].Tick(Time.deltaTime, boids);
    //        boidTrans[boids[i]].position = new Vector3(boids[i].position.x, boids[i].position.y, 0f);
    //        boidTrans[boids[i]].GetComponentInChildren<Image>().color = new Color(boids[i].color.x, boids[i].color.y, boids[i].color.z, 1f);
    //    }
    //}

    private void OnLeftMouseDownTick()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var mousePos = Input.mousePosition;
            SpawnNewBoid(new float2(mousePos.x, mousePos.y));
            for (int i = 0; i < 10; i++)
            {
                SpawnNewBoid(new float2(mousePos.x, mousePos.y));
            }
            dirty = true;
        }
    }

    private void SpawnNewBoid(float2 pos)
    {
        //Boid boid = new Boid
        //{
        //    maxSpeed = maxSpeed,
        //    position = pos,
        //    velocity = Randomf2() * maxSpeed,
        //    findRadius = checkRadius,
        //    awayRadius = awayRadius,
        //    outBorder = new float2(1910, 1070),
        //    color = new float3(Random01(), Random01(), Random01()),
        //};

        float2 position = pos;
        float2 velocity = Randomf2() * maxSpeed;
        float3 color = new float3(Random01(), Random01(), Random01());

        positionsTemp.Add(position);
        velocitysTemp.Add(velocity);
        colorsTemp.Add(color);
        boidTrans.Add(GameObject.Instantiate(prefab, parent).transform);
    }

    private float2 Randomf2()
    {
        return math.normalize(new float2(GetFlag() * Random01(), GetFlag() * Random01()));
    }

    private float Random01()
    {
        return UnityEngine.Random.value;
    }

    private float GetFlag()
    {
        if(Random01() > 0.5f)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }
}
