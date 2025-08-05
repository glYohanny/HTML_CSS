using UnityEngine;

public class UnitSelectionVisual : MonoBehaviour
{
    private GameObject selectionCircle;

    void Start()
    {
        // Busca el hijo llamado "SelectionCircle"
        selectionCircle = transform.Find("SelectionCircle")?.gameObject;
        if (selectionCircle != null)
            selectionCircle.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectionCircle != null)
            selectionCircle.SetActive(selected);
    }
} 