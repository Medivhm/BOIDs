using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace BOIDs
{
    public struct BoidSetting
    {
        public float maxSpeed;
        public float findRadius;   // 单位可视群体范围
        public float awayRadius;   // 单位间保持距离
        public float2 outBorder;   // 外框
    }

    [BurstCompile]
    public struct BoidJob : IJobParallelForTransform
    {
        public float dt;
        public BoidSetting setting;
        [NativeDisableParallelForRestriction] public NativeArray<float2> positions;
        [NativeDisableParallelForRestriction] public NativeArray<float2> velocitys;
        [NativeDisableParallelForRestriction] public NativeArray<float3> colors;

        public void Execute(int index, TransformAccess transform)
        {
            float2 position = positions[index];
            float2 velocity = velocitys[index];
            float3 color    = colors[index];


            color = ColorAvg(position, color);
            var awoidWall = AvoidWall(position) * setting.maxSpeed;
            var sepVel = Separation(position) * 1.3f;
            var cohVel = Cohesion(position);
            var aliVel = Alignment(position, velocity);

            velocity += sepVel + cohVel + aliVel + awoidWall;
            velocity = ClampMagnitude(velocity, setting.maxSpeed);
            position += velocity * setting.maxSpeed * dt;

            // 存储结果
            positions[index] = position;
            velocitys[index] = velocity;
            colors[index] = color;

            transform.position = new float3(position.x, position.y, 0f);
        }

        // 分离
        private float2 Separation(float2 posiiton)
        {
            float2 myPos = posiiton;
            float away = setting.awayRadius;
            int count = positions.Length;
            if (count == 0)
                return new float2(0);

            float2 velAddition = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                float dis = GetDistance(myPos, positions[i]);
                if (dis > 0 && dis < away)
                {
                    velAddition += (myPos - positions[i]) / dis;
                }
            }

            return velAddition;
        }

        // 颜色靠近
        private float3 ColorAvg(float2 position, float3 color)
        {
            float2 myPos = position;
            float radius = setting.findRadius;
            int count = colors.Length;
            if (count == 0)
                return color;

            float3 colorAddition = new float3(0);
            int addCount = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                float dis = GetDistance(myPos, positions[i]);
                if (dis < radius)  // 筛选出范围内的群体
                {
                    addCount++;
                    colorAddition += colors[i];
                }
            }

            if (addCount == 0)
                return color;


            // 聚合颜色
            colorAddition = colorAddition / addCount;

            return colorAddition;
        }

        // 聚合
        private float2 Cohesion(float2 position)
        {
            float2 myPos = position;
            float radius = setting.findRadius;
            float away = setting.awayRadius;
            int count = positions.Length;
            if (count == 0)
                return new float2(0);

            float2 posAddition = 0;
            int addCount = 0;
            for (int i = 0; i < count; i++)
            {
                float dis = GetDistance(myPos, positions[i]);
                if (dis < radius && dis > away)  // 筛选出范围内的群体
                {
                    addCount++;
                    posAddition += positions[i];
                }
            }

            if (addCount == 0)
                return new float2(0);


            // 聚合点
            posAddition = posAddition / addCount;

            return Nomalized(posAddition - myPos);
        }

        // 速度对齐
        private float2 Alignment(float2 position, float2 velocity)
        {
            float2 myPos = position;
            float radius = setting.findRadius;
            int count = positions.Length;
            if (count == 0)
                return new float2(0);

            float2 velAddition = 0;
            int addCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (GetDistance(myPos, positions[i]) < radius)
                {
                    addCount++;
                    velAddition += velocitys[i];
                }
            }

            return velAddition / addCount;
        }

        // 避障
        private float2 AvoidWall(float2 position)
        {
            float x = 0f, y = 0f;
            bool nearWall = false;
            if(position.x < 0)
            {
                nearWall = true;
                x = -position.x;
            }
            if(position.x > setting.outBorder.x)
            {
                nearWall = true;
                x = setting.outBorder.x - position.x;
            }
            if(position.y < 0)
            {
                nearWall = true;
                y = -position.y;
            }
            if (position.y > setting.outBorder.y)
            {
                nearWall = true;
                y = setting.outBorder.y - position.y;
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
