using UnityEngine;


public abstract class GameVehicle : MonoBehaviour
{
    public bool vehicleActive;

    public abstract string VehicleName { get; }

    public abstract void StartVehicle(VoxelCharacterController player);

    public abstract void StopVehicle();

    private void OnTriggerEnter(Collider col)
    {
        if (!vehicleActive && col.CompareTag("Player"))
        {
            col.GetComponent<VoxelCharacterController>().EnterVehicleRadius(this);
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (!vehicleActive && col.CompareTag("Player"))
        {
            col.GetComponent<VoxelCharacterController>().ExitVehicleRadius(this);
        }
    }
}
