using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class UIController : MonoBehaviour
{
    [SerializeField] private UIDocument lectureUI;
    [SerializeField] private VisualTreeAsset liTemplate;

    private GameManager gm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowLectureUI()
    {
        lectureUI.gameObject.SetActive(true);

        VisualElement root = lectureUI.rootVisualElement;
        //root.styleSheets.Add(Resources.Load<StyleSheet>("LectureListItemStyle"));

        Button closeButton = root.Q<Button>("Close");
        closeButton.clicked += CloseLectureUI;

        ListView degreeList = root.Q<ListView>("DegreeList");
        List<string> degrees = gm.GetDegrees().ToList();
        Debug.Log(degrees);

        degreeList.makeItem = () => liTemplate.Instantiate();
        degreeList.bindItem = (VisualElement e, int i) =>
        {
            Debug.Log($"Binding {i}: {degrees[i]}");
            Label label = e.Q<Label>();
            label.text = degrees[i];

            Button edit = e.Q<Button>("Edit");
            edit.clicked += () => OnClick("Edit");

            Button delete = e.Q<Button>("Delete");
            delete.clicked += () => OnClick("Delete");
        };

        degreeList.itemsSource = degrees;
        degreeList.Rebuild();
    }

    public void CloseLectureUI()
    {
        lectureUI.gameObject.SetActive(false);
    }

    private void OnClick(string action)
    {
        Debug.Log($"{action}ing");
    }
}
