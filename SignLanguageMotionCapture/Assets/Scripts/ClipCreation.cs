using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ClipCreation : MonoBehaviour
{
    Animator anim;                      // Animator ������Ʈ�� ���� ������ �����ϴ� ����
    AnimatorStateMachine stateMachine;  // Animator�� ���� ��迡 ���� ������ �����ϴ� ����
    AnimatorState[] emptyState;         // �� ����(Empty State)���� �����ϴ� �迭
    int num = 0;                         // �ؽ�Ʈ�� �����ϱ� ���� �ε��� ����

    public RaycastHit hit;
    public static Text resultText;                    // �ؽ�Ʈ ����� �����ϴ� ����
    public Text clickText = null;                    // Ŭ���� �ؽ�Ʈ�� �����ϴ� ����
    public AnimationClip defaultClip;    // �⺻ �ִϸ��̼� Ŭ���� �����ϴ� ����
    public GameObject Content_text;      // �ؽ�Ʈ�� ǥ�õ� GameObject�� �����ϴ� ����

    private void Start()
    {
        anim = GetComponent<Animator>();  // ��ũ��Ʈ�� ����� ��ü�� Animator ������Ʈ�� ���� ���� ȹ��

    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            clickText.text = hit.collider.transform.GetChild(0).GetChild(0).GetComponent<Text>().text;

            if (Physics.Raycast(ray, out hit))
            {
                // ��Ʈ�� ��ü�� ���� ó��
                Debug.Log(clickText.text);
            }
        }
    }

    public void Btn()
    {
        InitializeEmptyStates();  // �� ���� �迭 �ʱ�ȭ
        //ApplyText();             // �ؽ�Ʈ ����
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

    public void ApplyText()
    {

        // Content_text���� �ڽ� ��ü �� �ε����� �ش��ϴ� �ؽ�Ʈ ������Ʈ�� ������ ����
        resultText = Content_text.transform.GetChild(num).GetChild(0).GetComponent<Text>();

        num++;  // ���� �ؽ�Ʈ�� �����ϱ� ���� �ε��� ����

    }


    public void RemoveEmptyState()
    {
        // ������ �� ���µ��� ����
        foreach (AnimatorState state in emptyState)
        {
            if (state.name != defaultClip.name)
            {
                stateMachine.RemoveState(state);
            }
        }
    }

    public void RemoveAllEmptyState()
    {
        AnimatorController animatorController = anim.runtimeAnimatorController as AnimatorController;

        AnimatorControllerLayer[] layers = animatorController.layers;

        foreach (var layer in layers)
        {
            AnimatorStateMachine stateMachine = layer.stateMachine;
            RemoveClipsFromStateMachine(stateMachine);
        }

    }

    void RemoveClipsFromStateMachine(AnimatorStateMachine stateMachine)
    {
        if (stateMachine == null)
            return;

        // ��� Ŭ�� ����
        stateMachine.states = new ChildAnimatorState[0];

        // ���� StateMachines�� ���ؼ��� ��������� Ŭ�� ����
        foreach (var subStateMachine in stateMachine.stateMachines)
        {
            RemoveClipsFromStateMachine(subStateMachine.stateMachine);
        }
    }

    private void OnApplicationQuit()
    {
        RemoveEmptyState();  // ���ø����̼��� ����� �� ������ �� ���� ����
    }
}