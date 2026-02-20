using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float sideSpeed = 5f; 
    [SerializeField] private float maxLaneX = 1.5f; 

    void Update()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && (gm.IsGameFlowBlocked() || Time.timeScale < 1f))
        {
            return;
        }
        float horizontalInput = Input.GetAxis("Horizontal");
        float xMove = horizontalInput * sideSpeed * Time.deltaTime;
        
        Vector3 newPosition = transform.position;
        newPosition.x += xMove;
        newPosition.x = Mathf.Clamp(newPosition.x, -maxLaneX, maxLaneX);        
        
        transform.position = newPosition;
        transform.rotation = Quaternion.identity;
    }
}