using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using Tomino.Model; // BU SATIRI EKLEDİK: LocalizationProvider'ı tanıması için gerekli
using Text = UnityEngine.UI.Text;

namespace Tomino.View
{
    public class AlertView : MonoBehaviour
    {
        public Text titleText;
        public RectTransform buttonsContainer;
        public GameObject buttonPrefab;
        public LocalizationProvider localizationProvider;

        private ObjectPool<AlertButtonView> _buttonPool;
        private readonly List<AlertButtonView> _buttons = new();

        private ObjectPool<AlertButtonView> ButtonPool => _buttonPool ??= new ObjectPool<AlertButtonView>(CreateAlertButton, OnGetAlertButton, OnReleaseAlertButton);

        internal void Awake() => Hide();

        public void SetTitle(string textID) => titleText.text = localizationProvider.currentLocalization.GetLocalizedTextForID(textID);

        public void AddButton(string textID, UnityAction onClickAction, UnityAction pointerDownAction)
        {
            var alertButton = ButtonPool.Get();
            alertButton.PointerHandler.onPointerDown.AddListener(pointerDownAction);
            alertButton.Button.onClick.AddListener(onClickAction);
            alertButton.Button.onClick.AddListener(Hide);
            alertButton.Text.text = localizationProvider.currentLocalization.GetLocalizedTextForID(textID);
            alertButton.RectTransform.SetSiblingIndex(buttonsContainer.childCount - 1);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide()
        {
            _buttons.ForEach(b => ButtonPool.Release(b));
            _buttons.Clear();
            gameObject.SetActive(false);
        }

        private AlertButtonView CreateAlertButton()
        {
            var instance = Instantiate(buttonPrefab);
            var button = instance.GetComponent<AlertButtonView>();
            button.RectTransform.SetParent(buttonsContainer, false);
            instance.SetActive(false);
            return button;
        }

        private void OnGetAlertButton(AlertButtonView button) { button.gameObject.SetActive(true); _buttons.Add(button); }
        private void OnReleaseAlertButton(AlertButtonView button)
        {
            button.Button.onClick.RemoveAllListeners();
            button.PointerHandler.onPointerDown.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }
}