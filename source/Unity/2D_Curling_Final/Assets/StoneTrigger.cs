using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneTrigger : MonoBehaviour
{
    StoneCollision Coll;
    float DecreaseCoe;

    void Start()
    {
        this.Coll = gameObject.GetComponentInParent<StoneCollision>();
        DecreaseCoe = 0.75f;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("StoneTrigger"))
            Coll.NearStones.Add(new StoneCollision.ColliderInfo(collision.gameObject.transform.parent.name, collision.gameObject.GetComponentInParent<Rigidbody2D>().velocity * DecreaseCoe));
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("StoneTrigger"))
        {
            StoneCollision otherColl = collision.gameObject.GetComponentInParent<StoneCollision>();

            if (otherColl.CI.Collided > 0)
            {
                for (int i = 0; i < Coll.NearStones.Count; i++)
                {
                    if (Coll.NearStones[i].Name == otherColl.CI.Name)
                    {
                        Coll.NearStones[i] = new StoneCollision.ColliderInfo(Coll.NearStones[i].Name, otherColl.CI.PowerVector * DecreaseCoe);
                        otherColl.CI.Collided--;
                        break;
                    }
                }
                if (otherColl.CI.Collided == 0) otherColl.CI.InitColliderParam();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("StoneTrigger"))
        {
            for (int i = 0; i < Coll.NearStones.Count; i++)
            {
                if (Coll.NearStones[i].Name == collision.gameObject.transform.parent.name)
                {
                    Coll.NearStones.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
