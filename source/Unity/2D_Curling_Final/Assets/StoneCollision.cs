using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneCollision : MonoBehaviour
{
    public Rigidbody2D Rigid2D;
    public float DecreaseCoe;
    public struct ColliderInfo
    {
        public string Name;
        public Vector2 PowerVector;
        public Vector2 UnitX;
        public Vector2 UnitY;
        public Vector2 NewX;
        public Vector2 NewY;
        public int Collided;

        public ColliderInfo(string name, Vector2 powerVector/*, Vector2 unitX, Vector2 unitY, Vector2 newX, Vector2 newY*/)
        { Name = name; PowerVector = powerVector; UnitX = new Vector2(0, 0); UnitY = new Vector2(0, 0); NewX = new Vector2(0, 0); NewY = new Vector2(0, 0); Collided = 0; }

        public void InitColliderParam()
        {
            PowerVector = new Vector2(0, 0);
            UnitX = new Vector2(0, 0);
            UnitY = new Vector2(0, 0);
            NewX = new Vector2(0, 0);
            NewY = new Vector2(0, 0);
        }
    }
    public ColliderInfo CI; // 내꺼
    public List<ColliderInfo> NearStones; // Trigger범위 내의 다른거

    void Start()
    {
        this.Rigid2D = this.gameObject.GetComponent<Rigidbody2D>();
        DecreaseCoe = 0.75f;
        CI.Name = transform.name;
        NearStones = new List<ColliderInfo>();
    }

    Vector2 Rotate90(Vector2 v) { return new Vector2(v.y, -v.x); }


    /*
     * Continuous적인 정확한 충돌감지를 위한 Collision기능과
     * 충돌전후 속도벡터를 받기위한 더 큰 범위의 Trigger기능 혼용
     * Collision, Trigger 각각의 장점 동시활용
     * 의도치 않은 Trigger범위와 Collision범위에서도 함수가 작동하므로 빈객체를 스톤객체의 Child로 구현 : 스크립트 분리후 태그확인
     * 
     * -----OnTriggerEnter-----
     * 근처도달 - 주변 스톤에서 감지 - 속도 리스트추가
     * 
     * -----OnCollisionEnter-----
     * Continuous로 충돌감지
     * 먼저 동작하는 Script에서 접선 기준 새로운 기저 단위벡터 계산
     * 각자의 Velocity를 새로운 기저벡터로 표현
     * 충돌후 움직여야할 Velocity벡터 계산
     * RigidBody2D의 position(transform.position 이랑 다름)을 약간 변화함(백스텝)으로서 OnCollisionExit함수 강제호출(기본 물리엔진에 의한 충돌 후 붙어있는 경우 고려)
     * 
     * -----OnCollisionExit-----
     * 계산된 충돌후 Velocity벡터 적용
     * 연쇄충돌을 고려하여 변한 Velocity를 주변 스톤들의 가지고 있던 다른 Stone의 PowerVector갱신을 위한 파라미터 초기화
     * 
     * -----OnTriggerStay-----
     * 다른 스톤과의 충돌로 인해 PowerVector가 변하였다면 가까운 모든 스톤에서의 해당스톤 PowerVector의 Velocity 정보 갱신
     * 
     * -----OnTriggerExit-----
     * 멀어졌으므로 NearStone리스트에서 삭제
     * 
    */

    void OnCollisionEnter2D(Collision2D collision)
    {
        StoneCollision otherCollision = collision.gameObject.GetComponent<StoneCollision>();


        if (CI.UnitX.sqrMagnitude == 0)
        {
            CI.UnitY = new Vector2(collision.gameObject.transform.position.x - this.transform.position.x, collision.gameObject.transform.position.y - this.transform.position.y).normalized;
            CI.UnitX = Rotate90(CI.UnitY);
            for (int i = 0; i < NearStones.Count; i++)
            {
                if (NearStones[i].Name == collision.gameObject.transform.name)
                {
                    CI.NewY = Vector2.Dot(NearStones[i].PowerVector, CI.UnitY) * CI.UnitY;
                    otherCollision.CI.UnitX = CI.UnitX;
                    otherCollision.CI.UnitY = CI.UnitY;
                    otherCollision.CI.NewX = Vector2.Dot(NearStones[i].PowerVector, CI.UnitX) * CI.UnitX;

                    break;
                }
            }
            for (int i = 0; i < otherCollision.NearStones.Count; i++)
            {
                if (otherCollision.NearStones[i].Name == CI.Name)
                {
                    CI.NewX = Vector2.Dot(otherCollision.NearStones[i].PowerVector, CI.UnitX) * CI.UnitX;
                    otherCollision.CI.NewY = Vector2.Dot(otherCollision.NearStones[i].PowerVector, CI.UnitY) * CI.UnitY;
                    break;
                }
            }
            this.Rigid2D.velocity = new Vector2(0f, 0f);
            collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);

            this.Rigid2D.position = new Vector2(Rigid2D.position.x - CI.UnitY.x, Rigid2D.position.y - CI.UnitY.y);

            Rigid2D.AddForce(Rigid2D.position - CI.UnitY);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        this.Rigid2D.velocity = CI.NewX + CI.NewY;
        CI.Collided = NearStones.Count;
        CI.PowerVector = CI.NewX + CI.NewY;
        CI.UnitX = new Vector2(0, 0);
        CI.UnitY = new Vector2(0, 0);
    }
}