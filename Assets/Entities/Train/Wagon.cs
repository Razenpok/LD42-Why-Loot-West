﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wagon: MonoBehaviour
{
    [SerializeField] private float dpsGoal = 5;
    [SerializeField] private Animator visuals;
    [SerializeField] private GameObject[] droppedItems;

    public bool SkipToThis;

    private readonly DpsCounter dpsCounter = new DpsCounter();
    private List<Wall> walls = new List<Wall>();

    public event Action DpsGoalReached;
    public event Action<Item, Vector3> ItemCreated;

    private void Awake()
    {
        walls = GetComponentsInChildren<Wall>().ToList();
        foreach (var wall in walls)
        {
            wall.DamageReceived += Wall_DamageReceived;
        }
    }

    private void Update()
    {
        dpsCounter.Update();
    }

    private void Wall_DamageReceived(float damage, Vector3 position)
    {
        dpsCounter.Register(damage);
        if (Dps >= dpsGoal)
        {
            if (DpsGoalReached != null)
                DpsGoalReached();
        }

        const float maxItemsOnScreen = 5;
        var currentItemsOnScreen = FindObjectsOfType<WorldItemView>().Length;
        var spawnChance = 1 - 1 / maxItemsOnScreen * currentItemsOnScreen;
        if (Random.value < spawnChance)
        {
            var template = droppedItems[new System.Random().Next(droppedItems.Length)];
            var item = new Item(template);
            if (ItemCreated != null)
                ItemCreated(item, position);
        }
    }

    public float Dps
    {
        get { return dpsCounter.Dps; }
    }

    public float DpsGoal
    {
        get { return dpsGoal; }
    }

    public Coroutine Destroy()
    {
        return StartCoroutine(StartDestroying());
    }

    private IEnumerator StartDestroying()
    {
        visuals.SetTrigger("Destroy");
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MoveAway());
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator MoveAway()
    {
        const float moveDistance = 15f;
        var currentY = transform.position.y;
        foreach (var y in Easing.InCubic(currentY, currentY - moveDistance, 3))
        {
            var position = transform.position;
            position.y = y;
            transform.position = position;
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}