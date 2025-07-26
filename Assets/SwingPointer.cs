using UnityEngine;

public class SwingPointer : MonoBehaviour
{
    [Header("Pointer Settings")]
    public GameObject pointerPrefab;
    public string swingableTag = "Swingable";
    public float maxSwingDistance = 20f;

    private GameObject pointerInstance;
    private Renderer pointerRenderer;
    private Camera cam;

    public Color notSwingableColor = Color.red;
    public Color swingableColor = Color.green;
    public PlayerSwing playerSwing; // Assign this in the Inspector

    void Start()
    {
        cam = Camera.main;
        if (pointerPrefab != null)
        {
            pointerInstance = Instantiate(pointerPrefab);
            pointerRenderer = pointerInstance.GetComponent<Renderer>();
            pointerInstance.SetActive(false);
        }
    }

    void Update()
    {
        
        
        if (pointerInstance == null || cam == null) return;

        // Check if the player is swinging
        if (playerSwing != null && playerSwing.IsSwinging)
        {
            // Optionally, hide the pointer while swinging
            pointerInstance.SetActive(false);
            return;
        }

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxSwingDistance))
        {
            pointerInstance.SetActive(true);
            pointerInstance.transform.position = hit.point;
            if (hit.collider.CompareTag(swingableTag))
            {
                pointerRenderer.material.color = swingableColor;
                Debug.Log("Swingable");
            }
            else
            {
                pointerRenderer.material.color = notSwingableColor;
            }
        }
        else
        {
            pointerInstance.SetActive(false);
        }
    }
} 