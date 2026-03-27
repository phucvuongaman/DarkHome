// Deleted
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class ObjectInteractableController : MonoBehaviour
    {
        private BaseObject _object;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask inactiveLayer;

        private void Start()
        {
            _object = GetComponent<BaseObject>();
        }

        // private void Update()
        // {
        //     HandleActiveItem();
        // }

        private void HandleActiveItem()
        {
            switch (_object.InteractType)
            {
                case InteractableType.Item:
                    ItemObject();
                    break;
                case InteractableType.Door:
                    DoorObject();
                    break;
                case InteractableType.PuzzleCode:
                    PuzzleCodeInputObject();
                    break;
                case InteractableType.PuzzlePoint:
                    PuzzleItemPlacementObject();
                    break;
                case InteractableType.PuzzleSwitch:
                    PuzzleSwithcObject();
                    break;
                case InteractableType.PuzzleItemPlacement:
                    PuzzleItemPlacementObject();
                    break;
                default:
                    DefaultObject();
                    break;
            }
        }


        private void DefaultObject()
        {
            BaseObject item = GetComponent<BaseObject>();
            if (FlagManager.Instance.HasAllFlags(item.RequiredFlags))
                _object.gameObject.layer = LayerMask.NameToLayer("Interactable");
            else
                _object.gameObject.layer = LayerMask.NameToLayer("Inactive");
        }

        private void ItemObject()
        {
            Item item = GetComponent<Item>();
            if (FlagManager.Instance.HasAllFlags(item.RequiredFlags))
                _object.gameObject.layer = LayerMask.NameToLayer("Interactable");
            else
                _object.gameObject.layer = LayerMask.NameToLayer("Inactive");
        }

        private void DoorObject()
        {
            Door door = GetComponent<Door>();
            HingeJoint joint = GetComponent<HingeJoint>();
            Rigidbody rb = GetComponent<Rigidbody>();

            // if (HasQuestActive(door.requiredQuestID))
            if (FlagManager.Instance.HasAllFlags(door.RequiredFlags))
            {
                _object.gameObject.layer = LayerMask.NameToLayer("Interactable");
                // joint.useSpring = true;
                rb.isKinematic = false;
            }
            else
            {
                _object.gameObject.layer = LayerMask.NameToLayer("Inactive");
                // joint.useSpring = false;
                rb.isKinematic = true;
            }
        }


        private void AreaTriggerObject()
        {
            TriggerArea area = GetComponent<TriggerArea>();
            if (FlagManager.Instance.HasAllFlags(area.RequiredFlags))
                _object.gameObject.layer = LayerMask.NameToLayer("Interactable");
            else
                _object.gameObject.layer = LayerMask.NameToLayer("Inactive");
        }
        private void PuzzleItemPlacementObject()
        {
            ItemPlacementPuzzle itemPlace = GetComponent<ItemPlacementPuzzle>();
            if (FlagManager.Instance.HasAllFlags(itemPlace.RequiredFlags))
                _object.gameObject.layer = LayerMask.NameToLayer("Interactable");
            else
                _object.gameObject.layer = LayerMask.NameToLayer("Inactive");
        }
        private void PuzzleCodeInputObject()
        {
            PuzzleCodeInput codeInput = GetComponent<PuzzleCodeInput>();
            if (FlagManager.Instance.HasAllFlags(codeInput.RequiredFlags))
                _object.gameObject.layer = LayerMask.NameToLayer("Interactable");
            else
                _object.gameObject.layer = LayerMask.NameToLayer("Inactive");
        }
        private void PuzzleSwithcObject()
        {
            ItemPlacementPuzzle puzSwitch = GetComponent<ItemPlacementPuzzle>();
            if (FlagManager.Instance.HasAllFlags(puzSwitch.RequiredFlags))
                _object.gameObject.layer = LayerMask.NameToLayer("Interactable");
            else
                _object.gameObject.layer = LayerMask.NameToLayer("Inactive");
        }



    }
}