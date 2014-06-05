using UnityEngine;
using System.Collections;

public class PropulsorBehavior : MonoBehaviour {

    public GameObject Slow;
    public GameObject Idle;
    public GameObject SpeedUp;

    public enum PropulsorType
    {
        Slow,
        Idle,
        SpeedUp
    }

    private PropulsorType _PropulsorType;

    public void SetPropulsorType(PropulsorType type)
    {
        _PropulsorType = type;

        if (type == PropulsorType.Slow)
        {
            Slow.GetComponent<SpriteRenderer>().enabled = true;
            Idle.GetComponent<SpriteRenderer>().enabled = false;
            SpeedUp.GetComponent<SpriteRenderer>().enabled = false;
        }
        else if (type == PropulsorType.Idle)
        {
            Slow.GetComponent<SpriteRenderer>().enabled = false;
            Idle.GetComponent<SpriteRenderer>().enabled = true;
            SpeedUp.GetComponent<SpriteRenderer>().enabled = false;
        }
        else if (type == PropulsorType.SpeedUp)
        {
            Slow.GetComponent<SpriteRenderer>().enabled = false;
            Idle.GetComponent<SpriteRenderer>().enabled = false;
            SpeedUp.GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
