using UnityEngine;
using UnityEngine.EventSystems;

public class ShowOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Objek yang ingin ditampilkan saat hover")]
    public GameObject targetObject;

    private void Start()
    {
        if (targetObject != null)
            targetObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetObject != null)
            targetObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetObject != null)
            targetObject.SetActive(false);
    }
}
