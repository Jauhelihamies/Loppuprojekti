using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Image displayImage;

    private void Start()
    {
        if (displayImage != null)
        {
            displayImage.gameObject.SetActive(false); 
        }
    }

    public void FlashImage(Sprite newSprite, float duration)
    {
        if (displayImage == null) return;

   
        StopAllCoroutines();

        StartCoroutine(ShowAndHideRoutine(newSprite, duration));
    }

    private IEnumerator ShowAndHideRoutine(Sprite newSprite, float duration)
    {
        displayImage.sprite = newSprite;
        displayImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        displayImage.gameObject.SetActive(false);
    }
}
