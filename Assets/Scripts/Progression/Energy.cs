using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    // components
    Text energyValueText;
    Text refillTimerLabel;
    Text refillTimerValue;

    // energy variables
    const int BASE_ENERGY = 3;
    int maxEnergy = 3;
    public static int energyValue;

    // timer
    const float DEFAULT_REFILL_TIME = 3f;
    float secondsUntilEnergyRefill = DEFAULT_REFILL_TIME;

    void Start()
    {
        energyValueText = this.transform.Find("EnergyValue").GetComponent<Text>();
        refillTimerLabel = this.transform.Find("RefillTimerLabel").GetComponent<Text>();
        refillTimerValue = this.transform.Find("RefillTimerValue").GetComponent<Text>();
        energyValue = BASE_ENERGY;
        energyValueText.text = energyValue.ToString();
    }

    void Update()
    {
        if(energyValue == maxEnergy)
        {
            DisableEnergyTimer();
        } 
        else
        {
            if (!refillTimerLabel.gameObject.activeSelf && !refillTimerValue.gameObject.activeSelf)
                EnableEnergyTimer();

            secondsUntilEnergyRefill -= Time.deltaTime;
            refillTimerValue.text = ((int)secondsUntilEnergyRefill).ToString();

            if (secondsUntilEnergyRefill <= 0f)
            {
                timerEnded();
            }
        }

        energyValueText.text = energyValue.ToString();
    }

    void timerEnded()
    {
        if (energyValue != maxEnergy) 
            energyValue++;

        energyValueText.text = energyValue.ToString();

        secondsUntilEnergyRefill = DEFAULT_REFILL_TIME;
    }

    void DisableEnergyTimer()
    {
        refillTimerLabel.gameObject.SetActive(false);
        refillTimerValue.gameObject.SetActive(false);
    }

    void EnableEnergyTimer()
    {
        refillTimerLabel.gameObject.SetActive(true);
        refillTimerValue.gameObject.SetActive(true);
    }
}
