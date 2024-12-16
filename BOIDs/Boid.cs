using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace BOIDs
{
    [BurstCompile]
    public class Boid
    {
        public float maxSpeed;
        public float2 position;
        public float2 velocity;
        public float findRadius;   // 单位可视群体范围
        public float awayRadius;   // 单位间保持距离
        public float2 outBorder;   // 外框
        public float3 color;

        public void Tick(float dt, List<Boid> boids)
        {
            color = ColorAvg(boids);

            var awoidWall = AvoidWall() * maxSpeed;
            var sepVel = Separation(boids) * 1.3f;
            var cohVel = Cohesion(boids);
            var aliVel = Alignment(boids);
            velocity = velocity + sepVel + cohVel + aliVel + awoidWall;

            velocity = ClampMagnitude(velocity, maxSpeed);
            position += velocity * maxSpeed * dt;
        }

        // 分离
        private float2 Separation(List<Boid> boids)
        {
            float2 myPos = position;
            float away = awayRadius;
            int count = boids.Count;
            if (count == 0)
                return new float2(0);

            float2 velAddition = 0;
            for (int i = 0; i < count; i++)
            {
                float dis = GetDistance(myPos, boids[i].position);
                if (dis > 0 && dis < away)
                {
                    velAddition += (myPos - boids[i].position) / dis;
                }
            }

            return velAddition;
        }

        // 颜色靠近
        private float3 ColorAvg(List<Boid> boids)
        {
            float2 myPos = position;
            float radius = findRadius;
            int count = boids.Count;
            if (count == 0)
                return color;

            float3 colorAddition = new float3(0);
            int addCount = 0;
            for (int i = 0; i < count; i++)
            {
                float dis = GetDistance(myPos, boids[i].position);
                if (dis < radius)  // 筛选出范围内的群体
                {
                    addCount++;
                    colorAddition += boids[i].color;
                }
            }

            if (addCount == 0)
                return color;


            // 聚合点
            colorAddition = colorAddition / addCount;

            return colorAddition;
        }

        // 聚合
        private float2 Cohesion(List<Boid> boids)
        {
            float2 myPos = position;
            float radius = findRadius;
            float away = awayRadius;
            int count = boids.Count;
            if (count == 0)
                return new float2(0);

            float2 posAddition = 0;
            int addCount = 0;
            for (int i = 0; i < count; i++)
            {
                float dis = GetDistance(myPos, boids[i].position);
                if (dis < radius && dis > away)  // 筛选出范围内的群体
                {
                    addCount++;
                    posAddition += boids[i].position;
                }
            }

            if (addCount == 0)
                return new float2(0);


            // 聚合点
            posAddition = posAddition / addCount;

            return Nomalized(posAddition - myPos);
        }

        // 速度对齐
        private float2 Alignment(List<Boid> boids)
        {
            float2 myPos = position;
            float radius = findRadius;
            int count = boids.Count;
            if (count == 0)
                return new float2(0);

            float2 velAddition = 0;
            int addCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (GetDistance(myPos, boids[i].position) < radius)
                {
                    addCount++;
                    velAddition += boids[i].velocity;
                }
            }

            return velAddition / addCount;
        }

        // 避障
        private float2 AvoidWall()
        {
            float x = 0f, y = 0f;
            bool nearWall = false;
            if(position.x < 0)
            {
                nearWall = true;
                x = -position.x;
            }
            if(position.x > outBorder.x)
            {
                nearWall = true;
                x = outBorder.x - position.x;
            }
            if(position.y < 0)
            {
                nearWall = true;
                y = -position.y;
            }
            if (position.y > outBorder.y)
            {
                nearWall = true;
                y = outBorder.y - position.y;
            }


            if (nearWall)
                return Nomalized(new float2(x, y));
            else
                return new float2(0);
        }

        private float GetDistance(float2 pos1, float2 pos2)
        {
            return math.distance(pos1, pos2);
        }

        private float2 Nomalized(float2 f2)
        {
            return math.normalize(f2);
        }

        private float2 ClampMagnitude(float2 f2, float standardMag)
        {
            if(GetMagnitude(f2) > standardMag)
            {
                return Nomalized(f2) * standardMag;
            }
            return f2;
        }

        private float GetMagnitude(float2 f2)
        {
            return math.sqrt(f2.x * f2.x + f2.y * f2.y);
        }
    }

}
