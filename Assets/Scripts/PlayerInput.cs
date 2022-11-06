using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Utils;
using System;

using NDraw;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance;
    void Awake() { instance = this; }

    new public Camera camera;
    Vector2 camVelo;

    public Material previewGreenMat;
    public Material previewRedMat;

    public UnitsDatabase unitsDatabase;
    public Building buildingPrefabBeingPlaced;
    public Building buildingBeingConstructed;
    public float buildingProgress;

    public Unit unitBeingProduced;
    public float unitProgress;
    public Queue<Unit> unitBuildQueue = new Queue<Unit>();

    public int money = 1000;

    public List<Unit> selectedUnits = new List<Unit>();

    bool rectSelecting;
    Vector2 rectSelectStart;

    public List<Building> constructedBuildings = new List<Building>();
    public List<Unit> constructableUnits = new List<Unit>();
    public Dictionary<Unit, Building> unitProducedInBuildingMap = new Dictionary<Unit, Building>();

    Building selectedBuilding;

    Queue<Unit> queueBuff = new Queue<Unit>();

    void Update()
    {
        Vector3 up = Vector3.up;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            selectedUnits.Clear();
            selectedUnits.AddRange(FindObjectsOfType<Unit>());
        }

        // Camera movement

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        float dt = Time.deltaTime;
        const float maxCameraSpeed = 20.0f;

        Vector2 mousePos = Input.mousePosition;

        Vector2 camAccel = new Vector2(inputX, inputY);

        const float MOUSE_MOVE_MARGIN = 5;
        if (mousePos.x < MOUSE_MOVE_MARGIN)
            camAccel.x = -1;
        else if (mousePos.x > Screen.width - MOUSE_MOVE_MARGIN)
            camAccel.x = 1;

        if (mousePos.y < MOUSE_MOVE_MARGIN)
            camAccel.y = -1;
        else if (mousePos.y > Screen.height - MOUSE_MOVE_MARGIN)
            camAccel.y = 1;

        camVelo += camAccel * dt * 100.0f;
        camVelo *= camAccel.sqrMagnitude == 0 ? 0.9f : 1.0f;
        camVelo = Vector2.ClampMagnitude(camVelo, maxCameraSpeed);

        Transform camT = camera.transform;

        Vector3 forward = camT.forward;
        forward.y = 0;
        forward = forward.normalized;

        Vector3 right = camT.right;

        camT.position +=
            right * camVelo.x * dt +
            forward * camVelo.y * dt;

        // Grid picking

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        groundPlane.Raycast(ray, out float distance);
        Vector3 raycastPoint = ray.GetPoint(distance);

        Vector2Int coord = new Vector2Int(
            Mathf.FloorToInt(raycastPoint.x),
            Mathf.FloorToInt(raycastPoint.z)
            );

        var tile = World.instance.GetTile(coord.x, coord.y);

        Vector3 tilePos = new Vector3(tile.coord.x + 0.5f, 0, tile.coord.y + 0.5f);
        Draw.World.SetColor(tile.building ? Color.yellow : Color.white);
        Draw.World.Cube(tilePos + Vector3.up * 0.01f, new Vector3(1, 0, 1), Vector3.forward, Vector3.up);

        bool mouseOverUI = Nothke.ProtoGUI.GameWindow.IsMouseOverUI();
        bool LMBDown = !mouseOverUI && Input.GetMouseButtonDown(0);
        bool RMBDown = !mouseOverUI && Input.GetMouseButtonDown(1);

        if (!mouseOverUI)
        {
            if (buildingPrefabBeingPlaced)
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
            else
            {
                if (LMBDown)
                {
                    selectedBuilding = tile.building;
                }
            }
        }

        if (selectedBuilding)
        {
            // Draw
            float w = selectedBuilding.size.x;
            float h = selectedBuilding.size.y;
            float r = Mathf.Sqrt(w * w + h * h) * 0.5f + 0.2f;
            Draw.World.Circle(selectedBuilding.transform.position + up * 0.01f, r, up, 64);
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

        // give orders to units
        if (selectedUnits.Count > 0)
        {
            foreach (var unit in selectedUnits)
            {
                if (RMBDown)
                    unit.agent.SetDestination(raycastPoint);

                Draw.World.SetColor(Color.yellow);
                Draw.World.Circle(unit.transform.position + Vector3.up * 0.1f, 0.5f, Vector3.up, 16);
                //Debug.DrawRay(unit.transform.position + Vector3.up * 1.3f, Vector3.up * 0.4f, Color.cyan);
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


        // Destroy on delete
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            for (int i = selectedUnits.Count - 1; i >= 0; i--)
            {
                Destroy(selectedUnits[i].gameObject);
            }

            selectedUnits.Clear();

            if (selectedBuilding)
                Destroy(selectedBuilding.gameObject);
        }

        //Draw.World.Cube(Vector3.zero, Vector3.one * 20, Vector3.forward, up);
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

    void SpawnUnit(Unit unit)
    {
        var building = unitProducedInBuildingMap[unit];

        Instantiate(unitBeingProduced, building.transform.position + Vector3.forward, Quaternion.identity);

        unitBeingProduced = null;
        unitProgress = 0;

        money -= unit.constructable.cost;
    }

    internal void EnqueueUnit(Unit unit)
    {
        unitBuildQueue.Enqueue(unit);
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

        Queue<Unit> tempQueue = new Queue<Unit>(unitBuildQueue.Count);

        foreach (var unit in unitBuildQueue)
        {
            if (constructableUnits.Contains(unit))
                tempQueue.Enqueue(unit);
        }

        unitBuildQueue = tempQueue;

    }

    void CancelConstructingUnit()
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
}
