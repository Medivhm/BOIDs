using System;
using System.Collections.Generic;
using BOIDs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Runner : MonoBehaviour
{
    public int number;

    public GameObject prefab;
    public Transform parent;
    public float checkRadius;
    public float awayRadius;
    public float maxSpeed;

    public List<Boid> boids;
    public Dictionary<Boid, GameObject> boidGos;


    private void Awake()
    {
        boids = new List<Boid>();
        boidGos = new Dictionary<Boid, GameObject>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        Tick();
        number = boids.Count;
    }

    private void Tick()
    {
        OnLeftMouseDownTick();
        BoidsTick();
    }

    private void BoidsTick()
    {
        for(int i = 0; i < boids.Count;i++)
        {
            boids[i].Tick(Time.deltaTime, boids);
            boidGos[boids[i]].transform.position = new Vector3(boids[i].position.x, boids[i].position.y, 0f);
            boidGos[boids[i]].GetComponentInChildren<Image>().color = new Color(boids[i].color.x, boids[i].color.y, boids[i].color.z, 1f);
        }
    }

    private void OnLeftMouseDownTick()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var mousePos = Input.mousePosition;
            //SpawnNewBoid(new float2(mousePos.x, mousePos.y));
            for (int i = 0; i < 10; i++)
            {
                SpawnNewBoid(new float2(mousePos.x, mousePos.y));
            }
        }
    }

    private void SpawnNewBoid(float2 pos)
    {
        Boid boid = new Boid
        {
            maxSpeed = maxSpeed,
            position = pos,
            velocity = Randomf2() * maxSpeed,
            findRadius = checkRadius,
            awayRadius = awayRadius,
            outBorder = new float2(1910, 1070),
            color = new float3(Random01(), Random01(), Random01()),
        };

        boids.Add(boid);
        boidGos.Add(boid, GameObject.Instantiate(prefab, parent));
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
}
