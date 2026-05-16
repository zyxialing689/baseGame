using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Collision2D
{
    public class ZCollisionExampleAttack2D : MonoBehaviour
    {
        public ZCollisionBody2D attacker;
        public ZCollisionLayer targetMask = ZCollisionLayer.Monster | ZCollisionLayer.NPC | ZCollisionLayer.Hero;
        public Vector2 attackOffset = new Vector2(0.6f, 0f);
        public Vector2 attackSize = new Vector2(1.0f, 0.7f);
        public ZHitRule hitRule = ZHitRule.BodyAndGroundMatch;
        public ZTargetRelation relation = ZTargetRelation.Enemy;
        public bool allowSelfTarget;

        private readonly List<ZCollisionBody2D> results = new List<ZCollisionBody2D>(32);

        private void Reset()
        {
            attacker = GetComponent<ZCollisionBody2D>();
        }

        private void Update()
        {
            if (attacker == null)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector2 center = attacker.Position2D + attackOffset;
                int count = ZHitQuery2D.HitBoxNonAlloc(
                    attacker,
                    center,
                    attackSize,
                    targetMask,
                    relation,
                    hitRule,
                    results,
                    allowSelfTarget);

                for (int i = 0; i < count; i++)
                    Debug.Log("Hit target: " + results[i].name, results[i]);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attacker == null)
                return;

            Gizmos.color = Color.red;
            Vector2 center = attacker.Position2D + attackOffset;
            Gizmos.DrawWireCube(center, attackSize);
        }
    }
}
