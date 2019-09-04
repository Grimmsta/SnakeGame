using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public string sideInput = "Horizontal";
    public string upAndDownInput = "Vertical";
    public float speed;

    private Vector2 movement;
    private void Awake()
    {

    }
    //Todo: Check if we are going forward, no turning backwards (disable the input for what backwards)

    private void FixedUpdate()
    {
        transform.Translate(movement * speed);


        movement.x = Input.GetAxisRaw(sideInput);
        movement.y = Input.GetAxisRaw(upAndDownInput);

    }
}
