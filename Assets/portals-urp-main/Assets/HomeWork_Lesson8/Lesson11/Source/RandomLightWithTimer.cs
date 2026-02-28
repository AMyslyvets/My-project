using UnityEngine;
using System.Collections;

public class LightMerc : MonoBehaviour
{
    //public AudioSource sound;
    private Light _light;
    private float start_intensity;
    public float min_intensity;
    public float max_intensity;
    public float noise_speed;
    public bool flicker_ON;
    public bool random_timer;
    public float random_timer_value_MIN;
    public float random_timer_value_MAX;
    private float random_timer_value;
    public float start_timer_value;

    IEnumerator Start ()
    {
        _light = GetComponent<Light>();
        start_intensity = _light.intensity;
        yield return new WaitForSeconds(start_timer_value);
        //if (flicker_ON) sound.Play();

        while(random_timer)
        {
            random_timer_value = Random.Range(random_timer_value_MIN, random_timer_value_MAX);
            yield return new WaitForSeconds(random_timer_value);
            if(flicker_ON)
            {
                _light.intensity = start_intensity;
                //sound.Pause();
                flicker_ON = false;
            }
            else
            {
                //sound.Play();
                flicker_ON = true;
            }
        }
    }
    void Update()
    {
        if (flicker_ON) _light.intensity = Mathf.Lerp(min_intensity, max_intensity, Mathf.PerlinNoise(10, Time.time / noise_speed));
    }
}