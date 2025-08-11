using UnityEngine;

public class ObjectClicker : MonoBehaviour
{
    [SerializeField] Camera cam;    

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Mobilde dokunma da çalýþýr
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Týklanan nesne: " + hit.collider.transform.parent.name);

                // Eðer belirli bir tag'a sahipse etkileþim
                if (hit.collider.CompareTag("Interactable"))
                {
                    hit.collider.transform.parent.GetComponent<TileManager>()?.Interact();
                }
            }
        }
    }
}
