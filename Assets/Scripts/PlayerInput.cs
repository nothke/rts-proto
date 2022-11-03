using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Utils;

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

    private void Start()
    {
        //StartConstructingBuilding(buildingPrefabBeingPlaced);
    }

    void Update()
    {
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
    }

    public void StartConstructingBuilding(Building building)
    {
        buildingBeingConstructed = building;

    }
}
