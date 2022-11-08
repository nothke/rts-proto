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


    public Faction faction;

    bool rectSelecting;
    Vector2 rectSelectStart;

    GameObject lastObjectInPreview;

    public bool canSelectEnemyUnits;

    void Update()
    {
        Vector3 up = Vector3.up;

        // Camera movement

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        float dt = Time.deltaTime;
        const float maxCameraSpeed = 20.0f;

        Vector2 mousePos = Input.mousePosition;

        Vector2 camAccel = new Vector2(inputX, inputY);

#if !UNITY_EDITOR
        const float MOUSE_MOVE_MARGIN = 5;
        if (mousePos.x < MOUSE_MOVE_MARGIN)
            camAccel.x = -1;
        else if (mousePos.x > Screen.width - MOUSE_MOVE_MARGIN)
            camAccel.x = 1;

        if (mousePos.y < MOUSE_MOVE_MARGIN)
            camAccel.y = -1;
        else if (mousePos.y > Screen.height - MOUSE_MOVE_MARGIN)
            camAccel.y = 1;
#endif

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
            if (faction.buildingPrefabBeingPlaced)
            {
                var size = faction.buildingPrefabBeingPlaced.size;

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
                        faction.PlaceBuilding(faction.buildingPrefabBeingPlaced, buildingPosition, cornerCoord);
                    }
                }
            }
            else
            {
                if (LMBDown)
                {
                    faction.selectedBuilding = tile.building;
                }
            }
        }

        if (faction.buildingPrefabBeingPlaced && lastObjectInPreview != faction.buildingPrefabBeingPlaced)
            ObjectPreviewer.SetObject(faction.buildingPrefabBeingPlaced.gameObject);

        lastObjectInPreview = faction.buildingPrefabBeingPlaced ? faction.buildingPrefabBeingPlaced.gameObject : null;

        if (faction.selectedBuilding)
        {
            // Draw
            float w = faction.selectedBuilding.size.x;
            float h = faction.selectedBuilding.size.y;
            float r = Mathf.Sqrt(w * w + h * h) * 0.5f + 0.2f;
            Draw.World.Circle(faction.selectedBuilding.transform.position + up * 0.01f, r, up, 64);
        }

        if (RMBDown)
            faction.GiveMoveOrderToSelectedUnits(raycastPoint);

        foreach (var unit in faction.selectedUnits)
        {
            Draw.World.SetColor(Color.yellow);
            Draw.World.Circle(unit.transform.position + Vector3.up * 0.1f, 0.5f, Vector3.up, 16);
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

            faction.selectedUnits.Clear();

            if (Unit.all != null)
            {
                foreach (var unit in Unit.all)
                {
                    if (!canSelectEnemyUnits && unit.entity.faction != faction)
                        continue;

                    Vector3 ssPos = camera.WorldToScreenPoint(unit.transform.position);

                    Rect rect = new Rect(rectSelectStart, (Vector2)Input.mousePosition - rectSelectStart);

                    if (rect.Contains(ssPos, true))
                    {
                        faction.selectedUnits.Add(unit);
                    }
                }
            }
        }

        // Destroy on delete
        if (Input.GetKeyDown(KeyCode.Delete))
            faction.DestroySelectedUnits();

        //Draw.World.Cube(Vector3.zero, Vector3.one * 20, Vector3.forward, up);
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




}
