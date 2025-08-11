using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] GameObject BroccoliSteps;
    int currentStep = 0;

    // Timer deðiþkenleri
    private float timer = 0f;
    private float stepDuration = 3f; // Her step için 3 saniye
    private bool isGrowing = false;


    public void Interact()
    {
        BroccoliSteps.gameObject.SetActive(true);

        // Timer'ý baþlat
        timer = 0f;
        currentStep = 0;
        isGrowing = true;
        CropLifeCycle(); // Ýlk step'i göster

    }

    private void Update()
    {
        // Eðer büyüme aktifse timer'ý çalýþtýr
        if (isGrowing)
        {
            timer += Time.deltaTime;

            // 3 saniye dolduðunda
            if (timer >= stepDuration)
            {
                timer = 0f; // Timer'ý sýfýrla
                IncreaseCurrentStep(); // Sonraki step'e geç

                // Son step'e ulaþtýysak
                if (currentStep >= 3)
                {
                    isGrowing = false;
                    BroccoliSteps.gameObject.SetActive(false);
                    currentStep = 0;
                }
                else
                {
                    CropLifeCycle(); // Yeni step'i göster
                }
            }
        }
    }

    private void ShowStepOne(GameObject go)
    {
        go.transform.GetChild(0).gameObject.SetActive(true);
        go.transform.GetChild(1).gameObject.SetActive(false);
        go.transform.GetChild(2).gameObject.SetActive(false);
    }

    private void ShowStepTwo(GameObject go)
    {
        go.transform.GetChild(0).gameObject.SetActive(false);
        go.transform.GetChild(1).gameObject.SetActive(true);
        go.transform.GetChild(2).gameObject.SetActive(false);

    }

    private void ShowStepThree(GameObject go)
    {
        go.transform.GetChild(0).gameObject.SetActive(false);
        go.transform.GetChild(1).gameObject.SetActive(false);
        go.transform.GetChild(2).gameObject.SetActive(true);
    }

    private void CropLifeCycle()
    {
        switch
            (currentStep)
            {
                case 0:
                    ShowStepOne(BroccoliSteps);
                    break;
                case 1:
                    ShowStepTwo(BroccoliSteps);
                    break;
                case 2:
                    ShowStepThree(BroccoliSteps);
                    break;
                default:
                    Debug.Log("Böyle bir adým yok");
                    break;
        }
    }

    public void IncreaseCurrentStep()
    {
        if(currentStep < 3)
        {
            currentStep++;
        }
        else
        {
            currentStep = 0;
        }
    }
}
