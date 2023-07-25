using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private Animator anim => GetComponent<Animator>();

    private int HorizontalHash = 0;
    private int VerticalHash = 0;
    private int AttackHash = 0;


    private void Start()
    {
       HorizontalHash = Animator.StringToHash("Horizontal");
       VerticalHash = Animator.StringToHash("Vertical");
       AttackHash = Animator.StringToHash("Attack");
    }

    private void Update()
    {
        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical") * 5.65f;

        if (Input.GetMouseButtonDown(0)) anim.SetTrigger(AttackHash);

        anim.SetFloat(HorizontalHash, xAxis, 0.3f, Time.deltaTime);
        anim.SetFloat(VerticalHash, yAxis, 1.0f, Time.deltaTime);
    }
}