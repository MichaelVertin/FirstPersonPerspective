using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject barMax;
    [SerializeField] private GameObject barCurrent;

    private float __healthRatio = 1.0f;

    public void Awake()
    {
        UpdateBar();
    }

    public float HealthRatio
    {
        get { return __healthRatio; }
        set 
        {
            __healthRatio = value;
            if(__healthRatio < 0.0f) { __healthRatio = 0.0f; }
            if(__healthRatio > 1.0f) {  __healthRatio = 1.0f; }
            UpdateBar();
        }
    }

    public void UpdateBar()
    {
        // set color of bar
        // linear interpolate between green (1) and red (0)
        Vector3 red = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 green = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 interColor = (green * __healthRatio) + red * (1.0f - __healthRatio);
        barCurrent.GetComponent<Renderer>().material.color = new Color(interColor.x, interColor.y, interColor.z);

        // bar boundaries:
        //  -.5: left max
        //  +.5: right max
        // when ratio is 0, should be centered at -.5 to only be located at left
        // when ratio is 1, should be centered at 0 to be located between -.5 and +.5
        // all other values should be linearly interpolated
        Vector3 barPos = Vector3.zero;
        barPos.x = -.5f + __healthRatio / 2;
        barCurrent.transform.localPosition = barPos;

        // bar scale:
        // scale to ratio filled
        Vector3 barScale = barCurrent.transform.localScale;
        barScale.x = __healthRatio;
        barCurrent.transform.localScale = barScale;    
    }
}
