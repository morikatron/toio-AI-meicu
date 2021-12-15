using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace toio.AI.meicu
{
    [RequireComponent(typeof(Button))]
    public class UISwitch : MonoBehaviour
    {
        public Sprite spriteOn;
        public Sprite spriteOff;
        public bool initialOn = false;
        public UnityEvent OnClick;


        protected bool _isOn = false;
        public bool isOn {
            get { return _isOn; }
            set {
                _isOn = value;
                UpdateSprite();
            }
        }

        void OnEnable()
        {
            _isOn = initialOn;
            UpdateSprite();

            GetComponent<Button>().onClick.RemoveListener(HandleClick);
            GetComponent<Button>().onClick.AddListener(HandleClick);
        }

        void HandleClick()
        {
            isOn = !_isOn;
            OnClick.Invoke();
        }

        public void UpdateSprite()
        {
            GetComponent<Image>().sprite = _isOn? spriteOn : spriteOff;
        }
    }

}
