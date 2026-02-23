
﻿using System;
using GorillaLocomotion;
using UnityEngine;

namespace DevHoldableEngine
{
    public class DevHoldable : HoldableObject
    {
        public bool InHand = false,
            InLeftHand = false,
            PickUp = true,
            DidSwap = false,
            SwappedLeft = true;

        public float Distance = 0.13f,
            ThrowForce = 1.75f;

        public Action OnPickUp;
        public Action OnSwapHands;
        public Action OnPutDown;

        public virtual void OnGrab(bool isLeft)
        {
            OnPickUp?.Invoke();
        }

        public virtual void OnDrop(bool isLeft)
        {
            if (isLeft)
            {
                InLeftHand = true;
                InHand = false;
                transform.SetParent(null);

                EquipmentInteractor.instance.leftHandHeldEquipment = null;
            }
            else
            {
                InLeftHand = false;
                InHand = false;
                transform.SetParent(null);

                EquipmentInteractor.instance.rightHandHeldEquipment = null;
            }
            OnPutDown?.Invoke();
        }

        public void Update()
        {
            var leftGrip = ControllerInputPoller.instance.leftControllerGripFloat > 0.6f;
            var rightGrip = ControllerInputPoller.instance.rightControllerGripFloat > 0.6f;

            DidSwap = (!DidSwap || (!SwappedLeft ? leftGrip : rightGrip)) && DidSwap;

            var pickLeft =
                PickUp
                && leftGrip
                && Vector3.Distance(
                    GTPlayer.Instance.LeftHand.controllerTransform.position,
                    transform.position
                )
                    < Distance * GTPlayer.Instance.scale
                && !InHand
                && EquipmentInteractor.instance.leftHandHeldEquipment == null
                && !DidSwap;
            var swapLeft =
                InHand
                && leftGrip
                && rightGrip
                && !DidSwap
                && Vector3.Distance(
                    GTPlayer.Instance.LeftHand.controllerTransform.position,
                    transform.position
                )
                    < Distance * GTPlayer.Instance.scale
                && !SwappedLeft
                && EquipmentInteractor.instance.leftHandHeldEquipment == null;
            if (pickLeft || swapLeft)
            {
                DidSwap = swapLeft;
                SwappedLeft = true;
                InLeftHand = true;
                InHand = true;

                transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent);
                GorillaTagger.Instance.StartVibration(true, 0.07f, 0.07f);
                EquipmentInteractor.instance.leftHandHeldEquipment = this;
                if (DidSwap)
                    EquipmentInteractor.instance.rightHandHeldEquipment = null;

                OnGrab(true);
            }
            else if (!leftGrip && InHand && InLeftHand || !PickUp && InHand)
            {
                OnDrop(true);
                return;
            }

            bool pickRight =
                PickUp
                && rightGrip
                && Vector3.Distance(
                    GTPlayer.Instance.RightHand.controllerTransform.position,
                    transform.position
                )
                    < Distance * GTPlayer.Instance.scale
                && !InHand
                && EquipmentInteractor.instance.rightHandHeldEquipment == null
                && !DidSwap;
            bool swapRight =
                InHand
                && leftGrip
                && rightGrip
                && !DidSwap
                && Vector3.Distance(
                    GTPlayer.Instance.RightHand.controllerTransform.position,
                    transform.position
                )
                    < Distance * GTPlayer.Instance.scale
                && SwappedLeft
                && EquipmentInteractor.instance.rightHandHeldEquipment == null;
            if (pickRight || swapRight)
            {
                DidSwap = swapRight;
                SwappedLeft = false;

                InLeftHand = false;
                InHand = true;
                transform.SetParent(GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent);

                GorillaTagger.Instance.StartVibration(false, 0.07f, 0.07f);
                EquipmentInteractor.instance.rightHandHeldEquipment = this;
                if (DidSwap)
                    EquipmentInteractor.instance.leftHandHeldEquipment = null;

                OnGrab(false);
            }
            else if (!rightGrip && InHand && !InLeftHand || !PickUp && InHand)
            {
                OnDrop(false);
                return;
            }
        }

        public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
        {
            throw new NotImplementedException();
        }

        public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
        {
            throw new NotImplementedException();
        }

        public override void DropItemCleanup()
        {
            throw new NotImplementedException();
        }
    }
}
