﻿using UnityEngine;

namespace MC.Core
{
    public class MonsterSpawner : MonoBehaviour
    {
        public GameObject[] monsters;

        private Player player;

        private readonly float spawnMonsterInterval = 15;

        private float lastSpawnTime = 0;

        private TOD_Sky sky;

        private float counts = 0;

        private void Start()
        {
            sky = GameObject.FindObjectOfType<TOD_Sky>();
        }
        private void Update()
        {
            if (player == null)
            {
                player = GameObject.FindObjectOfType<Player>();
                return;
            }

            if (sky.IsNight)
            {
                if (Time.time - lastSpawnTime > spawnMonsterInterval && counts <= 4)
                {
                    var isHit = Physics.Raycast(Random.insideUnitSphere * 25 + Vector3.up * 50, Vector3.up * -1, out RaycastHit rayHit, 1000);

                    lastSpawnTime = Time.time;

                    if (isHit)
                    {
                        var monster = monsters[Random.Range(0, monsters.Length)];

                        var instance = Instantiate(monster, rayHit.point + Vector3.up * 2.5f, Quaternion.identity);
                        instance.GetComponent<Monster>().OnDeath += () =>
                        {
                            counts -= 1;
                        };

                        counts += 1;
                    }
                }
            }


        }

    }
}
