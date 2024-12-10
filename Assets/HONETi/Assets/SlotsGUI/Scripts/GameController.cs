using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    [Header("Text")]
    [SerializeField] TextMeshProUGUI _linesText;
    [SerializeField] TextMeshProUGUI _betText;
    [SerializeField] TextMeshProUGUI _totalBetText;

    [Header("Buttons")]
    [SerializeField] SpinButton _spinButton; 
    [SerializeField] Button _linesButtonPlus; 
    [SerializeField] Button _linesButtonMinus; 
    [SerializeField] Button _betButtonPlus; 
    [SerializeField] Button _betButtonMinus; 
    [SerializeField] Transform _payLinesParent;

    [Header("Params")]
    [SerializeField] float _spinTime;

    [Header("Components")]
    [SerializeField] Animator _animator;

    [Header("Winning")]
    [SerializeField] List<GameObject> _winningIcons;
    [SerializeField] List<Transform> _winningSlots;
    [SerializeField] List<GameObject> _winningFrames;
    [SerializeField] GameObject _bigWinAnimation;

    [Header("Level")]
    [SerializeField] Slider _levelSlider;
    [SerializeField] GameObject _levelUpAnimation;

    List<int[,]> winningCombinations = new List<int[,]>();
    List<int> winningCombinationLines = new List<int>();

    private int _currentWinningCombination;
    private bool _spinning;
    private int _lines = 19;
    private int _bet = 100;
    private float _currentLevelPercentage;

    private bool _autoSpin;
    private bool _guiActived {
                get { return _bigWinAnimation.activeSelf || _levelUpAnimation.activeSelf; }
                set { }
    }

    public void StartSpin(bool auto)
    {
        if (!_autoSpin && auto)
        {
            _autoSpin = true;
            StartCoroutine(AutoSpin());
        }
        else if (_autoSpin && !auto)
        {
            _autoSpin = false;
        }
        else if(!_autoSpin && !auto)
        {
            if (_spinning) return;
            StartCoroutine(Spin());
        }

        _spinning = true;
    }

    private IEnumerator AutoSpin()
    {
        while (_autoSpin)
        {
            if (_guiActived) yield return null;
            else yield return StartCoroutine(Spin());
        }
    }

    private IEnumerator Spin()
    {
        _animator.SetTrigger("reels_start");

        var line = _payLinesParent.GetChild(winningCombinationLines[_currentWinningCombination] - 1);
        line.GetComponent<Animator>().SetTrigger("Normal");

        for (int i = 0; i < _winningSlots.Count; i++)
        {
            //Disable winning frames
            for (int j = 0; j < _winningFrames.Count; j++)
                _winningFrames[j].SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < _winningSlots.Count; i++)
        {
            //Destroy childs of winning slot 
            for (int j = 0; j < _winningSlots[i].childCount; j++)
            {
                _winningSlots[i].GetComponent<Image>().enabled = true;
                Destroy(_winningSlots[i].GetChild(0).gameObject);
            }
        }

        ToggleSlotsBlur(true);
        yield return new WaitForSeconds(_spinTime - 0.3f);
        ToggleSlotsBlur(false);

        yield return new WaitForSeconds(0.3f);
        _animator.SetTrigger("reels_end");

        yield return new WaitForSeconds(0.15f);

        SetWinningCombination();

        yield return new WaitForSeconds(0.5f);

        EnableWiningFrames();

        yield return new WaitForSeconds(0.5f);

        if(_currentWinningCombination == 0) _bigWinAnimation.SetActive(true);

        _spinning = false;
        
        _currentLevelPercentage = Mathf.Clamp(_currentLevelPercentage+0.25f, 0, 1);
        _levelSlider.value = _currentLevelPercentage;

        if (_currentLevelPercentage >= 1)
        {
            _levelUpAnimation.SetActive(true);
            _currentLevelPercentage = 0;
        }
    }

    private void EnableWiningFrames()
    {
        for (int j = 0; j < _winningSlots.Count; j++)
        {
            var slot = _winningSlots[j];
            if (slot.childCount > 0)
            {
                var anims = slot.GetComponentsInChildren<Animation>();
                for (int i = 0; i < anims.Length; i++)
                    anims[i].Play();

                _winningFrames[j].SetActive(true);
            }
        }

        var line = _payLinesParent.GetChild(winningCombinationLines[_currentWinningCombination] - 1);
        line.GetComponent<Animator>().SetTrigger("Highlighted");
    }

    private void SetWinningCombination()
    {
        if (_currentWinningCombination == winningCombinationLines.Count-1)
            _currentWinningCombination = 0;
        else
            _currentWinningCombination++;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var idx = winningCombinations[_currentWinningCombination][j, i];
                if (idx != -1)
                {
                    var obj = Instantiate(_winningIcons[idx], _winningSlots[j + i * 3]);
                    obj.transform.localPosition = Vector3.zero;
                    _winningSlots[j + i * 3].GetComponent<Image>().enabled = false;
                    obj.GetComponent<Animation>().Stop();
                    foreach (Transform child in obj.transform) {
                        var anim = child.GetComponent<Animation>();
                        if (anim) anim.Stop();
                    }

                }
                else
                {
                    _winningSlots[j + i * 3].GetComponent<Image>().enabled = true;
                }
            }
        }
    }

    private void ToggleSlotsBlur(bool en)
    {
        foreach (Transform child in _animator.transform)
        {
            foreach (Transform subChild in child.transform)
            {
                var anim = subChild.GetComponent<Animator>();
                if (anim)
                {
                    if (en)
                        anim.SetTrigger("start_blur");
                    else
                        anim.SetTrigger("stop_blur");
                }
            }
        }
    }

    private void Start()
    {
        _spinButton._autoSpinEvent.AddListener(StartSpin);
        _linesButtonPlus.onClick.AddListener(LinePlusClick);
        _linesButtonMinus.onClick.AddListener(LineMinusClick);
        _betButtonPlus.onClick.AddListener(BetPlusClick);
        _betButtonMinus.onClick.AddListener(BetMinusClick);

        winningCombinations.Add(new int[,] { { -1, -1, -1, -1, -1 },
                                             {  1,  1,  1,  1,  1 },
                                             { -1, -1, -1, -1, -1 }});

        winningCombinationLines.Add(1);

        winningCombinations.Add(new int[,] { {  2, -1, -1, -1,  2 },
                                             { -1,  2, -1,  2, -1 },
                                             { -1, -1,  2, -1, -1 }});

        winningCombinationLines.Add(4);

        winningCombinations.Add(new int[,] { { -1, -1,  5, -1, -1 },
                                             {  5,  5, -1,  5,  5 },
                                             { -1, -1, -1, -1, -1 }});

        winningCombinationLines.Add(14);
    }

    private void BetMinusClick()
    {
        _bet = Mathf.Clamp(_bet - 100, 100, 1000);
        _betText.text = _bet.ToString();
        if (_bet == 100) _betButtonMinus.interactable = false;

        _betButtonPlus.interactable = true;

        CalculateTotalBet();
    }

    private void BetPlusClick()
    {
        _bet = Mathf.Clamp(_bet + 100, 100, 1500);
        _betText.text = _bet.ToString();
        if (_bet == 1500) _betButtonPlus.interactable = false;

        _betButtonMinus.interactable = true;

        CalculateTotalBet();
    }

    private void LineMinusClick()
    {
        _lines = Mathf.Clamp(--_lines, 1, 25);
        _linesText.text = _lines.ToString();
        if (_lines == 1) _linesButtonMinus.interactable = false;

        _linesButtonPlus.interactable = true;

        CalculateTotalBet();
        ToggleLines();
    }

    private void ToggleLines()
    {
        for (int i = 0; i < _payLinesParent.childCount; i++)
        {
            if (i < _lines)
                _payLinesParent.GetChild(i).GetComponent<Button>().interactable = true;
            else
                _payLinesParent.GetChild(i).GetComponent<Button>().interactable = false;
        }
    }

    private void LinePlusClick()
    {
        _lines = Mathf.Clamp(++_lines, 1, 25);
        _linesText.text = _lines.ToString();
        if (_lines == 25) _linesButtonPlus.interactable = false;

        _linesButtonMinus.interactable = true;

        CalculateTotalBet();
        ToggleLines();
    }

    private void CalculateTotalBet()
    {
        _totalBetText.text = (_lines * _bet).ToString();
    }
}
