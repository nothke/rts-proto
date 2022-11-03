using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    new public Camera camera;

    public Transform testT;

    public Building buildingPrefabBeingPlaced;

    public Transform cantPlaceCube;

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

        if (buildingPrefabBeingPlaced)
        {
            var tile = World.instance.GetTile(coord.x, coord.y);
            Debug.Log(tile.coord);

            Vector3 buildingPosition =
                new Vector3(tile.coord.x + 0.5f, 0, tile.coord.y + 0.5f);

            //buildingPrefabBeingPlaced.transform.position = buildingPosition;

            Nothke.Utils.ObjectPreviewer.SetObject(buildingPrefabBeingPlaced.gameObject);
            Nothke.Utils.ObjectPreviewer.Render(buildingPosition, Quaternion.identity, Vector3.one);


            if (tile.IsOcupied())
                cantPlaceCube.transform.position = buildingPosition;
            else
                cantPlaceCube.transform.position = Vector3.one * 1000;


            if (Input.GetMouseButtonDown(0))
            {
                if (!tile.IsOcupied())
                {
                    var building = Instantiate(buildingPrefabBeingPlaced);
                    building.transform.position = buildingPosition;

                    World.instance.PlaceBuilding(building, tile.coord);
                }


            }
        }
    }
}
