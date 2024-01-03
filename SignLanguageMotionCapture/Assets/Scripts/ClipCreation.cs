using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ClipCreation : MonoBehaviour
{
    Animator anim;                      // Animator 컴포넌트에 대한 참조를 저장하는 변수
    AnimatorStateMachine stateMachine;  // Animator의 상태 기계에 대한 참조를 저장하는 변수
    AnimatorState[] emptyState;         // 빈 상태(Empty State)들을 저장하는 배열
    int num = 0;                         // 텍스트에 접근하기 위한 인덱스 변수

    public RaycastHit hit;
    public static Text resultText;                    // 텍스트 결과를 저장하는 변수
    public Text clickText = null;                    // 클릭한 텍스트를 저장하는 변수
    public AnimationClip defaultClip;    // 기본 애니메이션 클립을 설정하는 변수
    public GameObject Content_text;      // 텍스트가 표시될 GameObject를 설정하는 변수

    private void Start()
    {
        anim = GetComponent<Animator>();  // 스크립트가 연결된 객체의 Animator 컴포넌트에 대한 참조 획득

    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            clickText.text = hit.collider.transform.GetChild(0).GetChild(0).GetComponent<Text>().text;

            if (Physics.Raycast(ray, out hit))
            {
                // 히트된 객체에 대한 처리
                Debug.Log(clickText.text);
            }
        }
    }

    public void Btn()
    {
        InitializeEmptyStates();  // 빈 상태 배열 초기화
        //ApplyText();             // 텍스트 적용
        AddEmptyStates();        // 빈 상태 추가
        SetupTransitions();       // 전이 설정
    }

    private void InitializeEmptyStates()
    {
        emptyState = new AnimatorState[SentenceToWords.words.Count + 1];  // 빈 상태 배열 초기화
    }

    private void AddEmptyStates()
    {

        var ac = anim.runtimeAnimatorController as AnimatorController;  // 실행 중인 애니메이터 컨트롤러에 대한 참조 획득

        if (ac != null)
        {
            stateMachine = ac.layers[0].stateMachine;  // 상태 기계에 대한 참조 획득

            List<string> words = SentenceToWords.words;  // SentenceToWords 스크립트에서 추출한 단어 목록

            for (int i = 0; i < words.Count + 1; i++)
            {
                // 현재 인덱스에 해당하는 단어에 대한 애니메이션 클립을 로드하거나, 기본 클립을 사용
                AnimationClip clip = (i != words.Count) ? Resources.Load(words[i]) as AnimationClip : defaultClip;
                // 빈 상태를 추가하고 해당 상태에 애니메이션 클립 연결
                emptyState[i] = stateMachine.AddState((i != words.Count) ? words[i] : defaultClip.name);
                emptyState[i].motion = clip;
            }
        }
    }

    private void SetupTransitions()
    {
        for (int i = 0; i < emptyState.Length - 1; i++)
        {
            // 각 빈 상태에서 다음 빈 상태로의 전이를 설정하고, Exit Time을 사용하여 전이 조건 설정
            AnimatorStateTransition transition = emptyState[i].AddTransition(emptyState[i + 1]);
            transition.hasExitTime = true;
        }
    }

    public void ApplyText()
    {

        // Content_text에서 자식 객체 중 인덱스에 해당하는 텍스트 컴포넌트를 가져와 저장
        resultText = Content_text.transform.GetChild(num).GetChild(0).GetComponent<Text>();

        num++;  // 다음 텍스트에 접근하기 위해 인덱스 증가

    }


    public void RemoveEmptyState()
    {
        // 생성한 빈 상태들을 제거
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

        // 모든 클립 제거
        stateMachine.states = new ChildAnimatorState[0];

        // 하위 StateMachines에 대해서도 재귀적으로 클립 제거
        foreach (var subStateMachine in stateMachine.stateMachines)
        {
            RemoveClipsFromStateMachine(subStateMachine.stateMachine);
        }
    }

    private void OnApplicationQuit()
    {
        RemoveEmptyState();  // 애플리케이션이 종료될 때 생성한 빈 상태 제거
    }
}