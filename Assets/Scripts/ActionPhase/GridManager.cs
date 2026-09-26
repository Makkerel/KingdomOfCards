using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    private Dictionary<Vector3, PhysicalBuilding> grid_state = new Dictionary<Vector3, PhysicalBuilding>();
    public event Action placedBuilding;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public PhysicalBuilding is_building_here(Vector3 position)
    {
        if (grid_state.TryGetValue(position, out PhysicalBuilding building)) {
            return building;
        }
        return null;
    }

    public void addGridState(Vector3 position, PhysicalBuilding building)
    {
        grid_state.Add(position, building);
        placedBuilding?.Invoke();
    }

    public void removeGridState(GameObject building)
    {
        grid_state.Remove(building.transform.position);
        if(grid_state.Count == 0) {
            CombatManager.Instance.LoseGame();
        }
    }
}
