using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceController : StateMachineBehaviour
{
    [SerializeField]
    private AudioClip m_clip;

    private AudioSource m_audioSource;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_audioSource = animator.gameObject.GetComponent<AudioSource>();
        if (m_audioSource != null && m_clip != null)
        {
            Debug.Log("OnStateEnter " + m_clip.name + " layer=" + layerIndex);
            m_audioSource.PlayOneShot(m_clip);
        }
    } /* OnStateEnter */

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("OnStateExit " + stateInfo.ToString() + " layer=" + layerIndex);
    } /* OnStateExit */

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
} /* class */
