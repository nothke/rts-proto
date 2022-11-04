using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Utils;
using System;

public class PlayerInput : MonoBehaviour
{
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

    public UnitProducer activeBarracks;

    bool rectSelecting;
    Vector2 rectSelectStart;

    private void Start()
    {

    }

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
            ref var tile = ref World.instance.GetTile(coord.x, coord.y);
            Debug.Log(tile.coord);

            Vector3 buildingPosition =
                new Vector3(tile.coord.x + 0.5f, 0, tile.coord.y + 0.5f);

            //buildingPrefabBeingPlaced.transform.position = buildingPosition;

            Material previewMat = tile.IsOcupied() ? previewRedMat : previewGreenMat;

            ObjectPreviewer.Render(buildingPosition, Quaternion.identity, Vector3.one * 1.02f, previewMat);

            if (LMBDown)
            {
                if (!tile.IsOcupied())
                {
                    var building = Instantiate(buildingPrefabBeingPlaced);
                    building.transform.position = buildingPosition;

                    tile.building = building;

                    buildingPrefabBeingPlaced = null;
                    //World.instance.PlaceBuilding(building, tile.coord);

                    var producer = building.GetComponent<UnitProducer>();
                    if (producer && !activeBarracks)
                        activeBarracks = producer;

                }
            }
        }

        // Construction progress
        if (buildingBeingConstructed)
        {
            buildingProgress += dt;

            if (buildingProgress > buildingBeingConstructed.timeToBuild)
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

            if (unitProgress > unitBeingProduced.timeToBuild)
            {
                var unitGO = Instantiate(unitBeingProduced);
                unitGO.transform.position = activeBarracks.transform.position + Vector3.forward;

                unitBeingProduced = null;
                unitProgress = 0;
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
        if (money >= building.cost)
        {
            money -= building.cost;
            buildingBeingConstructed = building;
        }
    }
}
