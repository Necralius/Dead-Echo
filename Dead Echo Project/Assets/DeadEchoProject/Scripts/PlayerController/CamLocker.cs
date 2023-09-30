using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamLocker : MonoBehaviour
{
    [SerializeField] private Transform camPos = null;
    [HideInInspector] public ControllerManager _playerController;

    private void Awake() => _playerController = camPos.GetComponentInParent<ControllerManager>();
    private void Update() => transform.position = camPos.position;
}