using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraitSelectionUIManager : MonoBehaviour
{
    public GameObject traitPanel; 
    public Transform traitButtonContainer;
    public int traitsToChoose = 2;
    public Button comfirmButton; // Optional: A button to confirm the selection
    public Button RedoButton; // Optional: A button to cancel the selection

    private List<ITrait> selectedTraits = new List<ITrait>();
    private List<ITrait> currentOptions = new List<ITrait>();

    private void Start()
    {
        ShowTraitSelection();
    }
    public void ShowTraitSelection()
    {
        comfirmButton.interactable = false;
        selectedTraits.Clear();
        if (currentOptions.Count == 0)
        {
            currentOptions = GetRandomTraits(4);
        }
        
        foreach (Transform child in traitButtonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var trait in currentOptions)
        {
            GameObject buttonGO = Instantiate(traitPanel, traitButtonContainer,true);
            TMP_Text[] texts = buttonGO.GetComponentsInChildren<TMP_Text>();
            texts[0].text = trait.Trait.ToString();
            texts[1].text = trait.Description;

            Button button = buttonGO.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => OnTraitSelected(trait, buttonGO));
        }
    }

    List<ITrait> GetRandomTraits(int count)
    {
        List<ITrait> copy = TraitManager.instance.traits;
        List<ITrait> result = new List<ITrait>();

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return result;
    }

    void OnTraitSelected(ITrait trait, GameObject buttonGO)
    {
        if (selectedTraits.Contains(trait)) return;

        selectedTraits.Add(trait);
        buttonGO.GetComponentInChildren<Button>().interactable = false;
        buttonGO.GetComponentInChildren<Image>().color = Color.red; // Change text color to gray
        if (selectedTraits.Count == traitsToChoose)
        {
            DisableAllButtons();
            comfirmButton.interactable = true;
        }
    }
    private void DisableAllButtons()
    {
        foreach (Transform child in traitButtonContainer)
        {
            Button button = child.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    public void ApplySelectedTraits()
    {
        Debug.Log("Selected Traits:");
        foreach (var trait in selectedTraits)
        {
            Debug.Log(trait.Trait);
        }
        TraitManager.instance.PlayerSelectedTrait = selectedTraits;
        // Hide UI or trigger next stage
        this.gameObject.SetActive(false);
    }
    public void RedoSelection()
    {
        // Reset the selection and show the UI again
        selectedTraits.Clear();
        traitButtonContainer.gameObject.SetActive(true);
        ShowTraitSelection();
    }
}
