using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class ClipCreation : MonoBehaviour
{
    Animator anim;                      // Animator ������Ʈ�� ���� ������ �����ϴ� ����
    AnimatorStateMachine stateMachine;  // Animator�� ���� ��迡 ���� ������ �����ϴ� ����
    AnimatorState[] emptyState;         // �� ����(Empty State)���� �����ϴ� �迭
    Text resultText;                    // �ؽ�Ʈ ����� �����ϴ� ����
    int num = 0;                         // �ؽ�Ʈ�� �����ϱ� ���� �ε��� ����

    public AnimationClip defaultClip;    // �⺻ �ִϸ��̼� Ŭ���� �����ϴ� ����
    public GameObject Content_text;      // �ؽ�Ʈ�� ǥ�õ� GameObject�� �����ϴ� ����

    private void Start()
    {
        anim = GetComponent<Animator>();  // ��ũ��Ʈ�� ����� ��ü�� Animator ������Ʈ�� ���� ���� ȹ��
    }

    public void Btn()
    {
        InitializeEmptyStates();  // �� ���� �迭 �ʱ�ȭ
        ApplyText();             // �ؽ�Ʈ ����
        AddEmptyStates();        // �� ���� �߰�
        SetupTransitions();       // ���� ����
    }

    private void InitializeEmptyStates()
    {
        emptyState = new AnimatorState[SentenceToWords.words.Count + 1];  // �� ���� �迭 �ʱ�ȭ
    }

    private void AddEmptyStates()
    {
        var ac = anim.runtimeAnimatorController as AnimatorController;  // ���� ���� �ִϸ����� ��Ʈ�ѷ��� ���� ���� ȹ��

        if (ac != null)
        {
            stateMachine = ac.layers[0].stateMachine;  // ���� ��迡 ���� ���� ȹ��

            List<string> words = SentenceToWords.words;  // SentenceToWords ��ũ��Ʈ���� ������ �ܾ� ���

            for (int i = 0; i < words.Count + 1; i++)
            {
                // ���� �ε����� �ش��ϴ� �ܾ ���� �ִϸ��̼� Ŭ���� �ε��ϰų�, �⺻ Ŭ���� ���
                AnimationClip clip = (i != words.Count) ? Resources.Load(words[i]) as AnimationClip : defaultClip;
                // �� ���¸� �߰��ϰ� �ش� ���¿� �ִϸ��̼� Ŭ�� ����
                emptyState[i] = stateMachine.AddState((i != words.Count) ? words[i] : defaultClip.name);
                emptyState[i].motion = clip;
            }
        }
    }

    private void SetupTransitions()
    {
        for (int i = 0; i < emptyState.Length - 1; i++)
        {
            // �� �� ���¿��� ���� �� ���·��� ���̸� �����ϰ�, Exit Time�� ����Ͽ� ���� ���� ����
            AnimatorStateTransition transition = emptyState[i].AddTransition(emptyState[i + 1]);
            transition.hasExitTime = true;
        }
    }

    private void ApplyText()
    {
        // Content_text���� �ڽ� ��ü �� �ε����� �ش��ϴ� �ؽ�Ʈ ������Ʈ�� ������ ����
        resultText = Content_text.transform.GetChild(num).GetChild(0).GetComponent<Text>();
        num++;  // ���� �ؽ�Ʈ�� �����ϱ� ���� �ε��� ����
    }

    private void RemoveEmptyState()
    {
        // ������ �� ���µ��� ����
        foreach (AnimatorState state in emptyState)
        {
            stateMachine.RemoveState(state);
        }
    }

    private void OnApplicationQuit()
    {
        RemoveEmptyState();  // ���ø����̼��� ����� �� ������ �� ���� ����
    }
}