using UnityEngine;
using UnityEngine.UI;

public class SquadSelectorUI : MonoBehaviour
{
    [SerializeField] private Transform SquadButtonPrefab;
    [SerializeField] private Transform SquadButtonsContainerTransform;

    private void Start()
    {
        CreateSquadButtons();
    }

    private void CreateSquadButtons()
    {
        foreach (Transform child in SquadButtonsContainerTransform) 
        { 
            Destroy(child.gameObject);
        }

        SquadScriptableObject[] squadSOs = GameManager.Instance.GetAvailableSquads(Factions.Rome);

        foreach (SquadScriptableObject squad in squadSOs)
        {
            Transform squadButtonTransform = Instantiate(SquadButtonPrefab, SquadButtonsContainerTransform);
            squadButtonTransform.GetComponent<SquadButtonUI>().InitButton(squad);
        }
    }
}
