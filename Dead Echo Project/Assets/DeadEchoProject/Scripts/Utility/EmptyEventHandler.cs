using UnityEngine;
using UnityEngine.Events;

public class EmptyEventHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent onSceneStart;
    [SerializeField] private UnityEvent onScenePause;
    [SerializeField] private UnityEvent onSceneResume;
    [SerializeField] private UnityEvent onSceneStop;

    [SerializeField] private UnityEvent onCostumCall;

    void Start()
    {
        onSceneStart.Invoke();
    }
    public void CostumCallEvent() => onCostumCall.Invoke();

    // Update is called once per frame
    void Update()
    {
        
    }
}
