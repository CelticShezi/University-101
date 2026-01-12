using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputAction gameSpeed;
    private InputAction move;
    private InputAction zoom;

    public float movementSpeed = 5;
    public float zoomSpeed = 75;

    public float minY = 10;
    public float maxY = 50;
    public float maxRange = 10;

    private void Awake()
    {
        gameSpeed = InputSystem.actions.FindAction("GameSpeed");
        gameSpeed.performed += ChangeGameSpeed;
        move = InputSystem.actions.FindAction("Move");
        zoom = InputSystem.actions.FindAction("Zoom");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();
        transform.Translate(moveInput * movementSpeed * Time.deltaTime);
        float x = Mathf.Clamp(transform.position.x, -maxRange, maxRange);
        float z = Mathf.Clamp(transform.position.z, -maxRange, maxRange);

        float zoomInput = zoom.ReadValue<float>();
        transform.Translate(new Vector3(0, 0, zoomInput * zoomSpeed * Time.deltaTime));
        float y = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(x, y, z);
    }

    void ChangeGameSpeed(InputAction.CallbackContext ctx)
    {
        int newSpeed = (int) ctx.ReadValue<float>();
        Debug.Log($"{newSpeed}");
        GameManager.Instance.ChangeGameSpeed(newSpeed);
    }
}
