using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] GameObject BroccoliSteps;
    int currentStep = 0;

    // Timer de�i�kenleri
    private float timer = 0f;
    private float stepDuration = 3f; // Her step i�in 3 saniye
    private bool isGrowing = false;


    public void Interact()
    {
        BroccoliSteps.gameObject.SetActive(true);

        // Timer'� ba�lat
        timer = 0f;
        currentStep = 0;
        isGrowing = true;
        CropLifeCycle(); // �lk step'i g�ster

    }

    private void Update()
    {
        // E�er b�y�me aktifse timer'� �al��t�r
        if (isGrowing)
        {
            timer += Time.deltaTime;

            // 3 saniye doldu�unda
            if (timer >= stepDuration)
            {
                timer = 0f; // Timer'� s�f�rla
                IncreaseCurrentStep(); // Sonraki step'e ge�

                // Son step'e ula�t�ysak
                if (currentStep >= 3)
                {
                    isGrowing = false;
                    BroccoliSteps.gameObject.SetActive(false);
                    currentStep = 0;
                }
                else
                {
                    CropLifeCycle(); // Yeni step'i g�ster
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
                    Debug.Log("B�yle bir ad�m yok");
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
