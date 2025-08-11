using UnityEngine;

public class ObjectClicker : MonoBehaviour
{
    [SerializeField] Camera cam;    

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Mobilde dokunma da �al���r
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("T�klanan nesne: " + hit.collider.transform.parent.name);

                // E�er belirli bir tag'a sahipse etkile�im
                if (hit.collider.CompareTag("Interactable"))
                {
                    hit.collider.transform.parent.GetComponent<TileManager>()?.Interact();
                }
            }
        }
    }
}
