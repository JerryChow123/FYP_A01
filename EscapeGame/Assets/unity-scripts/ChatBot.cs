using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBot : MonoBehaviour
{
    public float look_distance = 5f;
    public float turnSpeed = 1f;

    GameObject head;
    GameObject body;
    GameObject player;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        head = transform.Find("head").gameObject;
        body = transform.Find("body").gameObject;
        player = GameObject.Find("Player");
        animator = GetComponent<Animator>();
    }

	// Update is called once per frame
	void Update()
    {
        if (player == null)
            return;

        if (Vector3.Distance(transform.position, player.transform.position) <= look_distance)
        {
            Vector3 pos = player.transform.position;
            pos.y -= 0.4f;
            //transform.LookAt(pos);

            Vector3 relativePos = pos - transform.position;

            // the second argument, upwards, defaults to Vector3.up
            Quaternion r = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, r, turnSpeed * Time.deltaTime);

            animator.SetBool("Looking", true);

            //Debug.Log("LookAt Player !!");
        }
        else
		{
            animator.SetBool("Looking", false);
        }
    }
}
