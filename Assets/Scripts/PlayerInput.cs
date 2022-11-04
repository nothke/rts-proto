using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Utils;
using System;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance;
    void Awake() { instance = this; }

    new public Camera camera;

    public Transform testT;


    public Material previewGreenMat;
    public Material previewRedMat;

    public UnitsDatabase unitsDatabase;
    public Building buildingPrefabBeingPlaced;
    public Building buildingBeingConstructed;
    public float buildingProgress;

    public Unit unitBeingProduced;
    public float unitProgress;

    public int money = 1000;

    public List<Unit> selectedUnits = new List<Unit>();

    bool rectSelecting;
    Vector2 rectSelectStart;

    public List<Building> constructedBuildings = new List<Building>();
    public List<Unit> constructableUnits = new List<Unit>();
    public Dictionary<Unit, Building> unitProducedInBuildingMap = new Dictionary<Unit, Building>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedUnits.Clear();
            selectedUnits.AddRange(FindObjectsOfType<Unit>());
        }

        // Camera movement

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Transform camT = camera.transform;

        Vector3 forward = camT.forward;
        forward.y = 0;
        forward = forward.normalized;

        Vector3 right = camT.right;

        float dt = Time.deltaTime;
        const float cameraSpeed = 20.0f;

        camT.position +=
            forward * inputY * dt * cameraSpeed +
            right * inputX * dt * cameraSpeed;

        // Grid picking

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        groundPlane.Raycast(ray, out float distance);
        Vector3 raycastPoint = ray.GetPoint(distance);

        Vector2Int coord = new Vector2Int(
            Mathf.FloorToInt(raycastPoint.x),
            Mathf.FloorToInt(raycastPoint.z)
            );

        testT.position = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);

        bool mouseOverUI = Nothke.ProtoGUI.GameWindow.IsMouseOverUI();
        bool LMBDown = !mouseOverUI && Input.GetMouseButtonDown(0);
        bool RMBDown = !mouseOverUI && Input.GetMouseButtonDown(1);

        if (buildingPrefabBeingPlaced && !mouseOverUI)
        {
            var size = buildingPrefabBeingPlaced.size;

            Vector3 extents = new Vector3(
                size.x, 0,
                size.y) * 0.5f;

            Vector3 buildingCornerPoint = raycastPoint - extents;

            Vector2Int cornerCoord = new Vector2Int(
                Mathf.FloorToInt(buildingCornerPoint.x + 0.5f),
                Mathf.FloorToInt(buildingCornerPoint.z + 0.5f)
            );

            //ref var tile = ref World.instance.GetTile(coord.x, coord.y);
            //Debug.Log(tile.coord);

            bool occupied = !World.instance.CanPlace(cornerCoord, size);

            Vector3 buildingPosition =
                new Vector3(cornerCoord.x, 0, cornerCoord.y) + extents;

            Material previewMat = occupied ? previewRedMat : previewGreenMat;

            ObjectPreviewer.Render(buildingPosition, Quaternion.identity, Vector3.one * 1.02f, previewMat);

            if (LMBDown)
            {
                if (!occupied)
                {
                    PlaceBuilding(buildingPrefabBeingPlaced, buildingPosition, cornerCoord);
                }
            }
        }

        // Construction progress
        if (buildingBeingConstructed)
        {
            buildingProgress += dt;

            if (buildingProgress > buildingBeingConstructed.constructable.timeToBuild)
            {
                buildingPrefabBeingPlaced = buildingBeingConstructed;
                buildingBeingConstructed = null;
                buildingProgress = 0;

                ObjectPreviewer.SetObject(buildingPrefabBeingPlaced.gameObject);
            }
        }

        if (unitBeingProduced)
        {
            unitProgress += dt;

            if (unitProgress > unitBeingProduced.constructable.timeToBuild)
            {
                CreateUnit(unitBeingProduced);
            }
        }

        // Cleanup dead units
        for (int i = selectedUnits.Count - 1; i >= 0; i--)
        {
            if (selectedUnits[i] == null)
                selectedUnits.RemoveAt(i);
        }

        // give orders to units
        if (selectedUnits.Count > 0)
        {
            foreach (var unit in selectedUnits)
            {
                if (RMBDown)
                    unit.agent.SetDestination(raycastPoint);

                Debug.DrawRay(unit.transform.position + Vector3.up * 1.3f, Vector3.up * 0.4f, Color.cyan);
            }
        }

        // Rect selection
        if (LMBDown)
        {
            rectSelecting = true;
            rectSelectStart = Input.mousePosition;
        }

        if (rectSelecting)
        {
            if (Input.GetMouseButtonUp(0))
                rectSelecting = false;

            selectedUnits.Clear();

            if (Unit.all != null)
            {
                foreach (var unit in Unit.all)
                {
                    Vector3 ssPos = camera.WorldToScreenPoint(unit.transform.position);

                    Rect rect = new Rect(rectSelectStart, (Vector2)Input.mousePosition - rectSelectStart);

                    if (rect.Contains(ssPos, true))
                    {
                        selectedUnits.Add(unit);
                    }
                }
            }
        }
    }

    void PlaceBuilding(Building prefab, Vector3 buildingPosition, Vector2Int coord)
    {
        var building = Instantiate(prefab);
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

    void CreateUnit(Unit unit)
    {
        var building = unitProducedInBuildingMap[unit];

        Instantiate(unitBeingProduced, building.transform.position + Vector3.forward, Quaternion.identity);

        unitBeingProduced = null;
        unitProgress = 0;
    }

    internal void ProduceUnit(Unit unit)
    {
        unitBeingProduced = unit;
    }

    private void OnGUI()
    {
        if (rectSelecting)
        {
            Vector2 start = new Vector2(rectSelectStart.x, Screen.height - rectSelectStart.y);
            Vector2 end = Input.mousePosition;
            end.y = Screen.height - end.y;
            Vector2 size = end - start;

            GUI.Box(new Rect(start, size), "");
        }
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

    void UpdateProducers()
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
    }

    void CancelConstructingUnit()
    {
        unitBeingProduced = null;
        unitProgress = 0;
    }
}
