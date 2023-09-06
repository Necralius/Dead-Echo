using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickinessDetector : MonoBehaviour
{
    FPS_Controller _controller = null;

    private void Start()
    {
        _controller = GetComponentInParent<FPS_Controller>();
    }

    private void OnTriggerStay(Collider other)
    {
        AiStateMachine machine = GameSceneManager.instance.GetAIStateMachine(other.GetInstanceID());
        if (machine != null && _controller != null)
        {
            _controller.DoStickiness();
            machine.VisualThreat.Set(AITargetType.Visual_Player, 
                _controller.characterController, 
                _controller.transform.position, 
                Vector3.Distance(machine.transform.position, _controller.transform.position));
            machine.SetStateOverride(AIStateType.Attack);
        }
    }
}