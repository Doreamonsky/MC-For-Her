﻿using UnityEngine;

namespace MC.Core
{
    //骷髅怪 
    public class SkeletonMonster : Monster
    {
        public float swordDamage = 15;

        public Transform rayTransform;

        public Transform skeletonCenter;

        private readonly float damageInterval = 1f;

        private float lastDamageTime = 0;

        public override void Update()
        {
            if (target == null)
            {
                target = GameObject.FindObjectOfType<Player>();
                return;
            }

            var dir = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);

            var desireMove = moveVelocity * dir.normalized * Time.deltaTime;


            if (dir.magnitude > 2)
            {
                m_animator.SetFloat("Speed", 1);
                m_characterController.Move(-Vector3.up * 9.8f * Time.deltaTime + desireMove);
            }
            else
            {
                m_animator.SetFloat("Speed", 0f);

                if (Time.time - lastDamageTime > damageInterval)
                {
                    lastDamageTime = Time.time;
                    m_animator.SetTrigger("Attack");
                }
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(desireMove), Time.deltaTime * 2.5f);

        }

        public void OnAttackAnimation()
        {
            var dir = target.transform.position - skeletonCenter.position;

            if (dir.magnitude < 4)
            {
                target.ApplyDamage(swordDamage);
            }
        }

        public override void CheckDead()
        {
            if (health <= 0)
            {
                Destroy(gameObject);

                PoolManager.CreateObject("Explosion", skeletonCenter.position, Quaternion.LookRotation(Vector3.up).eulerAngles);

                OnDeath?.Invoke();
            }
        }
    }
}
