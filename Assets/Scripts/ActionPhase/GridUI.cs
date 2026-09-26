using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridUI : MonoBehaviour
{
    public static GridUI Instance { get; private set; }
    public LayerMask grid_layer;
    public Grid grid;
    public GameObject grid_indicator;
    public static Vector3 current_grid_world_position = Vector3.zero;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        grid_indicator.transform.localScale = new Vector3(grid.cellSize.x, grid.cellSize.z, 1f);
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void showIndicator(bool is_selected)
    {
        if(is_selected) {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            StartCoroutine(indicatorGuide());
        }
        else {
            StopAllCoroutines();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            grid_indicator.SetActive(false);
        }
    }

    private IEnumerator indicatorGuide()
    {
        while (true) {
            Vector3 mouse_postion = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mouse_postion);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, grid_layer)) {
                grid_indicator.SetActive(true);
                Vector3Int grid_position = grid.WorldToCell(hit.point);
                grid_indicator.transform.position = grid.GetCellCenterWorld(grid_position);
                current_grid_world_position = grid.GetCellCenterWorld(grid_position);
            }
            else {
                grid_indicator.SetActive(false);
            }
            yield return null;
        }
    }
}
