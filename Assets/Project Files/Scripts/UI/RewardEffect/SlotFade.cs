using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Project_Files.Scripts.UI.RewardEffect
{
  public class SlotFade : MonoBehaviour
  {
    [SerializeField] private CanvasGroup _tigerGroup;
    [SerializeField] private CanvasGroup _slotGroup;

    private void OnEnable() {
      _tigerGroup.alpha = 0;
      _tigerGroup.gameObject.SetActive(true);
      StartCoroutine(FadeCanvasGroup(_tigerGroup, 1, 1, 0));
    }

    public void OnClick() {
      StartCoroutine(FadeCanvasGroup(_tigerGroup, 0, 1, onComplete: () => {
        _slotGroup.gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(_slotGroup, 1, 1, 0));
      }));
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration,
      float startAlpha = -1f, Action onComplete = null) {
      if (Mathf.Approximately(startAlpha, -1)) {
        startAlpha = canvasGroup.alpha;
      } else {
        canvasGroup.alpha = 0;
      }

      float timeElapsed = 0f;

      while (timeElapsed < duration) {
        timeElapsed += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
        yield return null;
      }

      canvasGroup.alpha = targetAlpha;
      onComplete?.Invoke();
    }
  }
}