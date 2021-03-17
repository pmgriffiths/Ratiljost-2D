using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LampUI : MonoBehaviour
{

    Image currentLampShard;

    Image redShardImage;
    Image blueShardImage;
    Image greenShardImage;
    Image whiteShardImage;

    // Start is called before the first frame update
    void Awake()
    {
        currentLampShard = GameObject.Find("LampPanel/Lamp/CurrentShard").GetComponent<Image>(); // .GetComponent<Image>();
        redShardImage = GameObject.Find("LampPanel/Shards/RedShard").GetComponent<Image>();
        blueShardImage = GameObject.Find("LampPanel/Shards/BlueShard").GetComponent<Image>();
        greenShardImage = GameObject.Find("LampPanel/Shards/GreenShard").GetComponent<Image>();
        whiteShardImage = GameObject.Find("LampPanel/Shards/WhiteShard").GetComponent<Image>();

        SetShards(new List<Lamp.LampColours>());
        SetCurrentLampColour(Lamp.LampColours.OFF);
    }

    public void SetShards(List<Lamp.LampColours> lampColours)
    {
        whiteShardImage.enabled = lampColours.Contains(Lamp.LampColours.WHITE);
        redShardImage.enabled = lampColours.Contains(Lamp.LampColours.RED);
        greenShardImage.enabled = lampColours.Contains(Lamp.LampColours.GREEN);
        blueShardImage.enabled = lampColours.Contains(Lamp.LampColours.BLUE);
    }

    public void SetCurrentLampColour(Lamp.LampColours currentColour)
    {
        switch (currentColour)
        {
            case Lamp.LampColours.OFF:
                currentLampShard.enabled = false;
                break;

            case Lamp.LampColours.WHITE:
                currentLampShard.enabled = true;
                currentLampShard.sprite = whiteShardImage.sprite;
                break;

            case Lamp.LampColours.RED:
                currentLampShard.enabled = true;
                currentLampShard.sprite = redShardImage.sprite;
                break;
            case Lamp.LampColours.GREEN:
                currentLampShard.enabled = true;
                currentLampShard.sprite = greenShardImage.sprite;
                break;
            case Lamp.LampColours.BLUE:
                currentLampShard.enabled = true;
                currentLampShard.sprite = blueShardImage.sprite;
                break;
        }
    }


}
