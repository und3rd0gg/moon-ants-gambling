using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpinButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    [SerializeField] float _timeToAutoSpin = 2;
    [SerializeField] TextMeshProUGUI _spinText;
    [SerializeField] TextMeshProUGUI _tipText;

    bool _autoSpin;
    bool _holding;
    float _clickTime;

    public class AutoSpinEvent : UnityEvent<bool> { }
    public AutoSpinEvent _autoSpinEvent = new AutoSpinEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        if(_autoSpin)
        {
            _autoSpin = false;
            _autoSpinEvent.Invoke(_autoSpin);
            _holding = false;

            _spinText.text = "SPIN";
            _tipText.text = "hold for auto";
            return;
        }

        _clickTime = 0f;
        _holding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_holding) return;

        _autoSpin = false;
        _autoSpinEvent.Invoke(_autoSpin);

        _spinText.text = "SPIN";
        _tipText.text = "hold for auto";

        _holding = false;
    }

	// Update is called once per frame
	void Update () {

        if (_holding)
        {
            _clickTime += Time.deltaTime;
            if(_clickTime >= _timeToAutoSpin)
            {
                _autoSpin = true;
                _autoSpinEvent.Invoke(_autoSpin);
                _holding = false;

                _spinText.text = "AUTOSPIN";
                _tipText.text = "click to stop";
            }
        }
    }
}
