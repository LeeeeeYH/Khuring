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
    public ColliderInfo CI; // ����
    public List<ColliderInfo> NearStones; // Trigger���� ���� �ٸ���

    void Start()
    {
        this.Rigid2D = this.gameObject.GetComponent<Rigidbody2D>();
        DecreaseCoe = 0.75f;
        CI.Name = transform.name;
        NearStones = new List<ColliderInfo>();
    }

    Vector2 Rotate90(Vector2 v) { return new Vector2(v.y, -v.x); }


    /*
     * Continuous���� ��Ȯ�� �浹������ ���� Collision��ɰ�
     * �浹���� �ӵ����͸� �ޱ����� �� ū ������ Trigger��� ȥ��
     * Collision, Trigger ������ ���� ����Ȱ��
     * �ǵ�ġ ���� Trigger������ Collision���������� �Լ��� �۵��ϹǷ� ��ü�� ���水ü�� Child�� ���� : ��ũ��Ʈ �и��� �±�Ȯ��
     * 
     * -----OnTriggerEnter-----
     * ��ó���� - �ֺ� ���濡�� ���� - �ӵ� ����Ʈ�߰�
     * 
     * -----OnCollisionEnter-----
     * Continuous�� �浹����
     * ���� �����ϴ� Script���� ���� ���� ���ο� ���� �������� ���
     * ������ Velocity�� ���ο� �������ͷ� ǥ��
     * �浹�� ���������� Velocity���� ���
     * RigidBody2D�� position(transform.position �̶� �ٸ�)�� �ణ ��ȭ��(�齺��)���μ� OnCollisionExit�Լ� ����ȣ��(�⺻ ���������� ���� �浹 �� �پ��ִ� ��� ���)
     * 
     * -----OnCollisionExit-----
     * ���� �浹�� Velocity���� ����
     * �����浹�� ����Ͽ� ���� Velocity�� �ֺ� ������� ������ �ִ� �ٸ� Stone�� PowerVector������ ���� �Ķ���� �ʱ�ȭ
     * 
     * -----OnTriggerStay-----
     * �ٸ� ������� �浹�� ���� PowerVector�� ���Ͽ��ٸ� ����� ��� ���濡���� �ش罺�� PowerVector�� Velocity ���� ����
     * 
     * -----OnTriggerExit-----
     * �־������Ƿ� NearStone����Ʈ���� ����
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