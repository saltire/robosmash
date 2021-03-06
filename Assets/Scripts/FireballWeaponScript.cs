﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct Fireball {
    public GameObject obj;
    public Vector3 origin;
    public Vector3 target;
}

public class FireballWeaponScript : WeaponScript {
    public GameObject fireballPrefab;
    public int fireballCount = 3;
    public float fireTime = .5f;
    public float fireOriginHeight = .6f;
    public float fireArcHeight = 1;
    public int damage = 1;

    public FireSurfaceScript fireSurfacePrefab;

    List<Fireball> fireballs;
    float fireTimeRemaining = 0;

    GridScript grid;
    LayerMask buildingLayerMask;

    void Start() {
        grid = FindObjectOfType<GridScript>();

        fireballs = new List<Fireball>();
        for (int i = 0; i < fireballCount; i++) {
            Vector3 target = transform.position + transform.forward * (i + 1);
            if (grid.SquareExists(target.x, target.z)) {
                fireballs.Add(new Fireball() {
                    obj = Instantiate(fireballPrefab, transform.position, Quaternion.identity),
                    origin = transform.position + new Vector3(0, fireOriginHeight, 0),
                    target = grid.GetSquare(target.x, target.z).top,
                });
            }
        }

        if (fireballs.Count == 0) {
            Destroy(gameObject);
        }

        fireTimeRemaining = fireTime;

        buildingLayerMask = LayerMask.GetMask("Buildings");
    }

    void Update() {
        fireTimeRemaining -= Time.deltaTime;

        if (fireTimeRemaining > 0) {
            float normalizedFireTime = 1 - (fireTimeRemaining / fireTime);
            
            foreach (Fireball fireball in fireballs) {
                fireball.obj.transform.position = Vector3.Lerp(fireball.origin, fireball.target, normalizedFireTime) + 
                    new Vector3(0, fireArcHeight * Mathf.Sin(normalizedFireTime * Mathf.PI), 0);
            }
        }
        else {
            foreach (Fireball fireball in fireballs) {
                Destroy(fireball.obj);

                grid.SetSquareSurface(fireball.target.x, fireball.target.z, fireSurfacePrefab);

                Collider[] buildings = Physics.OverlapSphere(fireball.target, .25f, buildingLayerMask);
                if (buildings.Length > 0) {
                    Destroy(buildings[0].gameObject);
                }

                Vector3Int targetCoords = player.GetCoords(fireball.target);
                Vector3Int otherPlayerCoords = player.otherPlayer.GetCoords();

                if (otherPlayerCoords == targetCoords) {
                    player.otherPlayer.Damage(damage);
                }
            }

            Destroy(gameObject);
        }
    }
}
