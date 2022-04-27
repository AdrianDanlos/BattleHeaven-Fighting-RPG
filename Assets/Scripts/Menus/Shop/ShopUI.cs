using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum Transactions
{
    CHEST,
    ENERGY
}

public class ShopUI : MonoBehaviour
{
    // UI
    int previousTransaction;
    GameObject chestPopUp;
    GameObject buyConfirmation;
    GameObject chestRewards;
    GameObject chestPopUpChest;
    GameObject nextButton;
    GameObject inventoryButton;
    GameObject backToShopButton;
    GameObject noCurrencyButton;
    GameObject abortButton;
    GameObject confirmButton;
    TextMeshProUGUI messageText;

    // shop flow
    string chestButtonPressed;
    int gemsValue = 0;

    // data
    Fighter fighterData;

    // test
    [SerializeField] private List<GameObject> frameColors = new List<GameObject>();


    private void Awake()
    {
        chestPopUp = GameObject.Find("Canvas_PopUp_Chest");
        buyConfirmation = GameObject.Find("Canvas_Buy_Confirmation");
        chestRewards = GameObject.Find("Rewards");
        chestPopUpChest = GameObject.Find("PopUp_Chest");
        nextButton = GameObject.Find("Button_Next");
        inventoryButton = GameObject.Find("Button_Inventory");
        backToShopButton = GameObject.Find("Button_BackToShop");
        noCurrencyButton = GameObject.Find("Button_NoCurrency");
        abortButton = GameObject.Find("Button_Abort");
        confirmButton = GameObject.Find("Button_Confirm");
        messageText = GameObject.Find("Message_Text").GetComponent<TextMeshProUGUI>();
        fighterData = GameObject.Find("FighterWrapper").gameObject.transform.GetChild(0).GetComponent<Fighter>();

        // hide UI
        chestPopUp.SetActive(false);
        buyConfirmation.SetActive(false);
        chestRewards.SetActive(false);
        chestPopUpChest.SetActive(false);
        nextButton.SetActive(false);
        chestRewards.SetActive(false);
        inventoryButton.SetActive(false);
        backToShopButton.SetActive(false);
        noCurrencyButton.SetActive(false);
        abortButton.SetActive(false);
        confirmButton.SetActive(false);
    }

    public void ConfirmPurchase()
    {
        switch (previousTransaction)
        {
            case (int) Transactions.CHEST:
                HandleChestPopUp();
                break;
            case (int)Transactions.ENERGY:
                HandleEnergyPopUp();
                break;
            default:
                break;
        }
    }

    public void ClosePurchase()
    {
        buyConfirmation.SetActive(false);
        noCurrencyButton.SetActive(false);
        chestPopUp.SetActive(false);
        inventoryButton.SetActive(false);
        backToShopButton.SetActive(false);
    }

    public void BuyChest()
    {
        chestButtonPressed = EventSystem.current.currentSelectedGameObject.name;
        GetChestValueFromType(chestButtonPressed);

        // handle which chest was opened to change icon after
        if (CurrencyHandler.instance.hasEnoughGems(gemsValue))
        {
            buyConfirmation.SetActive(true);
            abortButton.SetActive(true);
            confirmButton.SetActive(true);
        }
        else
        {
            buyConfirmation.SetActive(true);
            abortButton.SetActive(false);
            confirmButton.SetActive(false);
            noCurrencyButton.SetActive(true);
            messageText.text = "Not enough gems!";
        }
    }

    public void HandleChestPopUp()
    {
        CurrencyHandler.instance.SubstractGems(gemsValue);
        buyConfirmation.SetActive(false);
        abortButton.SetActive(false);
        confirmButton.SetActive(false);

        // TODO: create chest object prefab and chest values static class 
        // TODO: update userdatabars in real time
        previousTransaction = (int)Transactions.CHEST;
        chestPopUp.SetActive(true);
        nextButton.SetActive(true);
        chestPopUpChest.SetActive(true);
        messageText.text = "Are you sure about buying this item ?";
    }

    public void HandleEnergyPopUp()
    {
        // TODO: energy 
    }

    public void OpenChest()
    {
        nextButton.SetActive(false);
        chestPopUpChest.SetActive(false);
        chestRewards.SetActive(true);
        inventoryButton.SetActive(true);
        backToShopButton.SetActive(true);


        // reward mockup
        switch (ChestManager.OpenChest(chestButtonPressed))
        {
            case "RARE":
                chestRewards.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = frameColors[1].GetComponent<SpriteRenderer>().sprite;
                break;
            case "EPIC":
                chestRewards.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = frameColors[2].GetComponent<SpriteRenderer>().sprite;
                break;
            case "LEGENDARY":
                chestRewards.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = frameColors[3].GetComponent<SpriteRenderer>().sprite;
                break;
        }
    }

    public void GoToInventory()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Inventory.ToString());
    }

    // fighter call from skillcollection
    public void GetFighterSkills()
    {
        Inventory.GetFighterSkillsData(fighterData.skills);
    }

    public void GetChestValueFromType(string chestType)
    {
        switch ((Chest.ShopChestTypes)System.Enum.Parse
            (typeof(Chest.ShopChestTypes), chestType.ToUpper()))
        {
            case Chest.ShopChestTypes.NORMAL:
                gemsValue = Chest.shopChestsValue[Chest.ShopChestTypes.NORMAL]["gems"];
                break;
            case Chest.ShopChestTypes.EPIC:
                gemsValue = Chest.shopChestsValue[Chest.ShopChestTypes.EPIC]["gems"];
                break;
            case Chest.ShopChestTypes.LEGENDARY:
                gemsValue = Chest.shopChestsValue[Chest.ShopChestTypes.LEGENDARY]["gems"];
                break;
        }
    }
}
