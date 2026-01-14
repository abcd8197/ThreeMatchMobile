using UnityEngine;
using UnityEngine.InputSystem;

namespace ThreeMatch
{
    public class Tester : MonoBehaviour
    {
        public ItemType ItemType;
        public int amount;

        private InputAction clickAction;

        private void Awake()
        {
            clickAction = InputSystem.actions.FindAction("Interact");
            clickAction.performed += ClickAction_performed;
        }

        private void OnDestroy()
        {
            clickAction.performed -= ClickAction_performed;
        }

        private void ClickAction_performed(InputAction.CallbackContext obj)
        {
            if (obj.phase != InputActionPhase.Performed)
                return;

            if (ItemType != ItemType.None)
            {
                if (amount > 0)
                    Main.Instance.GetManager<ItemManager>().AddItem(ItemType, amount);
                else if (amount < 0)
                    Main.Instance.GetManager<ItemManager>().UseItem(ItemType, amount);
            }
        }

    }
}