using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlidableMotionController : MonoBehaviour
{
    private Slider   timeSlider = null;

    private Dropdown motionDropdown = null;

    private Button playButton = null;

    private Animation myMotion = null;

    private Text playButtonText = null;

    private Boolean isPlay = false;

    // Start is called before the first frame update
    void Start()
    {
        var manager = FindObjectOfType<SlidableMotionManager>();
        if (manager == null)
        {
            throw new InvalidOperationException("manager need set to canvas.");
        }

        timeSlider = manager.timeSlider;
        motionDropdown = manager.motionDropdown;
        playButton = manager.playButton;

        if (motionDropdown == null)
        {
            throw new InvalidOperationException("motionDropdown can not null.");
        }
        if (timeSlider == null)
        {
            throw new InvalidOperationException("timeSlider can not null.");
        }
        if (playButton == null)
        {
            throw new InvalidOperationException("playButton can not null.");
        }

        myMotion = GetComponent<Animation>();
        if (myMotion == null)
        {
            throw new InvalidOperationException(gameObject.name + " is not have animation.\nSlidableMotionController need animation on Live2D model.");
        }

        if (myMotion.GetClipCount() == 0)
        {
            var clips = FindObjectsOfType<AnimationClip>();
            foreach (var animationClip in clips)
            {
                if ( !animationClip.legacy ) continue;
                
                myMotion.AddClip(animationClip, animationClip.name);
            }
        }
        
        InitMotionDropdown();
        setButtonEvent();
    }

    private void InitMotionDropdown()
    {
        if( motionDropdown == null ) return;
        if( myMotion.GetClipCount() < 1 ) return;
        
        motionDropdown.ClearOptions();
        
        foreach (AnimationState motionStat in myMotion)
        {
            var option = new Dropdown.OptionData(motionStat.name);
            motionDropdown.options.Add(option);
        }

        motionDropdown.value = 0;
    }

    private void setButtonEvent()
    {
        playButtonText = playButton.GetComponentInChildren<Text>();
        if (playButtonText == null)
        {
            throw new InvalidOperationException("playButton not have Text Area.");
        }
        playButton.onClick.AddListener( this.playOrStop );
    }
    
    // Update is called once per frame
    void Update()
    {
        MotionBake();
    }
    
    private void MotionBake()
    {
        if( motionDropdown == null ) return;
        if( timeSlider == null ) return;
        if( myMotion == null ) return;

        var selectedName = motionDropdown.options[motionDropdown.value].text;
        var targetClip = myMotion[selectedName].clip;
        myMotion.clip = targetClip;
        myMotion.Play (myMotion.clip.name);

        if (isPlay)
        {
            var addSlider = (1f / targetClip.length) * Time.deltaTime;
            timeSlider.value = (timeSlider.value + addSlider) % 1f;
        }

        myMotion[selectedName].speed = 0f;
        myMotion[selectedName].normalizedTime = timeSlider.value;
    }

    public void playOrStop()
    {
        isPlay = !isPlay;
        
        if (isPlay)
        {
            playButtonText.text = "▶";
        }
        else
        {
            playButtonText.text = "■";
        }
    }
}
