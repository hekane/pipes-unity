using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] public Slider m_pipeSegmentLenghtSlider;
    [SerializeField] public Slider m_detailSlider;
    [SerializeField] public Slider m_pipeRadiusSlider;
    [SerializeField] public TextMeshProUGUI r;
    [SerializeField] public TextMeshProUGUI l;
    [SerializeField] public TextMeshProUGUI d;
    private float lenght = 1;
    private float radius = 1;
    private float detail = 1;

    private void Start()
    {

        m_pipeSegmentLenghtSlider.onValueChanged.AddListener(OnSlider1ValueChanged);
        m_detailSlider.onValueChanged.AddListener(OnSlider2ValueChanged);
        m_pipeRadiusSlider.onValueChanged.AddListener(OnSlider3ValueChanged);
        l.text = m_pipeSegmentLenghtSlider.value.ToString();
        r.text = m_pipeRadiusSlider.value.ToString();
        d.text = m_detailSlider.value.ToString();

    }

    public void OnSlider1ValueChanged(float value)
    {
        lenght=value;
        l.text = value.ToString();
    }

    // Slider 2 callback
    public void OnSlider2ValueChanged(float value)
    {
        detail=value;
        d.text = value.ToString();
    }

    // Slider 3 callback
    public void OnSlider3ValueChanged(float value)
    {
        radius=value;
        r.text = value.ToString();
    }
    public float GetLenght()
    {
        return lenght;
    }
    public int GetRadius()
    {
        return (int)radius;
    }
    public float GetDetail()
    {
        return detail;
    }
}
