using UnityEngine;

public class PlantSway : MonoBehaviour
{
    public float SwaySpeed = 1f;
    public float SwayAmount = 5f; 

    private float _startRotation;

    void Start()
    {
        _startRotation = transform.eulerAngles.z;
    }

    void Update()
    {
        float sway = Mathf.Sin(Time.time * SwaySpeed) * SwayAmount;

        transform.rotation = Quaternion.Euler(0, 0, _startRotation + sway);
    }
}

