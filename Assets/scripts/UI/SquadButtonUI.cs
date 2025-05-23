using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SquadButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private Button _button;
    [SerializeField] private Outline _outline;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _deselectedColor;

    private SquadScriptableObject _squadSO;
    private static Action<SquadButtonUI> OnSquadUISelected;

    private void Start()
    {
        OnSquadUISelected += SquadButtonUI_OnSquadUISelected;
    }

    public void InitButton(SquadScriptableObject squadSO)
    {
        _textMeshPro.text = squadSO.SquadName;
        _squadSO = squadSO;

        _button.onClick.AddListener(Btn_Click);
    }

    private void Btn_Click()
    {
        GameManager.Instance.SetPlayerSelectedSquad(_squadSO);
        OnSquadUISelected?.Invoke(this);
    }

    private void SquadButtonUI_OnSquadUISelected(SquadButtonUI squadButtonUI)
    {
        if (squadButtonUI == this) 
            _outline.effectColor = _selectedColor;
        else
            _outline.effectColor = _deselectedColor;
    }
}
