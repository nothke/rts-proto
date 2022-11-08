using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public UnitsDatabase unitsDatabase;

    public int money = 1000;
    public Color color = Color.red;
    public Material factionMaterial;

    public Building buildingPrefabBeingPlaced;
    public Building buildingBeingConstructed;
    public float buildingProgress;

    public Unit unitBeingProduced;
    public float unitProgress;
    public Queue<Unit> unitBuildQueue = new Queue<Unit>();

    public List<Unit> selectedUnits = new List<Unit>();

    public List<Building> constructedBuildings = new List<Building>();
    public List<Unit> constructableUnits = new List<Unit>();
    public Dictionary<Unit, Building> unitProducedInBuildingMap = new Dictionary<Unit, Building>();

    public Building selectedBuilding;

    public void SelectAllUnits()
    {
        // TODO

        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedUnits.Clear();
            //selectedUnits.AddRange(FindObjectsOfType<Unit>());
        }
    }

    private void Awake()
    {
        factionMaterial = Instantiate(factionMaterial);
        factionMaterial.color = color;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Construction progress
        if (buildingBeingConstructed)
        {
            buildingProgress += dt;

            if (buildingProgress > buildingBeingConstructed.constructable.timeToBuild)
            {
                buildingPrefabBeingPlaced = buildingBeingConstructed;
                buildingBeingConstructed = null;
                buildingProgress = 0;
            }
        }

        if (unitBuildQueue.Count > 0 && !unitBeingProduced &&
            money >= unitBuildQueue.Peek().constructable.cost)
        {
            unitBeingProduced = unitBuildQueue.Dequeue();
        }

        if (unitBeingProduced)
        {
            unitProgress += dt;

            if (unitProgress > unitBeingProduced.constructable.timeToBuild)
            {
                SpawnUnit(unitBeingProduced);
            }
        }

        // Cleanup dead units
        for (int i = selectedUnits.Count - 1; i >= 0; i--)
        {
            if (selectedUnits[i] == null)
                selectedUnits.RemoveAt(i);
        }
    }

    public void DestroySelectedUnits()
    {
        for (int i = selectedUnits.Count - 1; i >= 0; i--)
        {
            Destroy(selectedUnits[i].gameObject);
        }

        selectedUnits.Clear();

        if (selectedBuilding)
            Destroy(selectedBuilding.gameObject);
    }

    public void PlaceBuilding(Building prefab, Vector3 buildingPosition, Vector2Int coord)
    {
        var building = Instantiate(prefab);

        building.entity.faction = this;

        building.transform.position = buildingPosition;

        //tile.building = building;
        World.instance.PlaceBuilding(building, coord);

        buildingPrefabBeingPlaced = null;

        constructedBuildings.Add(building);

        // Update producers:

        var producer = building.GetComponent<UnitProducer>();

        if (producer)
        {
            foreach (var unit in producer.canProduce)
            {
                if (!constructableUnits.Contains(unit))
                {
                    constructableUnits.Add(unit);
                    unitProducedInBuildingMap.Add(unit, building);
                }
            }
        }
    }

    public void SpawnUnit(Unit unit)
    {
        var building = unitProducedInBuildingMap[unit];

        Instantiate(unitBeingProduced, building.transform.position + Vector3.forward, Quaternion.identity);

        unitBeingProduced = null;
        unitProgress = 0;

        unit.entity.faction = this;

        money -= unit.constructable.cost;
    }

    internal void EnqueueUnit(Unit unit)
    {
        unitBuildQueue.Enqueue(unit);
    }

    public void StartConstructingBuilding(Building building)
    {
        if (money >= building.constructable.cost)
        {
            money -= building.constructable.cost;
            buildingBeingConstructed = building;
        }
    }

    public void BuildingGotDestroyed(Building building)
    {
        constructedBuildings.Remove(building);

        if (building.GetComponent<UnitProducer>())
            UpdateProducers();
    }

    public void UpdateProducers()
    {
        constructableUnits.Clear();
        unitProducedInBuildingMap.Clear();

        foreach (var building in constructedBuildings)
        {
            if (building.TryGetComponent<UnitProducer>(out var producer))
            {
                foreach (var unit in producer.canProduce)
                {
                    if (!constructableUnits.Contains(unit))
                    {
                        constructableUnits.Add(unit);
                        unitProducedInBuildingMap.Add(unit, building);
                    }
                }
            }
        }

        // Cancel production if building that produces the unit no longer exists
        if (unitBeingProduced)
        {
            foreach (var unit in constructableUnits)
            {
                if (unit == unitBeingProduced)
                    return;
            }

            Debug.Log("Building destroyed, production cancelled");
            CancelConstructingUnit();
        }

        Queue<Unit> tempQueue = new Queue<Unit>(unitBuildQueue.Count);

        foreach (var unit in unitBuildQueue)
        {
            if (constructableUnits.Contains(unit))
                tempQueue.Enqueue(unit);
        }

        unitBuildQueue = tempQueue;

    }

    public void CancelConstructingUnit()
    {
        unitBeingProduced = null;
        unitProgress = 0;
    }

    public void DequeueUnit(Unit unit)
    {
        if (unitBuildQueue.Count == 0)
        {
            if (unitBeingProduced == unit)
                CancelConstructingUnit();

            return;
        }

        List<Unit> unitBuff = new List<Unit>(unitBuildQueue);

        bool foundOne = false;

        for (int i = 0; i < unitBuff.Count; i++)
        //for (int i = unitBuff.Count - 1; i >= 0; i--)
        {
            if (unitBuff[i] == unit)
            {
                unitBuff.RemoveAt(i);
                foundOne = true;
                break;
            }
        }

        if (!foundOne)
        {
            if (unitBeingProduced == unit)
                CancelConstructingUnit();
        }
        else
        {

            unitBuildQueue.Clear();

            foreach (var unitInList in unitBuff)
            {
                unitBuildQueue.Enqueue(unitInList);
            }
        }
    }

    public void GiveMoveOrderToSelectedUnits(Vector3 to)
    {
        foreach (var unit in selectedUnits)
        {
            unit.agent.SetDestination(to);
        }
    }
}
